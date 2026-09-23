using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class TipoXMLController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = db.GetAll();
                var result = from c in tak
                             select new string[] { c.Id.ToString(),
            Convert.ToString(c.Id),
            Convert.ToString(c.Nombre)
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
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoXML ObjTipoXML = db.Get(id);
            if (ObjTipoXML == null)
            {
                return HttpNotFound();
            }
            return View(ObjTipoXML);
        }

        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(TipoXML ObjTipoXML)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Add(ref ObjTipoXML);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                    {
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                        {
                            sb.Append(err.ErrorMessage + "<br/>");
                        }
                    }
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
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TipoXML ObjTipoXML = db.Get(id);

            if (ObjTipoXML == null)
            {
                return HttpNotFound();
            }

            EditTipoXML view = new EditTipoXML();
            view.tipoXML = ObjTipoXML;

            List<XML_Excepcion> desligados = exc.GetByFile(ObjTipoXML.Id).ToList();
            List<Entidad> listEntidad = ent.GetAll().Where(o => o.Activo && o.Id != FGA.Utility.Utilitarios.entidadAdministradora).ToList();
            view.ligada = listEntidad.Where(p => !desligados.Any(p2 => p2.IdEntidad_Id == p.Id)).ToList();
            view.desLigada = listEntidad.Where(p => desligados.Any(p2 => p2.IdEntidad_Id == p.Id)).ToList();

            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection frm)
        {
            TipoXML ObjTipoXML = new TipoXML();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Update(ObjTipoXML);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                    {
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                        {
                            sb.Append(err.ErrorMessage + "<br/>");
                        }
                    }
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
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TipoXML ObjTipoXML = db.Get(id);

            if (ObjTipoXML == null)
            {
                return HttpNotFound();
            }
            return View(ObjTipoXML);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                db.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());

            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }


        public ActionResult Excepcion(string idEntidad, string idArchivo, string idAction)
        {
            XML_Excepcion entity = new XML_Excepcion();
            entity.IdArchivo_Id = idArchivo;
            entity.IdEntidad_Id = idEntidad;

            try
            {
                if (idAction.Contains("#ligado"))
                    exc.Add(ref entity);
                else
                    exc.Delete(entity);
            }
            catch (Exception) { }

        // ── Acciones JSON para modal CRUD de TipoXML ───────────────────────────

        public ActionResult GetTipoXML(string id)
        {
            try
            {
                var obj = db.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Tipo de XML no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    tipoXML = new
                    {
                        id = obj.Id,
                        nombre = obj.Nombre
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GuardarTipoXML(string id, string nombre, bool esNuevo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { success = false, message = "El código/ID es requerido." });

                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "El nombre es requerido." });

                if (esNuevo)
                {
                    TipoXML obj = new TipoXML();
                    obj.Id = id;
                    obj.Nombre = nombre;
                    db.Add(ref obj);
                    return Json(new { success = true, message = "Tipo de XML creado correctamente." });
                }
                else
                {
                    var obj = db.Get(id);
                    if (obj == null)
                        return Json(new { success = false, message = "Tipo de XML no encontrado." });

                    obj.Nombre = nombre;
                    db.Update(obj);
                    return Json(new { success = true, message = "Tipo de XML modificado correctamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar el tipo de XML: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarTipoXML(string id)
        {
            try
            {
                db.Delete(id);
                return Json(new { success = true, message = "Tipo de XML eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el tipo de XML: " + ex.Message });
            }
        }

        public ActionResult GetExcepciones(string id)
        {
            try
            {
                var ObjTipoXML = db.Get(id);
                if (ObjTipoXML == null)
                    return Json(new { success = false, message = "Tipo de XML no encontrado." }, JsonRequestBehavior.AllowGet);

                List<XML_Excepcion> desligados = exc.GetByFile(ObjTipoXML.Id).ToList();
                List<Entidad> listEntidad = ent.GetAll().Where(o => o.Activo && o.Id != FGA.Utility.Utilitarios.entidadAdministradora).ToList();

                var ligadas = listEntidad.Where(p => !desligados.Any(p2 => p2.IdEntidad_Id == p.Id))
                    .Select(e => new { id = e.Id, nombre = e.Nombre }).ToList();

                var desLigadas = listEntidad.Where(p => desligados.Any(p2 => p2.IdEntidad_Id == p.Id))
                    .Select(e => new { id = e.Id, nombre = e.Nombre }).ToList();

                return Json(new
                {
                    success = true,
                    tipoXML = new { id = ObjTipoXML.Id, nombre = ObjTipoXML.Nombre },
                    ligadas = ligadas,
                    desLigadas = desLigadas
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar excepciones: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private readonly FGA_En_Linea.TipoXMLService.ServiceOf_TipoXMLClient db = new FGA_En_Linea.TipoXMLService.ServiceOf_TipoXMLClient();
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.XML_ExcepcionService.XML_ExcepcionServiceClient exc = new FGA_En_Linea.XML_ExcepcionService.XML_ExcepcionServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Close();
                ent.Close();
                exc.Close();
            }
            base.Dispose(disposing);
        }
    }
}