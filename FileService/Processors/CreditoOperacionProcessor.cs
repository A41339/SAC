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
                                var registro = new XML_Credito_Operacion_DirInd
                                {
                                    IdEncabezado = encabezado,
                                    IdDeudor = info.GetVal("IdDeudor"),
                                    IdOperacion = info.GetVal("IdOperacion"),
                                    TipoCartera = info.GetInt("TipoCartera"),
                                    EstadoOperacionCrediticia = info.GetInt("EstadoOperacionCrediticia"),
                                    CuentaContablePrincipal = info.GetVal("CuentaContablePrincipal"),
                                    SaldoPrincipal = info.GetDecimal("SaldoPrincipal", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". El saldo principal: "),
                                    SaldoProductos = info.GetDecimal("SaldoProductos", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". El saldo productos: "),
                                    SaldoComisiones = info.GetDecimal("SaldoComisiones", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". El saldo comisiones: "),
                                    FechaVencimiento = info.GetDate("FechaVencimiento"),
                                    TasaInteresNominalVigente = info.GetDecimal("TasaInteresNominalVigente", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". La tasa de interés: "),
                                    MontoCuotaPrincipalActual = info.GetDecimal("MontoCuotaPrincipalActual", 0, "Id de operación:  " + info.GetVal("IdOperacion") + ". El monto cuota principal: "),
                                    CuentaContableProducto = info.GetVal("CuentaContableProducto"),
                                    IndNueva = info.GetBool("IndNueva"),
                                    CodigoCategoriaRiesgo = info.GetVal("CodigoCategoriaRiesgo"),
                                    IndicadorOperacionModificada = info.GetVal("IndicadorOperacionModificada"),
                                    TipoSegmento = info.GetVal("TipoSegmento"),
                                    CodigoEtapa = info.GetVal("CodigoEtapa"),
                                    TipoMonedaOperacion = info.GetVal("TipoMonedaOperacion"),
                                    MontoDesembolsado = info.GetDecimal("MontoDesembolsado"),
                                    FechaFormalizacion = info.GetDate("FechaFormalizacion"),
                                    EAD = info.GetDecimal("EAD"),
                                    MontoCuotaInteresesActual = info.GetDecimal("MontoCuotaInteresesActual"),
                                    IndicadorOperacionNueva = info.GetVal("IndicadorOperacionNueva"),
                                    MontoEstimacionEspecifica = info.GetDecimal("MontoEstimacionEspecifica"),
                                    MontoFormalizadoOperacionCrediticia = info.GetDecimal("MontoFormalizadoOperacionCrediticia")
                                };

                                if (registro.IdDeudor.Length > 30 || registro.IdDeudor.Length == 0)
                                    XmlErrorLogger.AgregarError("El id del deudor debe tener entre 1 y 30 caracteres. Valor inválido: " + registro.IdDeudor + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.IdOperacion.Length > 35 || registro.IdOperacion.Length == 0)
                                    XmlErrorLogger.AgregarError("El id de la operación debe tener entre 1 y 35 caracteres. Valor inválido: " + registro.IdOperacion + " del deudor " + registro.IdDeudor, encabezado, db);

                                if (registro.TipoCartera < 1 || registro.TipoCartera > 3)
                                    XmlErrorLogger.AgregarError("El tipo de cartera debe ser 1, 2 o 3. Valor inválido: " + registro.TipoCartera.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.EstadoOperacionCrediticia < 1 || registro.EstadoOperacionCrediticia > 6)
                                    XmlErrorLogger.AgregarError("El estado de la operación crediticia debe estar entre 1 y 6. Valor inválido: " + registro.EstadoOperacionCrediticia.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.CuentaContablePrincipal.Length != 8)
                                    XmlErrorLogger.AgregarError("La cuenta contable principal debe ser de 8 dígitos. Valor inválido: " + registro.CuentaContablePrincipal + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.SaldoPrincipal < 0)
                                    XmlErrorLogger.AgregarError("El saldo principal no puede ser negativo. Valor inválido: " + registro.SaldoPrincipal.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.SaldoProductos < 0)
                                    XmlErrorLogger.AgregarError("El saldo de productos no puede ser negativo. Valor inválido: " + registro.SaldoProductos.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.SaldoComisiones < 0)
                                    XmlErrorLogger.AgregarError("El saldo de comisiones no puede ser negativo. Valor inválido: " + registro.SaldoComisiones.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.TasaInteresNominalVigente < 0)
                                    XmlErrorLogger.AgregarError("La tasa de interés nominal no puede ser menor a cero. Valor inválido: " + registro.TasaInteresNominalVigente.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                if (registro.MontoCuotaPrincipalActual < 0)
                                    XmlErrorLogger.AgregarError("El monto de la cuota principal no puede ser menor a cero. Valor inválido: " + registro.MontoCuotaPrincipalActual.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

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
