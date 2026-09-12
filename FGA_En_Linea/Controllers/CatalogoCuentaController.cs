using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class CatalogoCuentaController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                string search = Request.Form.GetValues("search[value]")[0];
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                int totalRecords = 0;
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start)/pageSize : 0;
                var tak = cat.GetPage(skip, pageSize, search, ref totalRecords);
              
                var result = from c in tak
                             select new string[] { c.Cuenta.ToString(),
                                                    Convert.ToString(c.Cuenta),
                                                    Convert.ToString(c.Nombre),
                                                    Convert.ToString(c.Ind_Proyectar == true ?  "Sí" : "No")
                                                    };

                return Json(new {
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = result }, JsonRequestBehavior.AllowGet);
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

            CatalogoCuenta ObjCatalogo = cat.Get(id);

            if (ObjCatalogo == null)
                return HttpNotFound();

            return View(ObjCatalogo);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(CatalogoCuenta ObjCatalogoCuenta, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    ObjCatalogoCuenta.Ind_CapitalSocial = false;
                    ObjCatalogoCuenta.Ind_CarteraTotal = false;
                    ObjCatalogoCuenta.Ind_CuentasLiquidadas = false;
                    ObjCatalogoCuenta.Ind_Financiero = false;
                    ObjCatalogoCuenta.Ind_OtrosActivos = false;
                    ObjCatalogoCuenta.Ind_OtrosPasivos = false;
                    ObjCatalogoCuenta.Ind_Recuperacion = false;

                    cat.Add(ref ObjCatalogoCuenta);
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

            CatalogoCuenta ObjCatalogo = cat.Get(id);
            if (ObjCatalogo == null)
                return HttpNotFound();

            return View(ObjCatalogo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(CatalogoCuenta ObjCatalogoCuenta)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                CatalogoCuenta cuenta = cat.Get(ObjCatalogoCuenta.Cuenta);
                cuenta.Nombre = ObjCatalogoCuenta.Nombre;
                cuenta.Ind_Proyectar = ObjCatalogoCuenta.Ind_Proyectar;

                if (ModelState.IsValid)
                {
                    cat.Update(cuenta);
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

            CatalogoCuenta ObjCatalogo = cat.Get(id);

            if (ObjCatalogo == null)
                return HttpNotFound();

            return View(ObjCatalogo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                cat.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        private readonly FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient cat = new FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cat.Close();
            }
            base.Dispose(disposing);
        }
    }
}