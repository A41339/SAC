using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class CalendarioController : BaseController
    {
        public ActionResult Index(){
           return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = cal.GetAll();
                var result = from c in tak select new string[] { c.Id.ToString(),
                                                                Convert.ToString(c.Periodo.ToShortDateString()),
                                                                Convert.ToString(c.DiaNotificacion),
                                                                Convert.ToString(c.DiaLimite),
                                                                Convert.ToString(c.Enviado == true ? "Sí" : "No") };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {            }

            return null;
        }

        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Calendario ObjCalendario = cal.Get(id);

            if (ObjCalendario == null)
                return HttpNotFound();

            return View(ObjCalendario);
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Calendario ObjCalendario, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    cal.Add(ref ObjCalendario);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                            sb.Append(err.ErrorMessage + "<br/>");
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Calendario ObjCalendario = cal.Get(id);
            if (ObjCalendario == null)
                return HttpNotFound();

            return View(ObjCalendario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Calendario ObjCalendario, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    cal.Update(ObjCalendario);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                            sb.Append(err.ErrorMessage + "<br/>");
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        public ActionResult Delete(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Calendario ObjCalendario = cal.Get(id);

            if (ObjCalendario == null)
                return HttpNotFound();

            return View(ObjCalendario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                cal.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        private readonly FGA_En_Linea.CalendarioService.CalendarioServiceClient cal = new FGA_En_Linea.CalendarioService.CalendarioServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                cal.Close();

            base.Dispose(disposing);
        }
    }
}