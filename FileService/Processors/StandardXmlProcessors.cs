using FileService.Utils;
using FGA.Models;
using FGA.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FileService.Processors
{
    public class SuficienciaPatrimonialProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
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

                                    db.XML_Suficiencia_Patrimonial.Add(registro);

                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();

                resultado = (S20000 / ((S30000 + S40000) + 10 * (S50000 + S60000 + S70000))) * 100;

                if (Math.Abs(S10000 - resultado) > 0.1M)
                    XmlErrorLogger.AgregarError("No se cumple la condición. 10000 =  (20000/ ((30000 + 40000) + 10 * (50000 + 60000 + 70000))) * 100", encabezado, db);
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class IclProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
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

                                    db.XML_ICLs.Add(registro);
                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class IndicadoresFinancierosProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
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

                                    db.XML_Indicadores_Financieros.Add(registro);
                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class CalcePlazoProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
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

                                    db.XML_Rango_Calce_Plazo.Add(registro);
                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class ContableBrechaProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
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

                                    db.XML_Contable_Brecha.Add(registro);
                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();

                if (Math.Abs(B22110 + B22210 - B22310) > 1)
                    XmlErrorLogger.AgregarError("El monto de la cuenta 22310 debe ser igual a la cuenta 22110 + 22210", encabezado, db);

                if (Math.Abs(B22120 + B22220 - B22320) > 1)
                    XmlErrorLogger.AgregarError("El monto de la cuenta 22320 debe ser igual a la cuenta 22120 + 22220", encabezado, db);

                if (Math.Abs(B22111 + B22112 + B22211 + B22212 - B22310) > 1)
                    XmlErrorLogger.AgregarError("El monto de la cuenta 22310 debe ser igual a la cuenta 22111 + 22112 + 22211 + 22212", encabezado, db);

                if (Math.Abs(B22121 + B22122 + B22123 + B22221 + B22222 + B22223 - B22320) > 1)
                    XmlErrorLogger.AgregarError("El monto de la cuenta 22320 debe ser igual a la cuenta 22121 + 22122 + 22123 + 22221 + 22222 + 22223", encabezado, db);
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class ContableDatosProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            try
            {
                int cantidad = 0;
                XML_Contable_DatosAdicionales registro;
                List<XML_Contable_Estado> xml_contable_estados = null;
                if (contableEstadosId.HasValue)
                {
                    xml_contable_estados = db.XML_Contable_Estados
                        .Where(o => o.IdEncabezado.Id == contableEstadosId.Value).ToList();
                }

                decimal vD20018 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("23402000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20019 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("21500000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20020 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("24101000") || o.Cuenta.Equals("24110000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20021 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("24102000") || o.Cuenta.Equals("24111000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20022 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("23299000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20037 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("21199000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20181 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("26000000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20053 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("31102102")).Sum(o => o.SaldoFinal) : 0;
                decimal vD20146 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("34302101")).Sum(o => o.SaldoFinal) : 0;

                decimal vD20027 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("12101000") || o.Cuenta.Equals("12102000") ||
                                                                  o.Cuenta.Equals("12107000") || o.Cuenta.Equals("12201000") ||
                                                                  o.Cuenta.Equals("12202000") || o.Cuenta.Equals("12260000") ||
                                                                  o.Cuenta.Equals("12261000") || o.Cuenta.Equals("12301000") ||
                                                                  o.Cuenta.Equals("12302000") || o.Cuenta.Equals("12307000")).Sum(o => o.SaldoFinal) : 0;

                decimal vD20028 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("12101000") || o.Cuenta.Equals("12102000") ||
                                                      o.Cuenta.Equals("12107000") || o.Cuenta.Equals("12201000") ||
                                                      o.Cuenta.Equals("12202000") || o.Cuenta.Equals("12207000") ||
                                                      o.Cuenta.Equals("12260000") || o.Cuenta.Equals("12261000") ||
                                                      o.Cuenta.Equals("12301000") || o.Cuenta.Equals("12302000") ||
                                                      o.Cuenta.Equals("12307000")).Sum(o => o.SaldoFinal) : 0;

                decimal vD20186_87 = xml_contable_estados != null ? xml_contable_estados.Where(o => (o.Cuenta.StartsWith("12101") ||
                                                                  o.Cuenta.StartsWith("12102") || o.Cuenta.StartsWith("12107") ||
                                                                  o.Cuenta.StartsWith("12201") || o.Cuenta.StartsWith("12202") ||
                                                                  o.Cuenta.StartsWith("12207") || o.Cuenta.StartsWith("12260") ||
                                                                  o.Cuenta.StartsWith("12261") || o.Cuenta.StartsWith("12301") ||
                                                                  o.Cuenta.StartsWith("12302") || o.Cuenta.StartsWith("12307")) &&
                                                                  (o.Cuenta.EndsWith("100") || o.Cuenta.EndsWith("300"))).Sum(o => o.SaldoFinal) : 0;

                decimal vD20188_89 = xml_contable_estados != null ? xml_contable_estados.Where(o => (o.Cuenta.StartsWith("12101") ||
                                                                o.Cuenta.StartsWith("12102") || o.Cuenta.StartsWith("12107") ||
                                                                o.Cuenta.StartsWith("12201") || o.Cuenta.StartsWith("12202") ||
                                                                o.Cuenta.StartsWith("12207") || o.Cuenta.StartsWith("12260") ||
                                                                o.Cuenta.StartsWith("12261") || o.Cuenta.StartsWith("12301") ||
                                                                o.Cuenta.StartsWith("12302") || o.Cuenta.StartsWith("12307")) &&
                                                                (o.Cuenta.EndsWith("200"))).Sum(o => o.SaldoFinal) : 0;

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
                                        XmlErrorLogger.AgregarError("La cuenta D20018 debe tener un monto menor o igual a la cuenta 23402 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20019 && registro.MontoDatoAdicional > vD20019)
                                        XmlErrorLogger.AgregarError("La cuenta D20019 debe tener un monto menor o igual a la cuenta 215 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20020 && registro.MontoDatoAdicional > vD20020)
                                        XmlErrorLogger.AgregarError("La cuenta D20020 debe tener un monto menor o igual a la suma de la cuenta 24101 y 24110 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20021 && registro.MontoDatoAdicional > vD20021)
                                        XmlErrorLogger.AgregarError("La cuenta D20021 debe tener un monto menor o igual a la suma de la cuenta 24102 y 24111 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20022 && registro.MontoDatoAdicional > vD20022)
                                        XmlErrorLogger.AgregarError("La cuenta D20022 debe tener un monto menor o igual a la cuenta 23299 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20037 && registro.MontoDatoAdicional > vD20037)
                                        XmlErrorLogger.AgregarError("La cuenta D20037 debe ser menor o igual a la cuenta 21199 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20181 && registro.MontoDatoAdicional > vD20181)
                                        XmlErrorLogger.AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a la cuenta 26000 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20053 && registro.MontoDatoAdicional > vD20053)
                                        XmlErrorLogger.AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a la cuenta 31102102 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20027 && registro.MontoDatoAdicional > vD20027)
                                        XmlErrorLogger.AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12260 + 12261 + 12301 + 12302 + 12307 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20028 && registro.MontoDatoAdicional > vD20028)
                                        XmlErrorLogger.AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12207 + 12260 + 12261 + 12301 + 12302 + 12307 del xml contable estados", encabezado, db);

                                    if ((registro.CuentaCatalogo == 20186 || registro.CuentaCatalogo == 20187) && registro.MontoDatoAdicional > vD20186_87)
                                        XmlErrorLogger.AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12207 + 12260 + 12261 + 12301 + 12302 + 12307 cuyos últimos dígitos sean 100 o 300 del xml contable estados", encabezado, db);

                                    if ((registro.CuentaCatalogo == 20188 || registro.CuentaCatalogo == 20189) && registro.MontoDatoAdicional > vD20188_89)
                                        XmlErrorLogger.AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a sumatoria de las cuentas 12101 + 12102 + 12107 + 12201 + 12202 + 12207 + 12240 + 12241 + 12301 + 12302 + 12307 cuyos últimos dígitos sean 200 del xml contable estados", encabezado, db);

                                    if (registro.CuentaCatalogo == 20146 && registro.MontoDatoAdicional > vD20146)
                                        XmlErrorLogger.AgregarError("La cuenta D" + registro.CuentaCatalogo + " debe tener un monto menor o igual a la cuenta 34302101 del xml contable estados", encabezado, db);

                                    if ((registro.CuentaCatalogo == 20018 || registro.CuentaCatalogo == 20019
                                        || registro.CuentaCatalogo == 20020 || registro.CuentaCatalogo == 20021
                                        || registro.CuentaCatalogo == 20022 || registro.CuentaCatalogo == 20024
                                        || registro.CuentaCatalogo == 20025 || registro.CuentaCatalogo == 20026
                                        || registro.CuentaCatalogo == 20092 || registro.CuentaCatalogo == 20145
                                        || registro.CuentaCatalogo == 20191 || registro.CuentaCatalogo == 20192
                                        || registro.CuentaCatalogo == 20193 || registro.CuentaCatalogo == 20194
                                        || registro.CuentaCatalogo == 20105 || registro.CuentaCatalogo == 20115)
                                        && registro.MontoDatoAdicional < 0)
                                        XmlErrorLogger.AgregarError("La cuenta D " + registro.CuentaCatalogo + " debe tener un monto mayor o igual a 0", encabezado, db);

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

                                    db.XML_Contable_DatosAdicionales.Add(registro);
                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();

                if (D20189 > D20188)
                    XmlErrorLogger.AgregarError("El saldo de D20189 debe ser menor o igual al saldo de la cuenta D20188 ", encabezado, db);
                if (D20187 > D20186)
                    XmlErrorLogger.AgregarError("El saldo de D20187 debe ser menor o igual al saldo de la cuenta D20186 ", encabezado, db);
                if (D20186 + D20188 != D20027)
                    XmlErrorLogger.AgregarError("El saldo de D20186 + D20188 debe ser igual al saldo de la cuenta D20027 ", encabezado, db);
                if (D20187 + D20189 != D20028)
                    XmlErrorLogger.AgregarError("El saldo de D20187 + D20189 debe ser igual al saldo de la cuenta D20028 ", encabezado, db);
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class FlujoEfectivoProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            try
            {
                int cantidad = 0;
                XML_Flujo_EfectivoReal registro;
                List<XML_Contable_Estado> xml_contable_estados = null;
                if (contableEstadosId.HasValue)
                {
                    xml_contable_estados = db.XML_Contable_Estados
                        .Where(o => o.IdEncabezado.Id == contableEstadosId.Value).ToList();
                }

                decimal F10000 = 0, F11000 = 0, F12000 = 0;
                decimal vD11202 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("11202000")).Sum(o => o.SaldoFinal) : 0;
                decimal vD11000 = xml_contable_estados != null ? xml_contable_estados.Where(o => o.Cuenta.Equals("11000000")).Sum(o => o.SaldoFinal) : 0;

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

                                    db.XML_Flujo_EfectivoReal.Add(registro);
                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();

                if (F10000 != F11000 - F12000)
                    XmlErrorLogger.AgregarError("El monto de la cuenta 10000 debe ser igual a la cuenta 11000 - 12000", encabezado, db);

                if (F10000 != vD11000 - vD11202)
                    XmlErrorLogger.AgregarError("El monto de la cuenta 10000  monto: " + F10000.ToString() + " debe ser igual a la cuenta 110 (Disponibilidades) - el saldo de la subcuenta 11202 del xml contable estado. Saldo: " + (vD11000 - vD11202).ToString(), encabezado, db);
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class CapitalSocialProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            try
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
                                        XmlErrorLogger.AgregarError("La saldo real debe ser mayor a 0. Del contrato " + registro.Contrato, encabezado, db);

                                    db.XML_Capital_Sociales.Add(registro);

                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }

    public class InversionActivaProcessor : BaseXmlProcessor
    {
        public override void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
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
                                        XmlErrorLogger.AgregarError("La id instrumento no puede estar vacío. Del emisor " + registro.IdEmisor, encabezado, db);

                                    if (registro.ValorFacial < 0)
                                        XmlErrorLogger.AgregarError("La valor facial debe ser mayor a 0. Del instrumento " + registro.IdInstrumento, encabezado, db);

                                    if (registro.ValorTransado < 0)
                                        XmlErrorLogger.AgregarError("La valor transado debe ser mayor a 0. Del instrumento " + registro.IdInstrumento, encabezado, db);

                                    if (registro.ValorMercado < 0)
                                        XmlErrorLogger.AgregarError("La valor mercado debe ser mayor a 0. Del instrumento " + registro.IdInstrumento, encabezado, db);

                                    if (registro.CuentaContablePrincipal.Length != 8)
                                        XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe tener longitud de 8", encabezado, db);

                                    if (registro.CuentaContablePrincipal.StartsWith("0") || registro.CuentaContablePrincipal.StartsWith("9"))
                                        XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " no debe empezar por 8 o 9", encabezado, db);

                                    string caracter = registro.CuentaContablePrincipal.Substring(5, 1);
                                    if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                        XmlErrorLogger.AgregarError("El sexto dígito de la cuenta contable principal " + registro.CuentaContablePrincipal + " debe ser 0, 1, 2 o 3", encabezado, db);

                                    db.XML_Inversion_Activas.Add(registro);

                                    if (cantidad % Utilitarios.lineasCommit == 0)
                                    {
                                        GuardarLote(db, encabezado);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                    }
                }

                GuardarLote(db, encabezado);
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
            }
        }
    }
}
