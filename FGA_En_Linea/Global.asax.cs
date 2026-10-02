using FGA.Models;
using FGA.Utility;
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
            MvcHandler.DisableMvcResponseHeader = true;
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            FGA_En_Linea.LogService.ServiceOf_LogClient log = null;
            try
            {
                Exception ex = Server.GetLastError();
                if (ex == null) return;

                string path = "N/A";
                if (sender is HttpApplication app && app.Request != null && app.Request.Url != null)
                    path = app.Request.Url.PathAndQuery;

                int idUsuario = 0;
                int.TryParse(Env.GetUserInfo("userid"), out idUsuario);

                log = new FGA_En_Linea.LogService.ServiceOf_LogClient();
                var traceLog = new Log
                {
                    Controller = path,
                    Action = string.Empty,
                    Mensaje = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    Fecha = DateTime.Now,
                    IdUsuario_Id = idUsuario
                };

                if (!string.IsNullOrEmpty(traceLog.Mensaje) && traceLog.Mensaje.Length > 2)
                    log.Add(ref traceLog);
            }
            catch (Exception) { }
            finally
            {
                log.SafeClose();
            }
        }

        void Session_End(object sender, EventArgs e)
        {
            HttpRuntime.Cache.Remove("Login_" + Env.GetUserInfo("userid"));                              
        }
    }
}

