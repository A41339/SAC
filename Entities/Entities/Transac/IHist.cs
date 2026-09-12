using System;

namespace FGA.Models
{
    public interface IHist
    {
        Int64? Id { get; set; }

        DateTime Fecha { get; set; }

        Decimal Monto { get; set; }

        Decimal Desviacion { get; set; }

        Decimal Fluctuacion { get; set; }
    }
}
