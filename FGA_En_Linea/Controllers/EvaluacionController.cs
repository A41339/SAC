using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Entities.Entities.Evaluacion;
using FGA.Utility;
using FGA.Model;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Hosting;

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
        private FGA_En_Linea.EntidadService.EntidadServiceClient entidadService;

        public EvaluacionController()
        {
            categoriaService = new FGA_En_Linea.EvalCategoriaService.EvalCategoriaServiceClient();
            preguntaService = new FGA_En_Linea.EvalPreguntaService.EvalPreguntaServiceClient();
            subCategoriaService = new FGA_En_Linea.EvalSubCategoriaService.EvalSubCategoriaServiceClient();
            spService = new FGA_En_Linea.SPService.SPClient();
            usuarioService = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
            responsableService = new FGA_En_Linea.EvalResponsableCategoriaService.EvalResponsableCategoriaServiceClient();
            respuestaService = new FGA_En_Linea.EvalRespuestaUsuarioService.ServiceOf_EvalRespuestaUsuarioClient();
            entidadService = new FGA_En_Linea.EntidadService.EntidadServiceClient();
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

        public ActionResult ReporteEjecutivo(string idEntidad)
        {
            Load();
            if (string.IsNullOrEmpty(idEntidad))
            {
                idEntidad = Session["IdEntidad"] != null ? Session["IdEntidad"].ToString() : "0";
            }

            var entidadService = new FGA_En_Linea.EntidadService.EntidadServiceClient();
            string nombreEntidad = string.Empty;
            try
            {
                var entObj = entidadService.Get(idEntidad);
                if (entObj != null)
                {
                    nombreEntidad = entObj.Nombre;
                }
            }
            catch (Exception) { }

            if (string.IsNullOrEmpty(nombreEntidad) && Session["NomEntidad"] != null)
            {
                nombreEntidad = Session["NomEntidad"].ToString();
            }

            ViewBag.IdEntidad = idEntidad;
            ViewBag.NombreEntidad = nombreEntidad;
            ViewBag.FechaReporte = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            var historial = spService.FGA_ConsultarHistorial_X_Categoria(idEntidad, null);
            return View(historial != null ? historial.ToList() : new System.Collections.Generic.List<Entities.Entities.Procedures.FGA_Consultar_Historial_X_Categoria_Result>());
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
            if (Session["IdEntidad"] == null)
            {
                string ent = Env.GetUserInfo("entidad");
                Session["IdEntidad"] = !string.IsNullOrEmpty(ent) ? ent : Utility.Utilitarios.entidadDefault;
            }
            Model.Modelo_Evaluacion eval = new Model.Modelo_Evaluacion();
            eval.evaluaciones = spService.FGA_ConsultarEvaluacion(Session["IdEntidad"].ToString(), -1).ToList();
            eval.listaCategorias = categoriaService.GetAll().ToList();
            eval.misCategorias = categoriaService.GetByUser(int.Parse(Env.GetUserInfo("userid"))).ToList();
            ViewBag.EsAdmin = IsCurrentUserAdmin;
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
                                                    c.DETALLERESPUESTA
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
                             select new string[] {
                                 c.Id.ToString(),
                                 c.Id.ToString(),
                                 string.IsNullOrEmpty(c.Imagen) ? "Logo" : c.Imagen,
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

        public ActionResult DeletePregunta(string id)
        {
            try
            {
                preguntaService.Delete(id);
                return Json(new { success = true, message = "Pregunta eliminada correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la pregunta: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ModificarPregunta(int IdPregunta, string Enunciado, string OpcionA, string OpcionB, string OpcionC, string OpcionD)
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
                return Json(new { success = true, message = "Pregunta modificada correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al modificar la pregunta: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AgregarPregunta(int IdSubCategoria, string Enunciado, string OpcionA, string OpcionB, string OpcionC, string OpcionD)
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
                return Json(new { success = true, message = "Pregunta agregada correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al agregar la pregunta: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteSubCategoria(string id)
        {
            try
            {
                subCategoriaService.Delete(id);
                return Json(new { success = true, message = "Componente eliminado correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el componente: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ModificarSubCategoria(int IdSubCategoria, string Enunciado)
        {
            try
            {
                EvalSubCategoria subCategoria = subCategoriaService.Get(IdSubCategoria.ToString());
                subCategoria.Enunciado = Enunciado;
                subCategoriaService.Update(subCategoria);
                return Json(new { success = true, message = "Componente modificado correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al modificar el componente: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AgregarSubCategoria(int IdCategoria, string Enunciado)
        {
            try
            {
                EvalSubCategoria subCategoria = new EvalSubCategoria();
                subCategoria.IdCategoria = IdCategoria;
                subCategoria.Enunciado = Enunciado;
                subCategoriaService.Add(ref subCategoria);
                return Json(new { success = true, message = "Componente agregado correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al agregar el componente: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
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

        public ActionResult Confirmar(int idCategoria, int idSubCategoria)
        {
            try
            {
                string idEntidad = Session["IdEntidad"] != null ? Session["IdEntidad"].ToString() : (Env.GetUserInfo("identidad") ?? "0");
                var id = responsableService.Existe(idEntidad, 0, idCategoria);
                EvalResponsableCategoria resp;

                if (id == 0)
                {
                    resp = new EvalResponsableCategoria();
                    resp.IdEntidad_Id = idEntidad;
                    resp.IdCategoria_Id = idCategoria;
                    resp.IdUsuario_Id1 = 0;
                    resp.IdUsuario_Id2 = 0;
                    resp.Confirmada = ";" + idSubCategoria.ToString() + ";";
                    responsableService.Add(ref resp);
                }
                else
                {
                    resp = responsableService.Get(id.ToString());
                    string conf = resp.Confirmada ?? "";
                    if (!conf.Contains(";" + idSubCategoria.ToString() + ";"))
                    {
                        resp.Confirmada = (string.IsNullOrEmpty(conf) ? ";" : conf) + idSubCategoria.ToString() + ";";
                        responsableService.Update(resp);
                    }
                }

                // Notificación automática por correo al terminar la evaluación
                EnviarNotificacionFinalizacionSBR(idEntidad, idCategoria, idSubCategoria);

                return Json(new { success = true, message = "Evaluación confirmada y notificada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al confirmar: " + ex.Message });
            }
        }

        private void EnviarNotificacionFinalizacionSBR(string idEntidad, int idCategoria, int idSubCategoria)
        {
            try
            {
                // 1. Obtener datos de la Entidad
                string nombreEntidad = Session["NomEntidad"]?.ToString();
                if (string.IsNullOrEmpty(nombreEntidad) || nombreEntidad == "0")
                {
                    try
                    {
                        var entObj = new FGA_En_Linea.EntidadService.EntidadServiceClient().Get(idEntidad);
                        nombreEntidad = entObj?.Nombre ?? ("Entidad " + idEntidad);
                    }
                    catch
                    {
                        nombreEntidad = "Entidad " + idEntidad;
                    }
                }

                // 2. Obtener nombres de Categoría y SubCategoría (Componente)
                string nombreCategoria = "Categoría SBR";
                try
                {
                    var catObj = categoriaService.Get(idCategoria.ToString());
                    if (catObj != null && !string.IsNullOrWhiteSpace(catObj.Titulo))
                        nombreCategoria = catObj.Titulo;
                }
                catch { }

                string nombreSubCategoria = "Componente " + idSubCategoria;
                try
                {
                    var subCatObj = subCategoriaService.Get(idSubCategoria.ToString());
                    if (subCatObj != null && !string.IsNullOrWhiteSpace(subCatObj.Enunciado))
                        nombreSubCategoria = subCatObj.Enunciado;
                }
                catch { }

                // 3. Usuario que completó la evaluación
                string nombreUsuario = Session["NomUsuario"]?.ToString() ?? Env.GetUserInfo("name") ?? "Usuario SAC";

                // 4. Determinar avance global y de la categoría
                bool categoriaCompleta = false;
                bool todoCompleto = false;
                try
                {
                    var subcats = subCategoriaService.GetByCategory(idCategoria).ToList();
                    int respId = responsableService.Existe(idEntidad, 0, idCategoria);
                    if (respId > 0)
                    {
                        var respCat = responsableService.Get(respId.ToString());
                        string confirmadas = respCat?.Confirmada ?? "";
                        if (subcats.All(sc => confirmadas.Contains(";" + sc.Id + ";")))
                        {
                            categoriaCompleta = true;
                        }
                    }

                    var allCats = categoriaService.GetAll().Where(c => c.Ind_Estado != "I").ToList();
                    todoCompleto = allCats.All(c => {
                        int rId = responsableService.Existe(idEntidad, 0, c.Id);
                        if (rId == 0) return false;
                        var r = responsableService.Get(rId.ToString());
                        var scs = subCategoriaService.GetByCategory(c.Id).ToList();
                        return scs.All(sc => (r?.Confirmada ?? "").Contains(";" + sc.Id + ";"));
                    });
                }
                catch { }

                // 5. Parámetros de correo electrónico
                var paramService = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
                var listParam = paramService.GetAll().ToList();
                var servidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
                var cuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
                var passwordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();
                var destinatariosParam = listParam.Where(o => o.Llave == Utility.Utilitarios.Correo_Error_XML).Select(o => o.Valor).FirstOrDefault();

                var listaDest = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(destinatariosParam))
                {
                    foreach (var d in destinatariosParam.Split(';', ','))
                    {
                        if (!string.IsNullOrWhiteSpace(d) && !listaDest.Contains(d.Trim()))
                            listaDest.Add(d.Trim());
                    }
                }
                if (!string.IsNullOrWhiteSpace(cuentaCorreo) && !listaDest.Contains(cuentaCorreo.Trim()))
                {
                    listaDest.Add(cuentaCorreo.Trim());
                }

                string userEmail = Env.GetUserInfo("email");
                if (!string.IsNullOrWhiteSpace(userEmail) && !listaDest.Contains(userEmail.Trim()))
                {
                    listaDest.Add(userEmail.Trim());
                }

                if (string.IsNullOrWhiteSpace(servidorCorreo) || string.IsNullOrWhiteSpace(cuentaCorreo) || listaDest.Count == 0)
                    return;

                string destinatarios = string.Join(";", listaDest);

                // 6. Asunto y cuerpo del mensaje corporativo FFC
                string estadoTexto = todoCompleto
                    ? "100% de la Autoevaluación SBR Concluida"
                    : (categoriaCompleta
                        ? "Categoría Concluida: " + nombreCategoria
                        : "Componente Finalizado: " + nombreSubCategoria);

                string asunto = todoCompleto
                    ? "Notificación FFC: " + nombreEntidad + " ha completado el 100% de la Autoevaluación SBR"
                    : "Notificación FFC: " + nombreEntidad + " ha concluido evaluación SBR (" + nombreCategoria + ")";

                string badgeEstado = todoCompleto
                    ? "<span style=\"display: inline-block; background-color: #ecfdf5; color: #065f46; font-weight: 700; font-size: 12px; padding: 4px 12px; border-radius: 4px; border: 1px solid #a7f3d0;\">&#10003; 100% AUTOEVALUACIÓN SBR COMPLETADA</span>"
                    : "<span style=\"display: inline-block; background-color: #eff6ff; color: #1e40af; font-weight: 700; font-size: 12px; padding: 4px 12px; border-radius: 4px; border: 1px solid #bfdbfe;\">&#10003; COMPONENTE EVALUADO</span>";

                string cuerpo = "<div style=\"margin-bottom: 18px;\">"
                    + badgeEstado
                    + "</div>"
                    + "<p style=\"font-size: 15px; color: #1e293b; line-height: 1.6; margin: 0 0 16px 0;\">"
                    + "Estimados personeros del <strong>Fondo de Fortalecimiento Cooperativo (FFC)</strong>,"
                    + "</p>"
                    + "<p style=\"font-size: 14px; color: #334155; line-height: 1.6; margin: 0 0 20px 0;\">"
                    + "Se les informa que la entidad <strong>" + HttpUtility.HtmlEncode(nombreEntidad) + "</strong> ha finalizado la evaluación del componente en el sistema SAC bajo la normativa <strong>Acuerdo SUGEF 24-22 (Supervisión Basada en Riesgos)</strong>."
                    + "</p>"
                    + "<div style=\"background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 16px; margin-bottom: 20px;\">"
                    + "<table style=\"width: 100%; font-size: 13px; border-collapse: collapse;\">"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; width: 140px; border-bottom: 1px solid #e2e8f0;\">Entidad:</td><td style=\"padding: 8px 0; color: #0f172a; font-weight: 700; border-bottom: 1px solid #e2e8f0;\">" + HttpUtility.HtmlEncode(nombreEntidad) + "</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; border-bottom: 1px solid #e2e8f0;\">Categoría SBR:</td><td style=\"padding: 8px 0; color: #0f172a; font-weight: 700; border-bottom: 1px solid #e2e8f0;\">" + HttpUtility.HtmlEncode(nombreCategoria) + "</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; border-bottom: 1px solid #e2e8f0;\">Componente:</td><td style=\"padding: 8px 0; color: #0f172a; border-bottom: 1px solid #e2e8f0;\">" + HttpUtility.HtmlEncode(nombreSubCategoria) + "</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; border-bottom: 1px solid #e2e8f0;\">Fecha y Hora:</td><td style=\"padding: 8px 0; color: #0f172a; border-bottom: 1px solid #e2e8f0;\">" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; border-bottom: 1px solid #e2e8f0;\">Avance:</td><td style=\"padding: 8px 0; color: #0284c7; font-weight: 700; border-bottom: 1px solid #e2e8f0;\">" + estadoTexto + "</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600;\">Completado por:</td><td style=\"padding: 8px 0; color: #0f172a;\">" + HttpUtility.HtmlEncode(nombreUsuario) + "</td></tr>"
                    + "</table>"
                    + "</div>"
                    + "<p style=\"font-size: 13px; color: #64748b; line-height: 1.5; margin: 0 0 10px 0;\">"
                    + "Las respuestas y ponderaciones registradas se encuentran consolidadas en el módulo de Resultados e Historial SBR para su respectiva consulta y emisión de diagnósticos."
                    + "</p>";

                MailSend.Email.EnviarCorreoImagenes(asunto, cuerpo, destinatarios, servidorCorreo, cuentaCorreo, cuentaCorreo, passwordCorreo);
            }
            catch (Exception)
            {
                // En caso de inconsistencia de red SMTP, continuar sin interrumpir el flujo del usuario
            }
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

        // ── Acciones JSON para modal CRUD de Categorías ─────────────────────────

        public ActionResult GetCategoria(int id)
        {
            try
            {
                var obj = categoriaService.Get(id.ToString());
                if (obj == null)
                    return Json(new { success = false, message = "Categoría no encontrada." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    categoria = new
                    {
                        id = obj.Id,
                        titulo = obj.Titulo,
                        responsableSugerido = obj.ResponsableSugerido,
                        descripcion = obj.Descripcion,
                        puntosOpcionA = obj.PuntosOpcionA,
                        puntosOpcionB = obj.PuntosOpcionB,
                        puntosOpcionC = obj.PuntosOpcionC,
                        puntosOpcionD = obj.PuntosOpcionD,
                        imagen = obj.Imagen,
                        ind_Estado = obj.Ind_Estado
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GuardarCategoria(EvalCategoria ObjCategoria, HttpPostedFileBase file)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ObjCategoria.Titulo))
                    return Json(new { success = false, message = "El título de la categoría es requerido." });

                var imagen = Guid.NewGuid().ToString();
                var rutaBase = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] ?? @"C:\inetpub\wwwroot\img\secciones\";
                var dirCategoria = System.IO.Path.Combine(rutaBase, "categoria");
                var path = System.IO.Path.Combine(dirCategoria, imagen + ".jpg");

                if (!System.IO.Directory.Exists(dirCategoria))
                {
                    try { System.IO.Directory.CreateDirectory(dirCategoria); } catch { }
                }

                if (ObjCategoria.Id > 0)
                {
                    // Editar
                    var original = categoriaService.Get(ObjCategoria.Id.ToString());
                    if (original == null)
                        return Json(new { success = false, message = "Categoría no encontrada." });

                    ObjCategoria.Imagen = original.Imagen;
                    ObjCategoria.Ind_Estado = string.IsNullOrEmpty(ObjCategoria.Ind_Estado) ? original.Ind_Estado : ObjCategoria.Ind_Estado;

                    if (file != null && file.ContentLength > 0)
                    {
                        string ctype = (file.ContentType ?? "").ToLowerInvariant();
                        if (ctype.Contains("jpeg") || ctype.Contains("jpg") || ctype.Contains("png"))
                        {
                            file.SaveAs(path);
                            ObjCategoria.Imagen = imagen;
                        }
                        else
                        {
                            return Json(new { success = false, message = "La imagen debe estar en formato .jpg o .png." });
                        }
                    }

                    categoriaService.Update(ObjCategoria);
                    return Json(new { success = true, message = "Categoría modificada correctamente." });
                }
                else
                {
                    // Crear
                    ObjCategoria.Ind_Estado = "A";

                    if (file != null && file.ContentLength > 0)
                    {
                        string ctype = (file.ContentType ?? "").ToLowerInvariant();
                        if (ctype.Contains("jpeg") || ctype.Contains("jpg") || ctype.Contains("png"))
                        {
                            file.SaveAs(path);
                            ObjCategoria.Imagen = imagen;
                        }
                        else
                        {
                            return Json(new { success = false, message = "La imagen debe estar en formato .jpg o .png." });
                        }
                    }
                    else
                    {
                        ObjCategoria.Imagen = "Logo";
                    }

                    categoriaService.Add(ref ObjCategoria);
                    return Json(new { success = true, message = "Categoría creada correctamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar la categoría: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarCategoria(int id)
        {
            try
            {
                var obj = categoriaService.Get(id.ToString());
                if (obj == null)
                    return Json(new { success = false, message = "Categoría no encontrada." });

                obj.Ind_Estado = "I";
                categoriaService.Update(obj);
                return Json(new { success = true, message = "Categoría eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la categoría: " + ex.Message });
            }
        }

        #region Plan de Remediación

        private string GetPlanRemediacionDir()
        {
            // 1. Intentar leer desde dbo.Parametros
            string ruta = null;
            try
            {
                ruta = GetValorParametro(Utility.Utilitarios.Ruta_Planes_Remediacion);
            }
            catch { }

            // 2. Si no esta en BD, leer desde Web.config (appSettings)
            if (string.IsNullOrWhiteSpace(ruta))
            {
                try
                {
                    ruta = ConfigurationManager.AppSettings["RutaPlanesRemediacion"];
                }
                catch { }
            }

            // 3. Fallback dinamico relativo si no existe configuracion
            if (string.IsNullOrWhiteSpace(ruta))
            {
                ruta = "~/Uploads/PlanesRemediacion";
            }

            // 4. Si es ruta virtual relativa de ASP.NET (~/...), mapearla al servidor fisico
            string rutaFisica = ruta;
            if (ruta.StartsWith("~"))
            {
                try
                {
                    if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Server != null)
                    {
                        rutaFisica = System.Web.HttpContext.Current.Server.MapPath(ruta);
                    }
                    else if (HostingEnvironment.IsHosted)
                    {
                        rutaFisica = HostingEnvironment.MapPath(ruta);
                    }
                }
                catch { }
            }

            if (!Directory.Exists(rutaFisica))
            {
                Directory.CreateDirectory(rutaFisica);
            }
            return rutaFisica;
        }

        private PlanRemediacionMetadata ObtenerMetadataPlan(string idEntidad)
        {
            try
            {
                string dir = GetPlanRemediacionDir();
                string jsonPath = Path.Combine(dir, "Plan_Remediacion_" + idEntidad + ".json");
                if (System.IO.File.Exists(jsonPath))
                {
                    string content = System.IO.File.ReadAllText(jsonPath);
                    return JsonConvert.DeserializeObject<PlanRemediacionMetadata>(content);
                }
            }
            catch { }
            return null;
        }

        private string FormatearTamanio(long bytes)
        {
            if (bytes < 1024)
                return bytes + " B";
            if (bytes < 1024 * 1024)
                return (bytes / 1024.0).ToString("N1") + " KB";
            return (bytes / (1024.0 * 1024.0)).ToString("N2") + " MB";
        }

        private string ResolverIdEntidadActual(string idEntidadParam = null)
        {
            string id = idEntidadParam;
            if (!string.IsNullOrEmpty(id) && id != "0")
            {
                return id;
            }

            var sessId = Session["IdEntidad"]?.ToString();
            if (!string.IsNullOrEmpty(sessId) && sessId != "0")
            {
                id = sessId;
            }

            if (string.IsNullOrEmpty(id) || id == "0")
            {
                string uEnt = Env.GetUserInfo("identidad");
                if (!string.IsNullOrEmpty(uEnt) && uEnt != "0")
                {
                    id = uEnt;
                }
            }

            if (string.IsNullOrEmpty(id) || id == "0")
            {
                var usrObj = (Session["CurrentUserObj"] ?? Session["Usuario"]) as FGA.Models.Usuario;
                if (usrObj != null && !string.IsNullOrEmpty(usrObj.Entidad_Usuario_Id) && usrObj.Entidad_Usuario_Id != "0")
                {
                    id = usrObj.Entidad_Usuario_Id;
                }
            }

            if (string.IsNullOrEmpty(id) || id == "0")
            {
                try
                {
                    var primera = entidadService.GetAll().FirstOrDefault(o => o.Id != Utility.Utilitarios.entidadAdministradora && o.Activo);
                    if (primera != null)
                    {
                        id = primera.Id;
                    }
                }
                catch { }
            }

            if (string.IsNullOrEmpty(id) || id == "0")
            {
                id = Utility.Utilitarios.entidadDefault;
            }

            return id;
        }

        private string ObtenerCorreoContactosFFC()
        {
            string correoFFC = "";
            try
            {
                var ffcEnt = entidadService.Get(Utility.Utilitarios.entidadAdministradora);
                if (ffcEnt != null && !string.IsNullOrWhiteSpace(ffcEnt.Contacto))
                {
                    correoFFC = ffcEnt.Contacto.Trim();
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(correoFFC))
            {
                try
                {
                    var paramService = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
                    var listParam = paramService.GetAll().ToList();
                    var dirCorreo = listParam.FirstOrDefault(o => o.Llave == Utility.Utilitarios.Direccion_Correo)?.Valor;
                    var errCorreo = listParam.FirstOrDefault(o => o.Llave == Utility.Utilitarios.Correo_Error_XML)?.Valor;
                    paramService.SafeClose();

                    correoFFC = !string.IsNullOrWhiteSpace(errCorreo) ? errCorreo : dirCorreo;
                }
                catch { }
            }
            return correoFFC ?? "";
        }

        [HttpGet]
        public ActionResult VerificarPlanRemediacion(string idEntidad = null)
        {
            try
            {
                string id = ResolverIdEntidadActual(idEntidad);

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { existe = false, message = "No se ha seleccionado una entidad." }, JsonRequestBehavior.AllowGet);
                }

                string dir = GetPlanRemediacionDir();
                string pdfPath = Path.Combine(dir, "Plan_Remediacion_" + id + ".pdf");
                bool existe = System.IO.File.Exists(pdfPath);

                string nombreEntidad = "";
                string contactoEntidad = "";
                try
                {
                    var entObj = entidadService.Get(id);
                    if (entObj != null)
                    {
                        nombreEntidad = entObj.Nombre ?? "";
                        contactoEntidad = entObj.Contacto ?? "";
                    }
                }
                catch { }

                if (string.IsNullOrEmpty(nombreEntidad) && Session["NomEntidad"] != null)
                {
                    nombreEntidad = Session["NomEntidad"].ToString();
                }

                string correoFFC = ObtenerCorreoContactosFFC();

                var usrSession = (Session["CurrentUserObj"] ?? Session["Usuario"]) as FGA.Models.Usuario;
                string usuarioActual = usrSession?.Nombre ?? Env.GetUserInfo("name");
                if (string.IsNullOrWhiteSpace(usuarioActual)) usuarioActual = Env.GetUserInfo("username");
                if (string.IsNullOrWhiteSpace(usuarioActual)) usuarioActual = User?.Identity?.Name ?? "Usuario en sesión";

                string correoUsuarioActual = usrSession?.Correo ?? Env.GetUserInfo("email");
                if (string.IsNullOrWhiteSpace(correoUsuarioActual)) correoUsuarioActual = usrSession?.Correo ?? "";

                if (existe)
                {
                    var meta = ObtenerMetadataPlan(id);
                    var fi = new FileInfo(pdfPath);
                    return Json(new
                    {
                        existe = true,
                        idEntidad = id,
                        nombreEntidad = nombreEntidad,
                        contactoEntidad = contactoEntidad,
                        correoFFC = correoFFC,
                        nombreArchivo = meta?.NombreArchivoOriginal ?? Path.GetFileName(pdfPath),
                        fechaCarga = (meta?.FechaCarga ?? fi.LastWriteTime).ToString("dd/MM/yyyy HH:mm"),
                        usuarioCarga = meta?.UsuarioCarga ?? "FFC",
                        tamanio = FormatearTamanio(fi.Length),
                        notas = meta?.Notas ?? "",
                        urlVer = Url.Action("VerPlanRemediacion", "Evaluacion", new { idEntidad = id }),
                        urlDescargar = Url.Action("DescargarPlanRemediacion", "Evaluacion", new { idEntidad = id })
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        existe = false,
                        idEntidad = id,
                        nombreEntidad = nombreEntidad,
                        contactoEntidad = contactoEntidad,
                        correoFFC = correoFFC,
                        usuarioActual = usuarioActual,
                        correoUsuarioActual = correoUsuarioActual
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { existe = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult VerPlanRemediacion(string idEntidad = null)
        {
            string id = ResolverIdEntidadActual(idEntidad);

            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound("No se especificó la entidad.");
            }

            string dir = GetPlanRemediacionDir();
            string pdfPath = Path.Combine(dir, "Plan_Remediacion_" + id + ".pdf");

            if (!System.IO.File.Exists(pdfPath))
            {
                return HttpNotFound("El plan de remediación no se encuentra disponible.");
            }

            Response.AppendHeader("Content-Disposition", "inline; filename=Plan_Remediacion_" + id + ".pdf");
            return File(pdfPath, "application/pdf");
        }

        [HttpGet]
        public ActionResult DescargarPlanRemediacion(string idEntidad = null)
        {
            string id = ResolverIdEntidadActual(idEntidad);

            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound("No se especificó la entidad.");
            }

            string dir = GetPlanRemediacionDir();
            string pdfPath = Path.Combine(dir, "Plan_Remediacion_" + id + ".pdf");

            if (!System.IO.File.Exists(pdfPath))
            {
                return HttpNotFound("El plan de remediación no se encuentra disponible.");
            }

            var meta = ObtenerMetadataPlan(id);
            string downloadName = !string.IsNullOrEmpty(meta?.NombreArchivoOriginal)
                ? meta.NombreArchivoOriginal
                : ("Plan_Remediacion_" + id + ".pdf");

            return File(pdfPath, "application/pdf", downloadName);
        }

        [HttpPost]
        public ActionResult SolicitarPlanRemediacion(string idEntidad = null, string mensaje = null)
        {
            try
            {
                string id = ResolverIdEntidadActual(idEntidad);

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "No se especificó la entidad solicitante." });
                }

                string nombreEntidad = "";
                string contactoEntidad = "";
                try
                {
                    var entObj = entidadService.Get(id);
                    if (entObj != null)
                    {
                        nombreEntidad = entObj.Nombre;
                        contactoEntidad = entObj.Contacto ?? "";
                    }
                }
                catch { }

                if (string.IsNullOrEmpty(nombreEntidad) && Session["NomEntidad"] != null)
                {
                    nombreEntidad = Session["NomEntidad"].ToString();
                }

                var paramService = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
                var listParam = paramService.GetAll().ToList();
                var servidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
                var cuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
                var passwordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();
                var destinatariosParam = listParam.Where(o => o.Llave == Utility.Utilitarios.Correo_Error_XML).Select(o => o.Valor).FirstOrDefault();
                paramService.SafeClose();

                string correoFFC = ObtenerCorreoContactosFFC();
                var listaDest = new System.Collections.Generic.List<string>();

                // 1. Destinatarios FFC (lista oficial de contactos FFC en Entidades y Parametros)
                if (!string.IsNullOrWhiteSpace(correoFFC))
                {
                    foreach (var d in correoFFC.Split(';', ','))
                    {
                        if (!string.IsNullOrWhiteSpace(d) && !listaDest.Contains(d.Trim()))
                            listaDest.Add(d.Trim());
                    }
                }
                if (!string.IsNullOrWhiteSpace(destinatariosParam))
                {
                    foreach (var d in destinatariosParam.Split(';', ','))
                    {
                        if (!string.IsNullOrWhiteSpace(d) && !listaDest.Contains(d.Trim()))
                            listaDest.Add(d.Trim());
                    }
                }
                if (!string.IsNullOrWhiteSpace(cuentaCorreo) && !listaDest.Contains(cuentaCorreo.Trim()))
                {
                    listaDest.Add(cuentaCorreo.Trim());
                }

                // 2. Contactos registrados en la entidad supervisada solicitante
                if (!string.IsNullOrWhiteSpace(contactoEntidad))
                {
                    foreach (var c in contactoEntidad.Split(';', ','))
                    {
                        if (!string.IsNullOrWhiteSpace(c) && !listaDest.Contains(c.Trim()))
                            listaDest.Add(c.Trim());
                    }
                }

                // 3. Usuario solicitante en sesión
                var usrSession = (Session["CurrentUserObj"] ?? Session["Usuario"]) as FGA.Models.Usuario;
                string userEmail = usrSession?.Correo ?? Env.GetUserInfo("email");
                if (!string.IsNullOrWhiteSpace(userEmail) && !listaDest.Contains(userEmail.Trim()))
                {
                    listaDest.Add(userEmail.Trim());
                }

                string userName = usrSession?.Nombre ?? Env.GetUserInfo("name");
                if (string.IsNullOrEmpty(userName)) userName = Env.GetUserInfo("username");
                if (string.IsNullOrEmpty(userName)) userName = User?.Identity?.Name ?? "Usuario SAC";

                if (listaDest.Count == 0)
                {
                    return Json(new { success = false, message = "No se encontraron direcciones de correo válidas para remitir la solicitud." });
                }

                string destinatarios = string.Join(";", listaDest);
                string asunto = "Solicitud de Plan de Remediación SBR - " + nombreEntidad;

                string cuerpo = "<div style=\"font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 650px; margin: 0 auto; background: #ffffff; border-radius: 10px; border: 1px solid #e2e8f0; overflow: hidden;\">"
                    + "<div style=\"background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 100%); padding: 24px 28px; color: #ffffff;\">"
                    + "<h2 style=\"margin: 0 0 6px 0; font-size: 20px; font-weight: 700; color: #ffffff;\">Fondo de Fortalecimiento Cooperativo (FFC)</h2>"
                    + "<p style=\"margin: 0; font-size: 13px; color: #93c5fd;\">Supervisión Basada en Riesgos (Acuerdo SUGEF 24-22)</p>"
                    + "</div>"
                    + "<div style=\"padding: 24px 28px;\">"
                    + "<div style=\"margin-bottom: 20px;\">"
                    + "<span style=\"display: inline-block; background-color: #fef3c7; color: #92400e; font-weight: 700; font-size: 12px; padding: 6px 14px; border-radius: 6px; border: 1px solid #fde68a;\">&#9993; SOLICITUD DE PLAN DE REMEDIACIÓN SBR</span>"
                    + "</div>"
                    + "<p style=\"font-size: 15px; color: #1e293b; line-height: 1.6; margin: 0 0 16px 0;\">"
                    + "Estimado equipo del <strong>Fondo de Fortalecimiento Cooperativo (FFC)</strong>,"
                    + "</p>"
                    + "<p style=\"font-size: 14px; color: #334155; line-height: 1.6; margin: 0 0 20px 0;\">"
                    + "La entidad <strong>" + HttpUtility.HtmlEncode(nombreEntidad) + "</strong> solicita formalmente la elaboración y carga del <strong>Plan de Remediación SBR</strong> con las recomendaciones técnicas pertinentes para atender las oportunidades de mejora derivadas de su evaluación."
                    + "</p>"
                    + "<div style=\"background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 18px; margin-bottom: 20px;\">"
                    + "<table style=\"width: 100%; font-size: 13px; border-collapse: collapse;\">"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; width: 170px; border-bottom: 1px solid #e2e8f0;\">Entidad:</td><td style=\"padding: 8px 0; color: #0f172a; font-weight: 700; border-bottom: 1px solid #e2e8f0;\">" + HttpUtility.HtmlEncode(nombreEntidad) + " (C&oacute;digo: " + HttpUtility.HtmlEncode(id) + ")</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; border-bottom: 1px solid #e2e8f0;\">Solicitado por:</td><td style=\"padding: 8px 0; color: #0f172a; font-weight: 700; border-bottom: 1px solid #e2e8f0;\">" + HttpUtility.HtmlEncode(userName) + " (" + HttpUtility.HtmlEncode(userEmail) + ")</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; border-bottom: 1px solid #e2e8f0;\">Contacto(s) Entidad:</td><td style=\"padding: 8px 0; color: #0f172a; border-bottom: 1px solid #e2e8f0;\">" + HttpUtility.HtmlEncode(contactoEntidad) + "</td></tr>"
                    + "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; border-bottom: 1px solid #e2e8f0;\">Fecha y Hora:</td><td style=\"padding: 8px 0; color: #0f172a; border-bottom: 1px solid #e2e8f0;\">" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>";

                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    cuerpo += "<tr><td style=\"padding: 8px 0; color: #64748b; font-weight: 600; vertical-align: top;\">Observaciones:</td><td style=\"padding: 8px 0; color: #0f172a;\">" + HttpUtility.HtmlEncode(mensaje).Replace("\n", "<br/>") + "</td></tr>";
                }

                cuerpo += "</table>"
                    + "</div>"
                    + "<p style=\"font-size: 13px; color: #64748b; line-height: 1.5; margin: 0 0 10px 0;\">"
                    + "Una vez elaborado el documento, el equipo del FFC podrá cargarlo en el Sistema SAC a través del módulo de Administración de Planes de Remediación para que la entidad pueda visualizarlo y descargarlo en su panel de autoevaluación."
                    + "</p>"
                    + "</div>"
                    + "<div style=\"background: #f1f5f9; padding: 14px 28px; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0; text-align: center;\">"
                    + "Fondo de Fortalecimiento Cooperativo &bull; Plataforma SAC en Línea"
                    + "</div>"
                    + "</div>";

                MailSend.Email.EnviarCorreoImagenes(asunto, cuerpo, destinatarios, servidorCorreo, cuentaCorreo, cuentaCorreo, passwordCorreo);

                return Json(new { success = true, message = "La solicitud ha sido enviada exitosamente al FFC y a los contactos registrados de la entidad." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ocurrió un error al enviar la solicitud: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult AdminPlanRemediacion()
        {
            if (!IsCurrentUserAdmin)
            {
                return RedirectToAction("Evaluacion");
            }

            Load();
            ViewBag.EsAdmin = true;

            var listaEntidades = entidadService.GetAll()
                .Where(e => e.Id != Utility.Utilitarios.entidadAdministradora && e.Activo == true)
                .OrderBy(e => e.Nombre)
                .ToList();

            string dir = GetPlanRemediacionDir();
            var viewModels = new List<AdminPlanRemediacionViewModel>();

            foreach (var entidad in listaEntidades)
            {
                string pdfPath = Path.Combine(dir, "Plan_Remediacion_" + entidad.Id + ".pdf");
                bool existe = System.IO.File.Exists(pdfPath);

                var vm = new AdminPlanRemediacionViewModel
                {
                    IdEntidad = entidad.Id,
                    NombreEntidad = entidad.Nombre,
                    ContactoEntidad = entidad.Contacto,
                    TienePlan = existe
                };

                if (existe)
                {
                    var meta = ObtenerMetadataPlan(entidad.Id);
                    var fi = new FileInfo(pdfPath);
                    vm.NombreArchivoOriginal = meta?.NombreArchivoOriginal ?? Path.GetFileName(pdfPath);
                    vm.FechaCarga = meta?.FechaCarga ?? fi.LastWriteTime;
                    vm.UsuarioCarga = meta?.UsuarioCarga ?? "FFC";
                    vm.TamanioBytes = fi.Length;
                    vm.TamanioFormato = FormatearTamanio(fi.Length);
                    vm.Notas = meta?.Notas ?? "";
                }

                viewModels.Add(vm);
            }

            return View(viewModels);
        }

        [HttpPost]
        public ActionResult GuardarPlanRemediacion(string idEntidad, HttpPostedFileBase archivoPlan, string notas)
        {
            if (!IsCurrentUserAdmin)
            {
                return Json(new { success = false, message = "No tiene permisos para cargar planes de remediación." });
            }

            if (string.IsNullOrEmpty(idEntidad))
            {
                return Json(new { success = false, message = "Debe especificar la entidad." });
            }

            if (archivoPlan == null || archivoPlan.ContentLength == 0)
            {
                return Json(new { success = false, message = "Debe seleccionar un archivo PDF válido." });
            }

            string extension = Path.GetExtension(archivoPlan.FileName)?.ToLower();
            if (extension != ".pdf")
            {
                return Json(new { success = false, message = "El archivo debe tener formato PDF (.pdf)." });
            }

            try
            {
                string dir = GetPlanRemediacionDir();
                string pdfPath = Path.Combine(dir, "Plan_Remediacion_" + idEntidad + ".pdf");
                string jsonPath = Path.Combine(dir, "Plan_Remediacion_" + idEntidad + ".json");

                archivoPlan.SaveAs(pdfPath);

                string nombreEntidad = "";
                try
                {
                    var entObj = entidadService.Get(idEntidad);
                    if (entObj != null) nombreEntidad = entObj.Nombre;
                }
                catch { }

                string usuarioCarga = Env.GetUserInfo("name");
                if (string.IsNullOrEmpty(usuarioCarga)) usuarioCarga = Env.GetUserInfo("username");

                var meta = new PlanRemediacionMetadata
                {
                    IdEntidad = idEntidad,
                    NombreEntidad = nombreEntidad,
                    NombreArchivoOriginal = Path.GetFileName(archivoPlan.FileName),
                    FechaCarga = DateTime.Now,
                    UsuarioCarga = usuarioCarga,
                    TamanioBytes = archivoPlan.ContentLength,
                    Notas = notas ?? ""
                };

                System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(meta, Formatting.Indented));

                return Json(new { success = true, message = "Plan de remediación guardado correctamente para " + nombreEntidad });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar el plan de remediación: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarPlanRemediacion(string idEntidad)
        {
            if (!IsCurrentUserAdmin)
            {
                return Json(new { success = false, message = "No tiene permisos para eliminar planes de remediación." });
            }

            try
            {
                string dir = GetPlanRemediacionDir();
                string pdfPath = Path.Combine(dir, "Plan_Remediacion_" + idEntidad + ".pdf");
                string jsonPath = Path.Combine(dir, "Plan_Remediacion_" + idEntidad + ".json");

                if (System.IO.File.Exists(pdfPath)) System.IO.File.Delete(pdfPath);
                if (System.IO.File.Exists(jsonPath)) System.IO.File.Delete(jsonPath);

                return Json(new { success = true, message = "Plan de remediación eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el plan: " + ex.Message });
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                categoriaService.SafeClose();
                subCategoriaService.SafeClose();
                preguntaService.SafeClose();
                spService.SafeClose();
                usuarioService.SafeClose();
                responsableService.SafeClose();
                respuestaService.SafeClose();
                entidadService.SafeClose();
            }
            base.Dispose(disposing);
        }
    }
}