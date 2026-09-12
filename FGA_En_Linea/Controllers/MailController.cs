using System.Web.Mvc;

namespace FGA.Controllers
{

    public class MailController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Compose()
        {
            return View();
        }

        public ActionResult Read()
        {
            return View();
        }
    }
}