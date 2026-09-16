using System.Web.Mvc;

namespace Site.Controllers
{
    public class ParameterPartialController : Controller
    {

        [ChildActionOnly]
        public ActionResult Index()
        {
            Session[FGA.Utility.Utilitarios.show_YAxis] = Session[FGA.Utility.Utilitarios.isModified] is null ? null : Session[FGA.Utility.Utilitarios.show_YAxis];
            Session[FGA.Utility.Utilitarios.show_ColumnDetail] = Session[FGA.Utility.Utilitarios.isModified] is null ? true : Session[FGA.Utility.Utilitarios.show_ColumnDetail];
            Session[FGA.Utility.Utilitarios.show_LineDetail] = Session[FGA.Utility.Utilitarios.isModified] is null ? true : Session[FGA.Utility.Utilitarios.show_LineDetail];
            Session[FGA.Utility.Utilitarios.show_DateEachN] = Session[FGA.Utility.Utilitarios.isModified] is null ? null : Session[FGA.Utility.Utilitarios.show_DateEachN];
            Session[FGA.Utility.Utilitarios.show_MenuOculto] = Session[FGA.Utility.Utilitarios.isModified] is null ? null : Session[FGA.Utility.Utilitarios.show_MenuOculto];
            return PartialView("_ParameterPartial");
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Config(bool yAxis = false, bool columnDetail = true, bool lineDetail = true, bool dateDetail = false, bool menuDetail = false)
        {
            try
            {
                Session[FGA.Utility.Utilitarios.isModified] = true;
                Session[FGA.Utility.Utilitarios.show_YAxis] = yAxis ? (bool?)true : null;
                Session[FGA.Utility.Utilitarios.show_ColumnDetail] = columnDetail ? (bool?)true : null;
                Session[FGA.Utility.Utilitarios.show_LineDetail] = lineDetail ? (bool?)true : null;
                Session[FGA.Utility.Utilitarios.show_DateEachN] = dateDetail ? (bool?)true : null;
                Session[FGA.Utility.Utilitarios.show_MenuOculto] = menuDetail ? (bool?)true : null;
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}