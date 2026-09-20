using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class EntidadController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                ViewBag.Perfil_Entidad_Id = new SelectList(per.GetAll(), "Id", "Nombre").OrderBy(o => o.Text).ToList();
            }
            catch (Exception) { }
            return View();
        }

        [HttpGet]
        public ActionResult GetEntidad(string id)
        {
            try
            {
                Entidad e = ent.Get(id);
                if (e == null)
                    return Json(new { success = false, message = "Entidad no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = e.Id,
                        identificacion = e.Identificacion ?? "",
                        nombre = e.Nombre ?? "",
                        contacto = e.Contacto ?? "",
                        correoInforme = e.CorreoInforme ?? "",
                        perfilId = e.Perfil_Entidad_Id,
                        activo = e.Activo,
                        indCargar = e.Ind_Cargar,
                        indValidar = e.Ind_Validar,
                        indEvaluacion = e.Ind_Evaluacion,
                        logoUrl = "https://www.ffc.co.cr/img/secciones/afiliados/" + (e.Nombre ?? "").Trim() + ".jpg"
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar entidad: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleEntidad(string id)
        {
            try
            {
                Entidad e = ent.Get(id);
                if (e == null)
                    return Json(new { success = false, message = "Entidad no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = e.Id,
                        identificacion = e.Identificacion ?? "-",
                        nombre = e.Nombre ?? "-",
                        contacto = string.IsNullOrWhiteSpace(e.Contacto) ? "-" : e.Contacto,
                        correoInforme = string.IsNullOrWhiteSpace(e.CorreoInforme) ? "-" : e.CorreoInforme,
                        perfil = (e.Perfil_Entidad != null ? e.Perfil_Entidad.Nombre : "-"),
                        estado = e.Activo ? "Activo" : "Inactivo",
                        indCargar = (e.Ind_Cargar == true ? "Sí" : "No"),
                        indValidar = (e.Ind_Validar == true ? "Sí" : "No"),
                        indEvaluacion = (e.Ind_Evaluacion == true ? "Sí" : "No"),
                        logoUrl = "https://www.ffc.co.cr/img/secciones/afiliados/" + (e.Nombre ?? "").Trim() + ".jpg"
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar detalle: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GuardarEntidad(bool esNuevo, string id, string identificacion, string nombre, string contacto, string correoInforme, long? perfilId, bool activo, bool indCargar, bool indValidar, bool indEvaluacion, HttpPostedFileBase file)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { success = false, message = "El código de la entidad es obligatorio." });
                if (string.IsNullOrWhiteSpace(identificacion))
                    return Json(new { success = false, message = "La identificación es obligatoria." });
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "El nombre de la entidad es obligatorio." });

                id = id.Trim();
                identificacion = identificacion.Trim();
                nombre = nombre.Trim().ToUpper();

                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentType.Equals("image/jpeg") || file.ContentType.Equals("image/png") || file.ContentType.Equals("image/jpg"))
                    {
                        try
                        {
                            var rutaBase = System.Configuration.ConfigurationManager.AppSettings["RutaImg"];
                            if (!string.IsNullOrEmpty(rutaBase))
                            {
                                var path = rutaBase + "afiliados\\" + nombre + ".jpg";
                                file.SaveAs(path);
                            }
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        return Json(new { success = false, message = "La imagen del logo debe estar en formato .jpg o .png." });
                    }
                }

                if (esNuevo)
                {
                    Entidad repetido = ent.GetByIden(identificacion);
                    if (repetido != null)
                        return Json(new { success = false, message = "Ya existe una entidad con la identificación '" + identificacion + "'." });

                    Entidad repetidoId = ent.Get(id);
                    if (repetidoId != null)
                        return Json(new { success = false, message = "Ya existe una entidad con el código '" + id + "'." });

                    Entidad nueva = new Entidad
                    {
                        Id = id,
                        Identificacion = identificacion,
                        Nombre = nombre,
                        Contacto = contacto ?? "",
                        CorreoInforme = correoInforme ?? "",
                        Perfil_Entidad_Id = perfilId.GetValueOrDefault(),
                        Activo = activo,
                        Ind_Cargar = indCargar,
                        Ind_Validar = indValidar,
                        Ind_Evaluacion = indEvaluacion,
                        Logo = nombre + ".jpg"
                    };

                    ent.Add(ref nueva);
                    return Json(new { success = true, message = "Entidad creada exitosamente." });
                }
                else
                {
                    Entidad existing = ent.Get(id);
                    if (existing == null)
                        return Json(new { success = false, message = "La entidad que intenta modificar no existe." });

                    existing.Identificacion = identificacion;
                    existing.Nombre = nombre;
                    existing.Contacto = contacto ?? "";
                    existing.CorreoInforme = correoInforme ?? "";
                    existing.Perfil_Entidad_Id = perfilId.GetValueOrDefault();
                    existing.Activo = activo;
                    existing.Ind_Cargar = indCargar;
                    existing.Ind_Validar = indValidar;
                    existing.Ind_Evaluacion = indEvaluacion;
                    if (file != null && file.ContentLength > 0)
                        existing.Logo = nombre + ".jpg";

                    ent.Update(existing);
                    return Json(new { success = true, message = "Entidad actualizada exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar la entidad: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarEntidad(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { success = false, message = "Código de entidad inválido." });

                Entidad existing = ent.Get(id);
                if (existing == null)
                    return Json(new { success = false, message = "La entidad que intenta eliminar no existe." });

                ent.Delete(id);
                return Json(new { success = true, message = "Entidad eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar entidad: " + ex.Message });
            }
        }
        public ActionResult GetGrid()
        {
            try
            {
                var tak = ent.GetAll().Where(c => c != null && c.Id != "-1" && (c.Nombre == null || !c.Nombre.ToUpper().Contains("TODAS LAS COOPERATIVAS"))).ToArray();
                var result = from c in tak
                             select new string[] { c.Id.ToString(),
                                                    Convert.ToString(c.Id),
                                                    Convert.ToString(c.Identificacion),
                                                    Convert.ToString(c.Nombre),
                                                    Convert.ToString(c.Activo == true ? "Activo" : "Inactivo"),
                                                    Convert.ToString(c.Ind_Cargar == true ? "S\u00ed" : "No"),
                                                    Convert.ToString(c.Ind_Validar == true ? "S\u00ed" : "No"),
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

            Entidad ObjCompany = ent.Get(id);

            if (ObjCompany == null)
                return HttpNotFound();

            return View(ObjCompany);
        }

        public ActionResult Create()
        {
            ViewBag.Perfil_Entidad_Id = new SelectList(per.GetAll(), "Id", "Nombre").OrderBy(o => o.Text);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Entidad ObjEntidad, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    if (!(file is null))
                    {
                        if (file.ContentType.Equals("image/jpeg") || file.ContentType.Equals("image/png"))
                        {
                            var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "afiliados\\" + ObjEntidad.Nombre.Trim() + ".jpg";
                            file.SaveAs(path);
                        }
                        else
                            sb.Append("Error: La imagen debe estar en formato .jpg" + "<br/>");
                    }
                    else
                    {
                        if (ObjEntidad.Activo)
                        {
                            sb.Append("Error: No se puede agregar una entidad activa sin Logo" + "<br/>");
                            return Content(sb.ToString());
                        }
                    }

                    ObjEntidad.Nombre = ObjEntidad.Nombre.ToUpper();
                    Entidad repetido = ent.GetByIden(ObjEntidad.Identificacion);
                    if (repetido != null)
                    {
                        sb.Append("Error: La entidad ya existe.");
                        return Content(sb.ToString());
                    }

                    ent.Add(ref ObjEntidad);
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
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Entidad ObjCompany = ent.Get(id);
            ViewBag.Perfil_Entidad_Id = new SelectList(per.GetAll(), "Id", "Nombre", ObjCompany.Perfil_Entidad_Id).OrderBy(o => o.Text);

            if (ObjCompany == null)
                return HttpNotFound();

            return View(ObjCompany);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Entidad ObjEntidad, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {

                    if (!(file is null))
                    {
                        if (file.ContentType.Equals("image/jpeg") || file.ContentType.Equals("image/png"))
                        {
                            var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "afiliados\\" + ObjEntidad.Nombre.Trim() + ".jpg";
                            file.SaveAs(path);
                        }
                        else
                            sb.Append("La imagen debe estar en formato .jpg" + "<br/>");
                    }

                    ObjEntidad.Nombre = ObjEntidad.Nombre.ToUpper();

                    if (string.IsNullOrEmpty(ObjEntidad.Logo))
                        ObjEntidad.Logo = string.Empty;
                    if (string.IsNullOrEmpty(ObjEntidad.Contacto))
                        ObjEntidad.Contacto = string.Empty;

                    ent.Update(ObjEntidad);
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

            Entidad ObjCompany = ent.Get(id);

            if (ObjCompany == null)
                return HttpNotFound();

            return View(ObjCompany);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                ent.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.PerfilEntidadService.ServiceOf_PerfilEntidadClient per = new FGA_En_Linea.PerfilEntidadService.ServiceOf_PerfilEntidadClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ent.Close();
   
            base.Dispose(disposing);
        }
    }
}
