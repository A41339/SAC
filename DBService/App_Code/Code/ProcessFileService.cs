using FGA.Models;
using FGA.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Globalization;

public class ProcessFileService : IProcessFileService, IDisposable
{

    public void AgregarError(string detalle, XML_Encabezado encabezado, SIContext db)
    {
        try
        {
            if (encabezado.IdEntidad.Ind_Validar)
            {
                var error = new XML_Errores();
                error.IdEncabezado = encabezado;
                error.Detalle = detalle;
                db.XML_Errores.Add(error);
                db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }
    }

    public void AgregarErrorSugef(string detalle, Sugef_Encabezado encabezado, SIContext db)
    {
        try
        {
            var error = new Sugef_Errores();
            error.IdEncabezado = encabezado;
            error.Detalle = detalle;
            db.Sugef_Error.Add(error);
            db.SaveChanges();
        }
        catch (Exception)
        {
        }
    }

    private string GetCellValue(WorkbookPart workbookPart, Cell cell)
    {
        string cellValue = string.Empty;

        if (cell.DataType != null)
        {
            if (cell.DataType == CellValues.SharedString)
            {
                int id = -1;

                if (Int32.TryParse(cell.InnerText, out id))
                {
                    SharedStringItem item = GetSharedStringItemById(workbookPart, id);

                    if (item.Text != null)
                    {
                        cellValue = item.Text.Text;
                    }
                    else if (item.InnerText != null)
                    {
                        cellValue = item.InnerText;
                    }
                    else if (item.InnerXml != null)
                    {
                        cellValue = item.InnerXml;
                    }
                }
            }
        }

        return cellValue;
    }

    public static SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, int id)
    {
        return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
    }

    public bool Process(string xmlPath, Int64 idEncabezado)
    {
        bool result = true;
        var settings = new XmlReaderSettings();
        settings.IgnoreComments = true;
        settings.IgnoreWhitespace = true;
        settings.IgnoreProcessingInstructions = true;

        var reader = XmlReader.Create(xmlPath, settings);
        SIContext db = new SIContext();
        db.Configuration.AutoDetectChangesEnabled = false;
        XML_Encabezado encabezado = db.XML_Encabezados.Include("IdUsuario").Include("IdArchivo").Include("IdEntidad").FirstOrDefault(o => o.Id == idEncabezado);
        encabezado.Cantidad = 0;

        try
        {
            var cant = db.XML_Encabezados
                                     .Where(o => o.IdEntidad_Id == encabezado.IdEntidad_Id &&
                                            o.Periodo == encabezado.Periodo &&
                                            o.IdArchivo_Id == encabezado.IdArchivo_Id &&
                                            o.IdEstado_Id != Utilitarios.archivoEliminado).Count();

            if (cant > 1)
            {
                AgregarError("El archivo ya fue cargado previamente", encabezado, db);
                result = false;
            }

            var xml_contable_estados = db.XML_Encabezados
                                      .Where(o => o.IdEntidad_Id == encabezado.IdEntidad_Id &&
                                             o.IdEstado_Id == Utilitarios.archivoAceptado && o.Periodo == encabezado.Periodo &&
                                             o.IdArchivo_Id == Utilitarios.xml_contable_estado).FirstOrDefault();

            if (encabezado.IdArchivo_Id != Utilitarios.xml_contable_estado && xml_contable_estados == null)
            {
                AgregarError("No se ha cargado el archivo XML_Contable_Estados.xml", encabezado, db);
                result = false;
            }
            else
            {
                switch (encabezado.IdArchivo_Id)
                {
                    case Utilitarios.xml_contable_estado:
                        ProcesarContable_Estado_XML(reader, encabezado, db);
                        break;
                    default:
                        AgregarError("Archivo no implementado", encabezado, db);
                        break;
                }
            }

        }
        catch (Exception e)
        {
            AgregarError("Error inesperado: " + e.Message, encabezado, db);
            result = false;
        }
        finally
        {
            db.Configuration.AutoDetectChangesEnabled = true;
            var errores = db.XML_Errores.Where(o => o.IdEncabezado.Id == encabezado.Id).Count();

            if (errores == 0)
            {
                encabezado.IdEstado = db.ArchivoEstados.Where(o => o.Id == Utilitarios.archivoAceptado).FirstOrDefault();
            }
            else
            {
                encabezado.IdEstado = db.ArchivoEstados.Where(o => o.Id == Utilitarios.archivoEliminado).FirstOrDefault();
                try
                {
                    var listaErrores = db.XML_Errores.Where(o => o.IdEncabezado.Id == encabezado.Id);
                    string asunto = string.Format("{0} error al cargar el archivo {1} ", encabezado.IdEntidad.Nombre, encabezado.IdArchivo.Nombre);
                    string detalle = string.Format("El usuario {0} estaba cargando el archivo {1} " +
                        "y se generaron los siguientes errores: <br/><ul>", encabezado.IdUsuario.Nombre, encabezado.IdArchivo.Nombre);

                    foreach (XML_Errores err in listaErrores)
                    {
                        detalle += "<li>" + err.Detalle + "</li>";
                    }

                    detalle += "</ul><br/>";
                    MailSend.Email.EnviarCorreoImagenes(asunto, detalle,
                    db.Parametros.Where(o => o.Llave == Utilitarios.Correo_Error_XML).Select(i => i.Valor).FirstOrDefault(),
                    db.Parametros.Where(o => o.Llave == Utilitarios.Servidor_Correo).Select(i => i.Valor).FirstOrDefault(),
                    db.Parametros.Where(o => o.Llave == Utilitarios.Direccion_Correo).Select(i => i.Valor).FirstOrDefault(),
                    db.Parametros.Where(o => o.Llave == Utilitarios.Direccion_Correo).Select(i => i.Valor).FirstOrDefault(),
                    db.Parametros.Where(o => o.Llave == Utilitarios.Contrasena_Correo).Select(i => i.Valor).FirstOrDefault());
                    result = false;
                }
                catch (Exception)
                {
                }
            }

            db.Entry(encabezado).State = EntityState.Modified;
            db.SaveChanges();
            db.Dispose();
            reader.Dispose();
        }

        return result;
    }

