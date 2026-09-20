using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class FormulaController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var ObjUsuario = usr.Get(Env.GetUserInfo("userid"));
                var tak = formu.GetAll().Where(o => o.Entidad_Id == ObjUsuario.Entidad_Usuario_Id);
                var result = from c in tak
                             select new string[] {
                                 c.Id.ToString(),
                                 Convert.ToString(c.Nombre),
                                 Convert.ToString(c.Formula),
                                 c.Ind_Porcentaje ? "Sí" : "No"
                             };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetFormula(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Formulas objFormula = formu.Get(id);
                if (objFormula == null)
                    return Json(new { success = false, message = "Fórmula no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = objFormula.Id,
                        Nombre = objFormula.Nombre,
                        Formula = objFormula.Formula,
                        Ind_Porcentaje = objFormula.Ind_Porcentaje
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener la fórmula: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleFormula(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Formulas objFormula = formu.Get(id);
                if (objFormula == null)
                    return Json(new { success = false, message = "Fórmula no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = objFormula.Id,
                        Nombre = objFormula.Nombre ?? "-",
                        Formula = objFormula.Formula ?? "-",
                        Ind_Porcentaje = objFormula.Ind_Porcentaje ? "Sí" : "No"
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
        public ActionResult GuardarFormula(bool esNuevo, int? id, string nombre, string formula, bool indPorcentaje)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "El nombre de la fórmula es obligatorio." });

                if (string.IsNullOrWhiteSpace(formula))
                    return Json(new { success = false, message = "La expresión de la fórmula es obligatoria." });

                var objUsuario = usr.Get(Env.GetUserInfo("userid"));

                if (esNuevo)
                {
                    Formulas newFormula = new Formulas
                    {
                        Nombre = nombre.Trim(),
                        Formula = formula.Trim(),
                        Ind_Porcentaje = indPorcentaje,
                        Entidad_Id = objUsuario.Entidad_Usuario_Id
                    };
                    formu.Add(ref newFormula);
                    return Json(new { success = true, message = "Fórmula registrada exitosamente." });
                }
                else
                {
                    if (!id.HasValue)
                        return Json(new { success = false, message = "ID de la fórmula no válido." });

                    Formulas editFormula = formu.Get(id.Value.ToString());
                    if (editFormula == null)
                        return Json(new { success = false, message = "La fórmula no existe." });

                    editFormula.Nombre = nombre.Trim();
                    editFormula.Formula = formula.Trim();
                    editFormula.Ind_Porcentaje = indPorcentaje;

                    formu.Update(editFormula);
                    return Json(new { success = true, message = "Fórmula actualizada exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar la fórmula: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarFormula(int id)
        {
            try
            {
                formu.Delete(id.ToString());
                return Json(new { success = true, message = "Fórmula eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la fórmula: " + ex.Message });
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Formulas ObjFormula = formu.Get(id.ToString());
            if (ObjFormula == null)
                return HttpNotFound();

            return View(ObjFormula);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(String Formula, String Nombre, bool Ind_Porcentaje)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Formulas newFormula = new Formulas();

            try
            {
                if (ModelState.IsValid)
                {
                    newFormula.Formula = Formula;
                    newFormula.Nombre = Nombre;
                    newFormula.Ind_Porcentaje = Ind_Porcentaje;
                    var ObjUsuario = usr.Get(Env.GetUserInfo("userid"));
                    newFormula.Entidad_Id = ObjUsuario.Entidad_Usuario_Id;

                    formu.Add(ref newFormula);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                    {
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                            sb.Append(err.ErrorMessage + "<br/>");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Formulas ObjFormula = formu.Get(id.ToString());
            if (ObjFormula == null)
                return HttpNotFound();

            return View(ObjFormula);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(string Id, String Formula, String Nombre, bool Ind_Porcentaje)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    Formulas editFormula = formu.Get(Id);
                    editFormula.Ind_Porcentaje = Ind_Porcentaje;
                    editFormula.Formula = Formula;
                    editFormula.Nombre = Nombre;

                    formu.Update(editFormula);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                    {
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                            sb.Append(err.ErrorMessage + "<br/>");
                    }
                }
            }
            catch (Exception)
            {
                sb.Append("Error al realizar la modificación");
            }
            return Content(sb.ToString());
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Formulas ObjFormula = formu.Get(id.ToString());
            if (ObjFormula == null)
                return HttpNotFound();

            return View(ObjFormula);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                formu.Delete(id.ToString());
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }
            return Content(sb.ToString());
        }

        private readonly FGA_En_Linea.FormulaService.ServiceOf_FormulasClient formu = new FGA_En_Linea.FormulaService.ServiceOf_FormulasClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                formu.Close();
            }
            base.Dispose(disposing);
        }
    }
}