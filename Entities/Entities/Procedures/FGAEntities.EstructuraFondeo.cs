namespace Entities.Entities.Procedures
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

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
            var iDENTIDADParameter = iDENTIDAD != null ?
                new ObjectParameter("IDENTIDAD", iDENTIDAD) :
                new ObjectParameter("IDENTIDAD", typeof(string));

            var pERIODO1Parameter = pERIODO1.HasValue ?
                new ObjectParameter("PERIODO1", pERIODO1) :
                new ObjectParameter("PERIODO1", typeof(System.DateTime));

            var pERIODO2Parameter = pERIODO2.HasValue ?
                new ObjectParameter("PERIODO2", pERIODO2) :
                new ObjectParameter("PERIODO2", typeof(System.DateTime));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FGA_Consultar_Indicadores_Fondeo_Result>("FGA_Consultar_Indicadores_Fondeo", iDENTIDADParameter, pERIODO1Parameter, pERIODO2Parameter);
        }

        public virtual int FGA_Generar_Estructura_Fondeo(string iDENTIDAD, Nullable<System.DateTime> pERIODO)
        {
            var iDENTIDADParameter = iDENTIDAD != null ?
                new ObjectParameter("IDENTIDAD", iDENTIDAD) :
                new ObjectParameter("IDENTIDAD", typeof(string));

            var pERIODOParameter = pERIODO.HasValue ?
                new ObjectParameter("PERIODO", pERIODO) :
                new ObjectParameter("PERIODO", typeof(System.DateTime));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("FGA_Generar_Estructura_Fondeo", iDENTIDADParameter, pERIODOParameter);
        }
    }
}
