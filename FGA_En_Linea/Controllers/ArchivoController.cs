using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Entities.Entities.Procedures;
using FGA.Models;
using FGA_En_Linea.ProcessFileService;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System.Collections.Generic;

namespace FGA.Controllers
{
    public class ArchivoController : BaseController
    {
        public static string xml = System.Configuration.ConfigurationManager.AppSettings["RutaUploads"];

        public ActionResult Index()
        {
            Load();
            ViewBag.TiposXML = arc.GetAll().OrderBy(o => o.Nombre).ToList();
            return View(GetXML());
        }

        public ActionResult Sugef()
        {
            Load();
            return View(GetSUGEF());
        }

        public ActionResult SugefIndicador()
        {
            Load();
            return View(GetSUGEFIndicador());
        }


        public ActionResult SugefCartera()
        {
            Load();
            return View(GetSUGEFCartera());
        }

        public PartialViewResult GetPartial()
        {
            return PartialView("File", GetXML());
        }

        public PartialViewResult GetSugef()
        {
            return PartialView("FileSugef", GetSUGEF());
        }
        public PartialViewResult GetSugefIndicador()
        {
            return PartialView("FileSugefIndicador", GetSUGEFIndicador());
        }

        public PartialViewResult GetSugefCartera()
        {
            return PartialView("FileSugefCartera", GetSUGEFCartera());
        }

