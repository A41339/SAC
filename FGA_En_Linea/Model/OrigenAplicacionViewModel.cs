using System;
using System.Collections.Generic;

namespace FGA.Model
{
    public class OrigenAplicacionViewModel
    {
        public string EntidadId { get; set; }
        public string EntidadNombre { get; set; }
        public DateTime FechaBase { get; set; }
        public DateTime FechaComparacion { get; set; }
        public string TipoComparacion { get; set; }
        public string ComparacionTitulo { get; set; }
        public string LogoUrl { get; set; }

        public List<FilaBalanceComparativoItem> FilasBalance { get; set; } = new List<FilaBalanceComparativoItem>();
        public List<ItemOrigenAplicacion> ItemsOrigen { get; set; } = new List<ItemOrigenAplicacion>();
        public List<ItemOrigenAplicacion> ItemsAplicacion { get; set; } = new List<ItemOrigenAplicacion>();

        public decimal TotalOrigen { get; set; }
        public decimal TotalAplicacion { get; set; }
        public decimal DiferenciaCuadre => Math.Round(TotalOrigen - TotalAplicacion, 2);
    }

    public class FilaBalanceComparativoItem
    {
        public int Orden { get; set; }
        public string Seccion { get; set; }
        public string Concepto { get; set; }
        public int Nivel { get; set; }
        public bool EsNegrita { get; set; }
        public bool EsItalica { get; set; }
        public bool SinMontos { get; set; }
        public string CodigoContable { get; set; }
        public decimal MontoReferencia { get; set; }
        public decimal MontoBase { get; set; }
        public decimal Diferencia => Math.Round(MontoBase - MontoReferencia, 2);
    }

    public class ItemOrigenAplicacion
    {
        public string Tipo { get; set; } // "Origen" o "Aplicación"
        public string Concepto { get; set; }
        public string Categoria { get; set; } // "Activo", "Pasivo", "Patrimonio"
        public decimal Monto { get; set; }
        public decimal PorcentajeParticipacion { get; set; }
    }
}
