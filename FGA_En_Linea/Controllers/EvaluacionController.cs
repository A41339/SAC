using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Entities.Entities.Evaluacion;

namespace FGA.Controllers
{
    public class EvaluacionController : BaseController
    {
        private FGA_En_Linea.EvalCategoriaService.EvalCategoriaServiceClient categoriaService;
        private FGA_En_Linea.EvalSubCategoriaService.EvalSubCategoriaServiceClient subCategoriaService;
        private FGA_En_Linea.EvalPreguntaService.EvalPreguntaServiceClient preguntaService;
        private FGA_En_Linea.SPService.SPClient spService;
        private FGA_En_Linea.UsuarioService.UsuarioServiceClient usuarioService;
        private FGA_En_Linea.EvalResponsableCategoriaService.EvalResponsableCategoriaServiceClient responsableService;
        private FGA_En_Linea.EvalRespuestaUsuarioService.ServiceOf_EvalRespuestaUsuarioClient respuestaService;

        public EvaluacionController()
        {
            categoriaService = new FGA_En_Linea.EvalCategoriaService.EvalCategoriaServiceClient();
            preguntaService = new FGA_En_Linea.EvalPreguntaService.EvalPreguntaServiceClient();
            subCategoriaService = new FGA_En_Linea.EvalSubCategoriaService.EvalSubCategoriaServiceClient();
            spService = new FGA_En_Linea.SPService.SPClient();
            usuarioService = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
            responsableService = new FGA_En_Linea.EvalResponsableCategoriaService.EvalResponsableCategoriaServiceClient();
            respuestaService = new FGA_En_Linea.EvalRespuestaUsuarioService.ServiceOf_EvalRespuestaUsuarioClient();
        }

        public ActionResult Index()
        {
            Load();
            return View();
        }

        public ActionResult Resultados()
        {
            Load();
            Session["IdCategoria"] = 1;
            ViewBag.Categorias = new SelectList(categoriaService.GetAll().ToList(), "Id", "Titulo");
            return View();
        }

        public ActionResult BuscarResultados(String Entidades, int Categorias)
        {
            Load();
            ViewBag.Categorias = new SelectList(categoriaService.GetAll().ToList(), "Id", "Titulo");
            Session["IdEntidad"] = Entidades;
            Session["IdCategoria"] = Categorias;
            return View("Resultados");
        }

        public ActionResult Buscar(String Entidades)
        {
            Load();
            Session["IdEntidad"] = Entidades;
            return View("Historial");
        }

        public ActionResult Historial()
        {
            Load();
            return View();
        }

        public PartialViewResult GetDesglose(String idEntidad, int idCategoria)
        {
            var desglose = spService.FGA_ConsultarAvance_X_Categoria(idEntidad, idCategoria);
            return PartialView("_VerDesglose", desglose);
        }

        public ActionResult Avance()
        {
            var avance = spService.FGA_ConsultarAvance();
            return View(avance);
        }

        public ActionResult Evaluacion()
        {
            Load();
            Model.Modelo_Evaluacion eval = new Model.Modelo_Evaluacion();
            eval.evaluaciones = spService.FGA_ConsultarEvaluacion(Session["IdEntidad"].ToString(), -1).ToList();
            eval.listaCategorias = categoriaService.GetAll().ToList();
            eval.misCategorias = categoriaService.GetByUser(int.Parse(Env.GetUserInfo("userid"))).ToList();
            eval.eval = false;
            EvalResponsableCategoria resp;
            var id = 0;
            var cantidad = 0;

            foreach (var detalle in eval.listaCategorias)
            {
                id = responsableService.Existe(Session["IdEntidad"].ToString(), 0, detalle.Id);
                if (id == 0)
                {
                    resp = new EvalResponsableCategoria();
                    resp.IdEntidad_Id = Session["IdEntidad"].ToString();
                    resp.IdUsuario_Id1 = 0;
                    resp.IdUsuario_Id1 = 0;
                    resp.IdCategoria_Id = detalle.Id;
                    resp.Confirmada = string.Empty;
                    responsableService.Add(ref resp);
                }
                else
                {
                    resp = responsableService.Get(id.ToString());
                    if (resp.IdUsuario_Id1 != 0 || resp.IdUsuario_Id2 != 0)
                        cantidad = cantidad + 1;
                }
            }

            if (Env.GetUserInfo("entidad") != Utility.Utilitarios.nombreEntidadAdmin){
                if (cantidad == eval.listaCategorias.Count() || Session["Eval"] != null)
                    eval.eval = true;
            }

            eval.listaResponsables = responsableService.GetByEntidad(Session["IdEntidad"].ToString()).ToList();
            eval.listaUsuarios = usuarioService.GetByCompany(Session["IdEntidad"].ToString()).Where(o => o.Estado_Usuario_Id == Utility.Utilitarios.estadoActivo).OrderBy(o => o.Nombre).ToList();
            return View(eval);
        }

