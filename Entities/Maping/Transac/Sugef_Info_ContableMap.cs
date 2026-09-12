using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;


namespace FGA.Maping
{
    public class Sugef_Info_ContableMap : EntityTypeConfiguration<Sugef_Info_Contable>
    {
        public Sugef_Info_ContableMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Cuenta);
            Property(o => o.BancosComercialesEstado);
            Property(o => o.BancosLeyesEspeciales);
            Property(o => o.BancosPrivadosCoope);
            Property(o => o.EmpresaFinanNoBancaria);
            Property(o => o.EntidadesAutorizadasVivienda);
            Property(o => o.OtrasEntidadesFinancieras);
            Property(o => o.OrganizacionesCooperativas);
            Property(o => o.Total);

            ToTable("Sugef_Info_Contable");            
        }
    }
}
