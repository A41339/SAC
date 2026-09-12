using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FGA.Controllers;
using FGA.Models;
using FGA_En_Linea.CreditoService;

namespace FGA_En_Linea.Controllers
{
    [Authorize]
    public class CreditosController : BaseController
    {
        private CreditoServiceClient service = new CreditoServiceClient();

        // GET: Creditos/Index (Marketplace)
        public ActionResult Index()
        {
            var ofertas = service.GetOfertasActivas();
            return View(ofertas);
        }

        // GET: Creditos/MisSolicitudes
        public ActionResult MisSolicitudes()
        {
            string entidadId = Session["Entidad"]?.ToString();
            var solicitudes = service.GetSolicitudesPorEntidad(entidadId);
            return View(solicitudes);
        }

        // GET: Creditos/Gestion (Para Entidades Oferentes)
        public ActionResult Gestion()
        {
            string entidadId = Session["Entidad"]?.ToString();
            var solicitudes = service.GetSolicitudesRecibidas(entidadId);
            return View(solicitudes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Solicitar(int ofertaId, decimal monto, string comentarios, string telefono, string email)
        {
            try
            {
                var usuario = (Usuario)Session["Usuario"];
                var entidadId = Session["Entidad"]?.ToString();

                var solicitud = new Creditos_Solicitudes
                {
                    Oferta_Id = ofertaId,
                    EntidadSolicitante_Id = entidadId,
                    UsuarioSolicitante_Id = usuario.Id.Value,
                    Mon_MontoSolicitado = monto,
                    Justificacion = comentarios,
                    TelefonoNotificacionSMS = telefono,
                    EmailNotificacion = email
                };

                service.CrearSolicitud(solicitud);
                // La notificación SMS ya se maneja en el servicio

                TempData["Success"] = "Solicitud enviada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al enviar la solicitud: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: Creditos/Publicar
        public ActionResult Publicar()
        {
            ViewBag.TiposDoc = service.GetTiposDocumento();
            ViewBag.Monedas = service.GetMonedas();
            return View();
        }

        // POST: Creditos/Publicar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Publicar(Creditos_Ofertas oferta, int[] requisitos)
        {
            try
            {
                var usuario = (Usuario)Session["Usuario"];
                oferta.EntidadOferente_Id = Session["Entidad"]?.ToString();
                oferta.UsuarioPublicador_Id = usuario.Id.Value;

                service.PublicarOferta(oferta, requisitos?.ToList());

                TempData["Success"] = "Oferta publicada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al publicar la oferta: " + ex.Message;
                ViewBag.TiposDoc = service.GetTiposDocumento();
                ViewBag.Monedas = service.GetMonedas();
                return View(oferta);
            }
        }

        // POST: Creditos/Resolver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resolver(int solicitudId, int estadoId, string motivoRechazo)
        {
            try
            {
                var usuario = (Usuario)Session["Usuario"];
                service.ResolverSolicitud(solicitudId, estadoId, usuario.Id.Value, motivoRechazo);
                // El cambio de estado a "Firma Pendiente" y generación de Pagaré se maneja en el servicio

                TempData["Success"] = "Solicitud procesada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar la solicitud: " + ex.Message;
            }

            return RedirectToAction("Gestion");
        }

        // GET: Creditos/Formalizar
        public ActionResult Formalizar(int id)
        {
            var solicitudes = service.GetSolicitudesPorEntidad(Session["Entidad"]?.ToString());
            var model = solicitudes.FirstOrDefault(s => s.Id == id);
            if (model == null) return HttpNotFound();

            ViewBag.Requisitos = service.GetRequisitosOferta(model.Oferta_Id);
            ViewBag.Documentos = service.GetDocumentosPorSolicitud(id);

            return View(model);
        }

        // POST: Creditos/SubirDocumento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubirDocumento(int solicitudId, int tipoDocumentoId, HttpPostedFileBase archivo)
        {
            try
            {
                if (archivo != null && archivo.ContentLength > 0)
                {
                    string folder = System.Configuration.ConfigurationManager.AppSettings["RutaUploads"] ?? @"C:\FFC\Uploads\";
                    string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(archivo.FileName);
                    string path = Path.Combine(folder, fileName);

                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    archivo.SaveAs(path);

                    var usuario = (Usuario)Session["Usuario"];
                    var doc = new Creditos_Documentos
                    {
                        Solicitud_Id = solicitudId,
                        TipoDocumento_Id = tipoDocumentoId,
                        RutaArchivoPDF = path,
                        NombreArchivoOriginal = archivo.FileName,
                        UsuarioCarga_Id = usuario.Id.Value,
                        Fec_Carga = DateTime.Now,
                        Ind_FirmaValida = false
                    };

                    service.CargarDocumento(doc);
                    TempData["Success"] = "Documento cargado correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al subir documento: " + ex.Message;
            }

            return RedirectToAction("Formalizar", new { id = solicitudId });
        }

        // GET: Creditos/DescargarPagare
        public ActionResult DescargarPagare(int id)
        {
            var documentos = service.GetDocumentosPorSolicitud(id);
            var pagare = documentos.FirstOrDefault(d => d.TipoDocumento_Id == 1 && d.RutaArchivoPDF.Contains("Pagare"));
            
            if (pagare != null && System.IO.File.Exists(pagare.RutaArchivoPDF))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(pagare.RutaArchivoPDF);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, pagare.NombreArchivoOriginal);
            }

            TempData["Error"] = "El pagaré aún no ha sido generado o no se encuentra.";
            return RedirectToAction("Formalizar", new { id = id });
        }

        // POST: Creditos/CompletarFormalizacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompletarFormalizacion(int solicitudId)
        {
            try
            {
                var usuario = (Usuario)Session["Usuario"];
                // Cambiar a estado 5: En Revisión (Formalizado por cliente)
                service.ResolverSolicitud(solicitudId, 5, usuario.Id.Value, "Solicitante completó carga de documentos.");
                TempData["Success"] = "Trámite finalizado. La entidad financiera revisará sus documentos.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al finalizar trámite: " + ex.Message;
            }

            return RedirectToAction("MisSolicitudes");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (service.State == System.ServiceModel.CommunicationState.Opened)
                {
                    service.Close();
                }
            }
            base.Dispose(disposing);
        }
    }
}
