using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FGA.Model;
using FGA.Models;

namespace FGA.Controllers
{
    public class ExplorerController : BaseController
    {
        public static string xml = System.Configuration.ConfigurationManager.AppSettings["RutaUploads"];
        public static string informe = System.Configuration.ConfigurationManager.AppSettings["RutaInformes"];

        public ActionResult Index()
        {
            ViewData["Parent"] = xml;
            Session["FullAccess"] = true;
            IList<FileModel> files = FileModel.GetFiles(ViewData["Parent"].ToString());
            return View(files);
        }

        public ActionResult Consulta()
        {
            Load();
            LoadPeriodos();
            return View();
        }

        private void LoadPeriodos()
        {
            List<String> lst_Periodos = new List<string>();

            try
            {
                var lista = inf.ListaPeriodos(Session["IdEntidad"].ToString());
                for (int i = 0; i < lista.Count(); i++)
                {
                    lst_Periodos.Add(lista[i].Year.ToString());
                }
            }
            catch (Exception ex)
            {
            }

            ViewBag.Periodos = new SelectList(lst_Periodos, lst_Periodos[0].ToString());
        }

        public ActionResult Informe()
        {
            Load();
            var lista = ti.GetAll().Where(o => o.Estado == true);
            ViewBag.TipoInforme = new SelectList(lista, "Id", "Nombre");

            string id = Env.GetUserInfo("userid").ToString();
            Usuario ObjUser = usr.Get(id);
            string path = Path.Combine(informe, ObjUser.Entidad_Usuario.Nombre + "/Informe");
            Session["FileName"] = string.Empty;
            Session["FullAccess"] = false;
            ViewData["Parent"] = path;
            IList<FileModel> files = FileModel.GetFiles(ViewData["Parent"].ToString());
            return View(files);
        }

        public ActionResult Notificacion()
        {
            List<Entidad> lista = ent.GetAll().ToList();
            lista = lista.Where(o => o.Activo == true).OrderBy(o => o.Nombre).ToList();
           
            Entidad todas = new Entidad();
            todas.Id = "-1";
            todas.Nombre = "TODAS";
            lista.Add(todas);

            ViewBag.Entidades = new SelectList(lista, "Id", "Nombre", "-1");
            return View();
        }

        public ActionResult BuscarInformes(String Entidades, int Periodos)
        {
            Session["IdEntidad"] = Entidades;
            Load();
            LoadPeriodos();
            Session["Periodo"] = new DateTime(Periodos, 1, 1);
            return View("Consulta");
        }

        public ActionResult Requisites()
        {
            string id = Env.GetUserInfo("userid").ToString();
            Usuario ObjUser = usr.Get(id);
            string path = Path.Combine(informe, ObjUser.Entidad_Usuario.Nombre + "/Requisitos");

            Session["FullAccess"] = false;
            ViewData["Parent"] = path;
            IList<FileModel> files = FileModel.GetFiles(ViewData["Parent"].ToString());
            return View(files);
        }

        public ActionResult LoadActionLinks(string path, List<string> list)
        {
            ViewData["CheckedList"] = list;
            return PartialView("ActionLinks", path);
        }

        public ActionResult GetFiles(string path)
        {
            IList<FileModel> files = FileModel.GetFiles(path);
            ViewData["Parent"] = path;
            return PartialView("FileList", files);
        }

        public ActionResult GetInformes()
        {
            try
            {
                var tak = inf.GetInformes(Session["IdEntidad"].ToString(), Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString()));
                var result = from c in tak
                             select new string[] { "<img width=\"24\" height=\"24\" alt=\"Pdf\" src=\"/ffc/Content/Images/Pdf.png\" style=\"vertical-align: middle;\"> " +
                                                  "Reporte " + c.IdInforme.Nombre.ToLower(),
                                                  c.Periodo.ToShortDateString(),
                                                 "<div class=\"btn btn-default btn-file\"><i class=\"fa fa-paperclip\"></i><a data-toggle=\"tooltip\" target=\"_blank\" data-placement=\"top\" title=\"Consultar\" href=\"" + Env.GetSiteRoot() + "/uploads/FFC/informe/" + c.Archivo + "\"> Ver adjuntos</a></div>",
                                                 Session["IsFGA"].ToString() == "1" ?  "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Eliminar\"  href=\"javascript:deleteFile('" + c.Id.ToString() + "')\"><i class=\"btn btn-xs btn-danger icon fa fa-remove\"></i></a>" : ""
                             };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return Json(new { aaData = new[] { "", "", "" } }, JsonRequestBehavior.AllowGet); ;
        }

