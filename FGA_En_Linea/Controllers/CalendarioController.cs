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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = cal.GetAll();
                var result = from c in tak
                             select new string[] {
                                 c.Id.ToString(),
                                 c.Periodo.ToString("dd/MM/yyyy"),
                                 Convert.ToString(c.DiaNotificacion),
                                 Convert.ToString(c.DiaLimite),
                                 c.Enviado ? "Sí" : "No"
                             };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetCalendario(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Calendario obj = cal.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Registro de calendario no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Periodo = obj.Periodo.ToString("yyyy-MM-dd"),
                        DiaNotificacion = obj.DiaNotificacion,
                        DiaLimite = obj.DiaLimite,
                        Enviado = obj.Enviado
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener el calendario: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleCalendario(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Calendario obj = cal.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Registro no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Periodo = obj.Periodo.ToString("dd/MM/yyyy"),
                        DiaNotificacion = obj.DiaNotificacion,
                        DiaLimite = obj.DiaLimite,
                        Enviado = obj.Enviado ? "Sí" : "No"
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener el detalle: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarCalendario(bool esNuevo, int? id, DateTime periodo, int diaNotificacion, int diaLimite, bool enviado)
        {
            try
            {
                if (diaNotificacion < 1 || diaNotificacion > 31)
                    return Json(new { success = false, message = "El día de notificación debe estar entre 1 y 31." });

                if (diaLimite < 1 || diaLimite > 31)
                    return Json(new { success = false, message = "El día límite debe estar entre 1 y 31." });

                if (esNuevo)
                {
                    Calendario newCal = new Calendario
                    {
                        Periodo = periodo,
                        DiaNotificacion = diaNotificacion,
                        DiaLimite = diaLimite,
                        Enviado = enviado
                    };
                    cal.Add(ref newCal);
                    return Json(new { success = true, message = "Calendario registrado exitosamente." });
                }
                else
                {
                    if (!id.HasValue)
                        return Json(new { success = false, message = "ID de calendario no válido." });

                    Calendario editCal = cal.Get(id.Value.ToString());
                    if (editCal == null)
                        return Json(new { success = false, message = "El calendario no existe." });

                    editCal.Periodo = periodo;
                    editCal.DiaNotificacion = diaNotificacion;
                    editCal.DiaLimite = diaLimite;
                    editCal.Enviado = enviado;

                    cal.Update(editCal);
                    return Json(new { success = true, message = "Calendario actualizado exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar el calendario: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCalendario(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." });

                cal.Delete(id);
                return Json(new { success = true, message = "Calendario eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el calendario: " + ex.Message });
            }
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

        public ActionResult Create()
        {
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