using System;
using System.Data;
using System.Data.SqlClient;

namespace FileService.Bulk
{
    public static class SqlBulkCopyHelper
    {
        public static void EjecutarBulkCopy(SqlConnection conn, string tableName, DataTable dt, int timeoutSeconds = 300)
        {
            if (dt == null || dt.Rows.Count == 0) return;

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
            {
                bulkCopy.DestinationTableName = "[dbo].[" + tableName + "]";
                bulkCopy.BulkCopyTimeout = timeoutSeconds;
                bulkCopy.BatchSize = dt.Rows.Count;

                foreach (DataColumn col in dt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }

                bulkCopy.WriteToServer(dt);
            }
        }

        public static DateTime ValidarFechaSql(DateTime dt)
        {
            if (dt < new DateTime(1753, 1, 1))
                return new DateTime(1900, 1, 1);
            if (dt > new DateTime(9999, 12, 31))
                return new DateTime(9999, 12, 31);
            return dt;
        }
    }
}
