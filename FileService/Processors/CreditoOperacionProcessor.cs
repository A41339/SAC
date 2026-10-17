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
                            reader.MoveToAttribute("Registro");
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (reader.Name.Equals("Registro"))
                                    {
                                        XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                        cantidad += 1;
                                        XML_Credito_Operacion_DirInd registro;

                                        try
                                        {
                                            registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Operacion_DirInd
                                            {
                                                IdEncabezado = encabezado,
                                                IdDeudor = info.Element("IdDeudor").Value,
                                                IdOperacion = info.Element("IdOperacion").Value,
                                                TipoCartera = Utilitarios.ConvertirAEntero(info.Element("TipoCartera").Value),
                                                EstadoOperacionCrediticia = Utilitarios.ConvertirAEntero(info.Element("EstadoOperacionCrediticia").Value),
                                                CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value,
                                                SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo principal: "),
                                                SaldoProductos = Utilitarios.ConvertirADecimal(info.Element("SaldoProductos").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo productos: "),
                                                SaldoComisiones = Utilitarios.ConvertirADecimal(info.Element("SaldoComisiones").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo comisiones: "),
                                                FechaVencimiento = Utilitarios.ConvertirAFecha(info.Element("FechaVencimiento").Value),
                                                TasaInteresNominalVigente = Utilitarios.ConvertirADecimal(info.Element("TasaInteresNominalVigente").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". La tasa de interés: "),
                                                MontoCuotaPrincipalActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaPrincipalActual").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El monto cuota principal: "),
                                                CuentaContableProducto = info.Element("CuentaContableProducto") != null ? info.Element("CuentaContableProducto").Value : "",
                                                IndNueva = info.Element("IndNueva") != null && Utilitarios.ConvertirAEntero(info.Element("IndNueva").Value) == 1,
                                                CodigoCategoriaRiesgo = info.Element("CodigoCategoriaRiesgo") != null ? info.Element("CodigoCategoriaRiesgo").Value : "",
                                                IndicadorOperacionModificada = info.Element("IndicadorOperacionModificada") != null ? info.Element("IndicadorOperacionModificada").Value : "",
                                                TipoSegmento = info.Element("TipoSegmento") != null ? info.Element("TipoSegmento").Value : "",
                                                CodigoEtapa = info.Element("CodigoEtapa") != null ? info.Element("CodigoEtapa").Value : "",
                                                TipoMonedaOperacion = info.Element("TipoMonedaOperacion") != null ? info.Element("TipoMonedaOperacion").Value : "",
                                                MontoDesembolsado = info.Element("MontoDesembolsado") != null ? Utilitarios.ConvertirADecimal(info.Element("MontoDesembolsado").Value) : 0,
                                                FechaFormalizacion = info.Element("FechaFormalizacion") != null ? Utilitarios.ConvertirAFecha(info.Element("FechaFormalizacion").Value) : new DateTime(1900, 1, 1),
                                                EAD = info.Element("EAD") != null ? Utilitarios.ConvertirADecimal(info.Element("EAD").Value) : 0,
                                                MontoCuotaInteresesActual = info.Element("MontoCuotaInteresesActual") != null ? Utilitarios.ConvertirADecimal(info.Element("MontoCuotaInteresesActual").Value) : 0,
                                                IndicadorOperacionNueva = info.Element("IndicadorOperacionNueva") != null ? info.Element("IndicadorOperacionNueva").Value : "",
                                                MontoEstimacionEspecifica = info.Element("MontoEstimacionEspecifica") != null ? Utilitarios.ConvertirADecimal(info.Element("MontoEstimacionEspecifica").Value) : 0,
                                                MontoFormalizadoOperacionCrediticia = info.Element("MontoFormalizadoOperacionCrediticia") != null ? Utilitarios.ConvertirADecimal(info.Element("MontoFormalizadoOperacionCrediticia").Value) : 0
                                            }).FirstOrDefault();
                                        }
                                        catch (Exception)
                                        {
                                            registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Operacion_DirInd
                                            {
                                                IdEncabezado = encabezado,
                                                IdDeudor = info.Element("IdDeudor").Value,
                                                IdOperacion = info.Element("IdOperacion").Value,
                                                TipoCartera = Utilitarios.ConvertirAEntero(info.Element("TipoCartera").Value),
                                                EstadoOperacionCrediticia = Utilitarios.ConvertirAEntero(info.Element("EstadoOperacionCrediticia").Value),
                                                CuentaContablePrincipal = info.Element("CuentaContablePrincipal").Value,
                                                SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipal").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo principal: "),
                                                SaldoProductos = Utilitarios.ConvertirADecimal(info.Element("SaldoProductos").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo productos: "),
                                                SaldoComisiones = Utilitarios.ConvertirADecimal(info.Element("SaldoComisiones").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El saldo comisiones: "),
                                                FechaVencimiento = Utilitarios.ConvertirAFecha(info.Element("FechaVencimiento").Value),
                                                TasaInteresNominalVigente = Utilitarios.ConvertirADecimal(info.Element("TasaInteresNominalVigente").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". La tasa de interés: "),
                                                MontoCuotaPrincipalActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaPrincipalActual").Value, "Id de operación:  " + info.Element("IdOperacion").Value + ". El monto cuota principal: "),
                                                CuentaContableProducto = "",
                                                IndNueva = false,
                                                CodigoCategoriaRiesgo = "",
                                                IndicadorOperacionModificada = "",
                                                TipoSegmento = "",
                                                CodigoEtapa = "",
                                                TipoMonedaOperacion = "",
                                                MontoDesembolsado = 0,
                                                FechaFormalizacion = new DateTime(1900, 1, 1),
                                                EAD = 0,
                                                MontoCuotaInteresesActual = 0,
                                                IndicadorOperacionNueva = "",
                                                MontoEstimacionEspecifica = 0,
                                                MontoFormalizadoOperacionCrediticia = 0
                                            }).FirstOrDefault();
                                        }

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
                                    break;
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
