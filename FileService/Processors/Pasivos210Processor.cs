using FileService.Bulk;
using FileService.Utils;
using FGA.Models;
using FGA.Utility;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FileService.Processors
{
    public class Pasivos210Processor : IXmlProcessor
    {
        public void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            int cantidad = 0;
            decimal totalPasivos = 0;
            DataTable dt = CrearDataTablePasivos210();

            try
            {
                using (SqlConnection conn = SqlBulkCopyHelper.CrearConexionSql())
                {
                    while (reader != null && reader.Read())
                    {
                        try
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.LocalName.Equals("Registro", StringComparison.OrdinalIgnoreCase))
                            {
                                XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                var info = XmlRecordHelper.ObtenerElementoRegistro(xDoc);
                                if (info == null) continue;

                                cantidad += 1;
                                var registro = new XML_Pasivo_Cuenta_Contable_210
                                {
                                    IdEncabezado_Id = encabezado.Id,
                                    IdAcreedor = info.GetVal("IdAcreedor"),
                                    IdOperacion = info.GetVal("IdOperacion"),
                                    FechaFormalizacion = info.GetDate("FechaFormalizacion"),
                                    Tasa = info.GetDecimal("Tasa", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". La tasa: "),
                                    CuentaContablePrincipal = info.GetVal("CuentaContablePrincipal"),
                                    SaldoPrincipal = info.GetDecimal("SaldoPrincipal", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". El saldo principal: "),
                                    SaldoProducto = info.GetDecimal("SaldoProducto", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". El saldo producto: "),
                                    TipoMonedaObligacion = info.GetInt("TipoMonedaObligacion"),
                                    TipoTasa = info.GetVal("TipoTasa"),
                                    FechaVencimiento = info.GetDate("FechaVencimiento"),
                                    Condicion = "NUEVO",
                                    Tipo = "NO APLICA",
                                    TipoDepositoFGDLEY9816 = info.GetNullableInt("TipoDepositoFGDLEY9816")
                                };

                                if (registro.SaldoPrincipal > 0 || registro.SaldoProducto > 0)
                                {
                                    if (registro.TipoMonedaObligacion != 1 && registro.TipoMonedaObligacion != 2 && registro.TipoMonedaObligacion != 3)
                                        XmlErrorLogger.AgregarError("El tipo de moneda obligación debe tener un valor de 1, 2 o 3. Valor inválido: " + registro.TipoMonedaObligacion + " del acreedor " + registro.IdAcreedor, encabezado, db);

                                    if (registro.TipoTasa != "F" && registro.TipoTasa != "V")
                                        XmlErrorLogger.AgregarError("El tipo de tasa debe ser F o V. Valor inválido: " + registro.TipoTasa + " del acreedor " + registro.IdAcreedor, encabezado, db);

                                    if (registro.Tasa < 0)
                                        XmlErrorLogger.AgregarError("La tasa debe ser mayor a 0. Valor inválido: " + registro.Tasa.ToString() + " del acreedor " + registro.IdAcreedor, encabezado, db);

                                    if (registro.Tasa < 1)
                                        registro.Tasa *= 100;

                                    if (registro.CuentaContablePrincipal.Length != 8)
                                        XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe tener longitud de 8. Id operación: " + registro.IdOperacion, encabezado, db);
                                    else
                                    {
                                        if (!registro.CuentaContablePrincipal.StartsWith("21"))
                                            XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe empezar por 21", encabezado, db);

                                        if (registro.CuentaContablePrincipal.Length >= 6)
                                        {
                                            string caracter = registro.CuentaContablePrincipal.Substring(5, 1);
                                            if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                                XmlErrorLogger.AgregarError("El sexto dígito de la cuenta contable principal " + registro.CuentaContablePrincipal + " debe ser 0, 1, 2 o 3", encabezado, db);
                                        }
                                    }

                                    totalPasivos += registro.SaldoPrincipal + registro.SaldoProducto;

                                    DataRow row = dt.NewRow();
                                    row["IdOperacion"] = (object)registro.IdOperacion ?? DBNull.Value;
                                    row["IdEncabezado_Id"] = encabezado.Id;
                                    row["IdAcreedor"] = (object)registro.IdAcreedor ?? DBNull.Value;
                                    row["TipoMonedaObligacion"] = registro.TipoMonedaObligacion;
                                    row["TipoTasa"] = (object)registro.TipoTasa ?? DBNull.Value;
                                    row["Tasa"] = registro.Tasa;
                                    row["CuentaContablePrincipal"] = (object)registro.CuentaContablePrincipal ?? DBNull.Value;
                                    row["SaldoPrincipal"] = registro.SaldoPrincipal;
                                    row["SaldoProducto"] = registro.SaldoProducto;
                                    row["FechaFormalizacion"] = SqlBulkCopyHelper.ValidarFechaSql(registro.FechaFormalizacion);
                                    row["FechaVencimiento"] = SqlBulkCopyHelper.ValidarFechaSql(registro.FechaVencimiento);
                                    row["Condicion"] = (object)registro.Condicion ?? DBNull.Value;
                                    row["Tipo"] = (object)registro.Tipo ?? DBNull.Value;
                                    row["TipoDepositoFGDLEY9816"] = registro.TipoDepositoFGDLEY9816.HasValue ? (object)registro.TipoDepositoFGDLEY9816.Value : DBNull.Value;
                                    dt.Rows.Add(row);

                                    if (dt.Rows.Count >= 5000)
                                    {
                                        SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Pasivo_Cuenta_Contable_210", dt);
                                        dt.Clear();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            XmlErrorLogger.AgregarError(e.Message, encabezado, db);
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Pasivo_Cuenta_Contable_210", dt);
                        dt.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError("Detalle: " + baseEx.Message, encabezado, db);
                ServiceLogger.LogError(string.Format("Pasivos210Processor (Encabezado #{0})", encabezado.Id), e);
            }
            finally
            {
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }

            try
            {
                var duplicados = db.Database.SqlQuery<string>(
                    @"SELECT TOP 50 IdOperacion 
                      FROM [dbo].[XML_Pasivo_Cuenta_Contable_210] WITH(NOLOCK) 
                      WHERE IdEncabezado_Id = @p0 
                      GROUP BY IdOperacion 
                      HAVING COUNT(*) > 1", encabezado.Id).ToList();

                foreach (var item in duplicados)
                {
                    XmlErrorLogger.AgregarError("El id de operación: " + item + " aparece duplicado.", encabezado, db);
                }

                // Valida que el monto del archivo corresponda al monto de las cuentas contables
                if (contableEstadosId.HasValue)
                {
                    var monto210 = db.Database.SqlQuery<decimal?>(
                        @"SELECT TOP 1 SaldoFinal 
                          FROM [dbo].[XML_Contable_Estado] WITH(NOLOCK) 
                          WHERE IdEncabezado_Id = @p0 AND Cuenta = @p1", 
                        contableEstadosId.Value, Utilitarios.cuentaPasivos210).FirstOrDefault();

                    if (monto210.HasValue && Math.Abs((totalPasivos - monto210.Value)) > 1000)
                    {
                        XmlErrorLogger.AgregarError("El saldo de la cuenta " + Utilitarios.cuentaPasivos210 + ": " + monto210.Value.ToString("N2") + " del XML_Contable_Estados.xml no puede ser diferente en más de 1000 con " +
                            "la suma total de todos los pasivos del XML_Pasivos_Cuenta_210.xml: " + totalPasivos.ToString("N2"), encabezado, db);
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(string.Format("Pasivos210Processor validaciones posteriores (Encabezado #{0})", encabezado.Id), ex);
            }
        }

        private static DataTable CrearDataTablePasivos210()
        {
            DataTable dt = new DataTable("XML_Pasivo_Cuenta_Contable_210");
            dt.Columns.Add("IdOperacion", typeof(string));
            dt.Columns.Add("IdEncabezado_Id", typeof(long));
            dt.Columns.Add("IdAcreedor", typeof(string));
            dt.Columns.Add("TipoMonedaObligacion", typeof(int));
            dt.Columns.Add("TipoTasa", typeof(string));
            dt.Columns.Add("Tasa", typeof(decimal));
            dt.Columns.Add("CuentaContablePrincipal", typeof(string));
            dt.Columns.Add("SaldoPrincipal", typeof(decimal));
            dt.Columns.Add("SaldoProducto", typeof(decimal));
            dt.Columns.Add("FechaFormalizacion", typeof(DateTime));
            dt.Columns.Add("FechaVencimiento", typeof(DateTime));
            dt.Columns.Add("Condicion", typeof(string));
            dt.Columns.Add("Tipo", typeof(string));
            dt.Columns.Add("TipoDepositoFGDLEY9816", typeof(int)).AllowDBNull = true;
            return dt;
        }
    }
}
