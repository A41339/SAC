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
                             select new string[] {
                                 c.Id.ToString(),
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

        [HttpGet]
        public ActionResult GetParametro(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Parametros objParametro = param.Get(id);
                if (objParametro == null)
                    return Json(new { success = false, message = "Parámetro no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = objParametro.Id,
                        Llave = objParametro.Llave,
                        Valor = objParametro.Valor,
                        Descripcion = objParametro.Descripcion
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener el parámetro: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleParametro(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Parametros objParametro = param.Get(id);
                if (objParametro == null)
                    return Json(new { success = false, message = "Parámetro no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = objParametro.Id,
                        Llave = objParametro.Llave ?? "-",
                        Valor = objParametro.Valor ?? "-",
                        Descripcion = objParametro.Descripcion ?? "-"
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener el detalle: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult GuardarParametro(bool esNuevo, int? id, string llave, string valor, string descripcion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(llave))
                    return Json(new { success = false, message = "La clave (llave) del parámetro es obligatoria." });

                if (string.IsNullOrWhiteSpace(valor))
                    return Json(new { success = false, message = "El valor del parámetro es obligatorio." });

                if (esNuevo)
                {
                    Parametros newParam = new Parametros
                    {
                        Llave = llave.Trim(),
                        Valor = valor.Trim(),
                        Descripcion = (descripcion ?? "").Trim()
                    };
                    param.Add(ref newParam);
                    return Json(new { success = true, message = "Parámetro creado exitosamente." });
                }
                else
                {
                    if (!id.HasValue)
                        return Json(new { success = false, message = "ID de parámetro no válido." });

                    Parametros editParam = param.Get(id.Value.ToString());
                    if (editParam == null)
                        return Json(new { success = false, message = "El parámetro no existe." });

                    editParam.Llave = llave.Trim();
                    editParam.Valor = valor.Trim();
                    editParam.Descripcion = (descripcion ?? "").Trim();

                    param.Update(editParam);
                    return Json(new { success = true, message = "Parámetro actualizado exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar el parámetro: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarParametro(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." });

                param.Delete(id);
                return Json(new { success = true, message = "Parámetro eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el parámetro: " + ex.Message });
            }
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