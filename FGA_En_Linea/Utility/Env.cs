using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FGA.Models;
using FGA.Utility;
using Microsoft.Owin.Security;

namespace FGA
{
    public static class Env
    {
        public static void AddUpdateClaim(this IPrincipal currentPrincipal, string key, string value)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return;

            var existingClaim = identity.FindFirst(key);
            if (existingClaim != null)
                identity.RemoveClaim(existingClaim);

            identity.AddClaim(new Claim(key, value));
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identity), new AuthenticationProperties() { IsPersistent = true });
        }

        public static string GetUserRoleOrUsername(this HtmlHelper s, bool IsRoleID)
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string role = string.Empty;

            if (IsRoleID == true)
                role = identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();
            else
                role = identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            return role;
        }

        public static string GetUserInfo(string value)
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string ReturnVal = string.Empty;

            try
            {
                switch (value)
                {
                    case "name":
                        ReturnVal = identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                        break;
                    case "userid":
                        ReturnVal = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
                        break;
                    case "roleid":
                        ReturnVal = identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();
                        break;
                    case "logo":
                        ReturnVal = identity.Claims.Where(c => c.Type == ClaimTypes.UserData).Select(c => c.Value).SingleOrDefault();
                        break;
                    case "entidad":
                        ReturnVal = identity.Claims.Where(c => c.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
                        break;
                    case "gender":
                        ReturnVal = identity.Claims.Where(c => c.Type == ClaimTypes.Gender).Select(c => c.Value).SingleOrDefault();
                        break;
                    case "cambio":
                        ReturnVal = identity.Claims.Where(c => c.Type == "CambiarClave").Select(c => c.Value).SingleOrDefault();
                        break;
                    case "evaluacion":
                        ReturnVal = identity.Claims.Where(c => c.Type == "Evaluacion").Select(c => c.Value).SingleOrDefault();
                        break;
                    default:
                        ReturnVal = "";
                        break;
                }
            }
            catch (Exception)
            {
            }

            return ReturnVal;
        }

        public static string Language()
        {
            var currentContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            try
            {
                var routeData = RouteTable.Routes.GetRouteData(currentContext);
                string languageCode = (string)routeData.Values["cultureName"];
                return languageCode.ToLower();
            }
            catch (Exception)
            {
                return "en";
            }
        }

        public static string Decrypt(string cryptedString)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool");
            if (String.IsNullOrEmpty(cryptedString))
                throw new ArgumentNullException("The string which needs to be decrypted can not be null.");

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString));
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }

        public static string Encrypt(string originalString)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool");
            if (String.IsNullOrEmpty(originalString))
                throw new ArgumentNullException("The string which needs to be encrypted can not be null.");

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            string output = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            return output;
        }

        public static string GetSiteRoot()
        {
            string sOut = "";
            if (HttpContext.Current != null)
            {
                string Port = HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
                if (Port == null || Port == "80" || Port == "443")
                    Port = string.Empty;
                else
                    Port = ":" + Port;

                string Protocol = HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"];
                if (Protocol == null || Protocol.Equals("0"))
                    Protocol = "http://";
                else
                    Protocol = "https://";

                string appPath = HttpContext.Current.Request.ApplicationPath;
                if (appPath == "/")
                    appPath = "";

                sOut = Protocol + HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + Port + appPath;
            }
            return sOut;
        }
        public static MenuPermission[] GetRoleMenuPermissions(int roleId)
        {
            string cacheKey = "RoleMenuStructure_" + roleId;
            if (HttpRuntime.Cache != null)
            {
                var cached = HttpRuntime.Cache.Get(cacheKey) as MenuPermission[];
                if (cached != null)
                {
                    return cached;
                }
            }

            try
            {
                using (var men = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient())
                {
                    var rawList = men.GetMenu(roleId) ?? new MenuPermission[0];
                    var globleList = rawList.ToList();

                    foreach (var p in globleList.Where(p => p.Menu_MenuId != null))
                    {
                        string mUrl = (p.Menu_MenuId.MenuURL ?? "").Trim();
                        string mText = (p.Menu_MenuId.MenuText ?? "").Trim();
                        if (p.Menu_MenuId.Id == 9145 || p.Menu_MenuId.Id == 13001 || p.Menu_MenuId.Id == 9135 ||
                            mUrl.Equals("Role/Index", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Role", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Menu/Index", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Menu", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Evaluacion", StringComparison.OrdinalIgnoreCase) ||
                            mText.Equals("Roles", StringComparison.OrdinalIgnoreCase) ||
                            mText.IndexOf("Menú del Sistema", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Menu del Sistema", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Menu Sistema", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Configuración de Preguntas", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Configuracion de Preguntas", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            p.Menu_MenuId.ParentId = 9200; // Gestión Operativa
                        }
                        else if (p.Menu_MenuId.Id == 6002 || p.Menu_MenuId.Id == 6003 || p.Menu_MenuId.Id == 6005 ||
                                 mUrl.Equals("Evaluacion/Avance", StringComparison.OrdinalIgnoreCase) ||
                                 mUrl.Equals("Evaluacion/Historial", StringComparison.OrdinalIgnoreCase) ||
                                 mUrl.Equals("Evaluacion/Resultados", StringComparison.OrdinalIgnoreCase) ||
                                 mText.Equals("Avance SBR", StringComparison.OrdinalIgnoreCase) ||
                                 mText.Equals("Historial SBR", StringComparison.OrdinalIgnoreCase) ||
                                 mText.Equals("Resultados SBR", StringComparison.OrdinalIgnoreCase))
                        {
                            p.Menu_MenuId.ParentId = 9300; // Evaluación SBR dentro de Administración
                        }
                        else if (p.Menu_MenuId.Id == 9300)
                        {
                            p.Menu_MenuId.ParentId = 9000; // Hijo de Administración
                            p.Menu_MenuId.MenuText = "Evaluación SBR";
                            p.Menu_MenuId.MenuURL = "filter";
                            p.Menu_MenuId.MenuIcon = "<i class=\"fa fa-tasks\"></i>";
                        }
                        else if (p.Menu_MenuId.Id == 9000 || string.Equals(p.Menu_MenuId.MenuText, "Administración", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Menu_MenuId.MenuText, "Administracion", StringComparison.OrdinalIgnoreCase))
                        {
                            p.Menu_MenuId.ParentId = null;
                            p.Menu_MenuId.MenuURL = "root";
                        }
                    }

                    // Asegurar que el submódulo 9300 esté bajo Administración (9000)
                    var sbrAdmin = globleList.FirstOrDefault(p => p.Menu_MenuId != null && (p.Menu_MenuId.Id == 9300 || ((p.Menu_MenuId.MenuText ?? "").IndexOf("Evaluación SBR", StringComparison.OrdinalIgnoreCase) >= 0 && p.Menu_MenuId.Id != 6000)));
                    if (sbrAdmin == null)
                    {
                        if (globleList.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == 9000))
                        {
                            sbrAdmin = new MenuPermission
                            {
                                MenuId = 9300,
                                RoleId = roleId,
                                Menu_MenuId = new Menu
                                {
                                    Id = 9300,
                                    MenuText = "Evaluación SBR",
                                    MenuURL = "filter",
                                    ParentId = 9000,
                                    MenuIcon = "<i class=\"fa fa-tasks\"></i>",
                                    SortOrder = 4,
                                    Description = "Monitoreo, avance, historial y resultados de la evaluación Supervisión Basada en Riesgos (SBR)."
                                }
                            };
                            globleList.Add(sbrAdmin);
                        }
                    }
                    else
                    {
                        sbrAdmin.Menu_MenuId.Id = 9300;
                        sbrAdmin.Menu_MenuId.ParentId = 9000;
                        sbrAdmin.Menu_MenuId.MenuText = "Evaluación SBR";
                        sbrAdmin.Menu_MenuId.MenuURL = "filter";
                        sbrAdmin.Menu_MenuId.MenuIcon = "<i class=\"fa fa-tasks\"></i>";
                        sbrAdmin.Menu_MenuId.SortOrder = 4;
                    }

                    // Asegurar que el Menú Raíz "Evaluación SBR" (Id 6000) exista al nivel raíz (ParentId = null) para TODOS los roles
                    var sbrRoot = globleList.FirstOrDefault(p => p.Menu_MenuId != null && 
                        (p.Menu_MenuId.Id == 6000 || 
                         (p.Menu_MenuId.ParentId == null && !string.IsNullOrEmpty(p.Menu_MenuId.MenuText) && p.Menu_MenuId.MenuText.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0 && p.Menu_MenuId.Id != 9300)));

                    if (sbrRoot == null)
                    {
                        sbrRoot = new MenuPermission
                        {
                            MenuId = 6000,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 6000,
                                MenuText = "Evaluación SBR",
                                MenuURL = "root",
                                ParentId = null,
                                MenuIcon = "<i class=\"fa fa-pie-chart\"></i>",
                                SortOrder = 4,
                                Description = "Autoevaluación de los aspectos del Reglamento SUGEF 24-22"
                            }
                        };
                        globleList.Add(sbrRoot);
                    }
                    else
                    {
                        sbrRoot.Menu_MenuId.Id = 6000;
                        sbrRoot.Menu_MenuId.ParentId = null;
                        sbrRoot.Menu_MenuId.MenuText = "Evaluación SBR";
                        sbrRoot.Menu_MenuId.MenuIcon = "<i class=\"fa fa-pie-chart\"></i>";
                        sbrRoot.Menu_MenuId.MenuURL = "root";
                        sbrRoot.Menu_MenuId.SortOrder = 4;
                    }

                    // Asegurar que la opción hija "Autoevaluación" (cuestionario) esté registrada bajo Evaluación SBR raíz (6000)
                    var autoEvalChild = globleList.FirstOrDefault(p => p.Menu_MenuId != null && 
                        (p.Menu_MenuId.Id == 6001 || 
                         (!string.IsNullOrEmpty(p.Menu_MenuId.MenuURL) && p.Menu_MenuId.MenuURL.IndexOf("Evaluacion/Evaluacion", StringComparison.OrdinalIgnoreCase) >= 0) ||
                         (!string.IsNullOrEmpty(p.Menu_MenuId.MenuText) && p.Menu_MenuId.MenuText.IndexOf("Autoevaluaci", StringComparison.OrdinalIgnoreCase) >= 0)));

                    if (autoEvalChild == null)
                    {
                        autoEvalChild = new MenuPermission
                        {
                            MenuId = 6001,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 6001,
                                MenuText = "Autoevaluación",
                                MenuURL = "Evaluacion/Evaluacion",
                                ParentId = 6000,
                                MenuIcon = "<i class=\"fa fa-clipboard\"></i>",
                                SortOrder = 1,
                                Description = "Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR)."
                            }
                        };
                        globleList.Add(autoEvalChild);
                    }
                    else
                    {
                        autoEvalChild.Menu_MenuId.ParentId = 6000;
                        autoEvalChild.Menu_MenuId.MenuText = "Autoevaluación";
                        autoEvalChild.Menu_MenuId.MenuURL = "Evaluacion/Evaluacion";
                        autoEvalChild.Menu_MenuId.MenuIcon = "<i class=\"fa fa-clipboard\"></i>";
                        autoEvalChild.Menu_MenuId.SortOrder = 1;
                    }

                    // Asegurar que "Configuración de Preguntas SBR" (Id 9135) exista bajo Gestión Operativa (9200)
                    var pregConfig = globleList.FirstOrDefault(p => p.Menu_MenuId != null && 
                        (p.Menu_MenuId.Id == 9135 || 
                         (!string.IsNullOrEmpty(p.Menu_MenuId.MenuURL) && p.Menu_MenuId.MenuURL.IndexOf("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) >= 0) ||
                         (!string.IsNullOrEmpty(p.Menu_MenuId.MenuText) && p.Menu_MenuId.MenuText.IndexOf("Configuración de Preguntas", StringComparison.OrdinalIgnoreCase) >= 0)));

                    if (pregConfig == null)
                    {
                        if (globleList.Any(p => p.Menu_MenuId != null && (p.Menu_MenuId.Id == 9000 || p.Menu_MenuId.Id == 9200 || p.Menu_MenuId.ParentId == 9000)))
                        {
                            pregConfig = new MenuPermission
                            {
                                MenuId = 9135,
                                RoleId = roleId,
                                Menu_MenuId = new Menu
                                {
                                    Id = 9135,
                                    MenuText = "Configuración de Preguntas SBR",
                                    MenuURL = "Evaluacion/Index",
                                    ParentId = 9200,
                                    MenuIcon = "<i class=\"fa fa-question-circle\"></i>",
                                    SortOrder = 9,
                                    Description = "Mantenimiento y configuración del catálogo de preguntas de la evaluación SBR."
                                }
                            };
                            globleList.Add(pregConfig);
                        }
                    }
                    else
                    {
                        pregConfig.Menu_MenuId.ParentId = 9200;
                        pregConfig.Menu_MenuId.MenuText = "Configuración de Preguntas SBR";
                        pregConfig.Menu_MenuId.MenuURL = "Evaluacion/Index";
                        pregConfig.Menu_MenuId.MenuIcon = "<i class=\"fa fa-question-circle\"></i>";
                        pregConfig.Menu_MenuId.SortOrder = 9;
                    }

                    // Asegurar que las opciones de SBR en Administración existan si el usuario tiene Administración (9000)
                    if (globleList.Any(p => p.Menu_MenuId != null && (p.Menu_MenuId.Id == 9000 || p.Menu_MenuId.Id == 9300)))
                    {
                        if (!globleList.Any(p => p.Menu_MenuId != null && (p.Menu_MenuId.Id == 6002 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion/Avance", StringComparison.OrdinalIgnoreCase) >= 0)))
                        {
                            globleList.Add(new MenuPermission { MenuId = 6002, RoleId = roleId, Menu_MenuId = new Menu { Id = 6002, MenuText = "Avance SBR", MenuURL = "Evaluacion/Avance", ParentId = 9300, SortOrder = 1, MenuIcon = "<i class=\"fa fa-line-chart\"></i>", Description = "Monitoreo del porcentaje de avance en el diligenciamiento de la evaluación SBR." } });
                        }
                        if (!globleList.Any(p => p.Menu_MenuId != null && (p.Menu_MenuId.Id == 6003 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion/Historial", StringComparison.OrdinalIgnoreCase) >= 0)))
                        {
                            globleList.Add(new MenuPermission { MenuId = 6003, RoleId = roleId, Menu_MenuId = new Menu { Id = 6003, MenuText = "Historial SBR", MenuURL = "Evaluacion/Historial", ParentId = 9300, SortOrder = 2, MenuIcon = "<i class=\"fa fa-history\"></i>", Description = "Bitácora histórica de evaluaciones cerradas y enviadas." } });
                        }
                        if (!globleList.Any(p => p.Menu_MenuId != null && (p.Menu_MenuId.Id == 6005 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion/Resultados", StringComparison.OrdinalIgnoreCase) >= 0)))
                        {
                            globleList.Add(new MenuPermission { MenuId = 6005, RoleId = roleId, Menu_MenuId = new Menu { Id = 6005, MenuText = "Resultados SBR", MenuURL = "Evaluacion/Resultados", ParentId = 9300, SortOrder = 3, MenuIcon = "<i class=\"fa fa-bar-chart\"></i>", Description = "Resumen de calificaciones y nivel de cumplimiento por aspecto." } });
                        }
                    }

                    // Deduplicar: asegurar que exista EXACTAMENTE UN solo menú raíz para "Evaluación SBR"
                    var sbrRoots = globleList
                        .Where(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == null && 
                                   ((!string.IsNullOrEmpty(p.Menu_MenuId.MenuText) && p.Menu_MenuId.MenuText.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    p.Menu_MenuId.Id == 6000 || p.Menu_MenuId.Id == 9300))
                        .ToList();

                    if (sbrRoots.Count > 1)
                    {
                        // Si 9300 quedó en la raíz, reasignarlo bajo Administración (9000)
                        foreach (var rootExtra in sbrRoots)
                        {
                            if (rootExtra.Menu_MenuId.Id == 9300)
                            {
                                rootExtra.Menu_MenuId.ParentId = 9000;
                                rootExtra.Menu_MenuId.MenuText = "Evaluación SBR";
                                rootExtra.Menu_MenuId.MenuURL = "filter";
                            }
                        }

                        // Si aún quedara más de un elemento raíz con SBR, dejar únicamente uno
                        var sbrRootsRestantes = globleList
                            .Where(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == null && 
                                       ((!string.IsNullOrEmpty(p.Menu_MenuId.MenuText) && p.Menu_MenuId.MenuText.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                        p.Menu_MenuId.Id == 6000))
                            .ToList();

                        if (sbrRootsRestantes.Count > 1)
                        {
                            for (int i = 1; i < sbrRootsRestantes.Count; i++)
                            {
                                globleList.Remove(sbrRootsRestantes[i]);
                            }
                        }
                    }

                    // Normalizar el orden de los módulos raíz para asegurar la posición homogénea en la barra lateral
                    foreach (var rootItem in globleList.Where(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == null))
                    {
                        string t = (rootItem.Menu_MenuId.MenuText ?? "").Trim();
                        if (t.IndexOf("Tablero", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            rootItem.Menu_MenuId.SortOrder = 1;
                        }
                        else if (t.IndexOf("Estructura Financiera", StringComparison.OrdinalIgnoreCase) >= 0 || t.IndexOf("Información Financiera", StringComparison.OrdinalIgnoreCase) >= 0 || t.IndexOf("Informacion Financiera", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            rootItem.Menu_MenuId.SortOrder = 2;
                        }
                        else if (t.IndexOf("Cartera", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            rootItem.Menu_MenuId.SortOrder = 3;
                        }
                        else if (t.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0 || rootItem.Menu_MenuId.Id == 9300)
                        {
                            rootItem.Menu_MenuId.SortOrder = 4;
                        }
                        else if (t.IndexOf("Administra", StringComparison.OrdinalIgnoreCase) >= 0 || rootItem.Menu_MenuId.Id == 9000)
                        {
                            rootItem.Menu_MenuId.SortOrder = 5;
                        }
                        else if (t.IndexOf("Factura", StringComparison.OrdinalIgnoreCase) >= 0 || rootItem.Menu_MenuId.Id == 8000)
                        {
                            rootItem.Menu_MenuId.SortOrder = 6;
                        }
                    }

                    var result = globleList.ToArray();
                    if (HttpRuntime.Cache != null)
                    {
                        HttpRuntime.Cache.Insert(cacheKey, result, null, DateTime.Now.AddMinutes(60), System.Web.Caching.Cache.NoSlidingExpiration);
                    }
                    return result;
                }
            }
            catch (Exception)
            {
                return new MenuPermission[0];
            }
        }

        public static void InvalidateRoleMenuCache(int? roleId = null)
        {
            if (HttpRuntime.Cache == null) return;

            if (roleId.HasValue)
            {
                HttpRuntime.Cache.Remove("RoleMenuStructure_" + roleId.Value);
            }
            else
            {
                var keysToRemove = new List<string>();
                var enumerator = HttpRuntime.Cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string k = enumerator.Key?.ToString() ?? "";
                    if (k.StartsWith("RoleMenuStructure_"))
                    {
                        keysToRemove.Add(k);
                    }
                }
                foreach (var k in keysToRemove)
                {
                    HttpRuntime.Cache.Remove(k);
                }
            }
        }

        public static MvcHtmlString GetMenuBarPage(Nullable<int> ParentId)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int roleId = 0;
                int.TryParse(Env.GetUserInfo("roleid"), out roleId);
                var globle = GetRoleMenuPermissions(roleId);

                int? activeMenuId = DetermineActiveMenuId(globle);

                sb.Append("<ul class=\"sidebar-menu\"  data-widget=\"tree\">");
                sb.Append(GetMenuBar(ParentId, globle, activeMenuId));
                return MvcHtmlString.Create(sb.ToString());
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static string NormalizeMenuPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            path = path.Trim().Trim('/').ToLower();

            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                string appPath = (HttpContext.Current.Request.ApplicationPath ?? "").Trim('/').ToLower();
                if (!string.IsNullOrEmpty(appPath) && (path == appPath || path.StartsWith(appPath + "/")))
                {
                    path = path.Substring(appPath.Length).Trim('/');
                }
            }

            if (path.EndsWith("/index"))
            {
                path = path.Substring(0, path.Length - 6);
            }
            else if (path == "index")
            {
                path = string.Empty;
            }

            return path;
        }

        private static int? DetermineActiveMenuId(MenuPermission[] permissions)
        {
            if (permissions == null || permissions.Length == 0 || HttpContext.Current == null || HttpContext.Current.Request == null)
                return null;

            string currentRaw = HttpContext.Current.Request.Url.AbsolutePath ?? "";
            string currentNorm = NormalizeMenuPath(currentRaw);

            if (string.IsNullOrEmpty(currentNorm))
            {
                currentNorm = "home";
            }

            // Detección para Hub de módulos: /Modulo/Index/{id} o /Modulo/{id}
            if (currentNorm.StartsWith("modulo/index/") || currentNorm.StartsWith("modulo/"))
            {
                var parts = currentNorm.Split('/');
                if (parts.Length >= 2)
                {
                    string lastPart = parts[parts.Length - 1];
                    if (int.TryParse(lastPart, out int modId))
                    {
                        return modId;
                    }
                }
            }
            else if (currentNorm == "modulo" || currentNorm == "modulo/index")
            {
                var firstRoot = permissions.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == null && permissions.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == p.Menu_MenuId.Id));
                if (firstRoot != null && firstRoot.Menu_MenuId != null)
                {
                    return firstRoot.Menu_MenuId.Id;
                }
            }

            int? bestMatchId = null;
            int bestMatchScore = -1;

            foreach (var p in permissions)
            {
                if (p.Menu_MenuId == null) continue;
                string url = p.Menu_MenuId.MenuURL;
                if (string.IsNullOrEmpty(url) || url == "#" || url.StartsWith("javascript"))
                    continue;

                string itemNorm = NormalizeMenuPath(url);
                if (string.IsNullOrEmpty(itemNorm))
                    continue;

                int score = -1;

                if (currentNorm == itemNorm)
                {
                    score = 10000 + itemNorm.Length;
                }
                else if (currentNorm.StartsWith(itemNorm + "/"))
                {
                    score = 5000 + itemNorm.Length;
                }
                else if (!itemNorm.Contains("/") && currentNorm.StartsWith(itemNorm + "/"))
                {
                    score = 1000 + itemNorm.Length;
                }

                if (score > bestMatchScore)
                {
                    bestMatchScore = score;
                    bestMatchId = p.Menu_MenuId.Id;
                }
            }

            return bestMatchId;
        }

        private static bool HasActiveDescendant(MenuPermission[] q, int parentId, int targetActiveId)
        {
            if (q == null) return false;
            foreach (var child in q.Where(i => i.Menu_MenuId.ParentId == parentId))
            {
                if (child.Menu_MenuId.Id == targetActiveId)
                    return true;
                if (HasActiveDescendant(q, child.Menu_MenuId.Id, targetActiveId))
                    return true;
            }
            return false;
        }

        private static MvcHtmlString GetMenuBar(Nullable<int> ParentId, MenuPermission[] q)
        {
            int? activeMenuId = DetermineActiveMenuId(q);
            return GetMenuBar(ParentId, q, activeMenuId);
        }

        private static MvcHtmlString GetMenuBar(Nullable<int> ParentId, MenuPermission[] q, int? activeMenuId)
        {
            StringBuilder sb = new StringBuilder();

            if (q != null)
            {
                foreach (var item in q.Where(i => i.Menu_MenuId != null && i.Menu_MenuId.ParentId == ParentId).OrderBy(i => i.Menu_MenuId.SortOrder ?? 0).ThenBy(i => i.Menu_MenuId.MenuText))
                {
                    bool isSelfActive = activeMenuId.HasValue && item.Menu_MenuId.Id == activeMenuId.Value;
                    bool hasChildren = q.Any(j => j.Menu_MenuId.ParentId == item.Menu_MenuId.Id);
                    bool isTreeActive = (activeMenuId.HasValue && HasActiveDescendant(q, item.Menu_MenuId.Id, activeMenuId.Value)) || isSelfActive;
                    string itemStyle = isTreeActive ? "active" : string.Empty;

                    string rawDbIcon = item.Menu_MenuId.MenuIcon;
                    string menuIcon;
                    if (string.IsNullOrWhiteSpace(rawDbIcon))
                    {
                        menuIcon = "<i class=\"fa fa-folder-o\"></i>";
                    }
                    else if (!rawDbIcon.Trim().StartsWith("<"))
                    {
                        menuIcon = MenuCatalogService.FormatearHtmlIcono(rawDbIcon, "fa fa-folder-o");
                    }
                    else
                    {
                        menuIcon = rawDbIcon;
                    }

                    string itemUrl = (item.Menu_MenuId.MenuURL ?? "").Trim();
                    bool hasDirectUrl = !string.IsNullOrEmpty(itemUrl) && !itemUrl.Equals("root", StringComparison.OrdinalIgnoreCase) && !itemUrl.Equals("filter", StringComparison.OrdinalIgnoreCase) && itemUrl != "#";

                    if (hasDirectUrl)
                    {
                        // Enlace directo configurado en la base de datos (ej. Evaluacion/Evaluacion, Home/Index)
                        sb.Append("<li class=\"" + itemStyle + "\"> <a draggable=\"false\" style=\"font-size:13px;\" href=\"" + MicrosoftHelper.MSHelper.GetSiteRoot() + "/" + itemUrl.TrimStart('~', '/') + "\">" + menuIcon + "  <span style=\"font-size:13px;\">" + item.Menu_MenuId.MenuText + "</span> <span class=\"pull-right-container\"></span></a></li>");
                    }
                    else if (hasChildren)
                    {
                        // Enlace directo al Hub del módulo raíz contenedor
                        sb.Append("<li class=\"" + itemStyle + "\"> <a draggable=\"false\" style=\"font-size:13px;\" href=\"" + MicrosoftHelper.MSHelper.GetSiteRoot() + "/Modulo/Index/" + item.Menu_MenuId.Id + "\">" + menuIcon + "  <span style=\"font-size:13px;\">" + item.Menu_MenuId.MenuText + "</span> <span class=\"pull-right-container\"></span></a></li>");
                    }
                    else
                    {
                        // Opción raíz sin hijos
                        sb.Append("<li class=\"" + itemStyle + "\"> <a draggable=\"false\" style=\"font-size:13px;\" href=\"" + MicrosoftHelper.MSHelper.GetSiteRoot() + "/" + itemUrl + "\">" + menuIcon + "  <span style=\"font-size:13px;\">" + item.Menu_MenuId.MenuText + "</span> <span class=\"pull-right-container\"></span></a></li>");
                    }
                }
                sb.Append("</ul>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public static string ObtenerRutaLogoReporte(object sessionLogo)
        {
            if (sessionLogo == null)
                return string.Empty;

            string logo = sessionLogo.ToString();
            // Reemplaza dinamicamente cualquier variante del dominio publico (ej. /FFC, /FFC2, /FFC3) por la ruta del servidor interno
            string rutaReporte = System.Text.RegularExpressions.Regex.Replace(
                logo,
                @"https?://(www\.)?ffc\.co\.cr/FFC[0-9]*",
                "http://10.171.1.26/ffc",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // Respaldo para normalizar cualquier sufijo numerico en la IP interna a /ffc
            rutaReporte = System.Text.RegularExpressions.Regex.Replace(
                rutaReporte,
                @"http://10\.171\.1\.26/ffc[0-9]+",
                "http://10.171.1.26/ffc",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return rutaReporte;
        }
    }
}