using System;
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
        public static MvcHtmlString GetMenuBarPage(Nullable<int> ParentId)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
                FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient men = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient();
                var userId = Env.GetUserInfo("userid");
                var RoleId = int.Parse(Env.GetUserInfo("roleid"));
                Usuario ObjUser = usr.Get(userId);
                var evaluacion = Env.GetUserInfo("evaluacion") == "S" ? -1 : Utility.Utilitarios.opcionEvaluacion;
                var globle = men.GetMenu(RoleId).Where(o => o.MenuId != evaluacion).ToArray();

                int? activeMenuId = DetermineActiveMenuId((MenuPermission[])globle);

                sb.Append("<ul class=\"sidebar-menu\"  data-widget=\"tree\">");
                sb.Append(GetMenuBar(ParentId, (MenuPermission[])globle, activeMenuId));
                return MvcHtmlString.Create(sb.ToString());
            }
            catch (Exception e)
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
                foreach (var item in q.Where(i => i.Menu_MenuId.ParentId == ParentId).OrderBy(i => i.SortOrder))
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

                    if (hasChildren)
                    {
                        // Enlace directo al Hub del módulo raíz
                        sb.Append("<li class=\"" + itemStyle + "\"> <a draggable=\"false\" style=\"font-size:13px;\" href=\"" + MicrosoftHelper.MSHelper.GetSiteRoot() + "/Modulo/Index/" + item.Menu_MenuId.Id + "\">" + menuIcon + "  <span style=\"font-size:13px;\">" + item.Menu_MenuId.MenuText + "</span> <span class=\"pull-right-container\"></span></a></li>");
                    }
                    else
                    {
                        // Opción raíz sin hijos (enlace directo)
                        sb.Append("<li class=\"" + itemStyle + "\"> <a draggable=\"false\" style=\"font-size:13px;\" href=\"" + MicrosoftHelper.MSHelper.GetSiteRoot() + "/" + item.Menu_MenuId.MenuURL + "\">" + menuIcon + "  <span style=\"font-size:13px;\">" + item.Menu_MenuId.MenuText + "</span> <span class=\"pull-right-container\"></span></a></li>");
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