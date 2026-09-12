using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Inveresion_ActivaMap : EntityTypeConfiguration<XML_Inversion_Activa>
    {
        public XML_Inveresion_ActivaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.IdEmisor);
            Property(o => o.IdInstrumento);
            Property(o => o.CuentaContablePrincipal);
            Property(o => o.FechaVencimiento);
            Property(o => o.ValorFacial);
            Property(o => o.ValorTransado);
            Property(o => o.ValorMercado);
            Property(o => o.SaldoPrincipal);

            ToTable("XML_Inversion_Activa");

        }
    }
}
