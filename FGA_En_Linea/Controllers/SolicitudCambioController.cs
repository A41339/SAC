using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FGA.Controllers
{
    public class SolicitudCambioController : BaseController
    {
        public static string root = System.Configuration.ConfigurationManager.AppSettings["RutaUploads"];

        public ActionResult Index()
        {
            Session["Periodo1"] = DateTime.Now.ToShortDateString();
            Session["Periodo2"] = DateTime.Now.ToShortDateString();
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = sol.GetAll();
                var result = from c in tak
                             select new string[] { "RFC-FGA-" + c.Id.ToString().PadLeft(8, '0'),
            Convert.ToString(c.IdSolicitante.Nombre),
            Convert.ToString(c.IdResponsableActual.Nombre),
            Convert.ToString(Estado.GetNombre(c.IndEstado.Value)),
            Convert.ToString(c.FechaRegistro.Value.ToShortDateString()),
            Convert.ToString(c.Descripcion),
            Convert.ToString(c.IndEmergencia == true ? "Sí" : "No"),
            Utility.Utilitarios.GetToolBar(c.IdResponsableActual_Id.Value, c.IndEstado.Value, c.Id, int.Parse(Env.GetUserInfo("userid")), Url.Content("~/SolicitudCambio"),
            c.IdAprobador_Id.Value, c.IndMejoraEstetica)};

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        //public async Task<ActionResult> Download(string id)
        //{
        //    return await Task.Run(() => GenerateZip(id));
        //}

        //private ZipResult GenerateZip(string id)
        //{
        //    string idUsr = Env.GetUserInfo("userid").ToString();
        //    Usuario ObjUser = usr.Get(idUsr);
        //    SolicitudCambio ObjSol = sol.Get(id);
        //    string path = Path.Combine(root, ObjUser.Entidad_Usuario.Nombre + "\\" + "Requerimientos\\" + ObjSol.IdProyecto.Nombre + "\\" + ObjSol.Id.ToString());

        //    ZipResult result = new ZipResult();
        //    result.AddFile(FileModel.Decode(path));

        //    return result;
        //}

        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SolicitudCambio ObjSol = sol.Get(id);
            Usuario ObjUser = usr.Get(ObjSol.IdResponsableActual_Id.Value.ToString());
            SolicitudView view = new SolicitudView();
            view.solicitud = ObjSol;
            string path = Path.Combine(root, ObjUser.Entidad_Usuario.Nombre + "\\" + "Requerimientos\\" + ObjSol.IdProyecto.Nombre + "\\" + ObjSol.Id.ToString());

            IList<FileModel> files = FileModel.GetFiles(path);
            IList<Tarea> tareas = tar.GetBySolicitud(int.Parse(id), -1);
            view.files = files;
            view.tareas = tareas;

            if (ObjSol == null)
                return HttpNotFound();

            return View(view);
        }

        public ActionResult Create()
        {
            SolicitudCambio_Insert sol = new SolicitudCambio_Insert();
            ViewBag.IdProyecto_Id = new SelectList(proy.GetAll(), "Id", "Nombre").OrderBy(o => o.Text);
            ViewBag.IdSolicitante_Id = new SelectList(usr.GetAll().Where(o => o.Entidad_Usuario_Id == Utility.Utilitarios.entidadAdministradora && o.Estado_Usuario_Id == Utility.Utilitarios.estadoActivo), "Id", "Nombre", Env.GetUserInfo("userid")).OrderBy(o => o.Text);
            ViewBag.IdAprobador_Id = new SelectList(usr.GetAll().Where(o => o.Entidad_Usuario_Id == Utility.Utilitarios.entidadAdministradora && o.Estado_Usuario_Id == Utility.Utilitarios.estadoActivo), "Id", "Nombre").OrderBy(o => o.Text);
            ViewBag.SeccionesImplicadas = new SelectList(men.GetAll(), "Id", "MenuText").OrderBy(o => o.Text);
            return View(sol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection frm, List<HttpPostedFileBase> files)
        {
            SolicitudCambio ObjSol = new SolicitudCambio();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                ObjSol.IdSolicitante_Id = frm.GetValue("IdSolicitante_Id") is null ? (int?)null : int.Parse(frm.GetValue("IdSolicitante_Id").AttemptedValue.ToString());
                ObjSol.IndEmergencia = frm.GetValue("IndEmergencia") is null ? false : true;
                ObjSol.IndEstandar = frm.GetValue("IndEstandar") is null ? false : true;
                ObjSol.IndNormal = frm.GetValue("IndNormal") is null ? false : true;
                ObjSol.IndComplejo = frm.GetValue("IndComplejo") is null ? false : true;
                ObjSol.IdProyecto_Id = "FGA-02"; //frm.GetValue("IdProyecto_Id") is null ? string.Empty : frm.GetValue("IdProyecto_Id").AttemptedValue.ToString();
                ObjSol.Descripcion = frm.GetValue("sol.Descripcion") is null ? string.Empty : frm.GetValue("sol.Descripcion").AttemptedValue.ToString();
                ObjSol.IdUrgencia = frm.GetValue("IdUrgencia") is null ? (int?)null : int.Parse(frm.GetValue("IdUrgencia").AttemptedValue.ToString());
                ObjSol.IdImpacto = frm.GetValue("IdPrioridad") is null ? (int?)null : int.Parse(frm.GetValue("IdPrioridad").AttemptedValue.ToString());
                ObjSol.IdEnterno = frm.GetValue("IdEnterno") is null ? (int?)null : int.Parse(frm.GetValue("IdEnterno").AttemptedValue.ToString());
                ObjSol.SeccionesImplicadas = frm.GetValue("SeccionesImplicadas") is null ? (int?)null : int.Parse(frm.GetValue("SeccionesImplicadas").AttemptedValue.ToString());
                ObjSol.FechaLimite = string.IsNullOrEmpty(frm.GetValue("FechaLimite").AttemptedValue) ? (DateTime?)null : DateTime.Parse(frm.GetValue("FechaLimite").AttemptedValue.ToString());
                ObjSol.IdUsuariosAfectados = frm.GetValue("IdUsuariosAfectados") is null ? (int?)null : int.Parse(frm.GetValue("IdUsuariosAfectados").AttemptedValue.ToString());
                ObjSol.MotivoCambio = frm.GetValue("sol.MotivoCambio") is null ? string.Empty : frm.GetValue("sol.MotivoCambio").AttemptedValue.ToString();
                ObjSol.ObjetivoCambio = frm.GetValue("sol.ObjetivoCambio") is null ? string.Empty : frm.GetValue("sol.ObjetivoCambio").AttemptedValue.ToString();
                ObjSol.DetalleImpacto = frm.GetValue("DetalleImpacto") is null ? string.Empty : frm.GetValue("DetalleImpacto").AttemptedValue.ToString();
                ObjSol.RiesgoCambio = frm.GetValue("RiesgoCambio") is null ? string.Empty : frm.GetValue("RiesgoCambio").AttemptedValue.ToString();
                ObjSol.FechaInicioPrev = string.IsNullOrEmpty(frm.GetValue("FechaInicioPrev").AttemptedValue) ? (DateTime?)null : DateTime.Parse(frm.GetValue("FechaInicioPrev").AttemptedValue.ToString());
                ObjSol.FechaFinPrev = string.IsNullOrEmpty(frm.GetValue("FechaFinPrev").AttemptedValue) ? (DateTime?)null : DateTime.Parse(frm.GetValue("FechaFinPrev").AttemptedValue.ToString());
                ObjSol.IndNuevaFunc = frm.GetValue("IndNuevaFunc") is null ? false : true;
                ObjSol.IndNuevaVersion = frm.GetValue("IndNuevaVersion") is null ? false : true;
                ObjSol.IndAfectaServicio = frm.GetValue("IndAfectaServicio") is null ? false : true;
                ObjSol.IndMejoraEstetica = frm.GetValue("IndMejoraEstetica") is null ? false : true;
                ObjSol.IndPaseEntorno = frm.GetValue("IndPaseEntorno") is null ? false : true;
                ObjSol.IndMantenimiento = frm.GetValue("IndMantenimiento") is null ? false : true;
                ObjSol.IndMejoraServ = frm.GetValue("IndMejoraServ") is null ? false : true;
                ObjSol.IndNuevoServ = frm.GetValue("IndNuevoServ") is null ? false : true;
                ObjSol.IndMantAdaptativo = frm.GetValue("IndMantAdaptativo") is null ? false : true;
                ObjSol.IndMantCorrectivo = frm.GetValue("IndMantCorrectivo") is null ? false : true;

                if (ModelState.IsValid)
                {
                    Proyecto ObjProy = proy.Get(ObjSol.IdProyecto_Id);
                    ObjSol.IdResponsableActual_Id = ObjProy.IdAdmin_Id;
                    ObjSol.IdAprobador_Id = ObjProy.IdAdmin_Id;
                    ObjSol.IndEstado = (int)Enum.Enum_EstadoSolicitud.Registrado;  //Registrado
                    ObjSol.FechaRegistro = DateTime.Now;
                    sol.Add(ref ObjSol);

                    string id = Env.GetUserInfo("userid").ToString();
                    Usuario ObjUser = usr.Get(id);

                    var path = Path.Combine(Server.MapPath("~/Uploads"), ObjUser.Entidad_Usuario.Nombre);
                    bool exists = Directory.Exists(path);

                    if (!exists)
                        Directory.CreateDirectory(path);

                    path = Path.Combine(path, "Requerimientos");
                    exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    path = Path.Combine(path, ObjProy.Nombre.ToString());
                    exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    path = Path.Combine(path, ObjSol.Id.ToString());
                    exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null)
                        {
                            var InputFileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                            var ServerSavePath = Path.Combine(path, InputFileName);
                            file.SaveAs(ServerSavePath);
                        }
                    }

                    ObjSol = sol.Get(ObjSol.Id.ToString());
                    CreatedPdf(ObjSol, ObjProy.Nombre);
                    path = Path.Combine(root, ObjUser.Entidad_Usuario.Nombre + "\\" + "Requerimientos\\" + ObjProy.Nombre + "\\" + ObjSol.Id.ToString() + "\\Requerimiento.pdf");
                    EnviarCorreo(path, ObjSol, ObjProy);
                    sb.Append("Sumitted");
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

            //return RedirectToAction("Index");
        }

        public void Aprobar(string id)
        {
            SolicitudCambio ObjSol = sol.Get(id);
            if (ObjSol.IdResponsableActual_Id.Value.ToString() == Env.GetUserInfo("userid"))
            {
                Usuario ObjUser = usr.Get(Env.GetUserInfo("userid").ToString());
                String path = Path.Combine(root, ObjUser.Entidad_Usuario.Nombre + "\\" + "Requerimientos\\" + ObjSol.IdProyecto.Nombre + "\\" + ObjSol.Id.ToString() + "\\Requerimiento.pdf");

                Proyecto ObjProy = proy.Get(ObjSol.IdProyecto_Id);
                ObjSol.IdResponsableActual_Id = ObjSol.IndEstado == (int)Enum.Enum_EstadoSolicitud.Desarrollo ? ObjSol.IdProyecto.IdLider_Id : ObjSol.IdProyecto.IdDesarrollador_Id;
                ObjSol.IndEstado = ObjSol.IndEstado != (int)Enum.Enum_EstadoSolicitud.Rechazado ? ObjSol.IndEstado + 1 : (int)Enum.Enum_EstadoSolicitud.Desarrollo;

                EnviarCorreo(path, ObjSol, ObjProy);
                ObjSol.IdAprobador = null;
                ObjSol.IdResponsableActual = null;
                ObjSol.IdProyecto = null;
                ObjSol.IdSolicitante = null;
                ObjSol.Seccion = null;
                sol.Update(ObjSol);
            }
        }

        public void Rechazar(string id, string motivo)
        {
            SolicitudCambio ObjSol = sol.Get(id);
            Usuario ObjUser = usr.Get(Env.GetUserInfo("userid").ToString());
            String path = Path.Combine(root, ObjUser.Entidad_Usuario.Nombre + "\\" + "Requerimientos\\" + ObjSol.IdProyecto.Nombre + "\\" + ObjSol.Id.ToString() + "\\Requerimiento.pdf");
            Proyecto ObjProy = proy.Get(ObjSol.IdProyecto_Id);

            ObjSol.IdResponsableActual_Id = ObjSol.IndEstado == (int)Enum.Enum_EstadoSolicitud.Pruebas ? ObjSol.IdProyecto.IdDesarrollador_Id : ObjSol.IdProyecto.IdLider_Id;
            ObjSol.MotivoRechazo = motivo;
            ObjSol.FechaRechazo = DateTime.Now;
            ObjSol.IndEstado = ObjSol.IndEstado == (int)Enum.Enum_EstadoSolicitud.Pruebas ? ObjSol.IndEstado - 1 : (int)FGA.Enum.Enum_EstadoSolicitud.Rechazado;
            ObjSol.IdAprobador = null;
            ObjSol.IdResponsableActual = null;
            ObjSol.IdProyecto = null;
            ObjSol.IdSolicitante = null;
            ObjSol.Seccion = null;
            sol.Update(ObjSol);

            EnviarCorreo(path, ObjSol, ObjProy);

        }

        public void AddTask(string idSolicitud, string detalle, DateTime? fechaIni, DateTime? fechaFin)
        {
            SolicitudCambio ObjSol = sol.Get(idSolicitud);
            if (ObjSol.IndEstado == (int)Enum.Enum_EstadoSolicitud.Desarrollo)
            {
                ObjSol.FechaDesarrolloInicio = fechaIni;
                ObjSol.FechaDesarrolloFin = fechaFin;
            }
            else if (ObjSol.IndEstado == (int)Enum.Enum_EstadoSolicitud.Pruebas)
            {
                ObjSol.FechaPruebaInicio = fechaIni;
                ObjSol.FechaPruebaFin = fechaFin;
            }
            else
            {
                ObjSol.FechaPaseInicio = fechaIni;
                ObjSol.FechaPaseFin = fechaFin;
            }

            ObjSol.IdAprobador = null;
            ObjSol.IdResponsableActual = null;
            ObjSol.IdProyecto = null;
            ObjSol.IdSolicitante = null;
            ObjSol.Seccion = null;
            sol.Update(ObjSol);

            Tarea ObjT = new Tarea();
            ObjT.IdSolicitudCambio_Id = int.Parse(idSolicitud);
            ObjT.IdTipoTarea_Id = ObjSol.IndEstado.Value;
            ObjT.Detalle = detalle;
            tar.Add(ref ObjT);
        }

        public void DeleteTask(string idTarea)
        {
            tar.Delete(idTarea);
        }

        [AllowAnonymous]
        public PartialViewResult VerDetalle(string id)
        {
            SolicitudCambio ObjSol = sol.Get(id);
            Usuario ObjUser = usr.Get(ObjSol.IdResponsableActual_Id.Value.ToString());
            SolicitudView view = new SolicitudView();

            view.solicitud = ObjSol;
            string path = Path.Combine(root, ObjUser.Entidad_Usuario.Nombre + "\\" + "Requerimientos\\" + ObjSol.IdProyecto.Nombre + "\\" + ObjSol.Id.ToString());

            IList<FileModel> files = FileModel.GetFiles(path);
            IList<Tarea> tareas = tar.GetBySolicitud(int.Parse(id), -1);
            view.files = files;
            view.tareas = tareas;

            return PartialView("_VerDetalle", view);
        }

        public PartialViewResult VerTareas(string idSolicitud)
        {
            SolicitudCambio ObjSol = sol.Get(idSolicitud);

            try
            {
                if (ObjSol.IndEstado == (int)Enum.Enum_EstadoSolicitud.Desarrollo)
                {
                    Session["Periodo1"] = ObjSol.FechaDesarrolloInicio.HasValue ? ObjSol.FechaDesarrolloInicio.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    Session["Periodo2"] = ObjSol.FechaDesarrolloFin.HasValue ? ObjSol.FechaDesarrolloFin.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                }
                else if (ObjSol.IndEstado == (int)Enum.Enum_EstadoSolicitud.Pruebas)
                {
                    Session["Periodo1"] = ObjSol.FechaPruebaInicio.HasValue ? ObjSol.FechaPruebaInicio.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    Session["Periodo2"] = ObjSol.FechaPruebaFin.HasValue ? ObjSol.FechaPruebaFin.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                }
                else
                {
                    Session["Periodo1"] = ObjSol.FechaPaseInicio.HasValue ? ObjSol.FechaPaseInicio.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    Session["Periodo2"] = ObjSol.FechaPaseFin.HasValue ? ObjSol.FechaPaseFin.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                }
            }
            catch (Exception)
            {
            }

            var listaTareas = tar.GetBySolicitud(int.Parse(idSolicitud), ObjSol.IndEstado.Value);
            return PartialView("_VerTareas", listaTareas);
        }

        private void EnviarCorreo(string path, SolicitudCambio ObjSol, Proyecto ObjProj)
        {
            string destinatarios = string.Empty;
            destinatarios += ObjProj.IdReponsableTI.Correo + ";";

            if (!destinatarios.Contains(ObjSol.IdAprobador.Correo))
                destinatarios += ObjSol.IdAprobador.Correo + ";";

            if (!destinatarios.Contains(ObjSol.IdResponsableActual.Correo))
                destinatarios += ObjSol.IdResponsableActual.Correo + ";";

            if (!destinatarios.Contains(ObjSol.IdSolicitante.Correo))
                destinatarios += ObjSol.IdSolicitante.Correo + ";";

            if (!destinatarios.Contains(ObjProj.IdDesarrollador.Correo))
                destinatarios += ObjProj.IdDesarrollador.Correo + ";";

            var listParam = param.GetAll().ToList();
            var ServidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
            var CuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
            var PasswordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();

            MailSend.Email.EnviarCorreoAduntos("Requerimiento #" + ObjSol.Id.ToString(), "<br /><br />  Se le informa que el requerimiento #" + ObjSol.Id.ToString() +
                                               " solicitado el día " + ObjSol.FechaRegistro.Value.ToShortDateString() + " por " + ObjSol.IdSolicitante.Nombre +
                                               " con el siguiente detalle: " + ObjSol.Descripcion + " ha pasado a estado <b>" + Estado.GetTooltip(ObjSol.IndEstado.Value) + ".</b>" +
                                               " Realice las validaciones en la siguiente dirección: " + "https://www.ffc.co.cr/FFC/solicitudcambio/", destinatarios,
                                               ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo, path);
        }

        public void CreatedPdf(SolicitudCambio ObjSol, String nombreProyecto)
        {
            string idUsr = Env.GetUserInfo("userid").ToString();
            Usuario ObjUser = usr.Get(idUsr);
            string path = Path.Combine(root, ObjUser.Entidad_Usuario.Nombre + "\\" + "Requerimientos\\" + nombreProyecto + "\\" + ObjSol.Id.ToString() + "\\Requerimiento.pdf");
            Document document = new Document();

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();

                Font font5 = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                PdfPTable table = new PdfPTable(2);
                float[] widths = new float[2];
                widths[0] = 2f;
                widths[1] = 6f;

                table.SetWidths(widths);
                table.WidthPercentage = 100;

                Image img = Image.GetInstance(MicrosoftHelper.MSHelper.GetSiteRoot() + "/Content/images/Logo_Login.png");
                img.ScaleToFit(80, 80);
                document.Add(img);
                document.Add(Chunk.NEWLINE);

                Paragraph para = new Paragraph("Datos del solicitante", new Font(Font.FontFamily.HELVETICA, 10));
                PdfPCell cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);

                table.AddCell(cell);
                table.AddCell(new Phrase("Id", font5));
                table.AddCell(new Phrase("RFC-FGA-" + ObjSol.Id.ToString().PadLeft(8, '0'), font5));

                table.AddCell(new Phrase("Fecha de registro", font5));
                table.AddCell(new Phrase(ObjSol.FechaRegistro.Value.ToLongDateString(), font5));

                table.AddCell(new Phrase("Nombre del solicitante", font5));
                table.AddCell(new Phrase(ObjSol.IdSolicitante.Nombre, font5));

                table.AddCell(new Phrase("Responsable actual", font5));
                table.AddCell(new Phrase(ObjSol.IdResponsableActual.Nombre, font5));

                table.AddCell(new Phrase("Urgente", font5));
                table.AddCell(new Phrase(ObjSol.IndEmergencia ? "Sí" : "No", font5));

                table.AddCell(new Phrase("Estándar", font5));
                table.AddCell(new Phrase(ObjSol.IndEstandar ? "Sí" : "No", font5));

                table.AddCell(new Phrase("Normal", font5));
                table.AddCell(new Phrase(ObjSol.IndNormal ? "Sí" : "No", font5));

                table.AddCell(new Phrase("Complejo", font5));
                table.AddCell(new Phrase(ObjSol.IndComplejo ? "Sí" : "No", font5));

                para = new Paragraph("", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                para = new Paragraph("Identificación del cambio", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                table.AddCell(new Phrase("Proyecto", font5));
                table.AddCell(new Phrase(ObjSol.IdProyecto.Nombre, font5));

                table.AddCell(new Phrase("Descripción del cambio", font5));
                table.AddCell(new Phrase(ObjSol.Descripcion, font5));

                table.AddCell(new Phrase("Impacto", font5));
                table.AddCell(new Phrase(ObjSol.IdImpacto == 1 ? "Baja" : ObjSol.IdImpacto == 2 ? "Media" : "Alta", font5));

                table.AddCell(new Phrase("Urgencia", font5));
                table.AddCell(new Phrase(ObjSol.IdUrgencia == 1 ? "Baja" : ObjSol.IdUrgencia == 2 ? "Media" : "Alta", font5));

                table.AddCell(new Phrase("Entorno", font5));
                table.AddCell(new Phrase(ObjSol.IdEnterno == 1 ? "Desarrollo" : "Producción", font5));

                table.AddCell(new Phrase("Servicio afectado", font5));
                table.AddCell(new Phrase(ObjSol.Seccion.MenuText, font5));

                string usuarios = string.Empty;
                if (ObjSol.IdUsuariosAfectados.Value == 0)
                    usuarios = "Ninguno";
                else if (ObjSol.IdUsuariosAfectados.Value == 1)
                    usuarios = "De 1 a 5";
                else if (ObjSol.IdUsuariosAfectados.Value == 2)
                    usuarios = "De 1 a 10";
                else if (ObjSol.IdUsuariosAfectados.Value == 3)
                    usuarios = "De 1 a 15";
                else if (ObjSol.IdUsuariosAfectados.Value == 4)
                    usuarios = "De 1 a 20";
                else if (ObjSol.IdUsuariosAfectados.Value == 5)
                    usuarios = "De 1 a 25";
                else if (ObjSol.IdUsuariosAfectados.Value == 6)
                    usuarios = "Todos";

                table.AddCell(new Phrase("Usuarios afectados", font5));
                table.AddCell(new Phrase(usuarios, font5));

                table.AddCell(new Phrase("Fecha límite", font5));
                table.AddCell(new Phrase(ObjSol.FechaLimite.HasValue ? ObjSol.FechaLimite.Value.ToShortDateString() : "No aplica", font5));

                para = new Paragraph("", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                para = new Paragraph("Detalle del cambio", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                table.AddCell(new Phrase("Motivo del cambio", font5));
                table.AddCell(new Phrase(ObjSol.MotivoCambio, font5));

                table.AddCell(new Phrase("Objetivo del cambio", font5));
                table.AddCell(new Phrase(ObjSol.ObjetivoCambio, font5));

                table.AddCell(new Phrase("Impacto", font5));
                table.AddCell(new Phrase(ObjSol.DetalleImpacto, font5));

                table.AddCell(new Phrase("Riesgo", font5));
                table.AddCell(new Phrase(ObjSol.RiesgoCambio, font5));

                table.AddCell(new Phrase("Inicio previsto", font5));
                table.AddCell(new Phrase(ObjSol.FechaInicioPrev.HasValue ? ObjSol.FechaInicioPrev.Value.ToShortDateString() : "No aplica", font5));

                table.AddCell(new Phrase("Finalización previsto", font5));
                table.AddCell(new Phrase(ObjSol.FechaFinPrev.HasValue ? ObjSol.FechaFinPrev.Value.ToShortDateString() : "No aplica", font5));

                para = new Paragraph("", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                para = new Paragraph("Clasificación del cambio", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                table.AddCell(new Phrase("Mantenimiento evolutivo", font5));
                table.AddCell(new Phrase("", font5));

                table.AddCell(new Phrase("Nueva funcionalidad", font5));
                table.AddCell(new Phrase(ObjSol.IndNuevaFunc ? "Sí" : "No", font5));

                table.AddCell(new Phrase("Nueva versión", font5));
                table.AddCell(new Phrase(ObjSol.IndNuevaVersion ? "Sí" : "No", font5));

                table.AddCell(new Phrase("Nuevo servicio", font5));
                table.AddCell(new Phrase(ObjSol.IndNuevoServ ? "Sí" : "No", font5));

                table.AddCell(new Phrase("Pase de entorno", font5));
                table.AddCell(new Phrase(ObjSol.IndPaseEntorno ? "Sí" : "No", font5));

                para = new Paragraph("", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                table.AddCell(new Phrase("Mantenimiento adaptativo", font5));
                table.AddCell(new Phrase(ObjSol.IndMantAdaptativo ? "Sí" : "No", font5));

                para = new Paragraph("", new Font(Font.FontFamily.HELVETICA, 10));
                cell = new PdfPCell();
                cell.Colspan = 2;
                cell.AddElement(para);
                table.AddCell(cell);

                table.AddCell(new Phrase("Mantenimiento correctivo", font5));
                table.AddCell(new Phrase(ObjSol.IndMantCorrectivo ? "Sí" : "No", font5));

                document.Add(table);
                document.Close();

            }
        }

        private readonly FGA_En_Linea.SolicitudCambioService.SolicitudCambioServiceClient sol = new FGA_En_Linea.SolicitudCambioService.SolicitudCambioServiceClient();
        private readonly FGA_En_Linea.TareaService.TareaServiceClient tar = new FGA_En_Linea.TareaService.TareaServiceClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.ProyectoService.ProyectoServiceClient proy = new FGA_En_Linea.ProyectoService.ProyectoServiceClient();
        private readonly FGA_En_Linea.MenuService.ServiceOf_MenuClient men = new FGA_En_Linea.MenuService.ServiceOf_MenuClient();
        private readonly FGA_En_Linea.ParametrosService.ParametrosServiceClient param = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sol.Close();
                tar.Close();
                usr.Close();
                proy.Close();
                men.Close();
                param.Close();
            }
            base.Dispose(disposing);
        }
    }
}