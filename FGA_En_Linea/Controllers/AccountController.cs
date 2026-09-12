using FGA.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using System.Threading;
using System.Linq;
using FGA.Model;
using System.Threading.Tasks;

namespace FGA.Controllers
{
    public class AccountController : Controller
    {
        private const string ContrasenaInvalida = "Usuario y contraseña inválidos";
        readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient db = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        readonly FGA_En_Linea.ParametrosService.ParametrosServiceClient param = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
        readonly FGA_En_Linea.Bit_SessionesService.ServiceOf_Bit_SessionesClient bit = new FGA_En_Linea.Bit_SessionesService.ServiceOf_Bit_SessionesClient();

        public ActionResult Login() {

            return View();
        }

        public ActionResult Recovery() => View();

        [HttpPost]
        public ActionResult Recovery(FormCollection frmCollection)
        {
            string correo = frmCollection["Email"].ToString();
            Usuario login = db.GetByEmail(correo);

            var listParam = param.GetAll().ToList();
            var ServidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
            var CuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
            var PasswordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();

            if (login != null && login.Estado_Usuario_Id == Utility.Utilitarios.estadoActivo)
            {
                Utility.PasswordGenerator generator = new Utility.PasswordGenerator();
                string nuevaContrasena = generator.Generate();
                login.Contrasena = nuevaContrasena;
                Encripcion.Encripcion enc = new Encripcion.Encripcion();
                login.CambiarClave = "S";
                db.Update(login);

                try
                {
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
                        <b>Usuario:</b> " + login.Identificacion +
                       "<br/>" +
                       "<b>Contraseña Temporal:</b> " + enc.DecryptStr(login.Contrasena) +
                       "<br/>" +
                       "La contraseña deberá de digitarse en la herramienta, no permite la opción copiar - pegar";

                    MailSend.Email.EnviarCorreoImagenes("Recuperación de contraseña", mensaje, correo, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);                  
                    ViewBag.Msg = "Se ha generado una nueva contraseña.";
                }
                catch (Exception e) {
                    ViewBag.Msg = e.Message;
                }
            }
            else
                ViewBag.Msg = "Usuario inválido";
            return View();
        }


