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
    }
}