using FGA.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FGA.Filters
{
    public class TraceAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Task.Run(() => Log(filterContext));           
            base.OnActionExecuting(filterContext);
        }

        private void Log(ActionExecutingContext filterContext) {
            try
            {
                if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName != "Layout")
                {
                    string jsonPostedData = string.Empty;
                    try
                    {
                        var form = filterContext.HttpContext.Request.Form;
                        var dictionary = form.AllKeys.Where(o => o.ToLower() != "contrasena").ToDictionary(k => k, k => form[k]);
                        jsonPostedData = JsonConvert.SerializeObject(dictionary);
                    }
                    catch
                    { }

                    FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
                    FGA_En_Linea.LogService.ServiceOf_LogClient log = new FGA_En_Linea.LogService.ServiceOf_LogClient();
                    var idUsuario = int.Parse(Env.GetUserInfo("userid"));
                    var traceLog = new Log
                    {
                        Controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                        Action = filterContext.ActionDescriptor.ActionName,
                        Mensaje = jsonPostedData,
                        Fecha = DateTime.Now,
                        IdUsuario_Id = idUsuario
                    };

                    if (traceLog.Mensaje.Length > 2)
                    {
                        log.Add(ref traceLog);
                    }
                }
            }
            catch
            {
            }
        }
    }
}