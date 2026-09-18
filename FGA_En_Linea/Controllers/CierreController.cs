using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Office2010.Excel;
using Entities.Entities.Procedures;
using FGA.Models;
using FGA.Utility;

namespace FGA.Controllers
{
    public class CierreController : BaseController
    {
        public ActionResult Index()
        {
            Load();
            var entities = sp.FGA_Consultar_ArchivosCargados(Session["IdEntidad"].ToString(), Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString()), true).ToList();
            EstatusCarga view = new EstatusCarga();
            view.listaArchivos = entities;
            var aceptados = entities.Where(o => o.XML_ID == FGA.Utility.Utilitarios.archivoAceptado).Count();
            view.porcentajeCarga = Math.Round(aceptados == 0 ? 0 : 100.00 / entities.Count() * aceptados, 0);
            view.pendientes = entities.Count() - aceptados;
            return View(view);
        }

        public PartialViewResult GetPartial()
        {
            var files = sp.FGA_Consultar_Monitor();
            return PartialView("Status", files);
        }

        public PartialViewResult GetPerfiles()
        {
            var files = sp.FGA_Consultar_Monitor();
            return PartialView("StatusPerfiles", files);
        }

        [HttpGet]
        public string VerPendientes(string id)
        {
            var respuesta = "<br/><table id=\"tbRole\" class=\"table table-striped table-bordered\">";
            var files = sp.FGA_Consultar_ArchivosPendientes(id);

            foreach (string detalle in files)
                respuesta += "<tr><td>" + detalle + "</td></tr>";

            respuesta += "</table>";
            return respuesta;
        }

        public ActionResult CargarEntidad(String idEntidad)
        {
            Session["IdEntidad"] = idEntidad;
            Session["Check"] = true;
            Load();
            DateTime fechaEntidad = Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
                    
            if (Session["Periodo1"] is null)
                Session["Periodo1"] = fechaEntidad.AddMonths(-3).ToShortDateString();

            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.AddMonths(-1).ToShortDateString();


            return View("~/Views/Home/Index.cshtml");
        }

        public ActionResult Monitor()
        {
            var files = sp.FGA_Consultar_Monitor();
            ViewBag.Mensaje = string.Empty;
            return View(files);
        }

        public ActionResult Perfiles()
        {
            var files = sp.FGA_Consultar_Monitor();
            ViewBag.Mensaje = string.Empty;
            return View(files);
        }

        public ActionResult Cierre(string id, DateTime periodo)
        {
            var files = sp.FGA_Consultar_Monitor();
            Utility.ProcessFile.ProcesarCierre(id, periodo);

            var pendientes = sp.FGA_Consultar_ArchivosPendientes(id);
            if(pendientes.Length <= 0)
                ViewBag.Mensaje = "Proceso exitoso: El cierre de la entidad se envió a ejecutar correctamente";

            else
                ViewBag.Mensaje = "La entidad tiene archivos pendientes por cargar";
            
            return View("Monitor", files);
        }

        public ActionResult Buscar(String Entidades, String Periodo)
        {
            if (string.IsNullOrEmpty(Entidades))
                Entidades = Session["IdEntidad"] is null ? Utility.Utilitarios.entidadAdministradora : Session["IdEntidad"].ToString();

            Session["IdEntidad"] = Entidades;
            DateTime fecha;

            if (string.IsNullOrEmpty(Periodo))
                fecha = Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
            else
            {
                fecha = Utility.Utilitarios.ConvertirAFecha(Periodo);
                Session["Periodo"] = (new DateTime(fecha.Year, fecha.Month, 1)).ToShortDateString();
            }

            Load();
            var entities = sp.FGA_Consultar_ArchivosCargados(Entidades, fecha, true).ToList();
            EstatusCarga view = new EstatusCarga();
            view.listaArchivos = entities;
            var aceptados = entities.Where(o => o.XML_ID == FGA.Utility.Utilitarios.archivoAceptado).Count();
            view.porcentajeCarga = Math.Round(aceptados == 0 ? 0 : 100.00 / entities.Count() * aceptados, 0);
            view.pendientes = sp.FGA_Consultar_ArchivosPendientes(Entidades).Count(); 

            return View("Index", view);
        }