        public async Task<ActionResult> Download(string jlist)
        {
            try
            {
                if (!string.IsNullOrEmpty(jlist))
                    return await Task.Run(() => GenerateZip(jlist));
            }
            catch (Exception)
            {
            }

            return View("Requisites");
        }

        private ZipResult GenerateZip(string jlist)
        {
            jlist = jlist.Replace("$", "'");
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<string> list = serializer.Deserialize<List<string>>(jlist);
            ZipResult result = new ZipResult();
            foreach (string path in list)
            {
                result.AddFile(FileModel.Decode(path));
            }

            return result;
        }

        public ActionResult GoUpper(string path)
        {
            IList<FileModel> files = new List<FileModel>();

            string filePath = FileModel.Decode(path);
            if (filePath == xml)
                files = FileModel.GetFiles(xml);
            else if (Directory.Exists(filePath))
            {
                DirectoryInfo di = new DirectoryInfo(filePath);
                if (di.Parent != null)
                {
                    files = FileModel.GetFiles(di.Parent.FullName);
                    if (di.Parent.Parent != null)
                        ViewData["Parent"] = FileModel.Encode(di.Parent.FullName);
                    else
                        ViewData["Parent"] = "root";
                }
                else
                {
                    ViewData["Parent"] = xml;
                    files = FileModel.GetFiles(xml);
                }
            }
;
            return PartialView("FileList", files);
        }

        [HttpPost]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files)
        {
            string id = Env.GetUserInfo("userid").ToString();
            Usuario ObjUser = usr.Get(id);
            string path = Path.Combine(informe, ObjUser.Entidad_Usuario.Nombre);
            bool exists = Directory.Exists(path);

            if (!exists)
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "Requisitos");
            exists = Directory.Exists(path);

            if (!exists)
                Directory.CreateDirectory(path);

            foreach (var file in files)
                file.SaveAs(Path.Combine(path, file.FileName));

            ViewData["Parent"] = path;
            var listParam = param.GetAll().ToList();
            var ServidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
            var CuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
            var PasswordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();
            var Destinatarios = listParam.Where(o => o.Llave == Utility.Utilitarios.Correo_Error_XML).Select(o => o.Valor).FirstOrDefault();
            MailSend.Email.EnviarCorreoImagenes("La entidad " + ObjUser.Entidad_Usuario.Nombre + " ha cargado un archivo", "<br /><br /> El usuario " + ObjUser.Nombre + " ha cargado el archivos en el sistema.", CuentaCorreo, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);

