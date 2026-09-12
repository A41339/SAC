using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Entities.Entities.Procedures;
using FGA.Models;
using FGA.Utility;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;

namespace Historicos
{
    public class ProcessFile
    {

        public static bool GetInfoFile(string xmlPath, ref FGA.Models.File file)
        {
            try
            {
                var reader = XmlReader.Create(xmlPath);
                string ext = Path.GetExtension(xmlPath);
                FGA.Models.SIContext db = new FGA.Models.SIContext();
                if (ext.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
                {
                    while (reader != null && reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name.Equals("Encabezado"))
                                {
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    file = (from t in xDoc.Descendants("Encabezado").Elements()
                                            let IdEntidad = xDoc.Descendants("Encabezado").Elements().ElementAt(5).Value
                                            let IdArchivo = xDoc.Descendants("Encabezado").Elements().ElementAt(2).Value
                                            let Fecha = xDoc.Descendants("Encabezado").Elements().ElementAt(4).Value
                                            select new FGA.Models.File
                                            {
                                                Periodo = DateTime.Parse(Fecha),
                                                Entidad = db.Entidades.FirstOrDefault(o => o.Identificacion == IdEntidad).Nombre,
                                                Codigo = Utilitarios.ObtenerIdArchivo(IdArchivo),
                                                Archivo = Utilitarios.GetFileName(IdArchivo),
                                            }).FirstOrDefault();
                                    reader.Dispose();
                                }
                                break;
                        }
                    }
                }
                else
                {
                    file.Error = string.Empty;
                    return false;
                }

                if (file.Entidad == null)
                {
                    file.Error = "La entidad que esta intentando registrar no ha sido configurada. Revise el campo IdEntidad del XML";
                    return false;
                }
                else
                {
                    if (//file.Codigo == Utilitarios.xml_contable_estado
                    //    || file.Codigo == Utilitarios.xml_calce_plazo
                    //    || file.Codigo == Utilitarios.xml_contable_brecha
                    //    || file.Codigo == Utilitarios.xml_contable_datos_adicionales
                    //    || file.Codigo == Utilitarios.xml_flujo_efectivo
                    //    || file.Codigo == Utilitarios.xml_credito_bienes_realizables
                    //    || file.Codigo == Utilitarios.xml_credito_cuentas_cobrar
                    //    || file.Codigo == Utilitarios.xml_credito_cuota_atrasada
                    //    || file.Codigo == Utilitarios.xml_credito_deudor
                    //    || file.Codigo == Utilitarios.xml_credito_garantia_operaciones
                    //    || file.Codigo == Utilitarios.xml_credito_informacion_oper_no_reportadas
                    //    || file.Codigo == Utilitarios.xml_credito_oper_dirind
                        file.Codigo == Utilitarios.xml_icl
                        //|| file.Codigo == Utilitarios.xml_inversiones_activas
                        //|| file.Codigo == Utilitarios.xml_pasivos_cuentas_contables_210
                        //|| file.Codigo == Utilitarios.xml_indicadores_financieros
                        //|| file.Codigo == Utilitarios.xml_suficiencia_patrimonial
                        //|| file.Codigo == Utilitarios.xml_crediticio_bienesRealizablesNoReportadas_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_bienesRealizables_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_cuentasPorCobrarNoAsociadas_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_cuotasAtrasadas_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_deudores_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_garantiasCartasCredito_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_garantiasFacturasCedidas_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_garantiasFiduciarias_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_garantiasOperacion_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_garantiasPolizas_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_garantiasReales_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_garantiasValores_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_operacionesNoReportadas_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_operaciones_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_ingresoDeudores_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_origenRecursos_1421 
                        //|| file.Codigo == Utilitarios.xml_crediticio_actividadEconomica_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_naturalezaGasto_1421 
                        //|| file.Codigo == Utilitarios.xml_crediticio_cuentasCobrar_1421 
                        //|| file.Codigo == Utilitarios.xml_crediticio_creditosSindicados_1421 
                        //|| file.Codigo == Utilitarios.xml_crediticio_codeudores_1421 
                        //|| file.Codigo == Utilitarios.xml_crediticio_modificacion_1421 
                        //|| file.Codigo == Utilitarios.xml_crediticio_compradasRefinanciadas_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_cambioClimatico_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_operBienesRealizables_1421 
                        //|| file.Codigo == Utilitarios.xml_crediticio_fideicomiso_1421
                        //|| file.Codigo == Utilitarios.xml_crediticio_gravamenes_1421 
                        /*|| file.Codigo == Utilitarios.xml_crediticio_garantiasMobiliarias_1421*/)
                    {
                        file.Periodo = new DateTime(file.Periodo.Year, file.Periodo.Month, 1);
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception e)
            {
                file.Error = "Error al guardar el archivo: " + e.Message;
                return false;
            }
        }

        public static string Add(string file, FGA.Models.Usuario ObjUser, SIContext db)
        {
            XML_Encabezado encabezado = new XML_Encabezado();
            FGAEntities proc = new FGAEntities();
            try
            {
                var reader = XmlReader.Create(file);
                DateTime fecha = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

                try
                {
                    while (reader != null && reader.Read())
                    {
                        reader.MoveToAttribute("Encabezado");
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name.Equals("Encabezado"))
                                {
                                    XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                    encabezado = (from t in xDoc.Elements("Encabezado").Elements()
                                                  let IdEntidad = xDoc.Descendants("Encabezado").Elements().ElementAt(5).Value
                                                  let IdArchivo = Utilitarios.ObtenerIdArchivo(xDoc.Descendants("Encabezado").Elements().ElementAt(2).Value)
                                                  let Fecha = xDoc.Descendants("Encabezado").Elements().ElementAt(4).Value
                                                  select new XML_Encabezado
                                                  {
                                                      Periodo = DateTime.Parse(Fecha),
                                                      IdEntidad = db.Entidades.FirstOrDefault(o => o.Identificacion == IdEntidad),
                                                      IdArchivo = db.TipoXMLs.FirstOrDefault(o => o.Id == IdArchivo),
                                                  }).FirstOrDefault();
                                    reader.Dispose();
                                }
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    reader.Dispose();
                }

                var CantidadParameter = new ObjectParameter("Existe", typeof(int));
                proc.FGA_Valida_Existe_Archivo(encabezado.Periodo, encabezado.IdEntidad.Id, encabezado.IdArchivo.Id, CantidadParameter);

                if (int.Parse(CantidadParameter.Value.ToString()) == 0)
                {
                    encabezado.FechaCarga = DateTime.Now;
                    encabezado.IdEstado = db.ArchivoEstados.FirstOrDefault(o => o.Id == Utilitarios.archivoCargado);
                    encabezado.IdUsuario = ObjUser;
                    encabezado.Periodo = new DateTime(encabezado.Periodo.Year, encabezado.Periodo.Month, 1);
                    db.XML_Encabezados.Add(encabezado);
                    db.SaveChanges();
                    db.Entry(encabezado).GetDatabaseValues();
                    ProcessFile.Process(file, encabezado.Id);
                    return encabezado.IdEntidad.Id;
                }
            }
            catch (Exception)
            {
            }

            return string.Empty;
        }


        public static void Process(string xmlPath, Int64 idEncabezado)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.IgnoreProcessingInstructions = true;
            settings.CheckCharacters = false;

            var reader = XmlReader.Create(xmlPath, settings);
            SIContext db = new SIContext();
            db.Configuration.AutoDetectChangesEnabled = false;
            XML_Encabezado encabezado = db.XML_Encabezados.Include("IdArchivo").FirstOrDefault(o => o.Id == idEncabezado);

            try
            {
                switch (encabezado.IdArchivo_Id)
                {
                   /* case Utilitarios.xml_suficiencia_patrimonial:
                        ProcesarSuficienciaPatrimonial(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_indicadores_financieros:
                        ProcesarIndicadoresFinancieros(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_contable_estado:
                        ProcesarContable_Estado_XML(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_calce_plazo:
                        Procesar_Calce_Plazo(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_contable_brecha:
                        Procesar_Contable_Brecha(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_contable_datos_adicionales:
                        ProcesarContable_Datos(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_flujo_efectivo:
                        Procesar_Flujo_Efectivo(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_credito_cuota_atrasada:
                        ProcesarCuota_Atrasada_XML(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_credito_deudor:
                        ProcesarCredito_Deudor_XML(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_credito_oper_dirind:
                        ProcesarCredito_Operacion_DirInd_XML(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_pasivos_cuentas_contables_210:
                        Procesar_Pasivos_Cuenta_Contable_210(reader, encabezado, db);
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
                        ProcesarCredito_Operacion_DirInd_XML(reader, encabezado, db);
                        break;
                    case Utilitarios.xml_crediticio_deudores_1421:
                        ProcesarCredito_Deudor_XML(reader, encabezado, db);
                        break;*/
                    case Utilitarios.xml_icl:
                        Procesar_ICL(reader, encabezado, db);   
                        break;
                    /*case Utilitarios.xml_indicador_financiero:
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
                        break;*/
                    default:
                        AgregarError("Archivo no implementado", encabezado, db);
                        break;
                }
            }
            catch (Exception e)
            {
                AgregarError("Error inesperado: " + e.Message, encabezado, db);
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
                encabezado.IdEstado = db.ArchivoEstados.Where(o => o.Id == Utilitarios.archivoAceptado).FirstOrDefault();

                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
                db.Dispose();
                reader.Dispose();
            }
        }

