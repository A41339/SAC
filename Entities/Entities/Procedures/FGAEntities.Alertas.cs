namespace Entities.Entities.Procedures
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public class FGA_Configuracion_Alerta_Entidad_Result
    {
        public string IdEntidad { get; set; }
        public decimal UmbralVariacionPorc { get; set; }
        public decimal UmbralVariacionMonto { get; set; }
        public string ModoMonitoreo { get; set; }
        public bool EsPersonalizado { get; set; }
        public decimal GlobalPorc { get; set; }
        public decimal GlobalMonto { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioModificacion { get; set; }
    }

    public class FGA_Cuenta_Monitoreada_Entidad_Result
    {
        public int Id { get; set; }
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string Cuenta { get; set; }
        public string NombreCuenta { get; set; }
        public int Nivel { get; set; }
        public string TipoRegla { get; set; }
        public decimal? UmbralPorcPersonalizado { get; set; }
        public decimal? UmbralMontoPersonalizado { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string UsuarioRegistro { get; set; }
    }

    public partial class FGAEntities
    {
        public FGAEntities(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public virtual ObjectResult<FGA_Obtener_Alertas_Financieras_Result> FGA_Generar_Alertas_Variacion_Financiera(string iDENTIDAD, Nullable<System.DateTime> pERIODO, string mODALIDAD, string tIPO_COMPARACION)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("PERIODO", (object)pERIODO ?? DBNull.Value);
            var p3 = new SqlParameter("MODALIDAD", (object)mODALIDAD ?? "Acumulado");
            var p4 = new SqlParameter("TIPO_COMPARACION", (object)tIPO_COMPARACION ?? "Interanual");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Obtener_Alertas_Financieras_Result>(
                "EXEC [dbo].[FGA_Generar_Alertas_Variacion_Financiera] @IDENTIDAD, @PERIODO, @MODALIDAD, @TIPO_COMPARACION", p1, p2, p3, p4);
        }

        public virtual ObjectResult<FGA_Obtener_Alertas_Financieras_Result> FGA_Obtener_Alertas_Financieras(string iDENTIDAD, Nullable<bool> sOLO_NO_LEIDAS, Nullable<int> tOP)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("SOLO_NO_LEIDAS", (object)sOLO_NO_LEIDAS ?? false);
            var p3 = new SqlParameter("TOP", (object)tOP ?? 30);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Obtener_Alertas_Financieras_Result>(
                "EXEC [dbo].[FGA_Obtener_Alertas_Financieras] @IDENTIDAD, @SOLO_NO_LEIDAS, @TOP", p1, p2, p3);
        }

        public virtual int FGA_Marcar_Alerta_Leida(Nullable<int> aLERTA_ID, string iDENTIDAD, string uSUARIO)
        {
            var p1 = new SqlParameter("ALERTA_ID", (object)aLERTA_ID ?? DBNull.Value);
            var p2 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p3 = new SqlParameter("USUARIO", (object)uSUARIO ?? "SYSTEM");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreCommand(
                "EXEC [dbo].[FGA_Marcar_Alerta_Leida] @ALERTA_ID, @IDENTIDAD, @USUARIO", p1, p2, p3);
        }

        public virtual ObjectResult<FGA_Configuracion_Alerta_Entidad_Result> FGA_Obtener_Configuracion_Alerta_Entidad(string iDENTIDAD)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? "-1");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Configuracion_Alerta_Entidad_Result>(
                "EXEC [dbo].[FGA_Obtener_Configuracion_Alerta_Entidad] @IDENTIDAD", p1);
        }

        public virtual ObjectResult<FGA_Configuracion_Alerta_Entidad_Result> FGA_Guardar_Configuracion_Alerta_Entidad(string iDENTIDAD, Nullable<decimal> uMBRAL_PORC, Nullable<decimal> uMBRAL_MONTO, string mODO_MONITOREO, Nullable<bool> aCTIVO, string uSUARIO)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("UMBRAL_PORC", (object)uMBRAL_PORC ?? DBNull.Value);
            var p3 = new SqlParameter("UMBRAL_MONTO", (object)uMBRAL_MONTO ?? DBNull.Value);
            var p4 = new SqlParameter("MODO_MONITOREO", (object)mODO_MONITOREO ?? "TODAS");
            var p5 = new SqlParameter("ACTIVO", (object)aCTIVO ?? true);
            var p6 = new SqlParameter("USUARIO", (object)uSUARIO ?? "SYSTEM");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Configuracion_Alerta_Entidad_Result>(
                "EXEC [dbo].[FGA_Guardar_Configuracion_Alerta_Entidad] @IDENTIDAD, @UMBRAL_PORC, @UMBRAL_MONTO, @MODO_MONITOREO, @ACTIVO, @USUARIO", p1, p2, p3, p4, p5, p6);
        }

        public virtual ObjectResult<FGA_Cuenta_Monitoreada_Entidad_Result> FGA_Obtener_Cuentas_Monitoreadas_Entidad(string iDENTIDAD, Nullable<bool> sOLO_ACTIVAS)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? "-1");
            var p2 = new SqlParameter("SOLO_ACTIVAS", (object)sOLO_ACTIVAS ?? true);

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Cuenta_Monitoreada_Entidad_Result>(
                "EXEC [dbo].[FGA_Obtener_Cuentas_Monitoreadas_Entidad] @IDENTIDAD, @SOLO_ACTIVAS", p1, p2);
        }

        public virtual ObjectResult<FGA_Cuenta_Monitoreada_Entidad_Result> FGA_Guardar_Cuenta_Monitoreada_Entidad(string iDENTIDAD, string cUENTA, string nOMBRE_CUENTA, string tIPO_REGLA, Nullable<decimal> uMBRAL_PORC, Nullable<decimal> uMBRAL_MONTO, string uSUARIO)
        {
            var p1 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p2 = new SqlParameter("CUENTA", (object)cUENTA ?? DBNull.Value);
            var p3 = new SqlParameter("NOMBRE_CUENTA", (object)nOMBRE_CUENTA ?? DBNull.Value);
            var p4 = new SqlParameter("TIPO_REGLA", (object)tIPO_REGLA ?? "MONITOREAR");
            var p5 = new SqlParameter("UMBRAL_PORC", (object)uMBRAL_PORC ?? DBNull.Value);
            var p6 = new SqlParameter("UMBRAL_MONTO", (object)uMBRAL_MONTO ?? DBNull.Value);
            var p7 = new SqlParameter("USUARIO", (object)uSUARIO ?? "SYSTEM");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<FGA_Cuenta_Monitoreada_Entidad_Result>(
                "EXEC [dbo].[FGA_Guardar_Cuenta_Monitoreada_Entidad] @IDENTIDAD, @CUENTA, @NOMBRE_CUENTA, @TIPO_REGLA, @UMBRAL_PORC, @UMBRAL_MONTO, @USUARIO", p1, p2, p3, p4, p5, p6, p7);
        }

        public virtual int FGA_Eliminar_Cuenta_Monitoreada_Entidad(int iD, string iDENTIDAD, string uSUARIO)
        {
            var p1 = new SqlParameter("ID", iD);
            var p2 = new SqlParameter("IDENTIDAD", (object)iDENTIDAD ?? DBNull.Value);
            var p3 = new SqlParameter("USUARIO", (object)uSUARIO ?? "SYSTEM");

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreCommand(
                "EXEC [dbo].[FGA_Eliminar_Cuenta_Monitoreada_Entidad] @ID, @IDENTIDAD, @USUARIO", p1, p2, p3);
        }
    }
}