        public ActionResult Details(int? id)
        {
            Session["File"] = id == null ? 0 : id;
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var idEncabezado = long.Parse(HttpContext.Session["File"].ToString());
                var tak = err.GetByEncabezado(idEncabezado).ToArray();
                XML_Encabezado encabezado = enc.Get(idEncabezado.ToString());
                var usuarioLogueado = Env.GetUserInfo("userid").ToString();
                Usuario ObjUser = usr.Get(usuarioLogueado);

                if (ObjUser.Entidad_Usuario.Id != encabezado.IdEntidad.Id)
                    tak = new XML_Errores[0];

                var result = from c in tak
                             select new string[] {  Convert.ToString(encabezado.IdArchivo.Nombre),
                         Convert.ToString(c.Detalle),
                         };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        private FGA_Consultar_ArchivosCargados_Result[] GetXML()
        {
            string id = Env.GetUserInfo("userid");
            Usuario ObjUser = usr.Get(id.ToString());
            var periodo = sp.FGA_Consultar_FechaCierre(ObjUser.Entidad_Usuario.Id);
            var files = sp.FGA_Consultar_ArchivosCargados(ObjUser.Entidad_Usuario.Id, periodo, true);
            Session["PeriodoCarga"] = Utility.Utilitarios.toUpperFirstLetter(periodo.ToString("MMMM_yyyy"));
            return files;
        }

        private List<FGA_Consultar_ArchivosCargados_Result> GetSUGEF()
        {
            return GetSUGEFFiles("Balance");
        }

        private List<FGA_Consultar_ArchivosCargados_Result> GetSUGEFIndicador()
        {
            return GetSUGEFFiles("Indicador");
        }

        private List<FGA_Consultar_ArchivosCargados_Result> GetSUGEFCartera()
        {
            return GetSUGEFFiles("Cartera");
        }


        private List<FGA_Consultar_ArchivosCargados_Result> GetSUGEFFiles(String tipo)
        {
            string id = Env.GetUserInfo("userid");
            Usuario ObjUser = usr.Get(id.ToString());
            var periodo = sp.FGA_Consultar_FechaCierre(ObjUser.Entidad_Usuario.Id);
            var files = sp.FGA_Consultar_ArchivosCargados(ObjUser.Entidad_Usuario.Id, periodo, false);
            Session["PeriodoCarga"] = Utility.Utilitarios.toUpperFirstLetter(periodo.ToString("MMMM_yyyy"));
            return files.Where(a => a.NOMBRE_XML == tipo).ToList();
        }


        [HttpPost]
        public String Index(HttpPostedFileBase file)
        {
            string id = Env.GetUserInfo("userid").ToString();
            Usuario ObjUser = usr.Get(id);
            var periodo = sp.FGA_Consultar_FechaCierre(ObjUser.Entidad_Usuario.Id);
            string mensaje = string.Empty;

            try
            {
                if (file != null)
                {
                    if (file.ContentType.Equals("application/zip") || file.ContentType.Equals("application/x-zip-compressed") || file.ContentType.Equals("application/x-compressed"))
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(xml, ObjUser.Entidad_Usuario.Nombre);
                        bool exists = System.IO.Directory.Exists(path);

                        if (!exists)
                            System.IO.Directory.CreateDirectory(path);

                        path = Path.Combine(path, Utility.Utilitarios.toUpperFirstLetter(periodo.ToString("yyyy")));
                        exists = System.IO.Directory.Exists(path);

                        if (!exists)
                            System.IO.Directory.CreateDirectory(path);

                        path = Path.Combine(path, Utility.Utilitarios.toUpperFirstLetter(periodo.ToString("MMMM")));
                        exists = System.IO.Directory.Exists(path);

                        if (!exists)
                            System.IO.Directory.CreateDirectory(path);

                        var xmlPath = Path.Combine(path, fileName);
                        file.SaveAs(xmlPath);

                        //var reader = XmlReader.Create(xmlPath);
                        //XML_Encabezado encabezado = new XML_Encabezado();

                        //try
                        //{
                        //    while (reader != null && reader.Read())
                        //    {
                        //        switch (reader.NodeType)
                        //        {
                        //            case XmlNodeType.Element:
                        //                if (reader.Name.Equals("Encabezado"))
                        //                {
                        //                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                        //                    encabezado = (from t in xDoc.Descendants("Encabezado").Elements()
                        //                                  let IdEntidad = xDoc.Descendants("Encabezado").Elements().ElementAt(5).Value
                        //                                  let IdArchivo = xDoc.Descendants("Encabezado").Elements().ElementAt(2).Value
                        //                                  let Fecha = xDoc.Descendants("Encabezado").Elements().ElementAt(4).Value
                        //                                  select new XML_Encabezado
                        //                                  {
                        //                                      Periodo = DateTime.Parse(Fecha),
                        //                                      IdEntidad = ent.GetByIden(IdEntidad),
                        //                                      IdArchivo = arc.Get(IdArchivo),
                        //                                      IdArchivo_Id = Utility.Utilitarios.ObtenerIdArchivo(IdArchivo)
                        //                                  }).FirstOrDefault();
                        //                    reader.Dispose();
                        //                }
                        //                break;
                        //        }
                        //    }
                        //}
                        //catch (Exception)
                        //{
                        //}
                        //finally
                        //{
                        //    reader.Dispose();
                        //}

                        /* if (string.IsNullOrEmpty(Utility.Utilitarios.GetFileName(encabezado.IdArchivo_Id)))
                             mensaje = "El archivo que está cargando, no es requerido actualmente.";
                         else
                         {
                             encabezado.FechaCarga = DateTime.Now;
                             encabezado.IdEstado_Id = Utility.Utilitarios.archivoCargado;
                             encabezado.IdUsuario_Id = ObjUser.Id.Value;
                             encabezado.IdEntidad_Id = encabezado.IdEntidad.Id;

                             if (encabezado.IdEntidad == null)
                                 mensaje = "La entidad que esta intentando registrar no ha sido configurada. Revise el campo IdEntidad del XML";
                             else
                             {
                                 if (encabezado.IdEntidad_Id != ObjUser.Entidad_Usuario.Id)
                                     mensaje = "El usuario no puede cargar un archivo de otra entidad";
                                 else
                                 {
                                     if (enc.IsLoadingFile(encabezado.IdEntidad_Id, encabezado.Periodo))
                                         mensaje = "Existe un archivo cargandose en este momento. Espere a que finalice para cargar otro. ";
                                     else
                                     {
                                         if (enc.IsUpload(encabezado.IdEntidad_Id, encabezado.IdArchivo_Id, encabezado.Periodo))
                                             mensaje = "El archivo " + encabezado.IdArchivo.Nombre + " ya fue cargado para el periodo " + encabezado.Periodo.ToShortDateString();
                                         else
                                         {
                                             DateTime periodoCargar = sp.FGA_Consultar_FechaCierre(ObjUser.Entidad_Usuario.Id);
                                             if (encabezado.Periodo.Month != periodoCargar.Month || encabezado.Periodo.Year != periodoCargar.Year)
                                                 mensaje = "El periodo del archivo " + encabezado.IdArchivo.Nombre + " no es válido. Valor actual: " + encabezado.Periodo.ToShortDateString() + ". Debe ser del " + periodoCargar.ToString("MM/yyyy");
                                             else
                                             {
                                                 encabezado.Periodo = new DateTime(encabezado.Periodo.Year, encabezado.Periodo.Month, 1);
                                                 encabezado.IdEntidad = null;
                                                 encabezado.IdArchivo = null;
                                                 enc.Add(ref encabezado);*/
                        ProcessFileServiceClient process = new ProcessFileServiceClient();
                        mensaje = process.SaveFile(path, fileName, ObjUser.Entidad_Usuario.Id, ObjUser.Id.Value, periodo);

                        /*}
                    }
                }
            }
        }
    }*/
                    }
                    else
                        mensaje = "El archivo debe tener extensión .zip para poder cargarlo";
                }
                else {
                    mensaje = "Debe cargar el archivo .zip con los archivos del mes";
                }
            }
            catch (Exception e)
            {
                mensaje = "Error al guardar el archivo: " + e.Message;
            }

            return mensaje;
        }


