using System;
using System.Collections.Generic;

namespace FGA.Model
{
    public class BalanceGeneralViewModel
    {
        public string EntidadId { get; set; }
        public string NombreEntidad { get; set; }
        public string LogoUrl { get; set; }
        public string TipoComparacion { get; set; }
        public DateTime PeriodoReferencia { get; set; }

        public string[] EncabezadosPeriodos { get; set; } = new string[5];
        public DateTime[] FechasPeriodos { get; set; } = new DateTime[5];

        public string ComparacionTitulo { get; set; }
        public string ComparacionSubtitulo { get; set; }

        public List<FilaBalanceItem> Filas { get; set; } = new List<FilaBalanceItem>();

        public Graficos_Financieros Graficos { get; set; }

        public ComposicionBalanceDTO DatosComposicion { get; set; } = new ComposicionBalanceDTO();

        public bool EstaBalanceCuadrado { get; set; } = true;
        public string MensajeCuadre { get; set; } = "Balance Contable Cuadrado (Activo = Pasivo + Patrimonio)";
    }

    public class ComposicionBalanceDTO
    {
        public string[] PeriodosCategorias { get; set; } = new string[5];

        // Card 1: Composición del pasivo y patrimonio
        public decimal[] TotalPasivos { get; set; } = new decimal[5];
        public decimal[] TotalPatrimonio { get; set; } = new decimal[5];
        public decimal[] PorcPasivo { get; set; } = new decimal[5];
        public decimal[] PorcPatrimonio { get; set; } = new decimal[5];

        // Card 2: Composición del activo último período
        public string PeriodoUltimoLabel { get; set; }
        public decimal TotalActivoUltimo { get; set; }
        public decimal CarteraUltimo { get; set; }
        public decimal InversionesUltimo { get; set; }
        public decimal DisponibilidadesUltimo { get; set; }
        public decimal OtrosActivosUltimo { get; set; }

        public decimal PorcCarteraUltimo { get; set; }
        public decimal PorcInversionesUltimo { get; set; }
        public decimal PorcDisponibilidadesUltimo { get; set; }
        public decimal PorcOtrosActivosUltimo { get; set; }

        // Categorías de variaciones
        public string[] PeriodosComparacion { get; set; } = new string[4];

        // Card 3: Principales variaciones del activo
        public decimal[] VarCartera { get; set; } = new decimal[4];
        public decimal[] VarInversiones { get; set; } = new decimal[4];
        public decimal[] VarDisponibilidades { get; set; } = new decimal[4];
        public decimal[] VarOtrosActivos { get; set; } = new decimal[4];

        // Card 4: Variación del total de pasivos (Waterfall)
        public decimal SaldoInicialPasivos { get; set; }
        public decimal[] VarTotalPasivos { get; set; } = new decimal[4];
        public decimal SaldoFinalPasivos { get; set; }

        // Card 5: Principales variaciones del pasivo con costo
        public decimal[] VarPasivoConCosto { get; set; } = new decimal[4];

        // Card 6: Variación del patrimonio
        public decimal[] VarPatrimonio { get; set; } = new decimal[4];
    }

    public class FilaBalanceItem
    {
        public int Orden { get; set; }
        public string Seccion { get; set; } // ACTIVO, PASIVO, PATRIMONIO, TOTAL
        public string Concepto { get; set; }
        public int Nivel { get; set; } // 0: Grupo principal, 1: Subgrupo, 2: Cuenta detalle, 3: Subtotal/Total
        public bool EsNegrita { get; set; }
        public bool EsItalica { get; set; }
        public bool SinMontos { get; set; }
        public string CodigoContable { get; set; }

        // Montos en Millones de Colones para los 5 períodos
        public decimal[] Periodos { get; set; } = new decimal[5];

        // Variaciones
        public decimal VariacionAbsoluta { get; set; }
        public decimal VariacionRelativa { get; set; }

        // Análisis Vertical (% Representa respecto a Total Activo o Total Pasivo+Patrimonio)
        public decimal[] PorcentajesRepresenta { get; set; } = new decimal[5];

        public bool EsPrueba => Concepto != null && Concepto.Trim().Equals("Prueba", StringComparison.OrdinalIgnoreCase);
        public bool EsTotal => Nivel == 3;
        public bool EsCabecera => Nivel == 0 || SinMontos;
        public bool EsSeccion => Nivel == 0;
        public bool EsSubtotalItalica => EsTotal && EsItalica;
        public bool EsGranTotal => EsTotal && !EsItalica;
    }
}
