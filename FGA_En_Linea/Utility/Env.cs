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

                sb.Append("<ul class=\"sidebar-menu\"  data-widget=\"tree\">");
                sb.Append(GetMenuBar(ParentId, (MenuPermission[])globle));
                return MvcHtmlString.Create(sb.ToString());
            }
            catch (Exception e)
            {
            }

            return null;
        }

        private static MvcHtmlString GetMenuBar(Nullable<int> ParentId, MenuPermission[] q)
        {
            var path = HttpContext.Current.Request.Url.AbsoluteUri;
            StringBuilder sb = new StringBuilder();
            var menuStyle = string.Empty;

            if (q != null)
            {
                foreach (var item in q.Where(i => i.Menu_MenuId.ParentId == ParentId).OrderBy(i => i.SortOrder))
                {
                    var js = q;
                    menuStyle = string.Empty;

                    if (js.Count(j => j.Menu_MenuId.ParentId == item.Menu_MenuId.Id) > 0)
                    {

                        foreach (var detalle in q.Where(i => i.Menu_MenuId.ParentId == item.Menu_MenuId.Id))
                        {
                            if (path.Contains(detalle.Menu_MenuId.MenuURL))
                                menuStyle = "menu-open active";

                            if (q.Where(i => i.Menu_MenuId.ParentId == detalle.Menu_MenuId.Id && path.Contains(i.Menu_MenuId.MenuURL)).Count() >= 1)
                                menuStyle = "menu-open active";
                        }

                        sb.Append("<li class=\"treeview " + menuStyle + "\"> <a href=\"#\"> " + item.Menu_MenuId.MenuIcon + "<span style=\"font-size:13px;\">" + item.Menu_MenuId.MenuText + "</span><span class=\"pull-right-container\"> <i class=\"fa fa-angle-left pull-right\"></i></span> </a><ul class=\"treeview-menu\">");
                        sb.Append(GetMenuBar(item.Menu_MenuId.Id, q));
                        sb.Append("</li>");
                    }
                    else
                    {
                        if (item.Menu_MenuId.ParentId == null)
                            sb.Append("<li class=\"\"> <a style=\"font-size:13px;\" href=\"" + MicrosoftHelper.MSHelper.GetSiteRoot() + "/" + item.Menu_MenuId.MenuURL + "\">" + item.Menu_MenuId.MenuIcon + "  <span style=\"font-size:13px;\">" + item.Menu_MenuId.MenuText + "</span> <span class=\"pull-right-container\"></span></a></li>");
                        else
                            sb.Append("<li class=\"\"> <a style=\"font-size:13px;\" href=\"" + MicrosoftHelper.MSHelper.GetSiteRoot() + "/" + item.Menu_MenuId.MenuURL + "\">" + item.Menu_MenuId.MenuIcon + " " + item.Menu_MenuId.MenuText + "</a></li>");
                    }
                }
                sb.Append("</ul>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}