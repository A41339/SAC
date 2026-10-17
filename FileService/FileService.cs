using FGA.Models;
using FGA.Utility;
using FileService.Bulk;
using FileService.Notifications;
using FileService.Processors;
using FileService.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace FileService
{
    public partial class FileService : ServiceBase
    {
        private readonly String minutes = System.Configuration.ConfigurationManager.AppSettings["Minutes"];
        private readonly XmlProcessorFactory _processorFactory = new XmlProcessorFactory();

        public FileService()
        {
            InitializeComponent();
        }

        public void StartInteractive(string[] args)
        {
            OnStart(args);
        }

        public void StopInteractive()
        {
            OnStop();
        }

        public void EjecutarUnaVez()
        {
            ServiceLogger.Log("FileService ejecutando pasada única (/runonce)...");
            ProcessFiles();
            ServiceLogger.Log("FileService pasada única finalizada.");
        }

        protected override void OnStart(string[] args)
        {
            ServiceLogger.Log("FileService iniciando servicio...");
            int intervalMs = 20000;
            if (!string.IsNullOrEmpty(minutes) && int.TryParse(minutes, out int mins))
            {
                intervalMs = mins * 60 * 1000;
            }
            monitor.Interval = intervalMs;
            monitor.Enabled = true;
            monitor.Start();
            ServiceLogger.Log(string.Format("FileService iniciado. Monitoreo configurado cada {0} ms ({1} seg)", monitor.Interval, monitor.Interval / 1000));

            // Ejecución inmediata en segundo plano al arrancar el servicio
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ProcessFiles();
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogError("Ejecución inicial OnStart", ex);
                }
            });
        }

        protected override void OnStop()
        {
            ServiceLogger.Log("FileService deteniéndose...");
            monitor.Enabled = false;
            monitor.Stop();
            ServiceLogger.Log("FileService detenido.");
        }

        private void ProcessFiles()
        {  
            ServiceLogger.Log("Revisando cargas asincrónicas pendientes en base de datos...");

            List<CargaAsincronica> lista;
            try
            {
                using (var db = new SIContext())
                {
                    lista = db.CargaAsincronica.Where(o => o.IndEstado == 0).ToList();
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError("Conexión / consulta de CargaAsincronica en SIContext", ex);
                return;
            }

            if (lista == null || lista.Count == 0)
            {
                ServiceLogger.Log("No hay cargas asincrónicas pendientes (IndEstado = 0).");
                return;
            }

            ServiceLogger.Log(string.Format("Se encontraron {0} carga(s) asincrónica(s) pendiente(s).", lista.Count));

            foreach (CargaAsincronica detalle in lista)
            {
                ServiceLogger.Log(string.Format("Procesando Carga ID: {0}, Entidad: {1}, Ruta: {2}", detalle.Id, detalle.IdEntidad, detalle.FilePath));

                try
                {
                    using (var db = new SIContext())
                    {
                        var entity = db.CargaAsincronica.FirstOrDefault(o => o.Id == detalle.Id);
                        if (entity != null)
                        {
                            entity.IndEstado = 1;
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogError("Actualizar IndEstado = 1 para Carga ID " + detalle.Id, ex);
                }

                if (!Directory.Exists(detalle.FilePath))
                {
                    ServiceLogger.Log(string.Format("ADVERTENCIA: La ruta de archivos no existe o no es accesible: '{0}'", detalle.FilePath));
                    continue;
                }

                string[] files = Directory.GetFiles(detalle.FilePath);
                ServiceLogger.Log(string.Format("Archivos encontrados en la carpeta: {0}", files.Length));

                List<long> encabezadosConError = new List<long>();

                foreach (string extractedFile in files)
                {
                    ServiceLogger.Log(string.Format("Lectura de archivo XML: {0}", Path.GetFileName(extractedFile)));

                    try
                    {
                        XML_Encabezado encabezado = null;
                        using (var reader = XmlReader.Create(extractedFile))
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("Encabezado"))
                                {
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    encabezado = (from t in xDoc.Descendants("Encabezado").Elements()
                                                  let IdEntidad = xDoc.Descendants("Encabezado").Elements().ElementAt(5).Value
                                                  let IdArchivo = xDoc.Descendants("Encabezado").Elements().ElementAt(2).Value
                                                  let Fecha = xDoc.Descendants("Encabezado").Elements().ElementAt(4).Value
                                                  select new XML_Encabezado
                                                  {
                                                      Periodo = DateTime.ParseExact(Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                      IdArchivo_Id = Utilitarios.ObtenerIdArchivo(IdArchivo)
                                                  }).FirstOrDefault();
                                    break;
                                }
                            }
                        }

                        if (encabezado == null)
                        {
                            ServiceLogger.Log(string.Format("No se encontró nodo <Encabezado> válido en: {0}", Path.GetFileName(extractedFile)));
                            continue;
                        }

                        if (encabezado.IdArchivo_Id != Utilitarios.xml_contable_estado)
                        {
                            encabezado.FechaCarga = DateTime.Now;
                            encabezado.IdEstado_Id = FGA.Utility.Utilitarios.archivoCargado;
                            encabezado.IdUsuario_Id = detalle.IdUsuario;
                            encabezado.IdEntidad_Id = detalle.IdEntidad;
                            encabezado.Periodo = new DateTime(encabezado.Periodo.Year, encabezado.Periodo.Month, 1);
                            encabezado.IdEntidad = null;
                            encabezado.IdArchivo = null;

                            using (var db = new SIContext())
                            {
                                db.XML_Encabezados.Add(encabezado);
                                db.SaveChanges();
                            }

                            ServiceLogger.Log(string.Format("Encabezado registrado con ID: {0}, TipoArchivo: {1}", encabezado.Id, encabezado.IdArchivo_Id));

                            bool tieneErrores = Process(extractedFile, encabezado.Id, false);
                            if (tieneErrores)
                            {
                                encabezadosConError.Add(encabezado.Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceLogger.LogError("Procesamiento de archivo " + extractedFile, ex);
                    }
                }

                if (encabezadosConError.Count > 0)
                {
                    try
                    {
                        ServiceLogger.Log(string.Format("Enviando correo consolidado para {0} encabezado(s) con error...", encabezadosConError.Count));
                        XmlErrorNotifier.EnviarCorreoConsolidadoErrores(encabezadosConError);
                    }
                    catch (Exception ex)
                    {
                        ServiceLogger.LogError("Envío de correo consolidado de errores", ex);
                    }
                }

                try
                {
                    if (Directory.Exists(detalle.FilePath))
                    {
                        Directory.Delete(detalle.FilePath, true);
                        ServiceLogger.Log(string.Format("Carpeta temporal eliminada: {0}", detalle.FilePath));
                    }
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogError("Eliminar carpeta temporal " + detalle.FilePath, ex);
                }
            }

            ServiceLogger.Log("Ciclo de revisión finalizado.");
        }

        public bool Process(string xmlPath, Int64 idEncabezado, bool enviarCorreoIndividual = false)
        {
            var settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.IgnoreProcessingInstructions = true;

            var reader = XmlReader.Create(xmlPath, settings);
            SIContext db = new SIContext();
            db.Database.CommandTimeout = 300;
            db.Configuration.AutoDetectChangesEnabled = false;
            XML_Encabezado encabezado = db.XML_Encabezados.Include("IdUsuario").Include("IdArchivo").Include("IdEntidad").FirstOrDefault(o => o.Id == idEncabezado);
            encabezado.Cantidad = 0;
            bool tieneErrores = false;

            try
            {
                var cant = db.XML_Encabezados
                                         .Where(o => o.IdEntidad_Id == encabezado.IdEntidad_Id &&
                                                o.IdEstado_Id != Utilitarios.archivoEliminado && o.Periodo == encabezado.Periodo &&
                                                o.IdArchivo_Id == encabezado.IdArchivo_Id).Count();

                if (cant > 1)
                {
                    XmlErrorLogger.AgregarError("El archivo ya fue cargado previamente", encabezado, db);
                }
                else
                {
                    var xml_contable_estados = db.XML_Encabezados
                                              .Where(o => o.IdEntidad_Id == encabezado.IdEntidad_Id &&
                                                     o.IdEstado_Id == Utilitarios.archivoAceptado && o.Periodo == encabezado.Periodo &&
                                                     o.IdArchivo_Id == Utilitarios.xml_contable_estado).FirstOrDefault();

                    if (encabezado.IdArchivo_Id != Utilitarios.xml_contable_estado && xml_contable_estados == null)
                    {
                        XmlErrorLogger.AgregarError("No se ha cargado el archivo XML_Contable_Estados.xml", encabezado, db);
                    }
                    else
                    {
                        var processor = _processorFactory.ObtenerProcesador(encabezado.IdArchivo_Id);
                        if (processor == null)
                        {
                            XmlErrorLogger.AgregarError("Archivo no implementado", encabezado, db);
                        }
                        else
                        {
                            processor.Procesar(reader, encabezado, db, xml_contable_estados != null ? (long?)xml_contable_estados.Id : null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                XmlErrorLogger.AgregarError("Error inesperado: " + e.Message, encabezado, db);
                ServiceLogger.LogError("Process para Encabezado #" + idEncabezado, e);
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
                var errores = db.XML_Errores.Where(o => o.IdEncabezado_Id == encabezado.Id).Count();
                tieneErrores = errores > 0;

                if (!tieneErrores)
                {
                    encabezado.IdEstado = db.ArchivoEstados.Where(o => o.Id == Utilitarios.archivoAceptado).FirstOrDefault();
                }
                else
                {
                    encabezado.IdEstado = db.ArchivoEstados.Where(o => o.Id == Utilitarios.archivoErrores).FirstOrDefault();
                    if (enviarCorreoIndividual)
                    {
                        XmlErrorNotifier.EnviarCorreoConsolidadoErrores(new List<long> { encabezado.Id });
                    }
                }

                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
                db.Dispose();
                reader.Dispose();

                ServiceLogger.Log(string.Format("Encabezado #{0} procesado. Estado: {1}, Errores: {2}", idEncabezado, tieneErrores ? "Con errores" : "Aceptado", errores));
            }

            return tieneErrores;
        }

        private void Monitor_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.monitor.Stop();

            try
            {
                this.ProcessFiles();               
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError("Monitor_Tick ejecución general", ex);
            }
            finally
            {
                this.monitor.Start();
            }
        }
    }
}
