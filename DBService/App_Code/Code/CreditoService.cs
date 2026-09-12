using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using FGA.Models;
using System.IO;

public class CreditoService : ICreditoService
{
    public List<Creditos_Ofertas> GetOfertasActivas()
    {
        using (var db = new SIContext())
        {
            return db.Creditos_Ofertas
                .Include(o => o.EntidadOferente)
                .Include(o => o.Moneda)
                .Include(o => o.Requisitos.Select(r => r.TipoDocumento))
                .Where(o => o.Ind_EstadoActiva && (o.Fec_Vencimiento == null || o.Fec_Vencimiento > DateTime.Now))
                .ToList();
        }
    }

    public Creditos_Ofertas GetOfertaById(int id)
    {
        using (var db = new SIContext())
        {
            return db.Creditos_Ofertas
                .Include(o => o.EntidadOferente)
                .Include(o => o.Moneda)
                .Include(o => o.Requisitos.Select(r => r.TipoDocumento))
                .FirstOrDefault(o => o.Id == id);
        }
    }

    public void PublicarOferta(Creditos_Ofertas oferta, List<int> requisitosIds)
    {
        using (var db = new SIContext())
        {
            oferta.Fec_Publicacion = DateTime.Now;
            oferta.Ind_EstadoActiva = true;
            db.Creditos_Ofertas.Add(oferta);
            db.SaveChanges(); // Para obtener el Id

            if (requisitosIds != null)
            {
                foreach (var reqId in requisitosIds)
                {
                    db.Creditos_OfertaRequisitos.Add(new Creditos_OfertaRequisitos
                    {
                        Oferta_Id = oferta.Id,
                        TipoDocumento_Id = reqId
                    });
                }
                db.SaveChanges();
            }
        }
    }

    public List<Creditos_OfertaRequisitos> GetRequisitosOferta(int ofertaId)
    {
        using (var db = new SIContext())
        {
            return db.Creditos_OfertaRequisitos
                .Include(r => r.TipoDocumento)
                .Where(r => r.Oferta_Id == ofertaId)
                .ToList();
        }
    }

    public void InactivarOferta(int id, int usuarioModificaId)
    {
        using (var db = new SIContext())
        {
            var oferta = db.Creditos_Ofertas.Find(id);
            if (oferta != null)
            {
                oferta.Ind_EstadoActiva = false;
                oferta.UsuarioModifica_Id = usuarioModificaId;
                oferta.Fec_UltimaModificacion = DateTime.Now;
                db.SaveChanges();
            }
        }
    }