        [HttpPost]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult doLogin(String identificacion, String contrasena)
        {

            Resultado ObjResultado = new Resultado();
            ObjResultado.exito = false;
            Encripcion.Encripcion enc = new Encripcion.Encripcion();
            var login = db.GetByCredentials(identificacion, enc.EncryptStr(contrasena));

            if (login == null || login.Estado_Usuario.Id != Utility.Utilitarios.estadoActivo)
                ObjResultado.mensaje = ContrasenaInvalida;
            else
            {
                if (login.Entidad_Usuario.Activo == false)
                    ObjResultado.mensaje = "La entidad " + login.Entidad_Usuario.Nombre + " no se encuentra activa.";
                else
                {

                    if (login == null || login.Estado_Usuario.Id != Utility.Utilitarios.estadoActivo)
                        ObjResultado.mensaje = "Usuario inválido";
                    else
                    {
                        var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, login.Nombre.ToString()),
                                new Claim(ClaimTypes.Role, login.Role_Usuario.Id.ToString()),
                                new Claim(ClaimTypes.Sid, login.Id.ToString()),
                                new Claim(ClaimTypes.Surname, login.Entidad_Usuario.Nombre),
                                new Claim(ClaimTypes.Gender, login.Sexo_Usuario_Id.ToString()),
                                new Claim("CambiarClave", login.CambiarClave),
                                new Claim("Evaluacion", login.Entidad_Usuario.Ind_Evaluacion ? "S" : "N")
                            };

                        //BORRAR
                        Session["Usuario"] = login;
                        Session["Ind_Carga"] = login.Entidad_Usuario.Ind_Cargar;
                        Session["IdEntidad"] = Utility.Utilitarios.entidadDefault;
                        Session["IsFGA"] = login.Role_Usuario.EsEntidad ? 0 : 1;
                        HttpRuntime.Cache.Insert("Login_" + login.Id.ToString(), login.Id.ToString(), null, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);

                        try
                        {
                            if (System.IO.File.Exists(Server.MapPath("~/Content/images/" + login.Entidad_Usuario.Nombre + ".jpg")))
                                claims.Add(new Claim(ClaimTypes.UserData, login.Entidad_Usuario.Nombre + ".jpg"));
                            else
                                claims.Add(new Claim(ClaimTypes.UserData, string.IsNullOrEmpty(login.Entidad_Usuario.Logo) ? "default.png" : login.Entidad_Usuario.Logo));
                        }
                        catch (Exception)
                        {
                            claims.Add(new Claim(ClaimTypes.UserData, "default.png"));
                        }

                        Session.Timeout = 500; //login.Role_Usuario.EsEntidad ? 200 : 5; 

                        var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                        var authenticationManager = Request.GetOwinContext().Authentication;
                        authenticationManager.SignIn(identity);
                        var claimsPrincipal = new ClaimsPrincipal(identity);
                        Thread.CurrentPrincipal = claimsPrincipal;

                        Task.Run(() =>
                        {
                            Bit_Sessiones reg = new Bit_Sessiones();
                            reg.Fecha = DateTime.Now;
                            reg.IdEntidad = login.Entidad_Usuario_Id;
                            reg.IdRole = login.Role_Usuario_Id;
                            reg.IdUsuario = login.Id.Value;
                            bit.Add(ref reg);
                        });

                        ObjResultado.exito = true;
                        ObjResultado.mensaje = Url.Content(login.CambiarClave == "S" ? "~/Usuario/ChangePassword" : login.Role_Usuario.Url);

                    }
                    //}
                }
            }
            return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        }



        //[HttpPost]
        //[ValidateInput(false)]
        //[AllowAnonymous]
        //public JsonResult Login(String identificacion, String otp)
        //{
        //    Resultado ObjResultado = new Resultado();
        //    ObjResultado.exito = false;
        //    ObjResultado.mensaje = "Debe dar click a la opción \"Ingresar\"";

        //    if (!string.IsNullOrEmpty(otp))
        //    {
        //        var login = db.GetByIden(identificacion);

        //        if (login == null || login.Estado_Usuario.Id != Utility.Utilitarios.estadoActivo)
        //            ObjResultado.mensaje = "Usuario inválido";
        //        else
        //        {
        //            if (login.OTP != Env.Encrypt(otp) && login.Entidad_Usuario_Id != "99")
        //                ObjResultado.mensaje = "OTP inválido";
        //            else
        //            {
        //                var claims = new List<Claim>
        //                {
        //                    new Claim(ClaimTypes.Name, login.Nombre.ToString()),
        //                    new Claim(ClaimTypes.Role, login.Role_Usuario.Id.ToString()),
        //                    new Claim(ClaimTypes.Sid, login.Id.ToString()),
        //                    new Claim(ClaimTypes.Surname, login.Entidad_Usuario.Nombre),
        //                    new Claim(ClaimTypes.Gender, login.Sexo_Usuario_Id.ToString()),
        //                    new Claim("CambiarClave", login.CambiarClave)
        //                };

        //                //BORRAR
        //                Session["Ind_Carga"] = login.Entidad_Usuario.Ind_Cargar;
        //                Session["IdEntidad"] = Utility.Utilitarios.entidadDefault;
        //                Session["IsFGA"] = login.Role_Usuario.EsEntidad ? 0 : 1;

        //                try
        //                {
        //                    if (System.IO.File.Exists(Server.MapPath("~/Content/images/" + login.Entidad_Usuario.Nombre + ".jpg")))
        //                        claims.Add(new Claim(ClaimTypes.UserData, login.Entidad_Usuario.Nombre + ".jpg"));
        //                    else
        //                        claims.Add(new Claim(ClaimTypes.UserData, string.IsNullOrEmpty(login.Entidad_Usuario.Logo) ? "default.png" : login.Entidad_Usuario.Logo));
        //                }
        //                catch (Exception)
        //                {
        //                    claims.Add(new Claim(ClaimTypes.UserData, "default.png"));
        //                }

        //                var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
        //                var authenticationManager = Request.GetOwinContext().Authentication;
        //                authenticationManager.SignIn(identity);
        //                var claimsPrincipal = new ClaimsPrincipal(identity);
        //                Thread.CurrentPrincipal = claimsPrincipal;

        //                ObjResultado.exito = true;
        //                ObjResultado.mensaje = Url.Content(login.CambiarClave == "S" ? "~/Usuario/ChangePassword" : login.Role_Usuario.Url);

        //                Task.Run(() =>
        //                {
        //                    Bit_Sessiones reg = new Bit_Sessiones();
        //                    reg.Fecha = DateTime.Now;
        //                    reg.IdEntidad = login.Entidad_Usuario_Id;
        //                    reg.IdRole = login.Role_Usuario_Id;
        //                    reg.IdUsuario = login.Id.Value;
        //                    bit.Add(ref reg);
        //                });
        //            }
        //        }
        //    }

        //    return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //[ValidateInput(false)]
        //[AllowAnonymous]
        //public JsonResult doLogin(String identificacion, String contrasena)
        //{
        //    Resultado ObjResultado = new Resultado();
        //    ObjResultado.exito = false;
        //    Encripcion.Encripcion enc = new Encripcion.Encripcion();
        //    var login = db.GetByCredentials(identificacion, enc.EncryptStr(contrasena));

        //    if (login == null || login.Estado_Usuario.Id != Utility.Utilitarios.estadoActivo)
        //        ObjResultado.mensaje = ContrasenaInvalida;
        //    else
        //    {
        //        if (login.Entidad_Usuario.Activo == false)
        //            ObjResultado.mensaje = "La entidad " + login.Entidad_Usuario.Nombre + " no se encuentra activa.";
        //        else
        //        {
        //            var OTPCode = OTP.Generate.GenerateOTP(identificacion);
        //            var listParam = param.GetAll().ToList();
        //            var ServidorCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
        //            var CuentaCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
        //            var PasswordCorreo = listParam.Where(o => o.Llave == Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();

        //            MailSend.Email.EnviarCorreoImagenes("OTP", "<br />Estimad@ usuario: " + login.Nombre + " <br /><br /> El FFC le informa que se le ha generado el OTP: <b>"
        //                + OTPCode + "</b><br /><br />", login.Correo, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);

        //            login.OTP = Env.Encrypt(OTPCode);
        //            login.Entidad_Usuario = null;
        //            login.Estado_Usuario = null;
        //            login.Role_Usuario = null;
        //            login.Sexo_Usuario = null;
        //            db.Update(login);
        //            ObjResultado.exito = true;
        //        }
        //    }

        //    return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        //}


        public ActionResult Register => View();

        public ActionResult Signout()
        {
            HttpCookie c = new HttpCookie(".AspNet.ApplicationCookie")
            {
                Expires = DateTime.Now.AddDays(-1)
            };
            Response.Cookies.Add(c);

            HttpCookie d = new HttpCookie("__RequestVerificationToken")
            {
                Expires = DateTime.Now.AddDays(-1)
            };
            Response.Cookies.Add(d);

            var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
            AuthenticationManager.SignOut();
            Session.Abandon();
            return RedirectToAction("login", "Account");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ActionResult Unauthorized => View();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Close();
            }
            base.Dispose(disposing);
        }
    }
}