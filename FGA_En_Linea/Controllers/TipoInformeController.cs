using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class TipoInformeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = ti.GetAll();
                var result = from c in tak
                             select new string[] {
                                 c.Id.ToString(),
                                 Convert.ToString(c.Nombre),
                                 c.Estado ? "Activo" : "Inactivo"
                             };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetTipoInforme(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                TipoInforme obj = ti.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Tipo de informe no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Nombre = obj.Nombre,
                        Texto = obj.Texto ?? "",
                        Estado = obj.Estado
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener tipo de informe: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleTipoInforme(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                TipoInforme obj = ti.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Tipo de informe no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Nombre = obj.Nombre ?? "-",
                        Texto = obj.Texto ?? "",
                        Estado = obj.Estado ? "Activo" : "Inactivo"
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener detalle: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult GuardarTipoInforme(bool esNuevo, int? id, string nombre, string texto, bool estado)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "El nombre del tipo de informe es obligatorio." });

                if (esNuevo)
                {
                    TipoInforme newObj = new TipoInforme
                    {
                        Nombre = nombre.Trim().ToUpper(),
                        Texto = texto,
                        Estado = true
                    };
                    ti.Add(ref newObj);
                    return Json(new { success = true, message = "Tipo de informe registrado exitosamente." });
                }
                else
                {
                    if (!id.HasValue)
                        return Json(new { success = false, message = "ID de tipo de informe no válido." });

                    TipoInforme editObj = ti.Get(id.Value.ToString());
                    if (editObj == null)
                        return Json(new { success = false, message = "El tipo de informe no existe." });

                    editObj.Nombre = nombre.Trim().ToUpper();
                    editObj.Texto = texto;
                    editObj.Estado = estado;

                    ti.Update(editObj);
                    return Json(new { success = true, message = "Tipo de informe actualizado exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar el tipo de informe: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarTipoInforme(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." });

                ti.Delete(id);
                return Json(new { success = true, message = "Tipo de informe eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el tipo de informe: " + ex.Message });
            }
        }

        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TipoInforme ObjInforme = ti.Get(id);
            if (ObjInforme == null)
                return HttpNotFound();

            return View(ObjInforme);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(TipoInforme ObjInforme)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    ObjInforme.Nombre = ObjInforme.Nombre.ToUpper();
                    ObjInforme.Estado = true;
                    ti.Add(ref ObjInforme);
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

            TipoInforme ObjInforme = ti.Get(id);
            if (ObjInforme == null)
                return HttpNotFound();

            return View(ObjInforme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(TipoInforme ObjInforme)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    ObjInforme.Nombre = ObjInforme.Nombre.ToUpper();
                    ti.Update(ObjInforme);
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

            TipoInforme ObjInforme = ti.Get(id);
            if (ObjInforme == null)
                return HttpNotFound();

            return View(ObjInforme);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                ti.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        private readonly FGA_En_Linea.TipoInformeService.ServiceOf_TipoInformeClient ti = new FGA_En_Linea.TipoInformeService.ServiceOf_TipoInformeClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ti.Close();

            base.Dispose(disposing);
        }
    }
}