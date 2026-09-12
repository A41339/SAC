using System;
using System.Web.Mvc;

namespace FGA.Controllers
{
    public class SeguridadController : BaseController
    {
        public SeguridadController()
        {
        }

        public PartialViewResult GetPartial()
        {
            var files = sp.FGA_Consultar_Sessiones(Utility.Utilitarios.ConvertirAFecha(Session["Fecha"].ToString()));
            return PartialView("DetailSession", files);
        }

        public ActionResult Index()
        {
            Session["Fecha"] = DateTime.Now.ToShortDateString();
            var files = sp.FGA_Consultar_Sessiones(DateTime.Now);
            return View(files);
        }

        public ActionResult Buscar(DateTime Fecha)
        {
            Session["Fecha"] = Fecha.ToShortDateString();
            var files = sp.FGA_Consultar_Sessiones(Fecha);
            return View("Index", files);
        }

        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                sp.Close();

            base.Dispose(disposing);
        }
    }
}