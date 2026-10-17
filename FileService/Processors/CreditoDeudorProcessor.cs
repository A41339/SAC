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
    public class CreditoDeudorProcessor : IXmlProcessor
    {
        public void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            int cantidad = 0;
            DataTable dt = CrearDataTableCreditoDeudor();

            try
            {
                using (SqlConnection conn = SqlBulkCopyHelper.CrearConexionSql())
                {
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
                                            XmlErrorLogger.AgregarError("El id del deudor debe tener entre 1 y 30 caracteres. Valor inválido: " + registro.IdDeudor, encabezado, db);

                                        DataRow row = dt.NewRow();
                                        row["IdDeudor"] = (object)registro.IdDeudor ?? DBNull.Value;
                                        row["CategoriaRiesgo"] = (object)registro.CategoriaRiesgo ?? DBNull.Value;
                                        row["IdEncabezado_Id"] = encabezado.Id;
                                        dt.Rows.Add(row);

                                        if (dt.Rows.Count >= 5000)
                                        {
                                            SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Credito_Deudor", dt);
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
                        SqlBulkCopyHelper.EjecutarBulkCopy(conn, "XML_Credito_Deudor", dt);
                        dt.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                var baseEx = e.GetBaseException() ?? e;
                XmlErrorLogger.AgregarError(baseEx.Message, encabezado, db);
                ServiceLogger.LogError(string.Format("CreditoDeudorProcessor (Encabezado #{0})", encabezado.Id), e);
            }
            finally
            {
                encabezado.Cantidad = cantidad;
                db.Entry(encabezado).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        private static DataTable CrearDataTableCreditoDeudor()
        {
            DataTable dt = new DataTable("XML_Credito_Deudor");
            dt.Columns.Add("IdDeudor", typeof(string));
            dt.Columns.Add("CategoriaRiesgo", typeof(string));
            dt.Columns.Add("IdEncabezado_Id", typeof(long));
            return dt;
        }
    }
}
