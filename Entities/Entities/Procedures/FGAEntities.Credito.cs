namespace Entities.Entities.Procedures
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public partial class FGAEntities
    {
        public virtual ObjectResult<FGA_Consultar_Alertas_Tempranas_Credito_Result> FGA_Consultar_Alertas_Tempranas_Credito(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE, string cARTERA, string pRODUCTO, string sEGMENTO)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);
            var p3 = new SqlParameter("@CARTERA", (object)cARTERA ?? "Total");
            var p4 = new SqlParameter("@PRODUCTO", (object)pRODUCTO ?? "Todos");
            var p5 = new SqlParameter("@SEGMENTO", (object)sEGMENTO ?? "Todos");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Alertas_Tempranas_Credito_Result>(
                "EXEC [dbo].[FGA_Consultar_Alertas_Tempranas_Credito] @IDENTIDAD, @PERIODO_CORTE, @CARTERA, @PRODUCTO, @SEGMENTO", p1, p2, p3, p4, p5);
        }

        public virtual ObjectResult<FGA_Consultar_Serie_Alerta_Temprana_Result> FGA_Consultar_Serie_Alerta_Temprana(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE, string cODIGO, string cARTERA)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);
            var p3 = new SqlParameter("@CODIGO", (object)cODIGO ?? "IND-01");
            var p4 = new SqlParameter("@CARTERA", (object)cARTERA ?? "Total");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Serie_Alerta_Temprana_Result>(
                "EXEC [dbo].[FGA_Consultar_Serie_Alerta_Temprana] @IDENTIDAD, @PERIODO_CORTE, @CODIGO, @CARTERA", p1, p2, p3, p4);
        }

        public virtual int FGA_Generar_Alertas_Tempranas_Credito(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE, string cARTERA, string pRODUCTO, string sEGMENTO)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);
            var p3 = new SqlParameter("@CARTERA", (object)cARTERA ?? "Total");
            var p4 = new SqlParameter("@PRODUCTO", (object)pRODUCTO ?? "Todos");
            var p5 = new SqlParameter("@SEGMENTO", (object)sEGMENTO ?? "Todos");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreCommand(
                "EXEC [dbo].[FGA_Generar_Alertas_Tempranas_Credito] @IDENTIDAD, @PERIODO_CORTE, @CARTERA, @PRODUCTO, @SEGMENTO", p1, p2, p3, p4, p5);
        }
    }
}
