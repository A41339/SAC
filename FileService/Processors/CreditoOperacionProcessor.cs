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
    public class CreditoOperacionProcessor : IXmlProcessor
    {
        public void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            int cantidad = 0;
            DataTable dt = CrearDataTableCreditoOperacion();

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

                                string idOperacion = info.GetVal(new[] { "IdOperacionCredito", "IdOperacion" });
                                string idDeudor = info.GetVal(new[] { "IdDeudor" });

                                var registro = new XML_Credito_Operacion_DirInd
                                {
                                    IdEncabezado = encabezado,
                                    IdDeudor = idDeudor,
                                    IdOperacion = idOperacion,
                                    TipoCartera = info.GetInt(new[] { "TipoCarteraCrediticia", "TipoCartera" }),
                                    EstadoOperacionCrediticia = info.GetInt(new[] { "TipoEstadoOperacionCrediticia", "EstadoOperacionCrediticia" }),
                                    CuentaContablePrincipal = info.GetVal(new[] { "CuentaContablePrincipal" }).Trim(),
                                    CuentaContableProducto = info.GetVal(new[] { "CuentaContableProductosPorCobrar", "CuentaContableProductos", "CuentaContableProducto" }).Trim(),
                                    SaldoPrincipal = info.GetDecimal(new[] { "SaldoPrincipalOperacionCrediticia", "SaldoPrincipal" }, 0, "Id de operación: " + idOperacion + ". El saldo principal: "),
                                    SaldoProductos = info.GetDecimal(new[] { "SaldoProductosPorCobrar", "SaldoProductos" }, 0, "Id de operación: " + idOperacion + ". El saldo productos: "),
                                    SaldoComisiones = info.GetDecimal(new[] { "SaldoComisionesOperacionesContingentes", "SaldoComisiones" }, 0, "Id de operación: " + idOperacion + ". El saldo comisión: "),
                                    FechaVencimiento = info.GetDate(new[] { "FechaVencimiento" }),
                                    TasaInteresNominalVigente = info.GetDecimal(new[] { "TasaInteresNominalVigente" }, 0, "Id de operación: " + idOperacion + ". La tasa nominal: "),
                                    MontoCuotaPrincipalActual = info.GetDecimal(new[] { "MontoCuotaPrincipalActual" }, 0, "Id de operación: " + idOperacion + ". El monto cuota principal: "),
                                    IndNueva = info.GetBool(new[] { "IndNueva" }),
                                    CodigoCategoriaRiesgo = Utilitarios.ObtenerCategoriaRiesgo(info.GetVal(new[] { "CodigoCategoriaRiesgo" }, "0")),
                                    IndicadorOperacionModificada = info.GetVal(new[] { "IndicadorOperacionModificada" }).Trim(),
                                    TipoSegmento = info.GetVal(new[] { "TipoSegmento" }).Trim(),
                                    CodigoEtapa = info.GetVal(new[] { "CodigoEtapa" }).Trim(),
                                    TipoMonedaOperacion = info.GetVal(new[] { "TipoMonedaOperacion" }).Trim(),
                                    MontoDesembolsado = info.GetDecimal(new[] { "MontoDesembolsado" }, 0, "Id de operación: " + idOperacion + ". Monto desembolsado: "),
                                    FechaFormalizacion = info.GetDate(new[] { "FechaFormalizacion" }),
                                    EAD = info.GetDecimal(new[] { "EAD" }, 0, "Id de operación: " + idOperacion + ". EAD: "),
                                    MontoCuotaInteresesActual = info.GetDecimal(new[] { "MontoCuotaInteresesActual" }, 0, "Id de operación: " + idOperacion + ". El interes actual: "),
                                    MontoEstimacionEspecifica = info.GetDecimal(new[] { "MontoEstimacionEspecifica" }, 0, "Id de operación: " + idOperacion + ". Monto estimación: "),
                                    MontoFormalizadoOperacionCrediticia = info.GetDecimal(new[] { "MontoFormalizadoOperacionCrediticia" }, 0, "Id de operación: " + idOperacion + ". Monto formalizado: "),
                                    IndicadorOperacionNueva = info.GetVal(new[] { "IndicadorOperacionNueva" }, "N").Trim()
                                };

                                if (registro.TasaInteresNominalVigente < 1 && registro.TasaInteresNominalVigente > 0)
                                    registro.TasaInteresNominalVigente *= 100;

                                if (registro.TasaInteresNominalVigente > 100 || registro.TasaInteresNominalVigente < 0)
                                    XmlErrorLogger.AgregarError("La tasa de interes nominal debe estar comprendida entre 0 y 100. Valor inválido: " + registro.TasaInteresNominalVigente + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.IdDeudor.Length > 30 || registro.IdDeudor.Length == 0)
                                    XmlErrorLogger.AgregarError("El id del deudor debe tener entre 1 y 30 caracteres. Valor inválido: " + registro.IdDeudor + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.IdOperacion.Length > 35 || registro.IdOperacion.Length == 0)
                                    XmlErrorLogger.AgregarError("El id de la operación debe tener entre 1 y 35 caracteres. Valor inválido: " + registro.IdOperacion + " del deudor " + registro.IdDeudor, encabezado, db);

                                if (registro.TipoCartera < 0 || registro.TipoCartera > 11)
                                    XmlErrorLogger.AgregarError("El tipo de cartera debe tener entre 0 y 11 números. Valor inválido: " + registro.TipoCartera + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.EstadoOperacionCrediticia < 0 || registro.EstadoOperacionCrediticia > 11)
                                    XmlErrorLogger.AgregarError("El estado de la operación crediticia debe tener entre 0 y 11 números. Valor inválido: " + registro.EstadoOperacionCrediticia + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.CuentaContablePrincipal.Length != 8 && registro.MontoCuotaPrincipalActual > 0)
                                    XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe tener longitud de 8. Ya que tiene saldo. Id Operación: " + registro.IdOperacion, encabezado, db);
                                else
                                {
                                    if (registro.CuentaContablePrincipal.Length > 0)
                                    {
                                        if (registro.CuentaContablePrincipal.StartsWith("0") || registro.CuentaContablePrincipal.StartsWith("9"))
                                            XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " no debe empezar por 8 o 9", encabezado, db);

                                        if (registro.CuentaContablePrincipal.Length >= 6)
                                        {
                                            string caracter = registro.CuentaContablePrincipal.Substring(5, 1);
                                            if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                                XmlErrorLogger.AgregarError("El sexto dígito de la cuenta contable principal " + registro.CuentaContablePrincipal + " debe ser 0, 1, 2 o 3", encabezado, db);
                                        }
                                    }
                                }

                                DataRow row = dt.NewRow();
                                row["IdDeudor"] = (object)registro.IdDeudor ?? DBNull.Value;
                                row["IdOperacion"] = (object)registro.IdOperacion ?? DBNull.Value;
                                row["TipoCartera"] = registro.TipoCartera;
                                row["EstadoOperacionCrediticia"] = registro.EstadoOperacionCrediticia;
                                row["CuentaContablePrincipal"] = (object)registro.CuentaContablePrincipal ?? DBNull.Value;
                                row["SaldoPrincipal"] = registro.SaldoPrincipal;
                                row["SaldoProductos"] = registro.SaldoProductos;
                                row["SaldoComisiones"] = registro.SaldoComisiones;
                                row["FechaVencimiento"] = SqlBulkCopyHelper.ValidarFechaSql(registro.FechaVencimiento);
                                row["TasaInteresNominalVigente"] = registro.TasaInteresNominalVigente;
                                row["MontoCuotaPrincipalActual"] = registro.MontoCuotaPrincipalActual;
                                row["IdEncabezado_Id"] = encabezado.Id;
                                row["CuentaContableProducto"] = (object)registro.CuentaContableProducto ?? DBNull.Value;
                                row["IndNueva"] = registro.IndNueva;
                                row["CodigoCategoriaRiesgo"] = (object)registro.CodigoCategoriaRiesgo ?? DBNull.Value;
                                row["IndicadorOperacionModificada"] = (object)registro.IndicadorOperacionModificada ?? DBNull.Value;
                                row["TipoSegmento"] = (object)registro.TipoSegmento ?? DBNull.Value;
                                row["CodigoEtapa"] = (object)registro.CodigoEtapa ?? DBNull.Value;
                                row["TipoMonedaOperacion"] = (object)registro.TipoMonedaOperacion ?? DBNull.Value;
                                row["MontoDesembolsado"] = registro.MontoDesembolsado;
                                row["FechaFormalizacion"] = SqlBulkCopyHelper.ValidarFechaSql(registro.FechaFormalizacion);
                                row["EAD"] = registro.EAD;
                                row["MontoCuotaInteresesActual"] = registro.MontoCuotaInteresesActual;
                                row["IndicadorOperacionNueva"] = (object)registro.IndicadorOperacionNueva ?? DBNull.Value;
                                row["MontoEstimacionEspecifica"] = registro.MontoEstimacionEspecifica;
                                row["MontoFormalizadoOperacionCrediticia"] = registro.MontoFormalizadoOperacionCrediticia;
                                dt.Rows.Add(row);

                                if (dt.Rows.Count >= 5000)
                                {
                                    SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Credito_Operacion_DirInd", dt);
                                    dt.Clear();
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
                        SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Credito_Operacion_DirInd", dt);
                        dt.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
                ServiceLogger.LogError(string.Format("CreditoOperacionProcessor (Encabezado #{0})", encabezado.Id), e);
            }
            finally
            {
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }

            try
            {
                Entities.Entities.Procedures.FGAEntities sp = new Entities.Entities.Procedures.FGAEntities();
                sp.FGA_Valida_Operaciones_Dir_Contable(encabezado.Periodo, encabezado.IdEntidad_Id, encabezado.Id);
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(string.Format("CreditoOperacionProcessor validación contable SP (Encabezado #{0})", encabezado.Id), ex);
            }
        }

        private static DataTable CrearDataTableCreditoOperacion()
        {
            DataTable dt = new DataTable("XML_Credito_Operacion_DirInd");
            dt.Columns.Add("IdDeudor", typeof(string));
            dt.Columns.Add("IdOperacion", typeof(string));
            dt.Columns.Add("TipoCartera", typeof(int));
            dt.Columns.Add("EstadoOperacionCrediticia", typeof(int));
            dt.Columns.Add("CuentaContablePrincipal", typeof(string));
            dt.Columns.Add("SaldoPrincipal", typeof(decimal));
            dt.Columns.Add("SaldoProductos", typeof(decimal));
            dt.Columns.Add("SaldoComisiones", typeof(decimal));
            dt.Columns.Add("FechaVencimiento", typeof(DateTime));
            dt.Columns.Add("TasaInteresNominalVigente", typeof(decimal));
            dt.Columns.Add("MontoCuotaPrincipalActual", typeof(decimal));
            dt.Columns.Add("IdEncabezado_Id", typeof(long));
            dt.Columns.Add("CuentaContableProducto", typeof(string));
            dt.Columns.Add("IndNueva", typeof(bool));
            dt.Columns.Add("CodigoCategoriaRiesgo", typeof(string));
            dt.Columns.Add("IndicadorOperacionModificada", typeof(string));
            dt.Columns.Add("TipoSegmento", typeof(string));
            dt.Columns.Add("CodigoEtapa", typeof(string));
            dt.Columns.Add("TipoMonedaOperacion", typeof(string));
            dt.Columns.Add("MontoDesembolsado", typeof(decimal));
            dt.Columns.Add("FechaFormalizacion", typeof(DateTime));
            dt.Columns.Add("EAD", typeof(decimal));
            dt.Columns.Add("MontoCuotaInteresesActual", typeof(decimal));
            dt.Columns.Add("IndicadorOperacionNueva", typeof(string));
            dt.Columns.Add("MontoEstimacionEspecifica", typeof(decimal));
            dt.Columns.Add("MontoFormalizadoOperacionCrediticia", typeof(decimal));
            return dt;
        }
    }
}
