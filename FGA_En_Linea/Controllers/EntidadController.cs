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
            return View();
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
