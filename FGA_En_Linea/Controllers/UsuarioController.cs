using System; 
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using DocumentFormat.OpenXml.Office2010.Excel;
using FGA.Models;

namespace FGA.Controllers
{
    public class UsuarioController : BaseController
    {
        private void CargarCatalogosUsuario()
        {
            try
            {
                Load();
                var id = Env.GetUserInfo("userid");
                Usuario ObjUser = usr.Get(id);
                var roleMaestro = int.Parse(Utility.Utilitarios.roleMaestroEntidad);

                if (ObjUser.Entidad_Usuario.Id == Utility.Utilitarios.entidadAdministradora)
                {
                    ViewBag.Roles = new SelectList(rs.GetAll(), "Id", "Nombre", ObjUser.Role_Usuario.Id).OrderBy(o => o.Text).ToList();
                    ViewBag.Entidades = new SelectList(ent.GetAll(), "Id", "Nombre", Session["IdEntidad"]).OrderBy(o => o.Text).ToList();
                }
                else
                {
                    ViewBag.Roles = new SelectList(rs.GetAll().Where(o => o.EsEntidad == true && o.Id != roleMaestro), "Id", "Nombre", ObjUser.Role_Usuario.Id).OrderBy(o => o.Text).ToList();
                    ViewBag.Entidades = new SelectList(ent.GetAll().Where(o => o.Id == ObjUser.Entidad_Usuario_Id), "Id", "Nombre", ObjUser.Entidad_Usuario_Id).OrderBy(o => o.Text).ToList();
                }

                ViewBag.Sexos = new SelectList(sx.GetAll(), "Id", "Nombre").OrderBy(o => o.Text).ToList();
                ViewBag.Estados = new SelectList(usre.GetAll(), "Id", "Nombre").OrderBy(o => o.Text).ToList();
            }
            catch (Exception)
            {
            }
        }

        public ActionResult Index()
        {
            CargarCatalogosUsuario();
            return View();
        }

