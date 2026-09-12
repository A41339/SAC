using System.Web.Mvc;

namespace FGA.Controllers
{
    public class LayoutController : Controller
    {  
        [OutputCache(Duration = 2592000, VaryByParam = "none")]
        public ActionResult HeaderCssJs()
        {
            return PartialView();
        }

        [OutputCache(Duration = 2592000, VaryByParam = "none")]
        public ActionResult FooterCssJs()
        {
            return PartialView();
        }

        [OutputCache(Duration = 2592000, VaryByParam = "none")]
        public ActionResult DataTableCssJs()
        {
            return PartialView();
        }
    }
}
