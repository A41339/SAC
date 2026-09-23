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
                             select new string[] { c.Id.ToString(),
                                                    Convert.ToString(c.Nombre),
                                                    c.Estado ? "Activo" : "Inactivo",
                                                    };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
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

        // ── Acciones JSON para modal CRUD ──────────────────────────────

        public ActionResult GetTipoInforme(int id)
        {
            try
            {
                var obj = ti.Get(id.ToString());
                if (obj == null)
                    return Json(new { success = false, message = "Tipo de informe no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    tipoInforme = new
                    {
                        id = obj.Id,
                        nombre = obj.Nombre,
                        texto = obj.Texto,
                        estado = obj.Estado
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GuardarTipoInforme(int? id, string nombre, string texto, bool estado)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "El nombre es requerido." });

                if (id.HasValue && id.Value > 0)
                {
                    // Editar
                    var obj = ti.Get(id.Value.ToString());
                    if (obj == null)
                        return Json(new { success = false, message = "Tipo de informe no encontrado." });

                    obj.Nombre = nombre.ToUpper();
                    obj.Texto = texto;
                    obj.Estado = estado;
                    ti.Update(obj);
                    return Json(new { success = true, message = "Tipo de informe modificado correctamente." });
                }
                else
                {
                    // Crear
                    var obj = new TipoInforme();
                    obj.Nombre = nombre.ToUpper();
                    obj.Texto = texto;
                    obj.Estado = true;
                    ti.Add(ref obj);
                    return Json(new { success = true, message = "Tipo de informe creado correctamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarTipoInforme(int id)
        {
            try
            {
                ti.Delete(id.ToString());
                return Json(new { success = true, message = "Tipo de informe eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
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