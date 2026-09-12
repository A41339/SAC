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
                             select new string[] { Convert.ToString(c.Id),
                                                    Convert.ToString(c.Fecha.ToShortDateString()),
                                                    Convert.ToString(c.Detalle),
                                                    Convert.ToString(c.Link),
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