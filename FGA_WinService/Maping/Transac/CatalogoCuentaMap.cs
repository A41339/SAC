using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class CatalogoCuentaMap : EntityTypeConfiguration<CatalogoCuenta>
    {
        public CatalogoCuentaMap()
        {           
            HasKey(o => o.Cuenta);
            Property(o => o.Nombre);
            Property(o => o.Nivel);
            Property(o => o.Padre);
            Property(o => o.Ind_CapitalSocial);
            Property(o => o.Ind_CarteraTotal);
            Property(o => o.Ind_CuentasLiquidadas);
            Property(o => o.Ind_Financiero);
            Property(o => o.Ind_OtrosActivos);
            Property(o => o.Ind_OtrosPasivos);
            Property(o => o.Ind_Proyectar);
            Property(o => o.Ind_Recuperacion);

            ToTable("CatalogoCuenta");
            
        }
    }
}
