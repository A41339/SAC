using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace FGA
{
    public static class ModalDialogExtensions
    {
        sealed class DialogActionResult : ActionResult
        {
            public DialogActionResult(string message)
            {
                Message = message ?? string.Empty;
            }

            string Message { get; set; }

            public override void ExecuteResult(ControllerContext context)
            { 
                      
                context.HttpContext.Response.Write("<script>   window.location.assign('" + Message + "')   </script>");
            }
        }                

        public static MvcHtmlString ModalDialogActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string dialogTitle,string CssClass)
        {
            var dialogDivId = Guid.NewGuid().ToString();

            return ajaxHelper.ActionLink(linkText, actionName, routeValues: null,
                    ajaxOptions: new AjaxOptions
                    {
                        UpdateTargetId = dialogDivId,
                        InsertionMode = InsertionMode.Replace,
                        HttpMethod = "GET", 
                        OnBegin = string.Format(CultureInfo.InvariantCulture, "prepareModalDialog('{0}')", dialogDivId),
                        OnFailure = string.Format(CultureInfo.InvariantCulture, "clearModalDialog('{0}');alert('Ajax call failed')", dialogDivId),
                        OnSuccess = string.Format(CultureInfo.InvariantCulture, "openModalDialog('{0}', '{1}')", dialogDivId, dialogTitle)
                    },htmlAttributes: new { @class=CssClass });
            
        }

        public static MvcHtmlString ModalDialogActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName,string ControllerName, string dialogTitle,string CssClass)
        {
            var dialogDivId = Guid.NewGuid().ToString();
            
            return ajaxHelper.ActionLink(linkText, actionName, ControllerName, routeValues: null,
                    ajaxOptions: new AjaxOptions
                    {
                        UpdateTargetId = dialogDivId,
                        InsertionMode = InsertionMode.Replace,
                        HttpMethod = "GET",
                        OnBegin = string.Format(CultureInfo.InvariantCulture, "prepareModalDialog('{0}')", dialogDivId),
                        OnFailure = string.Format(CultureInfo.InvariantCulture, "clearModalDialog('{0}');alert('Ajax call failed')", dialogDivId),
                        OnSuccess = string.Format(CultureInfo.InvariantCulture, "openModalDialog('{0}', '{1}')", dialogDivId, dialogTitle)
                    }, htmlAttributes: new { @class = CssClass });
        }

        public static MvcForm BeginModalDialogForm(this AjaxHelper ajaxHelper)
        {
            return ajaxHelper.BeginForm(new AjaxOptions
            {
                HttpMethod = "POST" 
                //OnSuccess = string.Format(CultureInfo.InvariantCulture, "alert('Added Successfully')")
            });
        }

        public static bool IsThisAjaxRequest(this AjaxHelper ajaxHelper)
        {
            bool result = false;
            var currentContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            if (currentContext.Request.Headers["X-Requested-With"] != null
                && currentContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                result = true;
            }

            return result;
        }
        


        public static ActionResult DialogResult(this Controller controller)
        {
            return DialogResult(controller, string.Empty);
        }

        public static ActionResult DialogResult(this Controller controller, string message)
        { 
            return new DialogActionResult(message);
        }

        
    }
}
