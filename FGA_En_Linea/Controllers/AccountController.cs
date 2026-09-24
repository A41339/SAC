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
                generator.ExcludeSymbols = true;
                Encripcion.Encripcion enc = new Encripcion.Encripcion();

                string contrasenaEncriptada = generator.Generate();
                string contrasenaPlana = enc.DecryptStr(contrasenaEncriptada);

                login.Contrasena = contrasenaEncriptada;
                login.CambiarClave = "S";
                db.Update(login);

                try
                {
                    string mensaje = @"
                        Estimad@ Usuario,
                        <br/><br/>
                        Bienvenid@ al Sistema de Análisis Cooperativo SAC del Fondo de Fortalecimiento Cooperativo, nuestra entidad brinda soluciones financieras ágiles a las cooperativas de ahorro y crédito que contribuyan a mantener la solidez y estabilidad de nuestras afiliadas.
                        <br/><br/>
                        Para conocer más de nosotros le invitamos a visitar nuestro sitio web <a href='https://www.ffc.co.cr/' style='color: #31859C; font-weight: 600; text-decoration: none;'>ffc.co.cr</a>, llamar al 2257-1111 o puede contactar a las analistas de riesgo:
                        <br/><br/>
                        &bull; Cinthya Salazar <a href='mailto:csalazar@ffc.co.cr' style='color: #31859C; font-weight: 600; text-decoration: none;'>csalazar@ffc.co.cr</a><br/>
                        &bull; Viviana Zumbado <a href='mailto:vzumbado@ffc.co.cr' style='color: #31859C; font-weight: 600; text-decoration: none;'>vzumbado@ffc.co.cr</a>
                        <br/>
                        <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-left: 4px solid #31859C; border-radius: 8px; padding: 18px 22px; margin: 22px 0;'>
                            <div style='font-size: 14.5px; margin-bottom: 9px; color: #475569;'>
                                <strong style='color: #1e293b;'>Usuario:</strong> <span style='font-family: monospace; font-size: 15px; font-weight: 700; color: #2F5597;'>" + login.Identificacion + @"</span>
                            </div>
                            <div style='font-size: 14.5px; margin-bottom: 10px; color: #475569;'>
                                <strong style='color: #1e293b;'>Contraseña Temporal:</strong> <span style='display: inline-block; background-color: #e2e8f0; color: #2F5597; font-family: Consolas, Monaco, monospace; font-size: 15px; font-weight: 700; padding: 3px 10px; border-radius: 6px; letter-spacing: 1px;'>" + contrasenaPlana + @"</span>
                            </div>
                            <div style='font-size: 12.5px; color: #64748b; line-height: 1.5;'>
                                &#9432; <em>La contraseña deberá digitarse directamente en la herramienta (no permite la opción de copiar y pegar).</em>
                            </div>
                        </div>";

                    MailSend.Email.EnviarCorreoImagenes("Recuperación de contraseña", mensaje, correo, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);                  
                    ViewBag.Msg = "Se ha generado una nueva contraseña y se ha enviado a su correo.";
                    ViewBag.TipoMsg = "success";
                }
                catch (Exception e) {
                    ViewBag.Msg = "Error al procesar la solicitud: " + e.Message;
                    ViewBag.TipoMsg = "error";
                }
            }
            else
            {
                ViewBag.Msg = "Usuario inválido o inactivo. Verifique el correo electrónico registrado.";
                ViewBag.TipoMsg = "warning";
            }
            return View();
        }


        [HttpPost]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult doLogin(String identificacion, String contrasena)
        {
            Resultado ObjResultado = new Resultado();
            ObjResultado.exito = false;

            if (!string.IsNullOrEmpty(identificacion)) identificacion = identificacion.Trim();
            if (!string.IsNullOrEmpty(contrasena)) contrasena = contrasena.Trim();

            Encripcion.Encripcion enc = new Encripcion.Encripcion();
            var login = db.GetByCredentials(identificacion, enc.EncryptStr(contrasena));

            if (login == null || login.Estado_Usuario == null || login.Estado_Usuario.Id != Utility.Utilitarios.estadoActivo)
                ObjResultado.mensaje = ContrasenaInvalida;
            else
            {
                if (login.Entidad_Usuario != null && login.Entidad_Usuario.Activo == false)
                    ObjResultado.mensaje = "La entidad " + login.Entidad_Usuario.Nombre + " no se encuentra activa.";
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

                    Session["Usuario"] = login;
                    Session["Ind_Carga"] = login.Entidad_Usuario.Ind_Cargar;
                    string defaultEnt = login.Role_Usuario.EsEntidad ? login.Entidad_Usuario_Id : Utility.Utilitarios.entidadDefault;
                    Session["IdEntidad"] = defaultEnt;
                    Session["IsFGA"] = login.Role_Usuario.EsEntidad ? 0 : 1;
                    HttpRuntime.Cache.Insert("Login_" + login.Id.ToString(), login.Id.ToString(), null, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);

                    try
                    {
                        var spClient = new FGA_En_Linea.SPService.SPClient();
                        DateTime fechaCierre = spClient.FGA_Consultar_FechaCierre(defaultEnt);
                        DateTime p2 = fechaCierre.AddMonths(-1);
                        DateTime p1 = p2.AddYears(-1);
                        Session["Periodo"] = fechaCierre.ToShortDateString();
                        Session["Periodo2"] = p2.ToShortDateString();
                        Session["Periodo1"] = p1.ToShortDateString();
                        Session["TipoComparacion"] = "Interanual";
                    }
                    catch { }

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
        //    ObjResultado.mensaje = "Debe dar click a la opci�n \"Ingresar\"";

        //    if (!string.IsNullOrEmpty(otp))
        //    {
        //        var login = db.GetByIden(identificacion);

        //        if (login == null || login.Estado_Usuario.Id != Utility.Utilitarios.estadoActivo)
        //            ObjResultado.mensaje = "Usuario inv�lido";
        //        else
        //        {
        //            if (login.OTP != Env.Encrypt(otp) && login.Entidad_Usuario_Id != "99")
        //                ObjResultado.mensaje = "OTP inv�lido";
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