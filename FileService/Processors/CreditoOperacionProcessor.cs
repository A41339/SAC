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
            try
            {
                int cantidad = 0;
                DataTable dt = CrearDataTableCreditoOperacion();

                using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    conn.Open();

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
                                                IdDeudor = info.Element("IdDeudor")?.Value ?? "",
                                                IdOperacion = info.Element("IdOperacionCredito")?.Value ?? "",
                                                TipoCartera = Utilitarios.ConvertirAEntero(info.Element("TipoCarteraCrediticia")?.Value ?? "0"),
                                                EstadoOperacionCrediticia = Utilitarios.ConvertirAEntero(info.Element("TipoEstadoOperacionCrediticia")?.Value ?? "0"),
                                                CodigoCategoriaRiesgo = Utilitarios.ObtenerCategoriaRiesgo(info.Element("CodigoCategoriaRiesgo")?.Value ?? "0"),
                                                CuentaContablePrincipal = info.Element("CuentaContablePrincipal")?.Value?.Trim() ?? "",
                                                CuentaContableProducto = info.Element("CuentaContableProductosPorCobrar")?.Value?.Trim() ?? "",
                                                SaldoPrincipal = Utilitarios.ConvertirADecimal(info.Element("SaldoPrincipalOperacionCrediticia")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El saldo principal: "),
                                                SaldoProductos = Utilitarios.ConvertirADecimal(info.Element("SaldoProductosPorCobrar")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El saldo productos: "),
                                                SaldoComisiones = Utilitarios.ConvertirADecimal(info.Element("SaldoComisionesOperacionesContingentes")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El saldo comisión: "),
                                                FechaVencimiento = Utilitarios.ConvertirAFecha(info.Element("FechaVencimiento")?.Value ?? "01/01/1900"),
                                                TasaInteresNominalVigente = Utilitarios.ConvertirADecimal(info.Element("TasaInteresNominalVigente")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". La tasa nominal: "),
                                                MontoCuotaPrincipalActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaPrincipalActual")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El monto de cuota: "),
                                                IndNueva = false,
                                                IndicadorOperacionModificada = info.Element("IndicadorOperacionModificada")?.Value?.Trim() ?? "",
                                                TipoSegmento = info.Element("TipoSegmento")?.Value?.Trim() ?? "",
                                                CodigoEtapa = info.Element("CodigoEtapa")?.Value?.Trim() ?? "",
                                                TipoMonedaOperacion = info.Element("TipoMonedaOperacion")?.Value?.Trim() ?? "",
                                                MontoDesembolsado = Utilitarios.ConvertirADecimal(info.Element("MontoDesembolsado")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". Monto desembolsado: "),
                                                FechaFormalizacion = Utilitarios.ConvertirAFecha(info.Element("FechaFormalizacion")?.Value ?? "01/01/1900"),
                                                EAD = Utilitarios.ConvertirADecimal(info.Element("EAD")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". EAD: "),
                                                MontoCuotaInteresesActual = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaInteresesActual")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". El interes actual: "),
                                                MontoEstimacionEspecifica = Utilitarios.ConvertirADecimal(info.Element("MontoEstimacionEspecifica")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". Monto estimación: "),
                                                MontoFormalizadoOperacionCrediticia = Utilitarios.ConvertirADecimal(info.Element("MontoFormalizadoOperacionCrediticia")?.Value ?? "0",
                                                "Id de operación: " + info.Element("IdOperacionCredito")?.Value + ". Monto formalizado: "),
                                                IndicadorOperacionNueva = info.Element("IndicadorOperacionNueva")?.Value?.Trim() ?? "N",
                                            }).FirstOrDefault();                                        
                                        }

                                        if (registro.TasaInteresNominalVigente < 1)
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
                                            XmlErrorLogger.AgregarError("El tipo de cartera debe tener entre 0 y 11 números. Valor inválido: " + registro.TipoCartera + " de la operación " + registro.IdOperacion, encabezado, db);

                                        if (registro.CuentaContablePrincipal.Length != 8 && registro.MontoCuotaPrincipalActual > 0)
                                            XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " debe tener longitud de 8. Ya que tiene saldo. Id Operación: " + registro.IdOperacion, encabezado, db);
                                        else
                                        {
                                            if (registro.CuentaContablePrincipal.Length > 0)
                                            {
                                                if (registro.CuentaContablePrincipal.StartsWith("0") || registro.CuentaContablePrincipal.StartsWith("9"))
                                                    XmlErrorLogger.AgregarError("La cuenta contable principal " + registro.CuentaContablePrincipal + " no debe empezar por 8 o 9", encabezado, db);

                                                string caracter = registro.CuentaContablePrincipal.Substring(5, 1);
                                                if (caracter != "0" && caracter != "1" && caracter != "2" && caracter != "3")
                                                    XmlErrorLogger.AgregarError("El sexto dígito de la cuenta contable principal " + registro.CuentaContablePrincipal + " debe ser 0, 1, 2 o 3", encabezado, db);
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

                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();

                Entities.Entities.Procedures.FGAEntities sp = new Entities.Entities.Procedures.FGAEntities();
                sp.FGA_Valida_Operaciones_Dir_Contable(encabezado.Periodo, encabezado.IdEntidad_Id, encabezado.Id);
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
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
