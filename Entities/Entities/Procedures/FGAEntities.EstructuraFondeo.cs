namespace Entities.Entities.Procedures
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public partial class FGAEntities
    {
        public virtual ObjectResult<FGA_Consultar_Concentracion_Ahorrantes_Result> FGA_Consultar_Concentracion_Ahorrantes(string iDENTIDAD, Nullable<System.DateTime> pERIODOI, Nullable<System.DateTime> pERIODOF)
        {
            var iDENTIDADParameter = iDENTIDAD != null ?
                new ObjectParameter("IDENTIDAD", iDENTIDAD) :
                new ObjectParameter("IDENTIDAD", typeof(string));

            var pERIODOIParameter = pERIODOI.HasValue ?
                new ObjectParameter("PERIODOI", pERIODOI) :
                new ObjectParameter("PERIODOI", typeof(System.DateTime));

            var pERIODOFParameter = pERIODOF.HasValue ?
                new ObjectParameter("PERIODOF", pERIODOF) :
                new ObjectParameter("PERIODOF", typeof(System.DateTime));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FGA_Consultar_Concentracion_Ahorrantes_Result>("FGA_Consultar_Concentracion_Ahorrantes", iDENTIDADParameter, pERIODOIParameter, pERIODOFParameter);
        }

        public virtual ObjectResult<FGA_Consultar_Concentracion_Vencimiento_Result> FGA_Consultar_Concentracion_Vencimiento(string iDENTIDAD, Nullable<System.DateTime> pERIODOI, Nullable<System.DateTime> pERIODOF)
        {
            var iDENTIDADParameter = iDENTIDAD != null ?
                new ObjectParameter("IDENTIDAD", iDENTIDAD) :
                new ObjectParameter("IDENTIDAD", typeof(string));

            var pERIODOIParameter = pERIODOI.HasValue ?
                new ObjectParameter("PERIODOI", pERIODOI) :
                new ObjectParameter("PERIODOI", typeof(System.DateTime));

            var pERIODOFParameter = pERIODOF.HasValue ?
                new ObjectParameter("PERIODOF", pERIODOF) :
                new ObjectParameter("PERIODOF", typeof(System.DateTime));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FGA_Consultar_Concentracion_Vencimiento_Result>("FGA_Consultar_Concentracion_Vencimiento", iDENTIDADParameter, pERIODOIParameter, pERIODOFParameter);
        }

        public virtual ObjectResult<FGA_Consultar_Cantidad_Asociados_Ahorrantes_Result> FGA_Consultar_Cantidad_Asociados_Ahorrantes(string iDENTIDAD, Nullable<System.DateTime> pERIODOI, Nullable<System.DateTime> pERIODOF)
        {
            var iDENTIDADParameter = iDENTIDAD != null ?
                new ObjectParameter("IDENTIDAD", iDENTIDAD) :
                new ObjectParameter("IDENTIDAD", typeof(string));

            var pERIODOIParameter = pERIODOI.HasValue ?
                new ObjectParameter("PERIODOI", pERIODOI) :
                new ObjectParameter("PERIODOI", typeof(System.DateTime));

            var pERIODOFParameter = pERIODOF.HasValue ?
                new ObjectParameter("PERIODOF", pERIODOF) :
                new ObjectParameter("PERIODOF", typeof(System.DateTime));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FGA_Consultar_Cantidad_Asociados_Ahorrantes_Result>("FGA_Consultar_Cantidad_Asociados_Ahorrantes", iDENTIDADParameter, pERIODOIParameter, pERIODOFParameter);
        }

        public virtual ObjectResult<FGA_Consultar_Indicadores_Fondeo_Result> FGA_Consultar_Indicadores_Fondeo(string iDENTIDAD, Nullable<System.DateTime> pERIODO1, Nullable<System.DateTime> pERIODO2)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("PERIODO1", (object)pERIODO1 ?? DBNull.Value);
            var p3 = new SqlParameter("PERIODO2", (object)pERIODO2 ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Consultar_Indicadores_Fondeo_Result>(
                "EXEC [dbo].[FGA_Consultar_Indicadores_Fondeo] @IDENTIDAD, @PERIODO1, @PERIODO2", p1, p2, p3);
        }

        public virtual int FGA_Generar_Estructura_Fondeo(string iDENTIDAD, Nullable<System.DateTime> pERIODO)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("PERIODO", (object)pERIODO ?? DBNull.Value);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreCommand(
                "EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IDENTIDAD, @PERIODO", p1, p2);
        }
    }
}
