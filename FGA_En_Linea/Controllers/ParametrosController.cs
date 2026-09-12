using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class ParametrosController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = param.GetAll();
                var result = from c in tak
                             select new string[] { c.Id.ToString(),
                                                    Convert.ToString(c.Descripcion),
                                                    Convert.ToString(c.Llave),
                                                    Convert.ToString(c.Valor)
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

            Parametros ObjParametro = param.Get(id);

            if (ObjParametro == null)
                return HttpNotFound();

            return View(ObjParametro);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Parametros ObjParametros, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    param.Add(ref ObjParametros);
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

            Parametros ObjParametro = param.Get(id);
            if (ObjParametro == null)
                return HttpNotFound();

            return View(ObjParametro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Parametros ObjParametros)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {  
                if (ModelState.IsValid)
                {
                    param.Update(ObjParametros);
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

            Parametros ObjParametro = param.Get(id);

            if (ObjParametro == null)
                return HttpNotFound();

            return View(ObjParametro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                param.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        private readonly FGA_En_Linea.ParametrosService.ParametrosServiceClient param = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
            
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                param.Close();
            }
            base.Dispose(disposing);
        }
    }
}