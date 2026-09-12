using FGA.Models;
using FGA.Utility;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace FileService
{
    public partial class FileService : ServiceBase
    {
        private readonly String minutes = System.Configuration.ConfigurationSettings.AppSettings["Minutes"];

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
                                Process(extractedFile, encabezado.Id);
                            }
                        }
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

        public void Process(string xmlPath, Int64 idEncabezado)
        {
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
                                                o.IdEstado_Id != Utilitarios.archivoEliminado && o.Periodo == encabezado.Periodo &&
                                                o.IdArchivo_Id == encabezado.IdArchivo_Id).Count();

                if (cant > 1)
                {
                    AgregarError("El archivo ya fue cargado previamente", encabezado, db);
                }
                else
                {
                    var xml_contable_estados = db.XML_Encabezados
                                              .Where(o => o.IdEntidad_Id == encabezado.IdEntidad_Id &&
                                                     o.IdEstado_Id == Utilitarios.archivoAceptado && o.Periodo == encabezado.Periodo &&
                                                     o.IdArchivo_Id == Utilitarios.xml_contable_estado).FirstOrDefault();

                    if (encabezado.IdArchivo_Id != Utilitarios.xml_contable_estado && xml_contable_estados == null)
                    {
                        AgregarError("No se ha cargado el archivo XML_Contable_Estados.xml", encabezado, db);
                    }
                    else
                    {
                        switch (encabezado.IdArchivo_Id)
                        {
                            case Utilitarios.xml_suficiencia_patrimonial:
                                ProcesarSuficienciaPatrimonial(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_indicadores_financieros:
                                ProcesarIndicadoresFinancieros(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_calce_plazo:
                                Procesar_Calce_Plazo(reader, encabezado, xml_contable_estados.Id, db);
                                break;
                            case Utilitarios.xml_contable_brecha:
                                Procesar_Contable_Brecha(reader, encabezado, xml_contable_estados.Id, db);
                                break;
                            case Utilitarios.xml_contable_datos_adicionales:
                                ProcesarContable_Datos(reader, encabezado, xml_contable_estados.Id, db);
                                break;
                            case Utilitarios.xml_flujo_efectivo:
                                Procesar_Flujo_Efectivo(reader, encabezado, xml_contable_estados.Id, db);
                                break;
                            case Utilitarios.xml_credito_cuota_atrasada:
                                ProcesarCuota_Atrasada_XML(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_credito_deudor:
                                ProcesarCredito_Deudor_XML(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_credito_oper_dirind:
                                ProcesarCredito_Operacion_DirInd_XML(reader, encabezado, xml_contable_estados.Id, db);
                                break;
                            case Utilitarios.xml_pasivos_cuentas_contables_210:
                                Procesar_Pasivos_Cuenta_Contable_210(reader, encabezado, xml_contable_estados.Id, db);
                                break;
                            case Utilitarios.xml_inversiones_activas:
                                ProcesarInversionActiva(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_capital_social:
                                ProcesarCapitalSocial(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_crediticio_cuotasAtrasadas_1421:
                                ProcesarCuota_Atrasada_XML(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_crediticio_operaciones_1421:
                                ProcesarCredito_Operacion_DirInd_XML(reader, encabezado, xml_contable_estados.Id, db);
                                break;
                            case Utilitarios.xml_crediticio_deudores_1421:
                                ProcesarCredito_Deudor_XML(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_icl:
                                Procesar_ICL(reader, encabezado, db);
                                break;
                            case Utilitarios.xml_indicador_financiero:
                                break;
                            case Utilitarios.xml_credito_garantia_operaciones:
                                break;
                            case Utilitarios.xml_credito_cuentas_cobrar:
                                break;
                            case Utilitarios.xml_credito_bienes_realizables:
                                break;
                            case Utilitarios.xml_credito_informacion_oper_no_reportadas:
                                break;
                            case Utilitarios.xml_crediticio_bienesRealizablesNoReportadas_1421:
                                break;
                            case Utilitarios.xml_crediticio_bienesRealizables_1421:
                                break;
                            case Utilitarios.xml_crediticio_cuentasPorCobrarNoAsociadas_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasCartasCredito_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasFacturasCedidas_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasFiduciarias_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasPolizas_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasOperacion_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasReales_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasValores_1421:
                                break;
                            case Utilitarios.xml_crediticio_operacionesNoReportadas_1421:
                                break;
                            case Utilitarios.xml_crediticio_ingresoDeudores_1421:
                                break;
                            case Utilitarios.xml_crediticio_origenRecursos_1421:
                                break;
                            case Utilitarios.xml_crediticio_actividadEconomica_1421:
                                break;
                            case Utilitarios.xml_crediticio_naturalezaGasto_1421:
                                break;
                            case Utilitarios.xml_crediticio_cuentasCobrar_1421:
                                break;
                            case Utilitarios.xml_crediticio_creditosSindicados_1421:
                                break;
                            case Utilitarios.xml_crediticio_codeudores_1421:
                                break;
                            case Utilitarios.xml_crediticio_modificacion_1421:
                                break;
                            case Utilitarios.xml_crediticio_compradasRefinanciadas_1421:
                                break;
                            case Utilitarios.xml_crediticio_cambioClimatico_1421:
                                break;
                            case Utilitarios.xml_crediticio_operBienesRealizables_1421:
                                break;
                            case Utilitarios.xml_crediticio_fideicomiso_1421:
                                break;
                            case Utilitarios.xml_crediticio_gravamenes_1421:
                                break;
                            case Utilitarios.xml_crediticio_garantiasMobiliarias_1421:
                                break;
                            default:
                                AgregarError("Archivo no implementado", encabezado, db);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AgregarError("Error inesperado: " + e.Message, encabezado, db);
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
                    encabezado.IdEstado = db.ArchivoEstados.Where(o => o.Id == Utilitarios.archivoErrores).FirstOrDefault();
                    try
                    {
                       

                        var listaErrores = db.XML_Errores.Where(o => o.IdEncabezado.Id == encabezado.Id);
                        string asunto = string.Format(encabezado.IdEntidad.Nombre + ". Notificación de error en carga de archivo XML");
                        string detalle = string.Format("El usuario de carga asignado por la cooperativa subió el archivo {0} " +
                            "y se generaron los siguientes errores: <br/><ul>", encabezado.IdArchivo.Nombre);

                        foreach (XML_Errores err in listaErrores)
                        {
                            detalle += "<li>" + err.Detalle + "</li>";
                        }

                        detalle += "</ul><br/>Si requiere de nuestra asesoría, puede comunicarse al número 2257-1111 " +
                            "o contactar a las analistas de riesgo: " +
                            "<br/> <br/> " +
                            "Cinthya Salazar <a href='mailto:csalazar@ffc.co.cr'> csalazar@ffc.co.cr</a>" +
                            "<br/> " +
                            "Viviana Zumbado <a href='mailto:vzumbado@ffc.co.cr'> vzumbado@ffc.co.cr</a>" +
                            "<br/><br/>";

                        MailSend.Email.EnviarCorreoImagenes(asunto, detalle,
                        db.Parametros.Where(o => o.Llave == Utilitarios.Correo_Error_XML).Select(i => i.Valor).FirstOrDefault(),
                        db.Parametros.Where(o => o.Llave == Utilitarios.Servidor_Correo).Select(i => i.Valor).FirstOrDefault(),
                        db.Parametros.Where(o => o.Llave == Utilitarios.Direccion_Correo).Select(i => i.Valor).FirstOrDefault(),
                        db.Parametros.Where(o => o.Llave == Utilitarios.Direccion_Correo).Select(i => i.Valor).FirstOrDefault(),
                        db.Parametros.Where(o => o.Llave == Utilitarios.Contrasena_Correo).Select(i => i.Valor).FirstOrDefault());

                        Entidad entidad = db.Entidades.AsNoTracking().Where(o => o.Contacto != string.Empty && o.Activo == true && o.Id == encabezado.IdEntidad.Id).FirstOrDefault();

                        MailSend.Email.EnviarCorreoImagenes(asunto, detalle,
                        entidad.Contacto,
                        db.Parametros.Where(o => o.Llave == Utilitarios.Servidor_Correo).Select(i => i.Valor).FirstOrDefault(),
                        db.Parametros.Where(o => o.Llave == Utilitarios.Direccion_Correo).Select(i => i.Valor).FirstOrDefault(),
                        db.Parametros.Where(o => o.Llave == Utilitarios.Direccion_Correo).Select(i => i.Valor).FirstOrDefault(),
                        db.Parametros.Where(o => o.Llave == Utilitarios.Contrasena_Correo).Select(i => i.Valor).FirstOrDefault());
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
        }

        public void ProcesarSuficienciaPatrimonial(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Suficiencia_Patrimonial registro;
                decimal S10000 = 0, S20000 = 0, S30000 = 0, S40000 = 0, S50000 = 0, S60000 = 0, S70000 = 0, resultado = 0;


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

                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Suficiencia_Patrimonial
                                    {
                                        IdEncabezado = encabezado,
                                        CuentaCatalogo = Convert.ToInt64(info.Element("CuentaCatalogo").Value),
                                        GradualidadPonderacion = Utilitarios.ConvertirADecimal(info.Element("GradualidadPonderacion").Value),
                                        Moneda = Convert.ToInt32(info.Element("Moneda").Value),
                                        Monto = Utilitarios.ConvertirADecimal(info.Element("Monto").Value),
                                        MontoPonderado = Utilitarios.ConvertirADecimal(info.Element("MontoPonderado").Value),
                                        Ponderacion = Utilitarios.ConvertirADecimal(info.Element("Ponderacion").Value),
                                        TipoCatalogoSugef = Convert.ToInt32(info.Element("TipoCatalogoSUGEF").Value),
                                    }).FirstOrDefault();


                                    if (registro.CuentaCatalogo == 10000)
                                        S10000 += registro.MontoPonderado;
                                    else if (registro.CuentaCatalogo == 20000)
                                        S20000 += registro.MontoPonderado;
                                    else if (registro.CuentaCatalogo == 30000)
                                        S30000 += registro.MontoPonderado;
                                    else if (registro.CuentaCatalogo == 40000)
                                        S40000 += registro.MontoPonderado;
                                    else if (registro.CuentaCatalogo == 50000)
                                        S50000 += registro.MontoPonderado;
                                    else if (registro.CuentaCatalogo == 60000)
                                        S60000 += registro.MontoPonderado;
                                    else if (registro.CuentaCatalogo == 70000)
                                        S70000 += registro.MontoPonderado;

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Suficiencia_Patrimonial.Add(registro);

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

                resultado = (S20000 / ((S30000 + S40000) + 10 * (S50000 + S60000 + S70000))) * 100;

                if (Math.Abs(S10000 - resultado) > 0.1M)
                    AgregarError("No se cumple la condición. 10000 =  (20000/ ((30000 + 40000) + 10 * (50000 + 60000 + 70000))) * 100", encabezado, db);
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void Procesar_ICL(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_ICL registro;
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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_ICL
                                    {
                                        IdEncabezado = encabezado,
                                        CuentaCatalogo = info.Element("CuentaCatalogo").Value,
                                        Moneda = info.Element("Moneda").Value,
                                        Factor = Utilitarios.ConvertirADecimal(info.Element("Factor").Value),
                                        Monto = Utilitarios.ConvertirADecimal(info.Element("Monto").Value),
                                        MontoPonderado = Utilitarios.ConvertirADecimal(info.Element("MontoPonderado").Value),
                                    }).FirstOrDefault();

                                    encabezado.Cantidad = cantidad;
                                    db.XML_ICLs.Add(registro);
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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void ProcesarIndicadoresFinancieros(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Indicadores_Financieros registro;
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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Indicadores_Financieros
                                    {
                                        IdEncabezado = encabezado,
                                        CuentaCatalogo = Convert.ToInt64(info.Element("CuentaCatalogo").Value),
                                        Moneda = Convert.ToInt32(info.Element("Moneda").Value),
                                        TipoCatalogoSUGEF = Convert.ToInt32(Utilitarios.ConvertirADecimal(info.Element("TipoCatalogoSUGEF").Value)),
                                        MontoValor = Utilitarios.ConvertirADecimal(info.Element("MontoValor").Value),
                                    }).FirstOrDefault();

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Indicadores_Financieros.Add(registro);
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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void Procesar_Calce_Plazo(XmlReader reader, XML_Encabezado encabezado, long contableEstados, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Rango_Calce_Plazo registro;
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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Rango_Calce_Plazo
                                    {
                                        IdEncabezado = encabezado,
                                        CuentaCalcePlazo = Convert.ToInt64(info.Element("CuentaCalcePlazo").Value),
                                        RangoCalcePlazo = Convert.ToInt32(info.Element("RangoCalcePlazo").Value),
                                        TipoCatalogoSugef = Convert.ToInt32(Utilitarios.ConvertirADecimal(info.Element("TipoCatalogoSUGEF").Value)),
                                        MontoCalcePlazo = Utilitarios.ConvertirADecimal(info.Element("MontoCalcePlazo").Value),
                                    }).FirstOrDefault();

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Rango_Calce_Plazo.Add(registro);
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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void Procesar_Contable_Brecha(XmlReader reader, XML_Encabezado encabezado, long contableEstados, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Contable_Brecha registro;
                decimal B22110 = 0, B22210 = 0, B22310 = 0, B22120 = 0, B22220 = 0, B22320 = 0, B22111 = 0, B22112 = 0,
                        B22211 = 0, B22212 = 0, B22121 = 0, B22122 = 0, B22221 = 0, B22222 = 0, B22223 = 0, B22123 = 0;

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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Contable_Brecha
                                    {
                                        IdEncabezado = encabezado,
                                        CuentaBrecha = Convert.ToInt64(info.Element("CuentaBrecha").Value),
                                        RangoBrecha = Convert.ToInt32(info.Element("RangoBrecha").Value),
                                        TipoCatalogoSugef = Convert.ToInt32(Utilitarios.ConvertirADecimal(info.Element("TipoCatalogoSUGEF").Value)),
                                        MontoBrecha = Utilitarios.ConvertirADecimal(info.Element("MontoBrecha").Value),
                                    }).FirstOrDefault();

                                    if (registro.CuentaBrecha == 22110)
                                        B22110 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22210)
                                        B22210 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22310)
                                        B22310 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22120)
                                        B22120 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22220)
                                        B22220 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22320)
                                        B22320 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22111)
                                        B22111 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22112)
                                        B22112 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22211)
                                        B22211 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22212)
                                        B22212 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22121)
                                        B22121 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22122)
                                        B22122 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22123)
                                        B22123 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22221)
                                        B22221 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22222)
                                        B22222 += registro.MontoBrecha;
                                    else if (registro.CuentaBrecha == 22223)
                                        B22223 += registro.MontoBrecha;

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Contable_Brecha.Add(registro);
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

                if (Math.Abs(B22110 + B22210 - B22310) > 1)
                    AgregarError("El monto de la cuenta 22310 debe ser igual a la cuenta 22110 + 22210", encabezado, db);

                if (Math.Abs(B22120 + B22220 - B22320) > 1)
                    AgregarError("El monto de la cuenta 22320 debe ser igual a la cuenta 22120 + 22220", encabezado, db);

                if (Math.Abs(B22111 + B22112 + B22211 + B22212 - B22310) > 1)
                    AgregarError("El monto de la cuenta 22310 debe ser igual a la cuenta 22111 + 22112 + 22211 + 22212", encabezado, db);

                if (Math.Abs(B22121 + B22122 + B22123 + B22221 + B22222 + B22223 - B22320) > 1)
                    AgregarError("El monto de la cuenta 22320 debe ser igual a la cuenta 22121 + 22122 + 22123 + 22221 + 22222 + 22223", encabezado, db);

            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void ProcesarContable_Datos(XmlReader reader, XML_Encabezado encabezado, long contableEstados, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Contable_DatosAdicionales registro;
                List<XML_Contable_Estado> xml_contable_estados = db.XML_Contable_Estados
                                     .Where(o => o.IdEncabezado.Id == contableEstados).ToList();

                decimal vD20018 = xml_contable_estados.Where(o => o.Cuenta.Equals("23402000")).Sum(o => o.SaldoFinal);
                decimal vD20019 = xml_contable_estados.Where(o => o.Cuenta.Equals("21500000")).Sum(o => o.SaldoFinal);
                decimal vD20020 = xml_contable_estados.Where(o => o.Cuenta.Equals("24101000") || o.Cuenta.Equals("24110000")).Sum(o => o.SaldoFinal);
                decimal vD20021 = xml_contable_estados.Where(o => o.Cuenta.Equals("24102000") || o.Cuenta.Equals("24111000")).Sum(o => o.SaldoFinal);
                decimal vD20022 = xml_contable_estados.Where(o => o.Cuenta.Equals("23299000")).Sum(o => o.SaldoFinal);
                decimal vD20037 = xml_contable_estados.Where(o => o.Cuenta.Equals("21199000")).Sum(o => o.SaldoFinal);
                decimal vD20181 = xml_contable_estados.Where(o => o.Cuenta.Equals("26000000")).Sum(o => o.SaldoFinal);
                decimal vD20053 = xml_contable_estados.Where(o => o.Cuenta.Equals("31102102")).Sum(o => o.SaldoFinal);
                decimal vD20146 = xml_contable_estados.Where(o => o.Cuenta.Equals("34302101")).Sum(o => o.SaldoFinal);

                decimal vD20027 = xml_contable_estados.Where(o => o.Cuenta.Equals("12101000") || o.Cuenta.Equals("12102000") ||
                                                                  o.Cuenta.Equals("12107000") || o.Cuenta.Equals("12201000") ||
                                                                  o.Cuenta.Equals("12202000") || o.Cuenta.Equals("12260000") ||
                                                                  o.Cuenta.Equals("12261000") || o.Cuenta.Equals("12301000") ||
                                                                  o.Cuenta.Equals("12302000") || o.Cuenta.Equals("12307000")).Sum(o => o.SaldoFinal);

                decimal vD20028 = xml_contable_estados.Where(o => o.Cuenta.Equals("12101000") || o.Cuenta.Equals("12102000") ||
                                                      o.Cuenta.Equals("12107000") || o.Cuenta.Equals("12201000") ||
                                                      o.Cuenta.Equals("12202000") || o.Cuenta.Equals("12207000") ||
                                                      o.Cuenta.Equals("12260000") || o.Cuenta.Equals("12261000") ||
                                                      o.Cuenta.Equals("12301000") || o.Cuenta.Equals("12302000") ||
                                                      o.Cuenta.Equals("12307000")).Sum(o => o.SaldoFinal);

                decimal vD20186_87 = xml_contable_estados.Where(o => (o.Cuenta.StartsWith("12101") ||
                                                                  o.Cuenta.StartsWith("12102") || o.Cuenta.StartsWith("12107") ||
                                                                  o.Cuenta.StartsWith("12201") || o.Cuenta.StartsWith("12202") ||
                                                                  o.Cuenta.StartsWith("12207") || o.Cuenta.StartsWith("12260") ||
                                                                  o.Cuenta.StartsWith("12261") || o.Cuenta.StartsWith("12301") ||
                                                                  o.Cuenta.StartsWith("12302") || o.Cuenta.StartsWith("12307")) &&
                                                                  (o.Cuenta.EndsWith("100") || o.Cuenta.EndsWith("300"))).Sum(o => o.SaldoFinal);

                decimal vD20188_89 = xml_contable_estados.Where(o => (o.Cuenta.StartsWith("12101") ||
                                                                o.Cuenta.StartsWith("12102") || o.Cuenta.StartsWith("12107") ||
                                                                o.Cuenta.StartsWith("12201") || o.Cuenta.StartsWith("12202") ||
                                                                o.Cuenta.StartsWith("12207") || o.Cuenta.StartsWith("12260") ||
                                                                o.Cuenta.StartsWith("12261") || o.Cuenta.StartsWith("12301") ||
                                                                o.Cuenta.StartsWith("12302") || o.Cuenta.StartsWith("12307")) &&
                                                                (o.Cuenta.EndsWith("200"))).Sum(o => o.SaldoFinal);

                decimal D20189 = 0, D20188 = 0, D20187 = 0, D20186 = 0, D20027 = 0, D20028 = 0;
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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Contable_DatosAdicionales
                                    {
                                        IdEncabezado = encabezado,
                                        CuentaCatalogo = Convert.ToInt64(info.Element("CuentaCatalogo").Value),
                                        TipoMonedaDato = Convert.ToInt32(info.Element("TipoMonedaDato").Value),
                                        TipoCatalogoSugef = Convert.ToInt32(Utilitarios.ConvertirADecimal(info.Element("TipoCatalogoSUGEF").Value)),
                                        MontoDatoAdicional = Utilitarios.ConvertirADecimal(info.Element("MontoDatoAdicional").Value),
                                    }).FirstOrDefault();

                                    if (registro.CuentaCatalogo == 20018 && registro.MontoDatoAdicional > vD20018)
                                        AgregarError("La cuenta D20018 debe tener un monto menor o igual a la cuenta 23402 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20019 && registro.MontoDatoAdicional > vD20019)
                                        AgregarError("La cuenta D20019 debe tener un monto menor o igual a la cuenta 215 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20020 && registro.MontoDatoAdicional > vD20020)
                                        AgregarError("La cuenta D20020 debe tener un monto menor o igual a la suma de la cuenta 24101 y 24110 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20021 && registro.MontoDatoAdicional > vD20021)
                                        AgregarError("La cuenta D20021 debe tener un monto menor o igual a la suma de la cuenta 24102 y 24111 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20022 && registro.MontoDatoAdicional > vD20022)
                                        AgregarError("La cuenta D20022 debe tener un monto menor o igual a la cuenta 23299 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20037 && registro.MontoDatoAdicional > vD20037)
                                        AgregarError("La cuenta D20037 debe ser menor o igual a la cuenta 21199 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20181 && registro.MontoDatoAdicional > vD20181)
                                        AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a la cuenta 26000 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20053 && registro.MontoDatoAdicional > vD20053)
                                        AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a la cuenta 31102102 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20027 && registro.MontoDatoAdicional > vD20027)
                                        AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12260 + 12261 + 12301 + 12302 + 12307 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20028 && registro.MontoDatoAdicional > vD20028)
                                        AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12207 + 12260 + 12261 + 12301 + 12302 + 12307 del xml contable estados", encabezado, db);

                                    if ((registro.CuentaCatalogo == 20186 || registro.CuentaCatalogo == 20187) && registro.MontoDatoAdicional > vD20186_87)
                                        AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12207 + 12260 + 12261 + 12301 + 12302 + 12307 cuyos últimos dígitos sean 100 o 300 del xml contable estados", encabezado, db);

                                    if ((registro.CuentaCatalogo == 20188 || registro.CuentaCatalogo == 20189) && registro.MontoDatoAdicional > vD20188_89)
                                        AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12207 + 12240 + 12241 + 12301 + 12302 + 12307 cuyos últimos dígitos sean 200 del xml contable estados", encabezado, db);


                                    if (registro.CuentaCatalogo == 20146 && registro.MontoDatoAdicional > vD20146)
                                        AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a la cuenta 34302101 del xml contable estados", encabezado, db);

                                    if ((registro.CuentaCatalogo == 20018 || registro.CuentaCatalogo == 20019
                                        || registro.CuentaCatalogo == 20020 || registro.CuentaCatalogo == 20021
                                        || registro.CuentaCatalogo == 20022 || registro.CuentaCatalogo == 20024
                                        || registro.CuentaCatalogo == 20025 || registro.CuentaCatalogo == 20026
                                        || registro.CuentaCatalogo == 20092 || registro.CuentaCatalogo == 20145
                                        || registro.CuentaCatalogo == 20191 || registro.CuentaCatalogo == 20192
                                        || registro.CuentaCatalogo == 20193 || registro.CuentaCatalogo == 20194
                                        || registro.CuentaCatalogo == 20105 || registro.CuentaCatalogo == 20115)
                                        && registro.MontoDatoAdicional < 0)
                                        AgregarError("La cuenta D " + registro.CuentaCatalogo + " debe tener un monto mayor o igual a 0", encabezado, db);


                                    if (registro.CuentaCatalogo == 20027)
                                        D20027 = registro.MontoDatoAdicional;

                                    if (registro.CuentaCatalogo == 20028)
                                        D20028 = registro.MontoDatoAdicional;

                                    if (registro.CuentaCatalogo == 20186)
                                        D20186 = registro.MontoDatoAdicional;

                                    if (registro.CuentaCatalogo == 20187)
                                        D20187 = registro.MontoDatoAdicional;

                                    if (registro.CuentaCatalogo == 20188)
                                        D20188 = registro.MontoDatoAdicional;

                                    if (registro.CuentaCatalogo == 20189)
                                        D20189 = registro.MontoDatoAdicional;

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Contable_DatosAdicionales.Add(registro);
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

                if (D20189 > D20188)
                    AgregarError("El saldo de D20189 debe ser menor o igual al saldo de la cuenta D20188 ", encabezado, db);
                if (D20187 > D20186)
                    AgregarError("El saldo de D20187 debe ser menor o igual al saldo de la cuenta D20186 ", encabezado, db);
                if (D20186 + D20188 != D20027)
                    AgregarError("El saldo de D20186 + D20188 debe ser igual al saldo de la cuenta D20027 ", encabezado, db);
                if (D20187 + D20189 != D20028)
                    AgregarError("El saldo de D20187 + D20189 debe ser igual al saldo de la cuenta D20028 ", encabezado, db);

            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void Procesar_Flujo_Efectivo(XmlReader reader, XML_Encabezado encabezado, long contableEstados, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Flujo_EfectivoReal registro;
                List<XML_Contable_Estado> xml_contable_estados = db.XML_Contable_Estados
                                   .Where(o => o.IdEncabezado.Id == contableEstados).ToList();
                decimal F10000 = 0, F11000 = 0, F12000 = 0;
                decimal vD11202 = xml_contable_estados.Where(o => o.Cuenta.Equals("11202000")).Sum(o => o.SaldoFinal);
                decimal vD11000 = xml_contable_estados.Where(o => o.Cuenta.Equals("11000000")).Sum(o => o.SaldoFinal);

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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Flujo_EfectivoReal
                                    {
                                        IdEncabezado = encabezado,
                                        CuentaFlujoEfectivo = Convert.ToInt64(info.Element("CuentaFlujoEfectivo").Value),
                                        TipoCatalogoSugef = Convert.ToInt32(Utilitarios.ConvertirADecimal(info.Element("TipoCatalogoSUGEF").Value)),
                                        MontoFlujoEfectivo = Utilitarios.ConvertirADecimal(info.Element("MontoFlujoEfectivo").Value),
                                    }).FirstOrDefault();

                                    if (registro.CuentaFlujoEfectivo == 10000)
                                        F10000 = registro.MontoFlujoEfectivo;

                                    else if (registro.CuentaFlujoEfectivo == 11000)
                                        F11000 = registro.MontoFlujoEfectivo;

                                    else if (registro.CuentaFlujoEfectivo == 12000)
                                        F12000 = registro.MontoFlujoEfectivo;

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Flujo_EfectivoReal.Add(registro);
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

                if (F10000 != F11000 - F12000)
                    AgregarError("El monto de la cuenta 10000 debe ser igual a la cuenta 11000 - 12000", encabezado, db);

                if (F10000 != vD11000 - vD11202)
                    AgregarError("El monto de la cuenta 10000  monto: " + F10000.ToString() + " debe ser igual a la cuenta 110 (Disponibilidades) - el saldo de la subcuenta 11202 del xml contable estado. Saldo: " + (vD11000 - vD11202).ToString(), encabezado, db);

            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void ProcesarCapitalSocial(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            int cantidad = 0;
            XML_Capital_Social registro;
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
                                XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                cantidad += 1;
                                registro = xDoc.Elements("Registro").Select(info => new XML_Capital_Social
                                {
                                    IdEncabezado = encabezado,
                                    Contrato = info.Element("Contrato").Value,
                                    Des_Identificacion = info.Element("Des_Identificacion").Value,
                                    Fechainclusion = Utilitarios.ConvertirAFecha(info.Element("Fechainclusion").Value),
                                    SaldoReal = Utilitarios.ConvertirADecimal(info.Element("SaldoReal").Value, "Contrato: " + info.Element("Contrato").Value + ". El saldo: "),
                                }).FirstOrDefault();


                                if (registro.SaldoReal < 0)
                                    AgregarError("La saldo real debe ser mayor a 0. Del contrato " + registro.Contrato, encabezado, db);

                                encabezado.Cantidad = cantidad;
                                db.XML_Capital_Sociales.Add(registro);

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
        }

        public void ProcesarInversionActiva(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Inversion_Activa registro;
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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Inversion_Activa
                                    {
                                        IdEncabezado = encabezado,
                                        IdEmisor = info.Element("IdEmisor").Value,
                                        IdInstrumento = info.Element("IdInstrumento").Value,
                                        ValorFacial = Utilitarios.ConvertirADecimal(info.Element("ValorFacial").Value, "Emisor:  " + info.Element("IdEmisor").Value + ". El valor facial: "),
                                        ValorTransado = Utilitarios.ConvertirADecimal(info.Element("ValorTransado").Value, "Emisor:  " + info.Element("IdEmisor").Value + ". El valor transado: "),
                                        ValorMercado = Utilitarios.ConvertirADecimal(info.Element("ValorMercado").Value, "Emisor:  " + info.Element("IdEmisor").Value + ". El valor mercado: "),
                                        FechaVencimiento = Utilitarios.ConvertirAFechaNula(info.Element("FechaVencimiento").Value),
                                        CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value,
                                        SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value, "Emisor:  " + info.Element("IdEmisor").Value + ". El saldo principal: "),
                                    }).FirstOrDefault();


                                    if (string.IsNullOrEmpty(registro.IdInstrumento))
                                        AgregarError("La id instrumento no puede estar vacío. Del emisor " + registro.IdEmisor, encabezado, db);

                                    if (registro.ValorFacial < 0)
                                        AgregarError("La valor facial debe ser mayor a 0. Del instrumento " + registro.IdInstrumento, encabezado, db);

                                    if (registro.ValorTransado < 0)
                                        AgregarError("La valor transado debe ser mayor a 0. Del instrumento " + registro.IdInstrumento, encabezado, db);

                                    if (registro.ValorMercado < 0)
                                        AgregarError("La valor mercado debe ser mayor a 0. Del instrumento " + registro.IdInstrumento, encabezado, db);

                                    if (registro.CuentaContablePrincipal.Length != 8)
                                        AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe tener longitud de 8", encabezado, db);

                                    if (registro.CuentaContablePrincipal.StartsWith("0") || registro.CuentaContablePrincipal.StartsWith("9"))
                                        AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " no debe empezar por 8 o 9", encabezado, db);

                                    string caracter = registro.CuentaContablePrincipal.Substring(5, 1);
                                    if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                        AgregarError("El sexto dígito de la cuenta contable principal " + registro.CuentaContablePrincipal + " debe ser 0, 1, 2 o 3", encabezado, db);

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Inversion_Activas.Add(registro);

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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void Procesar_Pasivos_Cuenta_Contable_210(XmlReader reader, XML_Encabezado encabezado, long contableEstados, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Pasivo_Cuenta_Contable_210 registro;
                decimal totalPasivos = 0;

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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Pasivo_Cuenta_Contable_210
                                    {
                                        IdEncabezado = encabezado,
                                        IdAcreedor = info.Element("IdAcreedor").Value,
                                        IdOperacion = info.Element("IdOperacion").Value,
                                        FechaFormalizacion = Utilitarios.ConvertirAFecha(info.Element("FechaFormalizacion").Value),
                                        Tasa = Utilitarios.ConvertirADecimal(info.Element("Tasa").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". La tasa: "),
                                        CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value,
                                        SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo principal: "),
                                        SaldoProducto = Utilitarios.ConvertirADecimal(info.Element("SaldoProducto").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo principal: "),
                                        TipoMonedaObligacion = Utilitarios.ConvertirAEntero(info.Element("TipoMonedaObligacion").Value),
                                        TipoTasa = info.Element("TipoTasa").Value,
                                        FechaVencimiento = Utilitarios.ConvertirAFecha(info.Element("FechaVencimiento").Value),
                                        Condicion = "NUEVO",
                                        Tipo = "NO APLICA",
                                        TipoDepositoFGDLEY9816 = Utilitarios.ConvertirAEntero(info.Element("TipoDepositoFGDLEY9816").Value),
                                    }).FirstOrDefault();

                                    if (registro.SaldoPrincipal > 0 || registro.SaldoProducto > 0)
                                    {
                                        if (registro.TipoMonedaObligacion != 1 && registro.TipoMonedaObligacion != 2 && registro.TipoMonedaObligacion != 3)
                                            AgregarError("El tipo de moneda obligación debe tener un valor de 1, 2 o 3. Valor inválido: " + registro.TipoMonedaObligacion + " del acreedor " + registro.IdAcreedor, encabezado, db);

                                        if (registro.TipoTasa != "F" && registro.TipoTasa != "V")
                                            AgregarError("El tipo de tasa debe ser F o V. Valor inválido: " + registro.TipoTasa + " del acreedor " + registro.IdAcreedor, encabezado, db);

                                        if (registro.Tasa < 0)
                                            AgregarError("La tasa debe ser mayor a 0. Valor inválido: " + registro.Tasa.ToString() + " del acreedor " + registro.IdAcreedor, encabezado, db);

                                        if (registro.Tasa < 1)
                                            registro.Tasa *= 100;

                                        if (registro.CuentaContablePrincipal.Length != 8)
                                            AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe tener longitud de 8. Id operación: " + registro.IdOperacion, encabezado, db);
                                        else
                                        {
                                            if (!registro.CuentaContablePrincipal.StartsWith("21"))
                                                AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe empezar por 21", encabezado, db);

                                            string caracter = registro.CuentaContablePrincipal.Substring(5, 1);
                                            if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                                AgregarError("El sexto dígito de la cuenta contable principal " + registro.CuentaContablePrincipal + " debe ser 0, 1, 2 o 3", encabezado, db);
                                        }

                                        totalPasivos += registro.SaldoPrincipal + registro.SaldoProducto;
                                        encabezado.Cantidad = cantidad;
                                        db.XML_Pasivo_Cuenta_Contable_210s.Add(registro);

                                        if (cantidad % Utilitarios.lineasCommit == 0)
                                        {
                                            db.Entry(encabezado).State = EntityState.Modified;
                                            db.SaveChanges();
                                        }
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

                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();

                var duplicados = db.XML_Pasivo_Cuenta_Contable_210s.Include("IdEncabezado").Where(i => i.IdEncabezado.Id == encabezado.Id).GroupBy(i => i.IdOperacion)
                 .Where(x => x.Count() > 1)
                 .Select(val => val.Key).ToList();

                foreach (var item in duplicados)
                {
                    AgregarError("El id de operación: " + item + " aparece duplicado.", encabezado, db);
                }

                //Valida que el monto del archivo corresponda al monto de las cuentas contables
                var tablaCuentas = db.XML_Contable_Estados.Include("IdEncabezado").Where(o => o.IdEncabezado.Id == contableEstados && o.Cuenta == Utilitarios.cuentaPasivos210).ToList();
                var monto210 = tablaCuentas.FirstOrDefault().SaldoFinal;

                if (Math.Abs((totalPasivos - monto210)) > 1000)
                    AgregarError("El saldo de la cuenta " + Utilitarios.cuentaPasivos210 + ": " + monto210.ToString("N2") + " del XML_Contable_Estados.xml no puede ser diferente en más de 1000 con " +
                        "la suma total de todos los pasivos del XML_Pasivos_Cuenta_210.xml: " + totalPasivos.ToString("N2"), encabezado, db);
            }
            catch (Exception e)
            {
                AgregarError("Detalle: " + e.Message, encabezado, db);
            }
        }

        public void ProcesarCredito_Operacion_DirInd_XML(XmlReader reader, XML_Encabezado encabezado, long contableEstados, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Credito_Operacion_DirInd registro;
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
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    cantidad += 1;
                                    try
                                    {
                                        registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Operacion_DirInd
                                        {
                                            IdEncabezado = encabezado,
                                            IdDeudor = info.Element("IdDeudor").Value,
                                            IdOperacion = info.Element("IdOperacion").Value,
                                            TipoCartera = Utilitarios.ConvertirAEntero(info.Element("TipoCartera").Value),
                                            EstadoOperacionCrediticia = Utilitarios.ConvertirAEntero(info.Element("EstadoOperacionCrediticia").Value),
                                            CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value.Trim(),
                                            CuentaContableProducto = info.Element("CuentaContableProductos").Value.Trim(),
                                            SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo principal: "),
                                            SaldoProductos = Utilitarios.ConvertirADecimal(info.Element("SaldoProductos").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo productos: "),
                                            SaldoComisiones = Utilitarios.ConvertirADecimal(info.Element("SaldoComisiones").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo comisión: "),
                                            FechaVencimiento = Utilitarios.ConvertirAFecha(info.Element("FechaVencimiento").Value),
                                            TasaInteresNominalVigente = Utilitarios.ConvertirADecimal(info.Element("TasaInteresNominalVigente").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". La tasa nominal: "),
                                            MontoCuotaPrincipalActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaPrincipalActual").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El monto de cuota: "),
                                            IndNueva = false,
                                        }).FirstOrDefault();
                                    }
                                    catch (Exception)
                                    {
                                        registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Operacion_DirInd
                                        {
                                            IdEncabezado = encabezado,

                                            // Asignar valores seguros con validación de null
                                            IdDeudor = info.Element("IdDeudor")?.Value ?? "", // Valor predeterminado si no existe
                                            IdOperacion = info.Element("IdOperacionCredito")?.Value ?? "", // Valor predeterminado si no existe

                                            // Convertir valores de manera segura, asignando 0 si no se puede convertir
                                            TipoCartera = Utilitarios.ConvertirAEntero(info.Element("TipoCarteraCrediticia")?.Value ?? "0"),
                                            EstadoOperacionCrediticia = Utilitarios.ConvertirAEntero(info.Element("TipoEstadoOperacionCrediticia")?.Value ?? "0"),

                                            // Usar valores predeterminados si el valor del elemento es nulo
                                            CodigoCategoriaRiesgo = Utilitarios.ObtenerCategoriaRiesgo(info.Element("CodigoCategoriaRiesgo")?.Value ?? "0"),

                                            // Verificar si el elemento existe y luego asignar el valor
                                            CuentaContablePrincipal = info.Element("CuentaContablePrincipal")?.Value?.Trim() ?? "",
                                            CuentaContableProducto = info.Element("CuentaContableProductosPorCobrar")?.Value?.Trim() ?? "",

                                            // Convertir a decimal de manera segura
                                            SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipalOperacionCrediticia")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El saldo principal: "),
                                            SaldoProductos = Utilitarios.ConvertirADecimal(info.Element("SaldoProductosPorCobrar")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El saldo productos: "),
                                            SaldoComisiones = Utilitarios.ConvertirADecimal(info.Element("SaldoComisionesOperacionesContingentes")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El saldo comisión: "),

                                            // Conversión segura de fechas
                                            FechaVencimiento = Utilitarios.ConvertirAFecha(info.Element("FechaVencimiento")?.Value ?? "01/01/1900"),

                                            // Convertir tasa de interés de forma segura
                                            TasaInteresNominalVigente = Utilitarios.ConvertirADecimal(info.Element("TasaInteresNominalVigente")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". La tasa nominal: "),

                                            // Convertir monto de cuota de forma segura
                                            MontoCuotaPrincipalActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaPrincipalActual")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El monto de cuota: "),

                                            // Verificar si los valores existen antes de asignarlos
                                            IndNueva = false,
                                            IndicadorOperacionModificada = info.Element("IndicadorOperacionModificada")?.Value?.Trim() ?? "",
                                            TipoSegmento = info.Element("TipoSegmento")?.Value?.Trim() ?? "",
                                            CodigoEtapa = info.Element("CodigoEtapa")?.Value?.Trim() ?? "",
                                            TipoMonedaOperacion = info.Element("TipoMonedaOperacion")?.Value?.Trim() ?? "",

                                            // Usar valores predeterminados para los valores de dinero
                                            MontoDesembolsado = Utilitarios.ConvertirADecimal(info.Element("MontoDesembolsado")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". Monto desembolsado: "),

                                            // Convertir fecha de forma segura
                                            FechaFormalizacion = Utilitarios.ConvertirAFecha(info.Element("FechaFormalizacion")?.Value ?? "01/01/1900"),

                                            // Convertir EAD de forma segura
                                            EAD = Utilitarios.ConvertirADecimal(info.Element("EAD")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". EAD: "),

                                            // Convertir monto de cuota de intereses de forma segura
                                            MontoCuotaInteresesActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaInteresesActual")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El interes actual: "),

                                            // Convertir monto estimación de forma segura
                                            MontoEstimacionEspecifica = Utilitarios.ConvertirADecimal(info.Element("MontoEstimacionEspecifica")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". Monto estimación: "),

                                            // Convertir monto formalizado de forma segura
                                            MontoFormalizadoOperacionCrediticia = Utilitarios.ConvertirADecimal(info.Element("MontoFormalizadoOperacionCrediticia")?.Value ?? "0",
                                            "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". Monto formalizado: "),

                                            // Usar valor predeterminado en caso de que el elemento esté vacío o no exista
                                            IndicadorOperacionNueva = info.Element("IndicadorOperacionNueva")?.Value?.Trim() ?? "N",

                                        }).FirstOrDefault();                                        
                                    }

                                    if (registro.TasaInteresNominalVigente < 1)
                                        registro.TasaInteresNominalVigente *= 100;

                                    if (registro.TasaInteresNominalVigente > 100 || registro.TasaInteresNominalVigente < 0)
                                        AgregarError("La tasa de interes nominal debe estar comprendida entre 0 y 100. Valor inválido: " + registro.TasaInteresNominalVigente + " de la operación " + registro.IdOperacion, encabezado, db);

                                    if (registro.IdDeudor.Length > 30 || registro.IdDeudor.Length == 0)
                                        AgregarError("El id del deudor debe tener entre 1 y 30 caracteres. Valor inválido: " + registro.IdDeudor + " de la operación " + registro.IdOperacion, encabezado, db);

                                    if (registro.IdOperacion.Length > 35 || registro.IdOperacion.Length == 0)
                                        AgregarError("El id de la operación debe tener entre 1 y 35 caracteres. Valor inválido: " + registro.IdOperacion + " del deudor " + registro.IdDeudor, encabezado, db);

                                    if (registro.TipoCartera < 0 || registro.TipoCartera > 11)
                                        AgregarError("El tipo de cartera debe tener entre 0 y 11 números. Valor inválido: " + registro.TipoCartera + " de la operación " + registro.IdOperacion, encabezado, db);

                                    if (registro.EstadoOperacionCrediticia < 0 || registro.EstadoOperacionCrediticia > 11)
                                        AgregarError("El tipo de cartera debe tener entre 0 y 11 números. Valor inválido: " + registro.TipoCartera + " de la operación " + registro.IdOperacion, encabezado, db);

                                    if (registro.CuentaContablePrincipal.Length != 8 && registro.MontoCuotaPrincipalActual > 0)
                                        AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe tener longitud de 8. Ya que tiene saldo. Id Operación: " + registro.IdOperacion, encabezado, db);
                                    else
                                    {
                                        if (registro.CuentaContablePrincipal.Length > 0)
                                        {
                                            if (registro.CuentaContablePrincipal.StartsWith("0") || registro.CuentaContablePrincipal.StartsWith("9"))
                                                AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " no debe empezar por 8 o 9", encabezado, db);

                                            string caracter = registro.CuentaContablePrincipal.Substring(5, 1);
                                            if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                                AgregarError("El sexto dígito de la cuenta contable principal " + registro.CuentaContablePrincipal + " debe ser 0, 1, 2 o 3", encabezado, db);
                                        }
                                    }

                                    encabezado.Cantidad = cantidad;
                                    db.XML_Credito_Operaciones_DirInds.Add(registro);
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

                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();

                Entities.Entities.Procedures.FGAEntities sp = new Entities.Entities.Procedures.FGAEntities();
                sp.FGA_Valida_Operaciones_Dir_Contable(encabezado.Periodo, encabezado.IdEntidad_Id, encabezado.Id);

            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public void ProcesarCredito_Deudor_XML(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            int cantidad = 0;
            while (reader != null && reader.Read())
            {
                try
                {
                    XML_Credito_Deudor registro;
                    reader.MoveToAttribute("Registro");
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("Registro"))
                            {
                                XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                cantidad += 1;


                                try
                                {
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Deudor
                                    {
                                        IdEncabezado = encabezado,
                                        CategoriaRiesgo = info.Element("CategoriaRiesgo").Value,
                                        IdDeudor = info.Element("IdDeudor").Value
                                    }).FirstOrDefault();

                                    registro.TipoCategoriaRiesgoSBD = 0;
                                }
                                catch (Exception)
                                {
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Deudor
                                    {
                                        IdEncabezado = encabezado,
                                        CategoriaRiesgo = "",
                                        IdDeudor = info.Element("IdDeudor").Value
                                    }).FirstOrDefault();
                                }


                                registro.CategoriaRiesgo = registro.CategoriaRiesgo.Trim();

                                if (registro.IdDeudor.Length > 30 || registro.IdDeudor.Length == 0)
                                    AgregarError("El id del deudor debe tener entre 1 y 30 caracteres. Valor inválido: " + registro.IdDeudor, encabezado, db);

                                //if (registro.TipoCategoriaRiesgoSBD == 0)
                                //{
                                //    if (registro.CategoriaRiesgo != "A1" && registro.CategoriaRiesgo != "A2" && registro.CategoriaRiesgo != "B1" && registro.CategoriaRiesgo != "B2"
                                //        && registro.CategoriaRiesgo != "C1" && registro.CategoriaRiesgo != "C2" && registro.CategoriaRiesgo != "D" && registro.CategoriaRiesgo != "E"))
                                //        AgregarError("La categoría de riesgo debe ser uno de los siguiente valores (A1, A2, B1, B2, C1, C2, D, E). Valor inválido: " + registro.CategoriaRiesgo + " del deudor " + registro.IdDeudor, encabezado, db);
                                //}
                                //else
                                //{
                                //    registro.CategoriaRiesgo = registro.TipoCategoriaRiesgoSBD.ToString();
                                //    if (registro.CategoriaRiesgo != "1" && registro.CategoriaRiesgo != "2" && registro.CategoriaRiesgo != "3"
                                //        && registro.CategoriaRiesgo != "4" && registro.CategoriaRiesgo != "5" && registro.CategoriaRiesgo != "6")
                                //        AgregarError("La categoría de riesgo debe ser uno de los siguiente valores (1,2,3,4,5,6). Valor inválido: " + registro.CategoriaRiesgo + " del deudor " + registro.IdDeudor, encabezado, db);
                                //}

                                encabezado.Cantidad = cantidad;
                                db.XML_Credito_Deudores.Add(registro);

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
        }

        public void ProcesarCuota_Atrasada_XML(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            int cantidad = 0;
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
                                XML_Credito_Cuota_Atrasada registro = new XML_Credito_Cuota_Atrasada();

                                registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Cuota_Atrasada
                                {
                                    IdEncabezado = encabezado,
                                    DiasAtraso = Utilitarios.ConvertirAEntero(info.Element("DiasAtraso").Value),
                                    IdDeudor = info.Element("IdDeudor").Value,
                                    IdOperacion = info.Element("IdOperacionCredito").Value,
                                    MontoCuotaAtrasada = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaAtrasada").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". El monto de cuota: ")
                                }).FirstOrDefault();


                                if (registro.IdDeudor.Length > 30 || registro.IdDeudor.Length == 0)
                                    AgregarError("El id del deudor debe tener entre 1 y 30 caracteres. Valor inválido: " + registro.IdDeudor + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.IdOperacion.Length > 35 || registro.IdOperacion.Length == 0)
                                    AgregarError("El id de la operación debe tener entre 1 y 35 caracteres. Valor inválido: " + registro.IdOperacion + " del deudor " + registro.IdDeudor, encabezado, db);

                                if (registro.DiasAtraso < 0 || registro.DiasAtraso > 999999)
                                    AgregarError("Los días de atraso deben tener entre 1 y 6 números. Valor inválido: " + registro.DiasAtraso.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                encabezado.Cantidad = cantidad;
                                db.XML_Credito_Cuotas_Atrasadas.Add(registro);
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
        }

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
