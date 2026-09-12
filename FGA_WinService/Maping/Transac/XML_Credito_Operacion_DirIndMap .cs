using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Credito_Operacion_DirIndMap : EntityTypeConfiguration<XML_Credito_Operacion_DirInd>
    {
        public XML_Credito_Operacion_DirIndMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.IdDeudor);
            Property(o => o.IdOperacion);
            Property(o => o.CuentaContablePrincipal);
            Property(o => o.CuentaContableProducto);
            Property(o => o.EstadoOperacionCrediticia);
            Property(o => o.FechaVencimiento);
            Property(o => o.MontoCuotaPrincipalActual);
            Property(o => o.SaldoComisiones);
            Property(o => o.SaldoPrincipal);
            Property(o => o.SaldoProductos);
            Property(o => o.TasaInteresNominalVigente);
            Property(o => o.TipoCartera);
            ToTable("XML_Credito_Operacion_DirInd");           
        }
    }
}