        public ActionResult GetResultados()
        {
            try
            {
                var tak = spService.FGA_Consultar_Resultado_Evaluacion(Session["IdEntidad"].ToString(), int.Parse(Session["IdCategoria"].ToString()));
                var result = from c in tak
                             select new string[] {  (c.SUBCOMPONENTE.Contains("Total") ? "1" : "0"),
                                                    c.SUBCOMPONENTE.ToString(),
                                                    (c.FUERTE.Value == 0 ? " " : c.FUERTE.Value.ToString()),
                                                    (c.ACEPTABLE.Value == 0 ? " " : c.ACEPTABLE.Value.ToString()),
                                                    (c.MEJORABLE.Value == 0 ? " " : c.MEJORABLE.Value.ToString()),
                                                    (c.DEBIL.Value == 0 ? " " : c.DEBIL.Value.ToString()),
                              };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { }

            return null;
        }

        public ActionResult GetHistorial()
        {
            try
            {
                var tak = spService.FGA_ConsultarHistorial_X_Categoria(Session["IdEntidad"].ToString(), null);
                var result = from c in tak
                             select new string[] { c.FECHA.Value.ToString(),
                                                    c.NOMUSUARIO,
                                                    c.NOMCATEGORIA,
                                                    c.NOMSUBCATEGORIA,
                                                    c.PREGUNTA,
                                                    c.OPCIONSELECCIONADA,
                                                    c.DETALLERESPUESTA,
                                                    c.ACEPTABLE,
                                                    c.FUERTE
                              };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { }

            return null;
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = categoriaService.GetAll();
                var result = from c in tak
                             select new string[] { c.Id.ToString(),
                                                    c.Id.ToString(),
                                                    Convert.ToString(c.Titulo),
                                                     "<a data-toggle=\"tooltip\" data-id=\"" + c.Id.ToString() + "\" data-placement=\"top\" title=\"Componentes\" href=\"javascript:verSubCategorias('" + c.Id.ToString() + "','" + c.Titulo + "')\" class=\"btn_aceptar\"><i class=\"btn btn-info icon fa fa-book\"></i></a>"
                              };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { }

            return null;
        }

        public ActionResult GetPreguntas(int id)
        {
            try
            {
                var tak = preguntaService.GetBySubCategory(id);
                var result = from c in tak
                             select new string[] { c.Id.ToString(),
                                                    Convert.ToString(c.Enunciado),
                                                     "<div style=\"display:flex;gap:6px;align-items:center;\">" +
                                                     "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Borrar\" href=\"javascript:borrarPregunta('" + c.Id + "')\" style=\"display:inline-flex;align-items:center;justify-content:center;width:30px;height:30px;border-radius:7px;background:#ef4444;color:#fff;border:none;cursor:pointer;text-decoration:none;\"><i class=\"fa fa-trash\" style=\"font-size:13px;\"></i></a>" +
                                                     "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Modificar\" href=\"javascript:modificarPregunta('" + c.Id + "','" + c.Enunciado + "','" + c.OpcionA + "','" + c.OpcionB + "','" + c.OpcionC + "','" + c.OpcionD + "')\" style=\"display:inline-flex;align-items:center;justify-content:center;width:30px;height:30px;border-radius:7px;background:#143750;color:#fff;border:none;cursor:pointer;text-decoration:none;\"><i class=\"fa fa-edit\" style=\"font-size:13px;\"></i></a>" +
                                                     "</div>"
                              };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { }

            return null;
        }

        public ActionResult GetSubCategorias(int id)
        {
            try
            {
                var tak = subCategoriaService.GetByCategory(id);
                var result = from c in tak
                             select new string[] { c.Id.ToString(),
                                                    Convert.ToString(c.Enunciado),
                                                     "<div style=\"display:flex;gap:6px;align-items:center;\">" +
                                                     "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Borrar\" href=\"javascript:borrarSubCategoria('" + c.Id + "')\" style=\"display:inline-flex;align-items:center;justify-content:center;width:30px;height:30px;border-radius:7px;background:#ef4444;color:#fff;border:none;cursor:pointer;text-decoration:none;\"><i class=\"fa fa-trash\" style=\"font-size:13px;\"></i></a>" +
                                                     "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Modificar\" href=\"javascript:modificarSubCategoria('" + c.Id + "','" + c.Enunciado + "')\" style=\"display:inline-flex;align-items:center;justify-content:center;width:30px;height:30px;border-radius:7px;background:#143750;color:#fff;border:none;cursor:pointer;text-decoration:none;\"><i class=\"fa fa-edit\" style=\"font-size:13px;\"></i></a>" +
                                                     "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Preguntas\" href=\"javascript:verPreguntas('" + c.Id.ToString() + "','" + c.Enunciado + "')\" style=\"display:inline-flex;align-items:center;justify-content:center;width:30px;height:30px;border-radius:7px;background:#059669;color:#fff;border:none;cursor:pointer;text-decoration:none;\"><i class=\"fa fa-question\" style=\"font-size:13px;\"></i></a>" +
                                                     "</div>"
                              };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { }

            return null;
        }

        public PartialViewResult ViewEval(int id)
        {
            Session["Eval"] = true;
            Model.Modelo_Evaluacion eval = new Model.Modelo_Evaluacion();
            eval.evaluaciones = spService.FGA_ConsultarEvaluacion(Session["IdEntidad"].ToString(), id).ToList();
            eval.eval = true;
            return PartialView("_ViewEval", eval);
        }


        public PartialViewResult GetGridPreguntas()
        {
            return PartialView("_VerPreguntas");
        }

        public PartialViewResult GetGridSubCategorias()
        {
            return PartialView("_VerSubCategorias");
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(EvalCategoria ObjCategoria, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var imagen = Guid.NewGuid().ToString();
            var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "categoria\\" + imagen + ".jpg";

            try
            {
                if (ModelState.IsValid)
                {

                    if (!(file is null))
                    {
                        ObjCategoria.Imagen = imagen;
                        ObjCategoria.Ind_Estado = "A";
                        categoriaService.Add(ref ObjCategoria);

                        if (file.ContentType.Equals("image/jpeg") || file.ContentType.Equals("image/png"))
                        {
                            file.SaveAs(path);
                        }
                        else
                            sb.Append("Error: La imagen debe estar en formato .jpg" + "<br/>");

                        sb.Append("Sumitted");
                        return Content(sb.ToString());
                    }
                    else
                    {
                        sb.Append("Error: No se puede agregar una categoría sin Logo" + "<br/>");
                        return Content(sb.ToString());
                    }
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

            EvalCategoria ObjCompany = categoriaService.Get(id);
            if (ObjCompany == null)
                return HttpNotFound();

            return View(ObjCompany);
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            EvalCategoria ObjCompany = categoriaService.Get(id);
            if (ObjCompany == null)
                return HttpNotFound();

            return View(ObjCompany);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Delete(EvalCategoria ObjCategoria)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                ObjCategoria = categoriaService.Get(ObjCategoria.Id.ToString());
                ObjCategoria.Ind_Estado = "I";
                categoriaService.Update(ObjCategoria);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(EvalCategoria ObjCategoria, HttpPostedFileBase file)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var imagen = Guid.NewGuid().ToString();
            var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "categoria\\" + imagen + ".jpg";
            var original = categoriaService.Get(ObjCategoria.Id.ToString());

            try
            {
                if (ModelState.IsValid)
                {
                    ObjCategoria.Imagen = original.Imagen;

                    if (!(file is null))
                    {
                        if (file.ContentType.Equals("image/jpeg") || file.ContentType.Equals("image/png"))
                        {
                            file.SaveAs(path);
                            ObjCategoria.Imagen = imagen;
                        }
                        else
                            sb.Append("La imagen debe estar en formato .jpg" + "<br/>");
                    }

                    categoriaService.Update(ObjCategoria);
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

        public void DeletePregunta(string id)
        {
            try
            {
                preguntaService.Delete(id);
            }
            catch (Exception) { }
        }

        public void ModificarPregunta(int IdPregunta, string Enunciado, string OpcionA, string OpcionB, string OpcionC, string OpcionD)
        {
            try
            {
                EvalPregunta pregunta = preguntaService.Get(IdPregunta.ToString());
                pregunta.Enunciado = Enunciado;
                pregunta.OpcionA = OpcionA;
                pregunta.OpcionB = OpcionB;
                pregunta.OpcionC = OpcionC;
                pregunta.OpcionD = OpcionD;
                preguntaService.Update(pregunta);
            }
            catch (Exception) { }
        }

        public void AgregarPregunta(int IdSubCategoria, string Enunciado, string OpcionA, string OpcionB, string OpcionC, string OpcionD)
        {
            try
            {
                EvalPregunta pregunta = new EvalPregunta();
                pregunta.IdSubCategoria = IdSubCategoria;
                pregunta.Enunciado = Enunciado;
                pregunta.OpcionA = OpcionA;
                pregunta.OpcionB = OpcionB;
                pregunta.OpcionC = OpcionC;
                pregunta.OpcionD = OpcionD;
                preguntaService.Add(ref pregunta);
            }
            catch (Exception) { }
        }

        public void DeleteSubCategoria(string id)
        {
            try
            {
                subCategoriaService.Delete(id);
            }
            catch (Exception) { }
        }

        public void ModificarSubCategoria(int IdSubCategoria, string Enunciado)
        {
            try
            {
                EvalSubCategoria subCategoria = subCategoriaService.Get(IdSubCategoria.ToString());
                subCategoria.Enunciado = Enunciado;
                subCategoriaService.Update(subCategoria);
            }
            catch (Exception) { }
        }

        public void AgregarSubCategoria(int IdCategoria, string Enunciado)
        {
            try
            {
                EvalSubCategoria subCategoria = new EvalSubCategoria();
                subCategoria.IdCategoria = IdCategoria;
                subCategoria.Enunciado = Enunciado;
                subCategoriaService.Add(ref subCategoria);
            }
            catch (Exception) { }
        }

        public int AsignarResponsable(int idUsuario1, int idUsuario2, int idCategoria)
        {
            int cantidad = 0;
            try
            {
                var id = responsableService.Existe(Session["IdEntidad"].ToString(), 0, idCategoria);
                EvalResponsableCategoria resp = new EvalResponsableCategoria();
                resp = responsableService.Get(id.ToString());
                resp.IdUsuario_Id1 = idUsuario1 == 0 ? resp.IdUsuario_Id1 : idUsuario1;
                resp.IdUsuario_Id2 = idUsuario2 == 0 ? resp.IdUsuario_Id2 : idUsuario2;
                responsableService.Update(resp);
                cantidad = responsableService.GetByEntidad(Session["IdEntidad"].ToString()).Where(o => o.IdUsuario_Id1 == 0 && o.IdUsuario_Id2 == 0).Count();
            }
            catch (Exception) { }

            return (cantidad == 0 ? 1 : 0);
        }

        public void Confirmar(int idCategoria, int idSubCategoria)
        {
            try
            {
                var id = responsableService.Existe(Session["IdEntidad"].ToString(), 0, idCategoria);
                EvalResponsableCategoria resp = new EvalResponsableCategoria();
                resp = responsableService.Get(id.ToString());
                resp.Confirmada = (resp.Confirmada == string.Empty ? ";" : resp.Confirmada) + idSubCategoria.ToString() + ";";
                responsableService.Update(resp);
            }
            catch (Exception) { }
        }

        public void Respuesta(int idOpcion, int idPregunta)
        {
            try
            {
                EvalRespuestaUsuario resp = new EvalRespuestaUsuario();
                resp.IdEntidad_Id = Session["IdEntidad"].ToString();
                resp.IdUsuario_Id = int.Parse(Env.GetUserInfo("userid"));
                resp.IdPregunta_Id = idPregunta;
                resp.OpcionSeleccionada = idOpcion;
                respuestaService.Add(ref resp);
            }
            catch (Exception) { }
        }
    }
}