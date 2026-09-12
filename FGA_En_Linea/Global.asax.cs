using FGA.Models;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FGA
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {                    
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                Exception ex = Server.GetLastError();
                string path = "N/A";
                if (sender is HttpApplication)
                    path = ((HttpApplication)sender).Request.Url.PathAndQuery;

                FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
                FGA_En_Linea.LogService.ServiceOf_LogClient log = new FGA_En_Linea.LogService.ServiceOf_LogClient();
                var idUsuario = int.Parse(Env.GetUserInfo("userid"));
                var traceLog = new Log
                {
                    Controller = path,
                    Action = string.Empty,
                    Mensaje = ex.Message,
                    Fecha = DateTime.Now,
                    IdUsuario_Id = idUsuario
                };

                if (traceLog.Mensaje.Length > 2)
                    log.Add(ref traceLog);
            }
            catch (Exception) { }
        }

        void Session_End(object sender, EventArgs e)
        {
            HttpRuntime.Cache.Remove("Login_" + Env.GetUserInfo("userid"));                              
        }
    }
}

