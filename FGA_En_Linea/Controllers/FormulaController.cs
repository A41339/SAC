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
                             select new string[] { c.Id.ToString(), Convert.ToString(c.Nombre),  Convert.ToString(c.Formula),
                                                   c.Ind_Porcentaje == true ? "Sí" : "No"};
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Formulas ObjFormula = formu.Get(id.ToString());
            ObjFormula.Formula = ObjFormula.Formula;

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
            ObjFormula.Formula = ObjFormula.Formula;

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
            ObjFormula.Formula = ObjFormula.Formula;

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