namespace Entities.Entities.Procedures
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public partial class FGAEntities
    {
        public virtual ObjectResult<FGA_Consultar_Indicadores_Liquidez_Result> FGA_Consultar_Indicadores_Liquidez(string iDENTIDAD, Nullable<System.DateTime> pERIODO1, Nullable<System.DateTime> pERIODO2)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO1", (object)pERIODO1 ?? DBNull.Value);
            var p3 = new SqlParameter("@PERIODO2", (object)pERIODO2 ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Indicadores_Liquidez_Result>(
                "EXEC [dbo].[FGA_Consultar_Indicadores_Liquidez] @IDENTIDAD, @PERIODO1, @PERIODO2", p1, p2, p3);
        }

        public virtual ObjectResult<FGA_Consultar_Serie_VaR_Liquidez_Result> FGA_Consultar_Serie_VaR_Liquidez(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE, Nullable<int> cANTIDADPERIODOS)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);
            var p3 = new SqlParameter("@CANTIDAD", (object)cANTIDADPERIODOS ?? 100);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Serie_VaR_Liquidez_Result>(
                "EXEC [dbo].[FGA_Consultar_Serie_VaR_Liquidez] @IDENTIDAD, @PERIODO_CORTE, @CANTIDAD", p1, p2, p3);
        }

        public virtual ObjectResult<FGA_Consultar_GAP_Contractual_Result> FGA_Consultar_GAP_Contractual(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_GAP_Contractual_Result>(
                "EXEC [dbo].[FGA_Consultar_GAP_Contractual] @IDENTIDAD, @PERIODO_CORTE", p1, p2);
        }

        public virtual int FGA_Generar_GAP_Contractual(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreCommand(
                "EXEC [dbo].[FGA_Generar_GAP_Contractual] @IDENTIDAD, @PERIODO_CORTE", p1, p2);
        }

        public virtual ObjectResult<FGA_Consultar_Parametros_Liquidez_Result> FGA_Consultar_Parametros_Liquidez(string iDENTIDAD)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Parametros_Liquidez_Result>(
                "EXEC [dbo].[FGA_Consultar_Parametros_Liquidez] @IDENTIDAD", p1);
        }

        public virtual ObjectResult<FGA_Consultar_Calculo_VaR_Liquidez_Result> FGA_Consultar_Calculo_VaR_Liquidez(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Calculo_VaR_Liquidez_Result>(
                "EXEC [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] @IDENTIDAD, @PERIODO_CORTE", p1, p2);
        }

        public virtual ObjectResult<FGA_Consultar_Proyecciones_Liquidez_Result> FGA_Consultar_Proyecciones_Liquidez(string iDENTIDAD, Nullable<System.DateTime> pERIODOCORTE)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@PERIODO_CORTE", (object)pERIODOCORTE ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Proyecciones_Liquidez_Result>(
                "EXEC [dbo].[FGA_Consultar_Proyecciones_Liquidez] @IDENTIDAD, @PERIODO_CORTE", p1, p2);
        }

        public virtual int FGA_Guardar_Parametros_Liquidez(string iDENTIDAD, Nullable<decimal> rENOVACION_CDP, Nullable<decimal> dISMINUCION_RECAUDO, Nullable<decimal> dISPONIBILIDAD_LINEAS, Nullable<decimal> pAGO_DEUDA, Nullable<decimal> cUPONES, string uSUARIO, Nullable<decimal> eGRESOS_FIN = null, Nullable<decimal> gASTOS_ADM = null, Nullable<decimal> sALIDA_TOP20 = null, Nullable<decimal> cAPITAL_SOCIAL = null, Nullable<decimal> oTROS_INGRESOS = null, string pROYECTOR_VAR = null, string pROYECTOR_CARTERA = null, string pROYECTOR_CAPITAL = null, string pROYECTOR_OTROS_ING = null)
        {
            var p1 = new SqlParameter("@IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("@RENOVACION", (object)rENOVACION_CDP ?? DBNull.Value);
            var p3 = new SqlParameter("@DISMINUCION", (object)dISMINUCION_RECAUDO ?? DBNull.Value);
            var p4 = new SqlParameter("@LINEAS", (object)dISPONIBILIDAD_LINEAS ?? DBNull.Value);
            var p5 = new SqlParameter("@PAGO_DEUDA", (object)pAGO_DEUDA ?? DBNull.Value);
            var p6 = new SqlParameter("@CUPONES", (object)cUPONES ?? DBNull.Value);
            var p7 = new SqlParameter("@USUARIO", (object)uSUARIO ?? DBNull.Value);
            var p8 = new SqlParameter("@EGRESOS_FIN", (object)eGRESOS_FIN ?? 0.0100m);
            var p9 = new SqlParameter("@GASTOS_ADM", (object)gASTOS_ADM ?? 0.0150m);
            var p10 = new SqlParameter("@SALIDA_TOP20", (object)sALIDA_TOP20 ?? 0.0500m);
            var p11 = new SqlParameter("@CAPITAL_SOCIAL", (object)cAPITAL_SOCIAL ?? 0.0350m);
            var p12 = new SqlParameter("@OTROS_INGRESOS", (object)oTROS_INGRESOS ?? 0.0394m);
            var p13 = new SqlParameter("@PROYECTOR_VAR", (object)pROYECTOR_VAR ?? "VaR al 95%");
            var p14 = new SqlParameter("@PROYECTOR_CARTERA", (object)pROYECTOR_CARTERA ?? "Tasa de crecimiento");
            var p15 = new SqlParameter("@PROYECTOR_CAPITAL", (object)pROYECTOR_CAPITAL ?? "Tasa de crecimiento");
            var p16 = new SqlParameter("@PROYECTOR_OTROS_ING", (object)pROYECTOR_OTROS_ING ?? "Promedio 6 meses");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreCommand(
                "EXEC [dbo].[FGA_Guardar_Parametros_Liquidez] @IDENTIDAD, @RENOVACION, @DISMINUCION, @LINEAS, @PAGO_DEUDA, @CUPONES, @USUARIO, @EGRESOS_FIN, @GASTOS_ADM, @SALIDA_TOP20, @CAPITAL_SOCIAL, @OTROS_INGRESOS, @PROYECTOR_VAR, @PROYECTOR_CARTERA, @PROYECTOR_CAPITAL, @PROYECTOR_OTROS_ING",
                p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16);
        }
    }
}
