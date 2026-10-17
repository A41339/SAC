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

        protected override void OnStart(string[] args)
        {
            monitor.Interval = int.Parse(minutes) * 20 * 1000;
            monitor.Enabled = true;
            monitor.Start();
        }

        protected override void OnStop()
        {
            monitor.Enabled = false;
        }

        private void ProcessFiles()
        {  
            CargaAsincronicaService.CargaAsincronicaServiceClient serv = new CargaAsincronicaService.CargaAsincronicaServiceClient();
            var lista = serv.GetPendientesAsyc(); 
            XmlReader reader;
            XML_Encabezado encabezado = new XML_Encabezado();

            foreach (CargaAsincronica detalle in lista)
            {
                detalle.IndEstado = 1;
                serv.Update(detalle);

                List<long> encabezadosConError = new List<long>();

                foreach (string extractedFile in Directory.GetFiles(detalle.FilePath))
                {
                    try
                    {
                        reader = XmlReader.Create(extractedFile);
                        while (reader != null && reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (reader.Name.Equals("Encabezado"))
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
                                        reader.Dispose();
                                    }
                                    break;
                            }
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

                            using (XML_EncabezadoService.XML_EncabezadoServiceClient enc = new XML_EncabezadoService.XML_EncabezadoServiceClient())
                            {
                                enc.Add(ref encabezado);                               
                                bool tieneErrores = Process(extractedFile, encabezado.Id, false);
                                if (tieneErrores)
                                {
                                    encabezadosConError.Add(encabezado.Id);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                if (encabezadosConError.Count > 0)
                {
                    try
                    {
                        XmlErrorNotifier.EnviarCorreoConsolidadoErrores(encabezadosConError);
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    if (Directory.Exists(detalle.FilePath))
                    {
                        // Si la carpeta contiene archivos o subdirectorios, puedes usar el siguiente método
                        Directory.Delete(detalle.FilePath, true); // true para borrar también el contenido
                    }
                }
                catch (Exception) {                
                }
            }
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
            catch (Exception)
            {                 
            }
            finally
            {
                this.monitor.Start();
            }
        }
    }
}