            return PartialView("FileList", FileModel.GetFiles(path));
        }

        [HttpPost]
        public ActionResult UploadInforme(IEnumerable<HttpPostedFileBase> files)
        {
            string id = Env.GetUserInfo("userid").ToString();
            Usuario ObjUser = usr.Get(id);
            string path = Path.Combine(informe, ObjUser.Entidad_Usuario.Nombre);
            bool exists = Directory.Exists(path);

            if (!exists)
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "Informe");
            exists = Directory.Exists(path);

            if (!exists)
                Directory.CreateDirectory(path);

            Session["FileName"] = (Guid.NewGuid().ToString()) + ".pdf";
            Session["InformeActual"] = Path.Combine(path, Session["FileName"].ToString());
            foreach (var file in files)
                file.SaveAs(Session["InformeActual"].ToString());

            ViewData["Parent"] = path;
            return PartialView("_VerInforme", Session["FileName"].ToString());
        }

        public void DeleteFile(string Id)
        {
            inf.Delete(Id);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult EnviarNotificacion(string idEntidad, string asunto, string texto, string destinatarios)
        {
            Resultado result = new Resultado();
            Entidad ObjEntidad = ent.Get(idEntidad);
           
            try
            {
                Notificaciones reg = new Notificaciones();
                reg.IdEntidadId = Utility.Utilitarios.entidadAdministradora;
                reg.IdUsuarioId = int.Parse(Env.GetUserInfo("userid"));
                reg.Texto = Server.HtmlDecode(texto);
                reg.Asunto = Server.HtmlDecode(asunto);
                reg.Destinatarios = destinatarios;
                reg.FechaCarga = DateTime.Now;
                reg.Enviado = false;
                not.Add(ref reg);

                result.exito = true;
                result.mensaje = "Mensaje enviado # " + reg.Id.ToString();
            }
            catch (Exception ex)
            {
                result.exito = false;
                result.mensaje = "No se ha podido enviar el mensaje: " + ex.Message;
            }

            return Json(new { resultado = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GuardarInforme(string idEntidad, int idInforme, int mes, int anno, string texto, string destinatarios)
        {
            Resultado result = new Resultado();
            Entidad ObjEntidad = ent.Get(idEntidad);
            TipoInforme ObjInforme = ti.Get(idInforme.ToString());

            try
            {
                InformeMail reg = new InformeMail();
                reg.IdEntidad_Id = idEntidad;
                reg.IdInforme_Id = idInforme;
                reg.IdUsuario_Id = int.Parse(Env.GetUserInfo("userid"));
                reg.Periodo = new DateTime(anno, mes, 1);
                reg.Texto = Server.HtmlDecode(texto);
                reg.Archivo = Session["FileName"].ToString();
                reg.Destinatarios = destinatarios;
                reg.FechaCarga = DateTime.Now;
                reg.Enviado = false;
                inf.Add(ref reg);

                var path = Path.Combine(informe, ObjEntidad.Nombre);
                bool exists = Directory.Exists(path);

                if (!exists)
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, "Informes");
                exists = Directory.Exists(path);

                if (!exists)
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, anno.ToString());
                exists = Directory.Exists(path);

                if (!exists)
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, reg.Periodo.ToString("MMMM").ToUpper());
                exists = Directory.Exists(path);

                if (!exists)
                    Directory.CreateDirectory(path);
                path += "\\" + ObjInforme.Nombre + ".pdf";

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                System.IO.File.Copy(Session["InformeActual"].ToString(), path);

                var listParam = param.GetAll().ToList();
                var ServidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
                var CuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
                var PasswordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();
                var CorreoNotificacion = listParam.Where(o => o.Llave == Utility.Utilitarios.Notificacion_Carga_Informes).Select(o => o.Valor).FirstOrDefault();
                var DebeNotificar = listParam.Where(o => o.Llave == Utility.Utilitarios.Notificar_Entidades_Carga).Select(o => o.Valor).FirstOrDefault();

                if (DebeNotificar.Equals(Utility.Utilitarios.Si))
                {
                    MailSend.Email.EnviarCorreoAduntos("Informe " + ObjInforme.Nombre.ToLower() + " - " + reg.Periodo.ToString("MMMM").ToUpper() + " " + anno.ToString(),
                    texto, destinatarios, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo, path);
                }

                result.exito = true;
                result.mensaje = "Mensaje enviado # " + reg.Id.ToString();
            }
            catch (Exception ex)
            {
                result.exito = false;
                result.mensaje = "No se ha podido enviar el mensaje: " + ex.Message;
            }

            return Json(new { resultado = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GoTop()
        {
            ViewData["Parent"] = xml;
            IList<FileModel> files = FileModel.GetFiles(xml);
            return PartialView("FileList", files);
        }

        public PartialViewResult GetFrom(string IdEntidad)
        {
            var correos = ent.GetMails(IdEntidad);
            var destinatarios = string.Empty;

            foreach (string detalle in correos) {
                var registros = detalle is null ? new string[0] : detalle.Split(';');
                foreach(string info in registros)
                    if (!string.IsNullOrEmpty(info) && !destinatarios.Contains(info))
                        destinatarios += info + ";";
            }

            return PartialView("_ListaDestinatarios", correos is null ? new string[0] : destinatarios.Split(';'));
        }

        public PartialViewResult GetTxtMail(string IdInforme)
        {
            var texto = ti.Get(IdInforme).Texto;
            return PartialView("_Mail", texto);
        }

        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.TipoInformeService.ServiceOf_TipoInformeClient ti = new FGA_En_Linea.TipoInformeService.ServiceOf_TipoInformeClient();
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.InformeMailService.InformeMailServiceClient inf = new FGA_En_Linea.InformeMailService.InformeMailServiceClient();
        private readonly FGA_En_Linea.NotificacionesService.NotificacionesServiceClient not = new FGA_En_Linea.NotificacionesService.NotificacionesServiceClient();
        private readonly FGA_En_Linea.ParametrosService.ParametrosServiceClient param = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                usr.Close();
                ti.Close();
                ent.Close();
                inf.Close();
                param.Close();
            }
            base.Dispose(disposing);
        }
    }
}