        public static void ProcesarCapitalSocial(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {

        }

        public static void ProcesarSuficienciaPatrimonial(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Suficiencia_Patrimonial registro;
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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }


        public static void Procesar_ICL(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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

        public static void ProcesarIndicadoresFinancieros(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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

        public static void Procesar_Calce_Plazo(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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

        public static void Procesar_Contable_Brecha(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Contable_Brecha registro;
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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public static void ProcesarContable_Datos(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Contable_DatosAdicionales registro;
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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public static void Procesar_Flujo_Efectivo(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                int cantidad = 0;
                XML_Flujo_EfectivoReal registro;
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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public static void ProcesarInversionActiva(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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
                                        ValorFacial = Utilitarios.ConvertirADecimal(info.Element("ValorFacial").Value),
                                        ValorTransado = Utilitarios.ConvertirADecimal(info.Element("ValorTransado").Value),
                                        ValorMercado = Utilitarios.ConvertirADecimal(info.Element("ValorMercado").Value),
                                        FechaVencimiento = Utilitarios.ConvertirAFechaNula(info.Element("FechaVencimiento").Value),
                                        CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value,
                                        SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value),
                                    }).FirstOrDefault();

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
        public static void Procesar_Pasivos_Cuenta_Contable_210(XmlReader reader, XML_Encabezado encabezado, SIContext db)
        {
            string idOperacion = string.Empty;
            try
            {
                int cantidad = 0;
                XML_Pasivo_Cuenta_Contable_210 registro;

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
                                        FechaFormalizacion = Utilitarios.ConvertirAFechaNula(info.Element("FechaFormalizacion").Value),
                                        Tasa = Utilitarios.ConvertirADecimal(info.Element("Tasa").Value),
                                        CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value,
                                        SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value),
                                        SaldoProducto = Utilitarios.ConvertirADecimal(info.Element("SaldoProducto").Value),
                                        TipoMonedaObligacion = Utilitarios.ConvertirAEntero(info.Element("TipoMonedaObligacion").Value),
                                        TipoTasa = info.Element("TipoTasa").Value,
                                        FechaVencimiento = Utilitarios.ConvertirAFechaNula(info.Element("FechaVencimiento").Value),
                                        Condicion = "NUEVO",
                                        Tipo = "NO APLICA",
                                        TipoDepositoFGDLEY9816 = Utilitarios.ConvertirAEntero(info.Element("TipoDepositoFGDLEY9816").Value),
                                    }).FirstOrDefault();

                                    if (registro.Tasa < 1)
                                        registro.Tasa = registro.Tasa * 100;

                                    idOperacion = registro.IdOperacion;
                                    encabezado.Cantidad = cantidad;
                                    db.XML_Pasivo_Cuenta_Contable_210s.Add(registro);

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
                AgregarError("El id de operación " + idOperacion + " está duplicado. Detalle: " + e.Message, encabezado, db);
            }
        }

        public static void ProcesarCredito_Operacion_DirInd_XML(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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
                                            SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value),
                                            SaldoProductos = Utilitarios.ConvertirADecimal(info.Element("SaldoProductos").Value),
                                            SaldoComisiones = Utilitarios.ConvertirADecimal(info.Element("SaldoComisiones").Value),
                                            FechaVencimiento = Utilitarios.ConvertirAFechaNula(info.Element("FechaVencimiento").Value),
                                            TasaInteresNominalVigente = Utilitarios.ConvertirADecimal(info.Element("TasaInteresNominalVigente").Value),
                                            MontoCuotaPrincipalActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaPrincipalActual").Value),
                                        }).FirstOrDefault();
                                    }
                                    catch (Exception)
                                    {
                                        registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Operacion_DirInd
                                        {
                                            IdEncabezado = encabezado,
                                            IdDeudor = info.Element("IdDeudor").Value,
                                            IdOperacion = info.Element("IdOperacionCredito").Value,
                                            TipoCartera = Utilitarios.ConvertirAEntero(info.Element("TipoCarteraCrediticia").Value),
                                            EstadoOperacionCrediticia = Utilitarios.ConvertirAEntero(info.Element("TipoEstadoOperacionCrediticia").Value),
                                            CodigoCategoriaRiesgo = Utilitarios.ObtenerCategoriaRiesgo(info.Element("CodigoCategoriaRiesgo").Value),
                                            CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value.Trim(),
                                            CuentaContableProducto = info.Element("CuentaContableProductosPorCobrar").Value.Trim(),
                                            SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipalOperacionCrediticia").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". El saldo principal: "),
                                            SaldoProductos = Utilitarios.ConvertirADecimal(info.Element("SaldoProductosPorCobrar").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". El saldo productos: "),
                                            SaldoComisiones = Utilitarios.ConvertirADecimal(info.Element("SaldoComisionesOperacionesContingentes").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". El saldo comisión: "),
                                            FechaVencimiento = Utilitarios.ConvertirAFecha(info.Element("FechaVencimiento").Value),
                                            TasaInteresNominalVigente = Utilitarios.ConvertirADecimal(info.Element("TasaInteresNominalVigente").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". La tasa nominal: "),
                                            MontoCuotaPrincipalActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaPrincipalActual").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". El monto de cuota: "),
                                            IndNueva = false,
                                            IndicadorOperacionModificada = info.Element("IndicadorOperacionModificada").Value.Trim(),
                                            TipoSegmento = info.Element("TipoSegmento").Value.Trim(),
                                            CodigoEtapa = info.Element("CodigoEtapa").Value.Trim(),
                                            TipoMonedaOperacion = info.Element("TipoMonedaOperacion").Value.Trim(),
                                            MontoDesembolsado = Utilitarios.ConvertirADecimal(info.Element("MontoDesembolsado").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". Monto desembolsado: "),
                                            FechaFormalizacion = Utilitarios.ConvertirAFecha(info.Element("FechaFormalizacion").Value),
                                            EAD = Utilitarios.ConvertirADecimal(info.Element("EAD").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". EAD: "),
                                            MontoCuotaInteresesActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaInteresesActual").Value, "Id de operación:  " + info.Element("MontoCuotaInteresesActual").Value + ". El interes actual: "),
                                            MontoEstimacionEspecifica = Utilitarios.ConvertirADecimal(info.Element("MontoEstimacionEspecifica").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". Monto estimación: "),
                                            MontoFormalizadoOperacionCrediticia = Utilitarios.ConvertirADecimal(info.Element("MontoFormalizadoOperacionCrediticia").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". Monto formalizado: "),
                                            IndicadorOperacionNueva = info.Element("IndicadorOperacionNueva").Value.Trim(),
                                        }).FirstOrDefault();
                                    }

                                    if (registro.TasaInteresNominalVigente < 1)
                                        registro.TasaInteresNominalVigente = registro.TasaInteresNominalVigente * 100;

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
            }
            catch (Exception e)
            {
                AgregarError(e.Message, encabezado, db);
            }
        }

        public static void ProcesarCredito_Deudor_XML(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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
                                        TipoCategoriaRiesgoSBD = Utilitarios.ConvertirAEntero(info.Element("TipoCategoriaRiesgoSBD").Value),
                                        IdDeudor = info.Element("IdDeudor").Value
                                    }).FirstOrDefault();
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Deudor
                                        {
                                            IdEncabezado = encabezado,
                                            CategoriaRiesgo = info.Element("CategoriaRiesgo").Value,
                                            IdDeudor = info.Element("IdDeudor").Value
                                        }).FirstOrDefault();
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

                                    registro.TipoCategoriaRiesgoSBD = 0;
                                }

                                encabezado.Cantidad = cantidad;
                                db.XML_Credito_Deudores.Add(registro);

                                if (registro.CategoriaRiesgo == "0")
                                    registro.CategoriaRiesgo = registro.TipoCategoriaRiesgoSBD.ToString();

                                if (registro.CategoriaRiesgo == "0")
                                    registro.CategoriaRiesgo = "1";

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

        public static void ProcesarCuota_Atrasada_XML(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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
                                try
                                {
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Cuota_Atrasada
                                    {
                                        IdEncabezado = encabezado,
                                        DiasAtraso = Utilitarios.ConvertirAEntero(info.Element("DiasAtraso").Value),
                                        IdDeudor = info.Element("IdDeudor").Value,
                                        IdOperacion = info.Element("IdOperacion").Value,
                                        MontoCuotaAtrasada = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaAtrasada").Value)
                                    }).FirstOrDefault();
                                }
                                catch (Exception)
                                {
                                    registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Cuota_Atrasada
                                    {
                                        IdEncabezado = encabezado,
                                        DiasAtraso = Utilitarios.ConvertirAEntero(info.Element("DiasAtraso").Value),
                                        IdDeudor = info.Element("IdDeudor").Value,
                                        IdOperacion = info.Element("IdOperacionCredito").Value,
                                        MontoCuotaAtrasada = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaAtrasada").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". El monto de cuota: ")
                                    }).FirstOrDefault();
                                }

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

        public static void ProcesarContable_Estado_XML(XmlReader reader, XML_Encabezado encabezado, SIContext db)
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
                                registro = new XML_Contable_Estado();
                                registro.IdEncabezado = encabezado;
                                registro.Cuenta = xDoc.Element("Registro").Element("Cuenta").Value;
                                registro.Credito = Utilitarios.ConvertirADecimal(xDoc.Element("Registro").Element("Credito").Value);
                                registro.Debito = Utilitarios.ConvertirADecimal(xDoc.Element("Registro").Element("Debito").Value);
                                registro.SaldoFinal = Utilitarios.ConvertirADecimal(xDoc.Element("Registro").Element("SaldoFinal").Value);
                                registro.TipoCatalogoSUGEF = Convert.ToInt32(xDoc.Element("Registro").Element("TipoCatalogoSUGEF").Value);
                                encabezado.Cantidad = cantidad;
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

        public static void AgregarError(string detalle, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                XML_Errores error = new XML_Errores();
                error.IdEncabezado = encabezado;
                error.Detalle = detalle;
                db.XML_Errores.Add(error);
                db.SaveChanges();
            }
            catch (Exception)
            {
            }
        }
    }
}