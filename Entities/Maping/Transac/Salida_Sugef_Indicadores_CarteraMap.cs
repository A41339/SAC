using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;


namespace FGA.Maping
{
    public class Salida_Sugef_Indicadores_CarteraMap : EntityTypeConfiguration<Salida_Sugef_Indicadores_Cartera>
    {
        public Salida_Sugef_Indicadores_CarteraMap()
        {
            HasKey(u => u.Id);

            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.BancoLeyesEspeciales);
            Property(o => o.BancosPrivadosCoope);
            Property(o => o.EmpresaFinanNoBancaria);
            Property(o => o.EntidadesAutorizadasVivienda);
            Property(o => o.OrganizacionesCooperativas);
            Property(o => o.Total);
            Property(o => o.IdIndicador);

            ToTable("Salida_Sugef_Indicadores_Cartera");            
        }
    }
}
