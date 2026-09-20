using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class NoticiaController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = not.GetAll();
                var result = from c in tak
                             select new string[] {
                                 Convert.ToString(c.Id),
                                 c.Fecha.ToString("dd/MM/yyyy"),
                                 Convert.ToString(c.Detalle),
                                 Convert.ToString(c.Link)
                             };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetNoticia(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Noticia obj = not.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Noticia no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Fecha = obj.Fecha.ToString("yyyy-MM-dd"),
                        Detalle = obj.Detalle,
                        Link = obj.Link,
                        Ind_Mostrar_Fecha = obj.Ind_Mostrar_Fecha
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener la noticia: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleNoticia(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Noticia obj = not.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Noticia no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Fecha = obj.Fecha.ToString("dd/MM/yyyy"),
                        Detalle = obj.Detalle ?? "-",
                        Link = obj.Link ?? "",
                        Ind_Mostrar_Fecha = obj.Ind_Mostrar_Fecha ? "Sí" : "No"
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
        public ActionResult GuardarNoticia(bool esNuevo, int? id, DateTime fecha, string detalle, string link, bool indMostrarFecha)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(detalle))
                    return Json(new { success = false, message = "El detalle de la noticia es obligatorio." });

                if (esNuevo)
                {
                    Noticia newObj = new Noticia
                    {
                        Fecha = fecha,
                        Detalle = detalle.Trim(),
                        Link = (link ?? "").Trim(),
                        Ind_Mostrar_Fecha = indMostrarFecha
                    };
                    not.Add(ref newObj);
                    return Json(new { success = true, message = "Noticia registrada exitosamente." });
                }
                else
                {
                    if (!id.HasValue)
                        return Json(new { success = false, message = "ID de noticia no válido." });

                    Noticia editObj = not.Get(id.Value.ToString());
                    if (editObj == null)
                        return Json(new { success = false, message = "La noticia no existe." });

                    editObj.Fecha = fecha;
                    editObj.Detalle = detalle.Trim();
                    editObj.Link = (link ?? "").Trim();
                    editObj.Ind_Mostrar_Fecha = indMostrarFecha;

                    not.Update(editObj);
                    return Json(new { success = true, message = "Noticia actualizada exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar la noticia: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarNoticia(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." });

                not.Delete(id);
                return Json(new { success = true, message = "Noticia eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la noticia: " + ex.Message });
            }
        }

        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Noticia ObjNoticia = not.Get(id);
            if (ObjNoticia == null)
            {
                return HttpNotFound();
            }
            return View(ObjNoticia);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Noticia ObjNoticia)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    not.Add(ref ObjNoticia);
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
            Noticia ObjNoticia = not.Get(id);
            if (ObjNoticia == null)
            {
                return HttpNotFound();
            }

            return View(ObjNoticia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Noticia ObjNoticia)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    not.Update(ObjNoticia);
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

            Noticia ObjNoticia = not.Get(id);
            if (ObjNoticia == null)
            {
                return HttpNotFound();
            }
            return View(ObjNoticia);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                not.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        private readonly FGA_En_Linea.NoticiaService.ServiceOf_NoticiaClient not = new FGA_En_Linea.NoticiaService.ServiceOf_NoticiaClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                not.Close();
            }
            base.Dispose(disposing);
        }
    }
}