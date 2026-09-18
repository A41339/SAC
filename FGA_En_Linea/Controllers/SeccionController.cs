using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class SeccionController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = sec.GetAll();
                var result = from c in tak
                             select new string[] { Convert.ToString(c.Id),
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
            Seccion ObjSeccion = sec.Get(id);
            if (ObjSeccion == null)
            {
                return HttpNotFound();
            }
            return View(ObjSeccion);
        }

        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Seccion ObjSeccion)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    sec.Add(ref ObjSeccion);
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
            Seccion ObjSeccion = sec.Get(id);
            if (ObjSeccion == null)
            {
                return HttpNotFound();
            }

            return View(ObjSeccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Seccion ObjSeccion, HttpPostedFileBase file)
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
                            var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"];

                            if (ObjSeccion.Id == (int)Utility.Utilitarios.enum_secciones.afiliados)
                                path = path + "afiliados\\img.jpg";
                            else if (ObjSeccion.Id == (int)Utility.Utilitarios.enum_secciones.quienes_somos)
                                path = path + "quienes_somos\\img.jpg";
                            else if (ObjSeccion.Id == (int)Utility.Utilitarios.enum_secciones.conozca)
                                path = path + "contactenos\\img.jpg";
                            else if (ObjSeccion.Id == (int)Utility.Utilitarios.enum_secciones.parametros)
                                path = path + "logo\\Logo.jpg";
                            else if (ObjSeccion.Id == (int)Utility.Utilitarios.enum_secciones.unase)
                                path = path + "unase\\img.jpg";
                            else if (ObjSeccion.Id == (int)Utility.Utilitarios.enum_secciones.popUp)
                                path = path + "popup\\img.jpg";
                         
                            file.SaveAs(path);
                        }
                        else
                        {
                            sb.Append("Error: La imagen debe estar en formato .jpg o .png" + "<br/>");
                            return Content(sb.ToString());
                        }
                    }

                    sec.Update(ObjSeccion);
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

        private readonly FGA_En_Linea.SeccionService.ServiceOf_SeccionClient sec = new FGA_En_Linea.SeccionService.ServiceOf_SeccionClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sec.Close();
            }
            base.Dispose(disposing);
        }
    }
}