    public void ProcessCartera(string xlsPath, Int64 idEncabezado)
    {
        SIContext db = new SIContext();
        Sugef_Cartera info;
        Sugef_Encabezado encabezado = db.Sugef_Encabezdo.FirstOrDefault(o => o.Id == idEncabezado);
        try
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(xlsPath, false))
            {
                int cantidad = 0;
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                var sheets = workbookPart.Workbook.Descendants<Sheet>();
                Sheet sheet = sheets.FirstOrDefault();

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();

                Row headerRow = rows.First();
                var headerCells = headerRow.Elements<Cell>();
                int totalColumns = headerCells.Count();

                for (int i = 1; i < rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(rows[i].InnerText))
                    {
                        info = new Sugef_Cartera();
                        List<Cell> columnas = rows[i].Elements<Cell>().ToList();
                        info.IdCarga = encabezado;
                        info.NombreSector = GetCellValue(workbookPart, columnas[1]);
                        info.Actividad = GetCellValue(workbookPart, columnas[2]);
                        info.AlDia = Utilitarios.ConvertirADecimal(columnas[3].CellValue.InnerText);
                        info.Rango1_30Dias = Utilitarios.ConvertirADecimal(columnas[4].CellValue.InnerText);
                        info.Rango31_60Dias = Utilitarios.ConvertirADecimal(columnas[5].CellValue.InnerText);
                        info.Rango61_90Dias = Utilitarios.ConvertirADecimal(columnas[6].CellValue.InnerText);
                        info.Rango91_180Dias = Utilitarios.ConvertirADecimal(columnas[7].CellValue.InnerText);
                        info.Mas180Dias = Utilitarios.ConvertirADecimal(columnas[8].CellValue.InnerText);
                        info.CobroJudicial = Utilitarios.ConvertirADecimal(columnas[9].CellValue.InnerText);
                        info.Total = Utilitarios.ConvertirADecimal(columnas[10].CellValue.InnerText);

                        db.Sugef_Cartera.Add(info);
                        cantidad += 1;
                        encabezado.Cantidad = cantidad;
                        db.Entry(encabezado).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AgregarErrorSugef("Error inesperado: " + ex.Message, encabezado, db);
        }
        finally
        {
            db.Configuration.AutoDetectChangesEnabled = true;
            var errores = db.Sugef_Error.Where(o => o.IdEncabezado.Id == encabezado.Id).Count();
            if (errores == 0)
                encabezado.IdEstado_Id = Utilitarios.archivoAceptado;
            else
                encabezado.IdEstado_Id = Utilitarios.archivoErrores;

            db.Entry(encabezado).State = EntityState.Modified;
            db.SaveChanges();
            db.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public void ProcesarContable_Estado_XML(XmlReader reader, XML_Encabezado encabezado, SIContext db)
    {
        decimal saldo = 0;
        int cantidad = 0;
        XML_Contable_Estado registro;
        while (reader != null && reader.Read())
        {
            try
            {
                reader.MoveToAttribute("Registro");
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals("Registro"))
                        {
                            cantidad += 1;
                            XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                            registro = xDoc.Elements("Registro").Select(info => new XML_Contable_Estado
                            {
                                IdEncabezado = encabezado,
                                Cuenta = info.Element("Cuenta").Value,
                                Credito = Utilitarios.ConvertirADecimal(info.Element("Credito").Value, "Cuenta:  " + info.Element("Cuenta").Value + ". El monto de crédito: "),
                                Debito = Utilitarios.ConvertirADecimal(info.Element("Debito").Value, "Cuenta:  " + info.Element("Cuenta").Value + ". El monto de débito: "),
                                SaldoFinal = Utilitarios.ConvertirADecimal(info.Element("SaldoFinal").Value, "Cuenta:  " + info.Element("Cuenta").Value + ". El saldo final: "),
                                TipoCatalogoSUGEF = Convert.ToInt32(info.Element("TipoCatalogoSUGEF").Value),
                            }).FirstOrDefault();

                            encabezado.Cantidad = cantidad;
                            try
                            {
                                int cuenta = int.Parse(registro.Cuenta);
                                if (cuenta == Utilitarios.activos || cuenta == Utilitarios.gastos)
                                    saldo += registro.SaldoFinal;
                                if (cuenta == Utilitarios.pasivos || cuenta == Utilitarios.capitalPatrimonio || cuenta == Utilitarios.ingresos)
                                    saldo -= registro.SaldoFinal;
                            }
                            catch (Exception)
                            {
                                AgregarError("La cuenta " + registro.Cuenta + " debe ser un número", encabezado, db);
                            }

                            if (registro.Cuenta.Length != 8)
                                AgregarError("La cuenta " + registro.Cuenta + " debe tener longitud de 8.", encabezado, db);
                            else
                            {
                                if (registro.Cuenta.StartsWith("0") || registro.Cuenta.StartsWith("9"))
                                    AgregarError("La cuenta " + registro.Cuenta + " no debe empezar por 8 o 9", encabezado, db);

                                string caracter = registro.Cuenta.Substring(5, 1);
                                if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                    AgregarError("El sexto dígito de la cuenta " + registro.Cuenta + " debe ser 0, 1, 2 o 3", encabezado, db);
                            }

                            db.XML_Contable_Estados.Add(registro);
                            if (cantidad % Utilitarios.lineasCommit == 0)
                            {
                                db.Entry(encabezado).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        if (saldo != 0)
            AgregarError("Por favor revisar que la suma de activos y gastos sean igual a la suma de pasivos, capital de patrimonio e ingresos", encabezado, db);
    }

    public void ProcessIndicador(string xlsPath, Int64 idEncabezado)
    {
        SIContext db = new SIContext();
        Salida_Sugef_Indicadores_Cartera info;
        Sugef_Encabezado encabezado = db.Sugef_Encabezdo.FirstOrDefault(o => o.Id == idEncabezado);
        try
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(xlsPath, false))
            {
                int cantidad = 0;
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                var sheets = workbookPart.Workbook.Descendants<Sheet>();
                Sheet sheet = sheets.FirstOrDefault();

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();

                Row headerRow = rows.First();
                var headerCells = headerRow.Elements<Cell>();
                int totalColumns = headerCells.Count();

                for (int i = 1; i < rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(rows[i].InnerText))
                    {
                        info = new Salida_Sugef_Indicadores_Cartera();

                        switch (i)
                        {
                            case 1:
                                info.IdIndicador = 101;
                                break;
                            case 2:
                                info.IdIndicador = 102;
                                break;
                            case 3:
                                info.IdIndicador = 103;
                                break;
                            case 4:
                                info.IdIndicador = 104;
                                break;
                            case 5:
                                info.IdIndicador = 105;
                                break;
                            case 6:
                                info.IdIndicador = 106;
                                break;
                            case 7:
                                info.IdIndicador = 107;
                                break;
                            case 8:
                                info.IdIndicador = 108;
                                break;
                            case 9:
                                info.IdIndicador = 114;
                                break;
                            case 10:
                                info.IdIndicador = 115;
                                break;
                            case 11:
                                info.IdIndicador = 109;
                                break;
                            case 12:
                                info.IdIndicador = 110;
                                break;
                            case 13:
                                info.IdIndicador = 111;
                                break;
                            case 14:
                                info.IdIndicador = 112;
                                break;
                            case 15:
                                info.IdIndicador = 113;
                                break;
                        }
                        List<Cell> columnas = rows[i].Elements<Cell>().ToList();

                        info.IdCarga = encabezado;
                        info.BancoComercialEstado = Utilitarios.ConvertirADecimal(columnas[2].CellValue.InnerText);
                        info.BancoLeyesEspeciales = Utilitarios.ConvertirADecimal(columnas[3].CellValue.InnerText);
                        info.BancosPrivadosCoope = Utilitarios.ConvertirADecimal(columnas[4].CellValue.InnerText);
                        info.EmpresaFinanNoBancaria = Utilitarios.ConvertirADecimal(columnas[5].CellValue.InnerText);
                        info.OtrasEntidadesFinancieras = Utilitarios.ConvertirADecimal(columnas[6].CellValue.InnerText);
                        info.OrganizacionesCooperativas = Utilitarios.ConvertirADecimal(columnas[7].CellValue.InnerText);
                        info.EntidadesAutorizadasVivienda = Utilitarios.ConvertirADecimal(columnas[8].CellValue.InnerText);
                        info.Total = Utilitarios.ConvertirADecimal(columnas[9].CellValue.InnerText);

                        db.Salida_Sugef_Indicadores_Cartera.Add(info);
                        cantidad += 8;
                        encabezado.Cantidad = cantidad;
                        db.Entry(encabezado).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AgregarErrorSugef("Error inesperado: " + ex.Message, encabezado, db);
        }
        finally
        {
            db.Configuration.AutoDetectChangesEnabled = true;
            var errores = db.Sugef_Error.Where(o => o.IdEncabezado.Id == encabezado.Id).Count();
            if (errores == 0)
                encabezado.IdEstado_Id = Utilitarios.archivoAceptado;
            else
                encabezado.IdEstado_Id = Utilitarios.archivoErrores;

            db.Entry(encabezado).State = EntityState.Modified;
            db.SaveChanges();
            db.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public void ProcessIndustria(string xlsPath, Int64 idEncabezado)
    {
        SIContext db = new SIContext();
        Sugef_Info_Contable info;
        Sugef_Encabezado encabezado = db.Sugef_Encabezdo.FirstOrDefault(o => o.Id == idEncabezado);
        try
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(xlsPath, false))
            {
                int cantidad = 0;
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                var sheets = workbookPart.Workbook.Descendants<Sheet>();
                Sheet sheet = sheets.FirstOrDefault();

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();

                Row headerRow = rows.First();
                var headerCells = headerRow.Elements<Cell>();
                int totalColumns = headerCells.Count();

                for (int i = 1; i < rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(rows[i].InnerText))
                    {
                        List<Cell> columnas = rows[i].Elements<Cell>().ToList();

                        info = new Sugef_Info_Contable();
                        info.Cuenta = columnas[2].CellValue.InnerText;
                        info.IdCarga = encabezado;
                        info.BancosComercialesEstado = Utilitarios.ConvertirADecimal(columnas[3].CellValue.InnerText);
                        info.BancosLeyesEspeciales = Utilitarios.ConvertirADecimal(columnas[4].CellValue.InnerText);
                        info.BancosPrivadosCoope = Utilitarios.ConvertirADecimal(columnas[5].CellValue.InnerText);
                        info.EmpresaFinanNoBancaria = Utilitarios.ConvertirADecimal(columnas[6].CellValue.InnerText);
                        info.OtrasEntidadesFinancieras = Utilitarios.ConvertirADecimal(columnas[7].CellValue.InnerText);
                        info.OrganizacionesCooperativas = Utilitarios.ConvertirADecimal(columnas[8].CellValue.InnerText);
                        info.EntidadesAutorizadasVivienda = Utilitarios.ConvertirADecimal(columnas[9].CellValue.InnerText);
                        info.Total = Utilitarios.ConvertirADecimal(columnas[10].CellValue.InnerText);

                        cantidad += 1;
                        encabezado.Cantidad = cantidad;
                        db.Sugef_Info_Contable.Add(info);
                        db.Entry(encabezado).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AgregarErrorSugef("Error inesperado: " + ex.Message, encabezado, db);
        }
        finally
        {
            db.Configuration.AutoDetectChangesEnabled = true;
            var errores = db.Sugef_Error.Where(o => o.IdEncabezado.Id == encabezado.Id).Count();
            if (errores == 0)
                encabezado.IdEstado_Id = Utilitarios.archivoAceptado;
            else
                encabezado.IdEstado_Id = Utilitarios.archivoErrores;

            db.Entry(encabezado).State = EntityState.Modified;
            db.SaveChanges();
            db.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public string SaveFile(string zipPath, string fileName, string idEntidad, int idUsuario, DateTime fechaCarga)
    {
        SIContext db = new SIContext();
        db.Configuration.AutoDetectChangesEnabled = false;
        XmlReader reader;
        string mensaje = string.Empty;
        string zipFile = Path.Combine(zipPath, fileName);
        string extractPath = Path.Combine(zipPath, Guid.NewGuid().ToString());
        string contablePath = string.Empty;
        bool archivoContable = false;
        Entidad entidad = db.Entidades.Where(o => o.Id == idEntidad).FirstOrDefault();
       
        try
        {
            Directory.CreateDirectory(extractPath);
            ZipFile.ExtractToDirectory(zipFile, extractPath);
            XML_Encabezado encabezado = new XML_Encabezado();
            XML_Encabezado contableEstados = new XML_Encabezado();
            List<string> basicos = ((entidad.Perfil_Entidad_Id == Utilitarios.perfilPrudencial) ? db.TipoXMLs.Where(o => o.IndPrudencial == true).Select(o => o.Id).ToList() : db.TipoXMLs.Where(o => o.IndRiesgos == true).Select(o => o.Id).ToList());
            var archivos = Directory.GetFiles(extractPath);
            foreach (string extractedFile in archivos)
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
                                                      Periodo = DateTime.ParseExact(Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture) /*DateTime.Parse(Fecha)*/,
                                                      IdEntidad = db.Entidades.Where(o => o.Identificacion == IdEntidad).FirstOrDefault(),
                                                      IdArchivo = db.TipoXMLs.Where(o => o.Id == IdArchivo).FirstOrDefault(),
                                                      IdArchivo_Id = Utilitarios.ObtenerIdArchivo(IdArchivo)
                                                  }).FirstOrDefault();
                                    reader.Dispose();
                                }
                                break;
                        }
                    }

                    if (encabezado.IdEntidad.Identificacion.ToString() != entidad.Identificacion.ToString())
                    {
                        mensaje = "El archivo no pertenece a la entidad: " + entidad.Nombre + " Archivo: " + Path.GetFileName(extractedFile) + " Id en el archivo: " + encabezado.IdEntidad.Identificacion.ToString() + " Id Entidad: " + entidad.Identificacion.ToString();
                        return mensaje;
                    }

                    if (fechaCarga.Year != encabezado.Periodo.Year || fechaCarga.Month != encabezado.Periodo.Month)
                    {
                        encabezado.Periodo = fechaCarga;
                    }


                    basicos.Remove(encabezado.IdArchivo_Id);
                    if (encabezado.IdArchivo_Id == Utilitarios.xml_contable_estado)
                    {
                        archivoContable = true;
                        encabezado.FechaCarga = DateTime.Now;
                        encabezado.IdEstado_Id = FGA.Utility.Utilitarios.archivoCargado;
                        encabezado.IdUsuario_Id = idUsuario;
                        encabezado.IdEntidad_Id = encabezado.IdEntidad.Id;
                        encabezado.Periodo = new DateTime(encabezado.Periodo.Year, encabezado.Periodo.Month, 1);
                        encabezado.IdEntidad = null;
                        encabezado.IdArchivo = null;
                        contableEstados = encabezado;
                        contablePath = extractedFile;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (archivos == null)
                mensaje = "Los archivos deberán estar en el directorio raíz";
            else if (archivos.Count() == 0)
                mensaje = "Los archivos deberán estar en el directorio raíz";
            else if (archivoContable == false)
                mensaje = "Falta el archivo contable estado.";
            else if (basicos.Count > 0)
            {
                mensaje = "No se han incluido todos los archivos básico. Faltan los siguientes archivos: " + Environment.NewLine + Environment.NewLine;
                foreach (string detalle in basicos)
                {
                    mensaje += Utilitarios.GetFileName(detalle) + ", " + Environment.NewLine;      
                }
            }
            else
            {
                var encabezados = db.XML_Encabezados.Where(o => o.IdEntidad_Id == idEntidad && o.IdEstado_Id != Utilitarios.archivoEliminado && o.Periodo == contableEstados.Periodo).Include("IdUsuario").Include("IdArchivo").Include("IdEntidad").ToList();
                foreach (var enc in encabezados)
                {
                    enc.IdEstado_Id = Utilitarios.archivoEliminado;
                    db.Entry(enc).State = EntityState.Modified;
                    db.SaveChanges();
                }

                using (XML_EncabezadoService serv = new XML_EncabezadoService())
                {
                    serv.Add(ref contableEstados);
                    if (Process(contablePath, contableEstados.Id))
                    {
                        using (CargaAsincronicaService car = new CargaAsincronicaService())
                        {
                            CargaAsincronica carga = new CargaAsincronica();
                            carga.FilePath = extractPath;
                            carga.IdEntidad = idEntidad;
                            carga.IdUsuario = idUsuario;
                            carga.IndEstado = 0;
                            car.Add(ref carga);
                        }
                    }
                    else
                    {
                        mensaje = "El archivo contable estado presenta errores o ya fue cargado";
                    }
                }
            }
        }
        catch (Exception e)
        {
            mensaje = "Hubo un error en el procesamiento del archivo " + e.Message + " " + e.InnerException;
        }
        //finally
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(mensaje))
        //        {
        //            Directory.Delete(zipPath, true);
        //        }
        //        else
        //        {
        //            System.IO.File.Delete(zipFile);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        return mensaje;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}