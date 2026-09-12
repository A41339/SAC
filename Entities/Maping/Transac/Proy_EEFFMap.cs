using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class Proy_EEFFMap : EntityTypeConfiguration<Proy_EEFF>
    {
        public Proy_EEFFMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.IdEntidad);
            Property(o => o.PeriodoCorte);
            Property(o => o.TipoPlazo);
            Property(o => o.TipoTasa);
            Property(o => o.TipoCrecimiento);
            Property(o => o.TasaNuevaColocacion);
            Property(o => o.TasaCarteraVigente);
            Property(o => o.TasaCarteraVencida);
            Property(o => o.TasaInversiones);
            Property(o => o.TasaCaptaciones);
            Property(o => o.TasaObligaciones);
            Property(o => o.ReservasVoluntarias);
            Property(o => o.CrecimientoCartera);
            Property(o => o.CrecimientoCaptaciones);
            Property(o => o.CrecimientoCapital);
            Property(o => o.GastoAdmin);
            Property(o => o.GastoEstimac);
            ToTable("Proy_EEFF");            
        }
    }
}
