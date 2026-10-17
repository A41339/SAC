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
    public class CreditoCuotaAtrasadaProcessor : IXmlProcessor
    {
        public void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            int cantidad = 0;
            DataTable dt = CrearDataTableCuotaAtrasada();

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
                                        cantidad += 1;
                                        XDocument xDoc = XDocument.Load(reader.ReadSubtree());
                                        XML_Credito_Cuota_Atrasada registro = xDoc.Elements("Registro").Select(info => new XML_Credito_Cuota_Atrasada
                                        {
                                            IdEncabezado = encabezado,
                                            DiasAtraso = Utilitarios.ConvertirAEntero(info.Element("DiasAtraso").Value),
                                            IdDeudor = info.Element("IdDeudor").Value,
                                            IdOperacion = info.Element("IdOperacionCredito").Value,
                                            MontoCuotaAtrasada = Utilitarios.ConvertirADecimal(info.Element("MontoCuotaAtrasada").Value, "Id de operación:  " + info.Element("IdOperacionCredito").Value + ". El monto de cuota: ")
                                        }).FirstOrDefault();

                                        if (registro.IdDeudor.Length > 30 || registro.IdDeudor.Length == 0)
                                            XmlErrorLogger.AgregarError("El id del deudor debe tener entre 1 y 30 caracteres. Valor inválido: " + registro.IdDeudor + " de la operación " + registro.IdOperacion, encabezado, db);

                                        if (registro.IdOperacion.Length > 35 || registro.IdOperacion.Length == 0)
                                            XmlErrorLogger.AgregarError("El id de la operación debe tener entre 1 y 35 caracteres. Valor inválido: " + registro.IdOperacion + " del deudor " + registro.IdDeudor, encabezado, db);

                                        if (registro.DiasAtraso < 0 || registro.DiasAtraso > 999999)
                                            XmlErrorLogger.AgregarError("Los días de atraso deben tener entre 1 y 6 números. Valor inválido: " + registro.DiasAtraso.ToString() + " de la operación " + registro.IdOperacion, encabezado, db);

                                        DataRow row = dt.NewRow();
                                        row["IdDeudor"] = (object)registro.IdDeudor ?? DBNull.Value;
                                        row["IdOperacion"] = (object)registro.IdOperacion ?? DBNull.Value;
                                        row["DiasAtraso"] = registro.DiasAtraso;
                                        row["MontoCuotaAtrasada"] = registro.MontoCuotaAtrasada;
                                        row["IdEncabezado_Id"] = encabezado.Id;
                                        dt.Rows.Add(row);

                                        if (dt.Rows.Count >= 5000)
                                        {
                                            SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Credito_Cuota_Atrasada", dt);
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
                        SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Credito_Cuota_Atrasada", dt);
                        dt.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
                ServiceLogger.LogError(string.Format("CreditoCuotaAtrasadaProcessor (Encabezado #{0})", encabezado.Id), e);
            }
            finally
            {
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        private static DataTable CrearDataTableCuotaAtrasada()
        {
            DataTable dt = new DataTable("XML_Credito_Cuota_Atrasada");
            dt.Columns.Add("IdDeudor", typeof(string));
            dt.Columns.Add("IdOperacion", typeof(string));
            dt.Columns.Add("DiasAtraso", typeof(int));
            dt.Columns.Add("MontoCuotaAtrasada", typeof(decimal));
            dt.Columns.Add("IdEncabezado_Id", typeof(long));
            return dt;
        }
    }
}
