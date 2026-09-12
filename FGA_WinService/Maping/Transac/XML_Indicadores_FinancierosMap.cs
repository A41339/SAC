using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Indicadores_FinancierosMap : EntityTypeConfiguration<XML_Indicadores_Financieros>
    {
        public XML_Indicadores_FinancierosMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.CuentaCatalogo);
            Property(o => o.Moneda);
            Property(o => o.TipoCatalogoSUGEF);
            Property(o => o.MontoValor);

            ToTable("XML_Indicadores_Financieros");

        }
    }
}
