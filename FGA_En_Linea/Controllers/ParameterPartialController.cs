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

        public ActionResult Config(bool yAxis, bool columnDetail, bool lineDetail, bool dateDetail, bool menuDetail)
        {
            Session[FGA.Utility.Utilitarios.isModified] = yAxis;
            Session[FGA.Utility.Utilitarios.show_YAxis] = yAxis == true ? yAxis : (bool?)null;
            Session[FGA.Utility.Utilitarios.show_ColumnDetail] = columnDetail == true ? columnDetail : (bool?)null;
            Session[FGA.Utility.Utilitarios.show_LineDetail] = lineDetail == true ? lineDetail : (bool?)null;
            Session[FGA.Utility.Utilitarios.show_DateEachN] = dateDetail == true ? dateDetail : (bool?)null;
            Session[FGA.Utility.Utilitarios.show_MenuOculto] = menuDetail == true ? menuDetail : (bool?)null;
            return Json(true);
        }
    }
}