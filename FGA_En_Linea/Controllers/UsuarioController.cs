using System; 
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using DocumentFormat.OpenXml.Office2010.Excel;
using FGA.Models;

namespace FGA.Controllers
{
    public class UsuarioController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var id = int.Parse(Env.GetUserInfo("userid"));
                Usuario ObjUser = usr.Get(id.ToString());
                List<Usuario> tak = null;

                if (ObjUser.Entidad_Usuario.Id == Utility.Utilitarios.entidadAdministradora)
                    tak = usr.GetAll().Where(o => o.Id != id && o.Estado_Usuario_Id != Utility.Utilitarios.estadoBorrado).ToList();
                else
                    tak = usr.GetAll().Where(o=>o.Entidad_Usuario_Id == ObjUser.Entidad_Usuario_Id && o.Id != id && o.Estado_Usuario_Id != Utility.Utilitarios.estadoBorrado).ToList();

                var result = from c in tak
                             select new string[] { c.Id.ToString(),
                                                   Convert.ToString(c.Nombre),
                                                   Convert.ToString(c.Identificacion),
                                                   Convert.ToString(c.Correo),
                                                   Convert.ToString(c.Estado_Usuario.Nombre),
                                                   Convert.ToString(c.Role_Usuario.Nombre),
                                                   Convert.ToString(c.Entidad_Usuario.Nombre),
                                                   Convert.ToString(c.Sexo_Usuario.Nombre),
                                                   c.Telefono,
                                                   c.Puesto
                                                };
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

            Usuario ObjUser = usr.Get(id.ToString());

            if (ObjUser == null)
                return HttpNotFound();

