using System;
using System.Web.Mvc;
using FGA.Utility;

namespace FGA.Controllers
{
    public class SeguridadController : BaseController
    {
        public SeguridadController()
        {
        }

        public PartialViewResult GetPartial()
        {
            DateTime fecha = GetSessionDate("Fecha");
            var files = sp.FGA_Consultar_Sessiones(fecha);
            return PartialView("DetailSession", files);
        }

        public ActionResult Index()
        {
            SetSessionDate("Fecha", DateTime.Now);
            var files = sp.FGA_Consultar_Sessiones(DateTime.Now);
            return View(files);
        }

        public ActionResult Buscar(DateTime Fecha)
        {
            SetSessionDate("Fecha", Fecha);
            var files = sp.FGA_Consultar_Sessiones(Fecha);
            return View("Index", files);
        }

        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sp.SafeClose();
            }

            base.Dispose(disposing);
        }
    }
}