        [HttpGet]
        public ActionResult GetUsuario(int id)
        {
            try
            {
                Usuario usuario = usr.Get(id.ToString());
                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = usuario.Id,
                        identificacion = usuario.Identificacion ?? "",
                        nombre = usuario.Nombre ?? "",
                        telefono = usuario.Telefono ?? "",
                        puesto = usuario.Puesto ?? "",
                        correo = usuario.Correo ?? "",
                        entidadId = usuario.Entidad_Usuario_Id ?? "",
                        roleId = usuario.Role_Usuario_Id,
                        sexoId = usuario.Sexo_Usuario_Id,
                        estadoId = usuario.Estado_Usuario_Id ?? ""
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar usuario: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleUsuario(int id)
        {
            try
            {
                Usuario usuario = usr.Get(id.ToString());
                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = usuario.Id,
                        identificacion = usuario.Identificacion ?? "",
                        nombre = usuario.Nombre ?? "",
                        telefono = string.IsNullOrWhiteSpace(usuario.Telefono) ? "-" : usuario.Telefono,
                        puesto = string.IsNullOrWhiteSpace(usuario.Puesto) ? "-" : usuario.Puesto,
                        correo = string.IsNullOrWhiteSpace(usuario.Correo) ? "-" : usuario.Correo,
                        entidad = (usuario.Entidad_Usuario != null ? usuario.Entidad_Usuario.Nombre : "-"),
                        role = (usuario.Role_Usuario != null ? usuario.Role_Usuario.Nombre : "-"),
                        sexo = (usuario.Sexo_Usuario != null ? usuario.Sexo_Usuario.Nombre : "-"),
                        estado = (usuario.Estado_Usuario != null ? usuario.Estado_Usuario.Nombre : "-")
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar detalle del usuario: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GuardarUsuario(int? id, string identificacion, string nombre, string telefono, string puesto, string correo, string entidadId, int roleId, int sexoId, string estadoId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identificacion))
                    return Json(new { success = false, message = "La identificación es obligatoria." });
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "El nombre es obligatorio." });
                if (string.IsNullOrWhiteSpace(correo))
                    return Json(new { success = false, message = "El correo electrónico es obligatorio." });
                if (string.IsNullOrWhiteSpace(entidadId))
                    return Json(new { success = false, message = "Debe seleccionar una entidad." });

                identificacion = identificacion.Trim();
                nombre = nombre.Trim();
                correo = correo.Trim();

                if (!id.HasValue || id.Value == 0)
                {
                    // Crear nuevo usuario
                    Usuario repetido = usr.GetByIden(identificacion);
                    if (repetido != null && repetido.Estado_Usuario_Id != Utility.Utilitarios.estadoBorrado)
                    {
                        return Json(new { success = false, message = "Ya existe un usuario con la identificación '" + identificacion + "'." });
                    }

                    Utility.PasswordGenerator generator = new Utility.PasswordGenerator();
                    Usuario nuevo = new Usuario
                    {
                        Identificacion = identificacion,
                        Nombre = nombre,
                        Telefono = telefono ?? "",
                        Puesto = puesto ?? "",
                        Correo = correo,
                        Entidad_Usuario_Id = entidadId,
                        Role_Usuario_Id = roleId,
                        Sexo_Usuario_Id = sexoId,
                        Estado_Usuario_Id = Utility.Utilitarios.estadoActivo,
                        Contrasena = generator.Generate(),
                        CambiarClave = Utility.Utilitarios.Si,
                        Entidad_Usuario = null,
                        Estado_Usuario = null,
                        Role_Usuario = null,
                        Sexo_Usuario = null
                    };

                    usr.Add(ref nuevo);

                    try
                    {
                        var listParam = param.GetAll().ToList();
                        var ServidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
                        var CuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
                        var PasswordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();
                        var CorreoNotificacion = listParam.Where(o => o.Llave == Utility.Utilitarios.Notificacion_Creacion_Usuarios).Select(o => o.Valor).FirstOrDefault();

                        string mensaje = @"
                            Estimad@ Usuario,<br/><br/>
                            Bienvenid@ al Sistema de Análisis Cooperativo SAC del Fondo de Fortalecimiento Cooperativo.<br/><br/>
                            Su cuenta de usuario ha sido creada satisfactoriamente.<br/>
                            <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-left: 4px solid #31859C; border-radius: 8px; padding: 18px 22px; margin: 20px 0;'>
                                <div style='font-size: 14.5px; margin-bottom: 8px; color: #475569;'>
                                    <strong style='color: #1e293b;'>Identificación:</strong> <span style='font-family: monospace; font-size: 15px; font-weight: 700; color: #2F5597;'>" + nuevo.Identificacion + @"</span>
                                </div>
                                <div style='font-size: 14.5px; margin-bottom: 8px; color: #475569;'>
                                    <strong style='color: #1e293b;'>Nombre:</strong> <span style='font-size: 14.5px; font-weight: 600; color: #1e293b;'>" + nuevo.Nombre + @"</span>
                                </div>
                                <div style='font-size: 14.5px; margin-bottom: 10px; color: #475569;'>
                                    <strong style='color: #1e293b;'>Contraseña Temporal:</strong> <span style='display: inline-block; background-color: #e2e8f0; color: #2F5597; font-family: Consolas, Monaco, monospace; font-size: 15px; font-weight: 700; padding: 3px 10px; border-radius: 6px; letter-spacing: 1px;'>" + nuevo.Contrasena + @"</span>
                                </div>
                                <div style='font-size: 12.5px; color: #64748b; line-height: 1.5;'>
                                    &#9432; <em>Por motivos de seguridad, se le solicitará cambiar su contraseña al iniciar sesión.</em>
                                </div>
                            </div>";

                        MailSend.Email.EnviarCorreoImagenes("Notificación de creación de usuario", mensaje, nuevo.Correo, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);
                    }
                    catch (Exception) { }

                    return Json(new { success = true, message = "Usuario creado exitosamente. Se ha enviado una contraseña temporal a su correo." });
                }
                else
                {
                    // Modificar usuario existente
                    Usuario usuario = usr.Get(id.Value.ToString());
                    if (usuario == null)
                        return Json(new { success = false, message = "El usuario a modificar no existe." });

                    usuario.Identificacion = identificacion;
                    usuario.Nombre = nombre;
                    usuario.Telefono = telefono ?? "";
                    usuario.Puesto = puesto ?? "";
                    usuario.Correo = correo;
                    usuario.Entidad_Usuario_Id = entidadId;
                    usuario.Role_Usuario_Id = roleId;
                    usuario.Sexo_Usuario_Id = sexoId;
                    if (!string.IsNullOrEmpty(estadoId))
                        usuario.Estado_Usuario_Id = estadoId;

                    usuario.Entidad_Usuario = null;
                    usuario.Estado_Usuario = null;
                    usuario.Role_Usuario = null;
                    usuario.Sexo_Usuario = null;

                    usr.Update(usuario);
                    return Json(new { success = true, message = "Usuario actualizado exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar el usuario: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarUsuario(int id)
        {
            try
            {
                var idLogueado = int.Parse(Env.GetUserInfo("userid"));
                if (id == idLogueado)
                    return Json(new { success = false, message = "No puede eliminar su propio usuario mientras tiene la sesión activa." });

                Usuario usuario = usr.Get(id.ToString());
                if (usuario == null)
                    return Json(new { success = false, message = "El usuario que intenta eliminar no existe." });

                usuario.Estado_Usuario_Id = Utility.Utilitarios.estadoBorrado;
                usuario.Entidad_Usuario = null;
                usuario.Estado_Usuario = null;
                usuario.Role_Usuario = null;
                usuario.Sexo_Usuario = null;
                usr.Update(usuario);

                return Json(new { success = true, message = "Usuario eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar usuario: " + ex.Message });
            }
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
                        Bienvenid@ al Sistema de An�lisis Cooperativo SAC del Fondo de Fortalecimiento Cooperativo, nuestra entidad brinda soluciones financieras �giles a las cooperativas de ahorro y cr�dito que contribuyan a mantener la solidez y estabilidad de nuestras afiliadas.
                        <br/>
                        <br/>
                        Para conocer m�s de nosotros le invitamos a visitar nuestro sitio web <a href='https://www.ffc.co.cr/'>ffc.co.cr</a>, llamar al 2257-1111 o puede contactar a las analistas de riesgo:
                        <br/>
                        <br/>
                        Cinthya Salazar <a href='mailto:csalazar@ffc.co.cr'>csalazar@ffc.co.cr</a>
                        <br/>
                        Viviana Zumbado <a href='mailto:vzumbado@ffc.co.cr'>vzumbado@ffc.co.cr</a>
                        <br/>
                        <br/>
                        <b>Nuevo Usuario:</b> " + usuario.Nombre +
                        "<br/>" +
                        "<b>Identificaci�n:</b> " + usuario.Identificacion +
                        "<br/>" +
                        "<b>Contrase�a Temporal:</b> " + enc.DecryptStr(usuario.Contrasena) + 
                        "<br/>" +
                        "La contrase�a deber� de digitarse en la herramienta, no permite la opci�n copiar - pegar";

                    MailSend.Email.EnviarCorreoImagenes("Notificaci�n de creaci�n de usuario", mensaje, usuario.Correo, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);

                    MailSend.Email.EnviarCorreoImagenes("Creaci�n de usuario", "<br /><br /> Notificaci�n de creaci�n de usuario: <br /><br /> La entidad: " + Entidad.Nombre +
                    " ha creado el usuario " + usuario.Nombre + " con identificaci�n: " + usuario.Identificacion + "<br /><br />", CorreoNotificacion, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);

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
                sb.Append("Error al realizar la modificaci�n");
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
                        ViewBag.Mensaje = "La contrase�a no cumple los requisitos";

                    var contrasenaNueva = ObjUser.GetValue("contrasenaNueva").AttemptedValue;
                    if (contrasenaNueva.Length < 8)
                        ViewBag.Mensaje =  "La contrase�a debe tener m�nimo 8 caracteres";

                    Encripcion.Encripcion enc = new Encripcion.Encripcion();
                    var contrasenaActual = enc.EncryptStr(ObjUser.GetValue("contrasenaActual").AttemptedValue);
                    contrasenaNueva = enc.EncryptStr(ObjUser.GetValue("contrasenaNueva").AttemptedValue);
                    var contrasenaConfirma = enc.EncryptStr(ObjUser.GetValue("contrasenaConfirma").AttemptedValue);

                    if(contrasenaActual != usuario.Contrasena)
                        ViewBag.Mensaje = "La contrase�a actual no coincide con la registrada";

                    else if (contrasenaNueva != contrasenaConfirma)
                        ViewBag.Mensaje = "La nueva contrase�a no coincide con la confirmaci�n";

                    if (string.IsNullOrEmpty(ViewBag.Mensaje))
                    {
                        usuario.Contrasena = enc.EncryptStr(ObjUser.GetValue("contrasenaNueva").AttemptedValue);
                        usuario.Entidad_Usuario = null;
                        usuario.Estado_Usuario = null;
                        usuario.Role_Usuario = null;
                        usuario.Sexo_Usuario = null;
                        usuario.CambiarClave = Utility.Utilitarios.No;
                        usr.Update(usuario);

                        Usuario usuarioActualizado = usr.Get(id);
                        if (usuarioActualizado != null)
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, usuarioActualizado.Nombre.ToString()),
                                new Claim(ClaimTypes.Role, usuarioActualizado.Role_Usuario.Id.ToString()),
                                new Claim(ClaimTypes.Sid, usuarioActualizado.Id.ToString()),
                                new Claim(ClaimTypes.Surname, usuarioActualizado.Entidad_Usuario.Nombre),
                                new Claim(ClaimTypes.Gender, usuarioActualizado.Sexo_Usuario_Id.ToString()),
                                new Claim("CambiarClave", Utility.Utilitarios.No),
                                new Claim("Evaluacion", usuarioActualizado.Entidad_Usuario.Ind_Evaluacion ? "S" : "N")
                            };

                            try
                            {
                                if (System.IO.File.Exists(Server.MapPath("~/Content/images/" + usuarioActualizado.Entidad_Usuario.Nombre + ".jpg")))
                                    claims.Add(new Claim(ClaimTypes.UserData, usuarioActualizado.Entidad_Usuario.Nombre + ".jpg"));
                                else
                                    claims.Add(new Claim(ClaimTypes.UserData, string.IsNullOrEmpty(usuarioActualizado.Entidad_Usuario.Logo) ? "default.png" : usuarioActualizado.Entidad_Usuario.Logo));
                            }
                            catch (Exception)
                            {
                                claims.Add(new Claim(ClaimTypes.UserData, "default.png"));
                            }

                            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                            var authenticationManager = Request.GetOwinContext().Authentication;
                            authenticationManager.SignIn(identity);
                            var claimsPrincipal = new ClaimsPrincipal(identity);
                            Thread.CurrentPrincipal = claimsPrincipal;
                        }

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
                ViewBag.Mensaje = "Error al realizar la modificaci�n";
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
                sb.Append("Error al realizar la modificaci�n");
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