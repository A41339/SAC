using FGA.Models;
using System;
using System.Configuration;

namespace FileService.Utils
{
    public static class XmlErrorLogger
    {
        public static void AgregarError(string detalle, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                if (encabezado != null && (encabezado.IdEntidad == null || encabezado.IdEntidad.Ind_Validar))
                {
                    if (db != null)
                    {
                        db.Database.ExecuteSqlCommand(
                            "INSERT INTO [dbo].[XML_Errores] ([IdEncabezado_Id], [Detalle]) VALUES (@p0, @p1)",
                            encabezado.Id, detalle ?? string.Empty);
                    }
                    else
                    {
                        using (var conn = Bulk.SqlBulkCopyHelper.CrearConexionSql())
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO [dbo].[XML_Errores] ([IdEncabezado_Id], [Detalle]) VALUES (@p0, @p1)";
                            cmd.Parameters.AddWithValue("@p0", encabezado.Id);
                            cmd.Parameters.AddWithValue("@p1", detalle ?? string.Empty);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    ServiceLogger.Log(string.Format("Error registrado en XML_Errores para Encabezado #{0}: {1}", encabezado.Id, detalle));
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(string.Format("XmlErrorLogger falló al guardar error para Encabezado #{0}: {1}", encabezado != null ? encabezado.Id : 0, detalle), ex);
            }
        }
    }
}
