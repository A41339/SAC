namespace Entities.Entities.Procedures
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public partial class FGAEntities
    {
        public virtual ObjectResult<FGA_Rpt_Balance_General_5Periodos_Result> FGA_Rpt_Balance_General_5Periodos(string iDENTIDAD, Nullable<System.DateTime> pERIODO_REF, string tIPO_COMPARACION)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("PERIODO_REF", (object)pERIODO_REF ?? DBNull.Value);
            var p3 = new SqlParameter("TIPO_COMPARACION", (object)tIPO_COMPARACION ?? "Trimestral");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Rpt_Balance_General_5Periodos_Result>(
                "EXEC [dbo].[FGA_Rpt_Balance_General_5Periodos] @IDENTIDAD, @PERIODO_REF, @TIPO_COMPARACION", p1, p2, p3);
        }
    }
}