        [HttpPost]
        public String Sugef(HttpPostedFileBase xlsFile)
        {
            int id = int.Parse(Env.GetUserInfo("userid").ToString());
            String mensaje;

            try
            {
                mensaje = string.Empty;
                if (xlsFile.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                {

                    if (xlsFile != null)
                    {
                        var path = Path.Combine(xml, "FGA");
                        bool exists = Directory.Exists(path);

                        if (!exists)
                            Directory.CreateDirectory(path);

                        string guid = Guid.NewGuid().ToString() + ".xlsx";
                        var xlsPath = Path.Combine(path, guid);
                        xlsFile.SaveAs(xlsPath);
                        ProcessFileServiceClient process = new ProcessFileServiceClient();
                        Sugef_Encabezado sugef_Encabezado = new Sugef_Encabezado();
                        DateTime periodo = DateTime.Now;

                        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(xlsPath, false))
                        {
                            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                            var sheets = workbookPart.Workbook.Descendants<Sheet>();
                            Sheet sheet = sheets.FirstOrDefault();
                            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                            var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();
                            periodo = DateTime.FromOADate(int.Parse(rows[1].Elements<Cell>().ToList()[1].CellValue.InnerText));
                        }
                        sugef_Encabezado.Nombre = "Balance";
                        sugef_Encabezado.IdEstado_Id = 1;
                        sugef_Encabezado.IdUsuario_Id = id;
                        sugef_Encabezado.Periodo = periodo;
                        sugef_Encabezado.FechaCarga = DateTime.Now;
                        this.senc.Add(ref sugef_Encabezado);

                        ProcessFileServiceClient processFileServiceClient = new ProcessFileServiceClient();
                        processFileServiceClient.ProcessIndustria(xlsPath, sugef_Encabezado.Id);
                    }
                }
                else
                    mensaje = "El archivo debe estar en formato excel";
            }
            catch (Exception e)
            {
                mensaje = "Error al guardar el archivo: " + e.Message;
            }

            return mensaje;
        }

        [HttpPost]

        public String SugefIndicador(HttpPostedFileBase xlsFile)
        {
            int id = int.Parse(Env.GetUserInfo("userid").ToString());
            String mensaje;

            try
            {
                mensaje = string.Empty;
                if (xlsFile.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                {
                    if (xlsFile != null)
                    {
                        var path = Path.Combine(xml, "FFC");
                        bool exists = Directory.Exists(path);

                        if (!exists)
                            Directory.CreateDirectory(path);

                        string guid = Guid.NewGuid().ToString() + ".xlsx";
                        var xlsPath = Path.Combine(path, guid);
                        xlsFile.SaveAs(xlsPath);
                        ProcessFileServiceClient process = new ProcessFileServiceClient();
                        Sugef_Encabezado sugef_Encabezado = new Sugef_Encabezado();
                        DateTime periodo = DateTime.Now;

                        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(xlsPath, false))
                        {
                            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                            var sheets = workbookPart.Workbook.Descendants<Sheet>();
                            Sheet sheet = sheets.FirstOrDefault();
                            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                            var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();
                            periodo = DateTime.FromOADate(int.Parse(rows[1].Elements<Cell>().ToList()[0].CellValue.InnerText));
                        }
                        sugef_Encabezado.Nombre = "Indicador";
                        sugef_Encabezado.IdEstado_Id = 1;
                        sugef_Encabezado.IdUsuario_Id = id;
                        sugef_Encabezado.Periodo = periodo;
                        sugef_Encabezado.FechaCarga = DateTime.Now;
                        this.senc.Add(ref sugef_Encabezado);

                        ProcessFileServiceClient processFileServiceClient = new ProcessFileServiceClient();
                        processFileServiceClient.ProcessIndicador(xlsPath, sugef_Encabezado.Id);
                    }
                }
                else
                    mensaje = "El archivo debe estar en formato excel";
            }
            catch (Exception e)
            {
                mensaje = "Error al guardar el archivo: " + e.Message;
            }

            return mensaje;
        }