            return View(ObjUser);
        }

        public ActionResult Create()
        {
            Load();
            var id = Env.GetUserInfo("userid");
            Usuario ObjUser = usr.Get(id);
            var roleMaestro = int.Parse(Utility.Utilitarios.roleMaestroEntidad);

            if (ObjUser.Entidad_Usuario.Id == Utility.Utilitarios.entidadAdministradora)
            {
                ViewBag.Roles = new SelectList(rs.GetAll(), "Id", "Nombre", ObjUser.Role_Usuario.Id).OrderBy(o => o.Text);
                ViewBag.Entidades = new SelectList(ent.GetAll(), "Id", "Nombre", Session["IdEntidad"]);
            }
            else
                ViewBag.Roles = new SelectList(rs.GetAll().Where(o => o.EsEntidad == true && o.Id != roleMaestro), "Id", "Nombre", ObjUser.Role_Usuario.Id).OrderBy(o => o.Text);

            ViewBag.Sexos = new SelectList(sx.GetAll(), "Id", "Nombre").OrderBy(o => o.Text);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection ObjUser)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Usuario usuario = new Usuario();
            Utility.PasswordGenerator generator = new Utility.PasswordGenerator();

            try
            {
                if (ModelState.IsValid)
                {                 
                    usuario.Correo = ObjUser.GetValue("Correo").AttemptedValue;
                    usuario.Identificacion = ObjUser.GetValue("Identificacion").AttemptedValue;
                    usuario.Nombre = ObjUser.GetValue("Nombre").AttemptedValue;
                    usuario.Telefono = ObjUser.GetValue("Telefono").AttemptedValue;
                    usuario.Puesto = ObjUser.GetValue("Puesto").AttemptedValue;

                    Usuario repetido = usr.GetByIden(usuario.Identificacion);
                    if (repetido != null)
                    {
                        sb.Append("Error: El usuario ya existe.");
                        return Content(sb.ToString());
                    }

                    usuario.Estado_Usuario_Id = Utility.Utilitarios.estadoActivo;
                    usuario.Role_Usuario_Id = int.Parse(ObjUser.GetValue("Roles").AttemptedValue);
                    usuario.Entidad_Usuario_Id = ObjUser.GetValue("Entidades").AttemptedValue;
                    usuario.Sexo_Usuario_Id = int.Parse(ObjUser.GetValue("Sexos").AttemptedValue);
                    usuario.Contrasena = generator.Generate();
                    usuario.Entidad_Usuario = null;
                    usuario.Estado_Usuario = null;
                    usuario.Role_Usuario = null;
                    usuario.Sexo_Usuario = null;
                    usuario.CambiarClave = Utility.Utilitarios.Si;
                    usr.Add(ref usuario);

                    var listParam = param.GetAll().ToList();
                    var ServidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
                    var CuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
                    var PasswordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();
                    var CorreoNotificacion = listParam.Where(o => o.Llave == Utility.Utilitarios.Notificacion_Creacion_Usuarios).Select(o => o.Valor).FirstOrDefault();
                    var Entidad = ent.Get(ObjUser.GetValue("Entidades").AttemptedValue);
                    Encripcion.Encripcion enc = new Encripcion.Encripcion();

                    string mensaje = @"
                        Estimad@ Usuario,
                        <br/>
                        <br/>
                        Bienvenid@ al Sistema de Análisis Cooperativo SAC del Fondo de Fortalecimiento Cooperativo, nuestra entidad brinda soluciones financieras ágiles a las cooperativas de ahorro y crédito que contribuyan a mantener la solidez y estabilidad de nuestras afiliadas.
                        <br/>
                        <br/>
                        Para conocer más de nosotros le invitamos a visitar nuestro sitio web <a href='https://www.ffc.co.cr/'>ffc.co.cr</a>, llamar al 2257-1111 o puede contactar a las analistas de riesgo:
                        <br/>
                        <br/>
                        Cinthya Salazar <a href='mailto:csalazar@ffc.co.cr'>csalazar@ffc.co.cr</a>
                        <br/>
                        Viviana Zumbado <a href='mailto:vzumbado@ffc.co.cr'>vzumbado@ffc.co.cr</a>
                        <br/>
                        <br/>
                        <b>Nuevo Usuario:</b> " + usuario.Nombre +
                        "<br/>" +
                        "<b>Identificación:</b> " + usuario.Identificacion +
                        "<br/>" +
                        "<b>Contraseña Temporal:</b> " + enc.DecryptStr(usuario.Contrasena) + 
                        "<br/>" +
                        "La contraseña deberá de digitarse en la herramienta, no permite la opción copiar - pegar";

                    MailSend.Email.EnviarCorreoImagenes("Notificación de creación de usuario", mensaje, usuario.Correo, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);

                    MailSend.Email.EnviarCorreoImagenes("Creación de usuario", "<br /><br /> Notificación de creación de usuario: <br /><br /> La entidad: " + Entidad.Nombre +
                    " ha creado el usuario " + usuario.Nombre + " con identificación: " + usuario.Identificacion + "<br /><br />", CorreoNotificacion, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);

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

        public ActionResult Edit(int? id)
        {
            var idLogueado = Env.GetUserInfo("userid");
            Usuario ObjLogueado = usr.Get(idLogueado);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Usuario ObjUser = usr.Get(id.ToString());          
            Encripcion.Encripcion enc = new Encripcion.Encripcion();
            ObjUser.Contrasena = enc.DecryptStr(ObjUser.Contrasena);
            var roleMaestro = int.Parse(Utility.Utilitarios.roleMaestroEntidad);
            ViewBag.Estados = new SelectList(usre.GetAll(), "Id", "Nombre", ObjUser.Estado_Usuario.Id);

            if (ObjLogueado.Entidad_Usuario_Id == Utility.Utilitarios.entidadAdministradora)
            {
                ViewBag.Roles = new SelectList(rs.GetAll(), "Id", "Nombre", ObjUser.Role_Usuario.Id).OrderBy(o => o.Text);
                ViewBag.Entidades = new SelectList(ent.GetAll(), "Id", "Nombre", ObjUser.Entidad_Usuario_Id);
            }
            else
            {
                ViewBag.Roles = new SelectList(rs.GetAll().Where(o => o.EsEntidad == true && o.Id != roleMaestro), "Id", "Nombre", ObjUser.Role_Usuario.Id).OrderBy(o => o.Text);
                ViewBag.Entidades = new SelectList(ent.GetAll().Where(o => o.Id == ObjUser.Entidad_Usuario_Id), "Id", "Nombre", ObjUser.Entidad_Usuario_Id);
            }

            ViewBag.Sexos = new SelectList(sx.GetAll(), "Id", "Nombre", ObjUser.Sexo_Usuario.Id);
            if (ObjUser == null)
                return HttpNotFound();

            return View(ObjUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection ObjUser)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    string idEntidad = ObjUser.GetValue("Entidades").AttemptedValue;
                    var usuarios = usr.GetByCompany(idEntidad)
                        .Where(o => o.Estado_Usuario.Id == Utility.Utilitarios.estadoActivo)
                        .Count();

                    string id = ObjUser.GetValue("Id").AttemptedValue;
                    Usuario usuario = usr.Get(id);
                    usuario.Correo = ObjUser.GetValue("Correo").AttemptedValue;
                    usuario.Identificacion = ObjUser.GetValue("Identificacion").AttemptedValue;
                    usuario.Nombre = ObjUser.GetValue("Nombre").AttemptedValue;
                    usuario.Telefono = ObjUser.GetValue("Telefono").AttemptedValue;
                    usuario.Puesto = ObjUser.GetValue("Puesto").AttemptedValue;
                    string idEstado = ObjUser.GetValue("Estados").AttemptedValue;
                    usuario.Estado_Usuario_Id = idEstado;
                    usuario.Role_Usuario_Id = int.Parse(ObjUser.GetValue("Roles").AttemptedValue); ;
                    usuario.Entidad_Usuario_Id = idEntidad;
                    usuario.Sexo_Usuario_Id = int.Parse(ObjUser.GetValue("Sexos").AttemptedValue); ;
                    usuario.Entidad_Usuario = null;
                    usuario.Estado_Usuario = null;
                    usuario.Role_Usuario = null;
                    usuario.Sexo_Usuario = null;
                    usr.Update(usuario);
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

        public ActionResult ChangePassword(int? id)
        {
            var idUser = Env.GetUserInfo("userid");
            Usuario ObjUser = usr.Get(idUser);
            Encripcion.Encripcion enc = new Encripcion.Encripcion();
            ObjUser.Contrasena = enc.DecryptStr(ObjUser.Contrasena);
            Load();
            ViewBag.Roles = new SelectList(rs.GetAll(), "Id", "Nombre", ObjUser.Role_Usuario.Id);
            ViewBag.Estados = new SelectList(usre.GetAll(), "Id", "Nombre", ObjUser.Estado_Usuario.Id);
            ViewBag.Sexos = new SelectList(sx.GetAll(), "Id", "Nombre", ObjUser.Sexo_Usuario.Id);

            if (ObjUser == null)
                return HttpNotFound();

            return View(ObjUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ChangePassword(FormCollection ObjUser)
        {
            var id = ObjUser.GetValue("Id").AttemptedValue;
            Usuario usuario = usr.Get(id);

            try
            {
                if (ModelState.IsValid)
                {                 
                   
                    var validPassword = ObjUser.GetValue("validPassword").AttemptedValue;

                    if(validPassword == Utility.Utilitarios.No)
                        ViewBag.Mensaje = "La contraseña no cumple los requisitos";

                    var contrasenaNueva = ObjUser.GetValue("contrasenaNueva").AttemptedValue;
                    if (contrasenaNueva.Length < 8)
                        ViewBag.Mensaje =  "La contraseña debe tener mínimo 8 caracteres";

                    Encripcion.Encripcion enc = new Encripcion.Encripcion();
                    var contrasenaActual = enc.EncryptStr(ObjUser.GetValue("contrasenaActual").AttemptedValue);
                    contrasenaNueva = enc.EncryptStr(ObjUser.GetValue("contrasenaNueva").AttemptedValue);
                    var contrasenaConfirma = enc.EncryptStr(ObjUser.GetValue("contrasenaConfirma").AttemptedValue);

                    if(contrasenaActual != usuario.Contrasena)
                        ViewBag.Mensaje = "La contraseña actual no coincide con la registrada";

                    else if (contrasenaNueva != contrasenaConfirma)
                        ViewBag.Mensaje = "La nueva contraseña no coincide con la confirmación";

                    if (string.IsNullOrEmpty(ViewBag.Mensaje))
                    {
                        usuario.Contrasena = enc.EncryptStr(ObjUser.GetValue("contrasenaNueva").AttemptedValue);
                        usuario.Entidad_Usuario = null;
                        usuario.Estado_Usuario = null;
                        usuario.Role_Usuario = null;
                        usuario.Sexo_Usuario = null;
                        usuario.CambiarClave = Utility.Utilitarios.No;
                        usr.Update(usuario);

                        ViewBag.Exito = "Contraseña modificada correctamente";
                    }
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                            ViewBag.Mensaje.Append(err.ErrorMessage + "<br/>");
                }
            }
            catch (Exception)
            {
                ViewBag.Mensaje = "Error al realizar la modificación";
            }
            return View(usuario);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Usuario ObjUser = usr.Get(id.ToString());

            if (ObjUser == null)
                return HttpNotFound();

            return View(ObjUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    Usuario usuario = usr.Get(id.ToString());
                    usuario.Estado_Usuario_Id = Utility.Utilitarios.estadoBorrado;
                    usuario.Entidad_Usuario = null;
                    usuario.Estado_Usuario = null;
                    usuario.Role_Usuario = null;
                    usuario.Sexo_Usuario = null;
                    usr.Update(usuario);
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

        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.RoleService.ServiceOf_RoleClient rs = new FGA_En_Linea.RoleService.ServiceOf_RoleClient();
        private readonly FGA_En_Linea.UsuarioEstadoService.ServiceOf_UsuarioEstadoClient usre = new FGA_En_Linea.UsuarioEstadoService.ServiceOf_UsuarioEstadoClient();
        private readonly FGA_En_Linea.SexoService.ServiceOf_SexoClient sx = new FGA_En_Linea.SexoService.ServiceOf_SexoClient();
        private readonly FGA_En_Linea.ParametrosService.ParametrosServiceClient param = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                usr.Close();
                ent.Close();
                rs.Close();
                usre.Close();
                sx.Close();
                param.Close();
            }
            base.Dispose(disposing);
        }
    }
}