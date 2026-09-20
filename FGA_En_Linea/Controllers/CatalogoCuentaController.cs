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
                int skip = start != null ? Convert.ToInt32(start) / pageSize : 0;
                var tak = cat.GetPage(skip, pageSize, search, ref totalRecords);

                var result = from c in tak
                             select new string[] {
                                 c.Cuenta.ToString(),
                                 Convert.ToString(c.Cuenta),
                                 Convert.ToString(c.Nombre),
                                 Convert.ToString(c.Ind_Proyectar == true ? "Sí" : "No")
                             };

                return Json(new
                {
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetCuenta(string id)
        {
            try
            {
                CatalogoCuenta c = cat.Get(id);
                if (c == null)
                    return Json(new { success = false, message = "Cuenta contable no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        cuenta = c.Cuenta ?? "",
                        nombre = c.Nombre ?? "",
                        indProyectar = c.Ind_Proyectar
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar cuenta: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GuardarCuenta(bool esNuevo, string cuenta, string nombre, bool indProyectar)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cuenta))
                    return Json(new { success = false, message = "El número de cuenta contable es obligatorio." });
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "El nombre de la cuenta es obligatorio." });

                cuenta = cuenta.Trim();
                nombre = nombre.Trim();

                if (esNuevo)
                {
                    CatalogoCuenta repetido = cat.Get(cuenta);
                    if (repetido != null)
                        return Json(new { success = false, message = "Ya existe una cuenta contable con el número '" + cuenta + "'." });

                    CatalogoCuenta nueva = new CatalogoCuenta
                    {
                        Cuenta = cuenta,
                        Nombre = nombre,
                        Ind_Proyectar = indProyectar,
                        Ind_CapitalSocial = false,
                        Ind_CarteraTotal = false,
                        Ind_CuentasLiquidadas = false,
                        Ind_Financiero = false,
                        Ind_OtrosActivos = false,
                        Ind_OtrosPasivos = false,
                        Ind_Recuperacion = false
                    };

                    cat.Add(ref nueva);
                    return Json(new { success = true, message = "Cuenta contable registrada exitosamente." });
                }
                else
                {
                    CatalogoCuenta existing = cat.Get(cuenta);
                    if (existing == null)
                        return Json(new { success = false, message = "La cuenta contable a modificar no existe." });

                    existing.Nombre = nombre;
                    existing.Ind_Proyectar = indProyectar;

                    cat.Update(existing);
                    return Json(new { success = true, message = "Cuenta contable actualizada exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar la cuenta: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarCuenta(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { success = false, message = "Número de cuenta inválido." });

                CatalogoCuenta existing = cat.Get(id);
                if (existing == null)
                    return Json(new { success = false, message = "La cuenta contable que intenta eliminar no existe." });

                cat.Delete(id);
                return Json(new { success = true, message = "Cuenta contable eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la cuenta contable: " + ex.Message });
            }
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
                cat.Close();

            base.Dispose(disposing);
        }
    }
}