        [HttpPost]

        public String SugefCartera(HttpPostedFileBase xlsFile)
        {
            int id = int.Parse(Env.GetUserInfo("userid").ToString());
            String mensaje;

            try
            {
                mensaje = string.Empty;
                if (xlsFile.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                {
                    if (xlsFile != null)
                    {
                        var path = Path.Combine(xml, "FFC");
                        bool exists = Directory.Exists(path);

                        if (!exists)
                            Directory.CreateDirectory(path);

                        string guid = Guid.NewGuid().ToString() + ".xlsx";
                        var xlsPath = Path.Combine(path, guid);
                        xlsFile.SaveAs(xlsPath);
                        ProcessFileServiceClient process = new ProcessFileServiceClient();
                        Sugef_Encabezado sugef_Encabezado = new Sugef_Encabezado();
                        DateTime periodo = DateTime.Now;

                        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(xlsPath, false))
                        {
                            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                            var sheets = workbookPart.Workbook.Descendants<Sheet>();
                            Sheet sheet = sheets.FirstOrDefault();
                            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                            var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();
                            periodo = DateTime.FromOADate(int.Parse(rows[1].Elements<Cell>().ToList()[0].CellValue.InnerText));
                        }
                        sugef_Encabezado.Nombre = "Cartera";
                        sugef_Encabezado.IdEstado_Id = 1;
                        sugef_Encabezado.IdUsuario_Id = id;
                        sugef_Encabezado.Periodo = periodo;
                        sugef_Encabezado.FechaCarga = DateTime.Now;
                        this.senc.Add(ref sugef_Encabezado);

                        ProcessFileServiceClient processFileServiceClient = new ProcessFileServiceClient();
                        processFileServiceClient.ProcessCartera(xlsPath, sugef_Encabezado.Id);
                    }
                }
                else
                    mensaje = "El archivo debe estar en formato excel";
            }
            catch (Exception e)
            {
                mensaje = "Error al guardar el archivo: " + e.Message;
            }

            return mensaje;
        }

        public ActionResult Eliminar(int id)
        {
            try
            {
                var encabezado = senc.Get(id.ToString());
                encabezado.IdEstado_Id = Utility.Utilitarios.archivoEliminado;
                encabezado.IdEstado = null;
                encabezado.IdUsuario = null;
                senc.Update(encabezado);
                Load();
            }
            catch (Exception)
            {
            }

            return View("Sugef", GetSUGEF());
        }

        public ActionResult EliminarArchIndicador(int id)
        {
            try
            {
                var encabezado = senc.Get(id.ToString());
                encabezado.IdEstado_Id = Utility.Utilitarios.archivoEliminado;
                encabezado.IdEstado = null;
                encabezado.IdUsuario = null;
                senc.Update(encabezado);

                Load();
            }
            catch (Exception)
            {
            }

            return View("SugefIndicador", GetSUGEFIndicador());
        }

        public ActionResult EliminarArchCartera(int id)
        {
            try
            {
                var encabezado = senc.Get(id.ToString());
                encabezado.IdEstado_Id = Utility.Utilitarios.archivoEliminado;
                encabezado.IdEstado = null;
                encabezado.IdUsuario = null;
                senc.Update(encabezado);

                Load();
            }
            catch (Exception)
            {
            }

            return View("SugefCartera", GetSUGEFCartera());
        }


        private readonly FGA_En_Linea.TipoXMLService.ServiceOf_TipoXMLClient arc = new FGA_En_Linea.TipoXMLService.ServiceOf_TipoXMLClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.ArchivoEstadoService.ServiceOf_ArchivoEstadoClient arces = new FGA_En_Linea.ArchivoEstadoService.ServiceOf_ArchivoEstadoClient();
        private readonly FGA_En_Linea.XML_EncabezadoService.XML_EncabezadoServiceClient enc = new FGA_En_Linea.XML_EncabezadoService.XML_EncabezadoServiceClient();
        private readonly FGA_En_Linea.Sugef_EncabezadoService.Sugef_EncabezadoServiceClient senc = new FGA_En_Linea.Sugef_EncabezadoService.Sugef_EncabezadoServiceClient();
        private readonly FGA_En_Linea.XML_ErrorService.XML_ErrorServiceClient err = new FGA_En_Linea.XML_ErrorService.XML_ErrorServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        public ArchivoController()
        {
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                arc.Close();
                usr.Close();
                ent.Close();
                arces.Close();
                enc.Close();
                err.Close();
            }
            base.Dispose(disposing);
        }
    }
}