        public ActionResult Eliminar(int id)
        {
            var entities = new List<FGA_Consultar_ArchivosCargados_Result>();
            EstatusCarga view = new EstatusCarga();

            try
            {
                string entidad = Session["IdEntidad"].ToString();
                var encabezado = enc.Get(id.ToString());
                encabezado.IdEstado_Id = Utility.Utilitarios.archivoEliminado;
                encabezado.IdEntidad = null;
                encabezado.IdArchivo = null;
                encabezado.IdEstado = null;
                encabezado.IdUsuario = null;
                enc.Update(encabezado);
                entities = sp.FGA_Consultar_ArchivosCargados(entidad, Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString()), true).ToList();
               
                view.listaArchivos = entities;
                var aceptados = entities.Where(o => o.XML_ID == FGA.Utility.Utilitarios.archivoAceptado).Count();
                view.pendientes = sp.FGA_Consultar_ArchivosPendientes(entidad).Count();
                view.porcentajeCarga = Math.Round(aceptados == 0 ? 0 : 100.00 / entities.Count() * aceptados, 0);
                

                Load();
            }
            catch (Exception)
            {
            }

            return View("Index", view);
        }

        public ActionResult AceptarArchivo(int id)
        {
            var entities = new List<FGA_Consultar_ArchivosCargados_Result>();
            EstatusCarga view = new EstatusCarga();

            try
            {
                string entidad = Session["IdEntidad"].ToString();
                var encabezado = enc.Get(id.ToString());
                encabezado.IdEstado_Id = Utility.Utilitarios.archivoAceptado;
                encabezado.IdEntidad = null;
                encabezado.IdArchivo = null;
                encabezado.IdEstado = null;
                encabezado.IdUsuario = null;
                enc.Update(encabezado);
                entities = sp.FGA_Consultar_ArchivosCargados(entidad, Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString()), true).ToList();
                
                view.listaArchivos = entities;
                var aceptados = entities.Where(o => o.XML_ID == FGA.Utility.Utilitarios.archivoAceptado).Count();
                view.porcentajeCarga = Math.Round(aceptados == 0 ? 0 : 100.00 / entities.Count() * aceptados, 0);
                view.pendientes = sp.FGA_Consultar_ArchivosPendientes(entidad).Count();
                Load();
            }
            catch (Exception)
            {
            }

            return View("Index", view);
        }

        public ActionResult Details(int? id)
        {
            HttpContext.Session["File"] = id == null ? 0 : id;
            return View();
        }

        public ActionResult GetGrid()
        {
            long file = long.Parse(HttpContext.Session["File"].ToString());
            var tak = err.GetByEncabezado(file);
            var usuarioLogueado = Env.GetUserInfo("userid");

            XML_Encabezado encabezado = enc.Get(file.ToString());
            Usuario ObjUser = usr.Get(usuarioLogueado);

            if (ObjUser.Entidad_Usuario.Id != encabezado.IdEntidad.Id && ObjUser.Entidad_Usuario.Id != Utility.Utilitarios.entidadAdministradora)
                tak = new XML_Errores[0];

            var result = from c in tak
                         select new string[] {  Convert.ToString(encabezado.IdArchivo.Nombre),
                         Convert.ToString(c.Detalle),
                         };

            return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
        }

        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.XML_EncabezadoService.XML_EncabezadoServiceClient enc = new FGA_En_Linea.XML_EncabezadoService.XML_EncabezadoServiceClient();
        private readonly FGA_En_Linea.ArchivoEstadoService.ServiceOf_ArchivoEstadoClient arce = new FGA_En_Linea.ArchivoEstadoService.ServiceOf_ArchivoEstadoClient();
        private readonly FGA_En_Linea.XML_ErrorService.XML_ErrorServiceClient err = new FGA_En_Linea.XML_ErrorService.XML_ErrorServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        public CierreController()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                usr.Close();
                ent.Close();
                enc.Close();
                arce.Close();
                err.Close();
                sp.Close();
            }
            base.Dispose(disposing);
        }
    }
}