    public List<Creditos_Solicitudes> GetSolicitudesPorEntidad(string entidadId)
    {
        using (var db = new SIContext())
        {
            return db.Creditos_Solicitudes
                .Include(s => s.Oferta.EntidadOferente)
                .Include(s => s.Oferta.Moneda)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.EntidadSolicitante_Id == entidadId)
                .OrderByDescending(s => s.Fec_Solicitud)
                .ToList();
        }
    }

    public List<Creditos_Solicitudes> GetSolicitudesRecibidas(string entidadOferenteId)
    {
        using (var db = new SIContext())
        {
            return db.Creditos_Solicitudes
                .Include(s => s.EntidadSolicitante)
                .Include(s => s.Oferta.Moneda)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.Oferta.EntidadOferente_Id == entidadOferenteId)
                .OrderByDescending(s => s.Fec_Solicitud)
                .ToList();
        }
    }

    public void CrearSolicitud(Creditos_Solicitudes solicitud)
    {
        using (var db = new SIContext())
        {
            solicitud.Fec_Solicitud = DateTime.Now;
            // Estado inicial: Pendiente (asumiendo Id 1 según el script)
            solicitud.EstadoSolicitud_Id = 1; 
            db.Creditos_Solicitudes.Add(solicitud);
            db.SaveChanges();
            
            // Disparar envío de SMS inicial al Oferente
            var oferta = db.Creditos_Ofertas.Find(solicitud.Oferta_Id);
            if (oferta != null)
            {
                string mensaje = "FFC: Nueva solicitud de crédito por " + solicitud.Mon_MontoSolicitado.ToString("N2") + " de la entidad " + solicitud.EntidadSolicitante_Id.ToString() + ".";
                bool exito = FGA.Utilities.SMSHelper.SendSMS(oferta.TelefonoNotificacionSMS, mensaje).Result;

                db.Creditos_SMSLog.Add(new Creditos_SMSLog
                {
                    Solicitud_Id = solicitud.Id,
                    TelefonoDestino = oferta.TelefonoNotificacionSMS,
                    Mensaje = mensaje,
                    Fec_Envio = DateTime.Now,
                    Ind_Exitoso = exito
                });
                db.SaveChanges();
            }
        }
    }

    public void ResolverSolicitud(int solicitudId, int estadoId, int usuarioId, string motivoRechazo = null)
    {
        using (var db = new SIContext())
        {
            var sol = db.Creditos_Solicitudes.Find(solicitudId);
            if (sol != null)
            {
                sol.EstadoSolicitud_Id = estadoId;
                sol.UsuarioResolucion_Id = usuarioId;
                sol.Fec_Resolucion = DateTime.Now;
                sol.MotivoRechazo = motivoRechazo;

                // Si se aprueba (Aceptada), generamos el Pagaré automáticamente
                if (estadoId == 2)
                {
                    try
                    {
                        var generator = new Entities.Entities.Transac.PagareDoc();
                        string ruta = generator.Generate(sol);
                        
                        // Cambiamos a estado 4 (Firma Pendiente)
                        sol.EstadoSolicitud_Id = 4;
                        
                        // Registramos el documento generado en la bitácora o tabla de documentos
                        var docPagare = new Creditos_Documentos
                        {
                            Solicitud_Id = sol.Id,
                            TipoDocumento_Id = 1, // Pagaré (asumiendo 1 es el ID para Pagaré)
                            RutaArchivoPDF = ruta,
                            NombreArchivoOriginal = Path.GetFileName(ruta),
                            Fec_Carga = DateTime.Now,
                            UsuarioCarga_Id = usuarioId,
                            Ind_FirmaValida = false
                        };
                        db.Creditos_Documentos.Add(docPagare);
                    }
                    catch (Exception ex)
                    {
                        // Log error pero permitimos que la resolución continúe
                        // En un sistema real usaríamos un logger
                    }
                }

                db.SaveChanges();
                
                // Disparar envío de SMS de resolución al Solicitante
                // Necesitamos el teléfono del usuario solicitante o de la entidad
                var userSol = db.Usuarios.Find(sol.UsuarioSolicitante_Id);
                if (userSol != null && !string.IsNullOrEmpty(userSol.Telefono))
                {
                    string estadoStr = estadoId == 2 ? "ACEPTADA" : (estadoId == 3 ? "RECHAZADA" : "ACTUALIZADA");
                    string mensaje = "FFC: Su solicitud de crédito #" + sol.Id + " ha sido " + estadoStr + ".";
                    bool exito = FGA.Utilities.SMSHelper.SendSMS(userSol.Telefono, mensaje).Result;

                    db.Creditos_SMSLog.Add(new Creditos_SMSLog
                    {
                        Solicitud_Id = sol.Id,
                        TelefonoDestino = userSol.Telefono,
                        Mensaje = mensaje,
                        Fec_Envio = DateTime.Now,
                        Ind_Exitoso = exito
                    });
                    db.SaveChanges();
                }
            }
        }
    }

    public void CargarDocumento(Creditos_Documentos documento)
    {
        using (var db = new SIContext())
        {
            documento.Fec_Carga = DateTime.Now;
            db.Creditos_Documentos.Add(documento);
            db.SaveChanges();
        }
    }

    public void ValidarDocumento(int documentoId, bool esValido, int usuarioSacId, string comentarios)
    {
        using (var db = new SIContext())
        {
            var doc = db.Creditos_Documentos.Find(documentoId);
            if (doc != null)
            {
                doc.Ind_FirmaValida = esValido;
                doc.ValidadoPorSAC_Id = usuarioSacId;
                doc.Fec_ValidacionSAC = DateTime.Now;
                doc.ComentariosRevision = comentarios;
                db.SaveChanges();
            }
        }
    }

    public List<Creditos_TipoDocumento> GetTiposDocumento()
    {
        using (var db = new SIContext())
        {
            return db.Creditos_TipoDocumento.Where(t => t.Ind_Activo).ToList();
        }
    }

    public List<Creditos_Documentos> GetDocumentosPorSolicitud(int solicitudId)
    {
        using (var db = new SIContext())
        {
            return db.Creditos_Documentos
                .Include(d => d.TipoDocumento)
                .Where(d => d.Solicitud_Id == solicitudId)
                .ToList();
        }
    }

    public List<Creditos_Moneda> GetMonedas()
    {
        using (var db = new SIContext())
        {
            return db.Creditos_Moneda.Where(m => m.Ind_Activo).ToList();
        }
    }
}
