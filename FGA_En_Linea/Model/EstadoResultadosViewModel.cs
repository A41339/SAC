using System;
using System.Collections.Generic;

namespace FGA.Model
{
    public class EstadoResultadosViewModel
    {
        public string EntidadId { get; set; }
        public string NombreEntidad { get; set; }
        public string LogoUrl { get; set; }
        public string Modalidad { get; set; } = "Acumulado";
        public string TipoComparacion { get; set; } = "Interanual";
        public DateTime PeriodoReferencia { get; set; }

        public string[] EncabezadosPeriodos { get; set; } = new string[5];
        public DateTime[] FechasPeriodos { get; set; } = new DateTime[5];

        public string ComparacionTitulo { get; set; }
        public string ComparacionSubtitulo { get; set; }

        public List<FilaEstadoResultadosItem> Filas { get; set; } = new List<FilaEstadoResultadosItem>();

        public Graficos_Financieros Graficos { get; set; }
        public ComposicionERDTO DatosComposicion { get; set; } = new ComposicionERDTO();
    }

    public class ComposicionERDTO
    {
        public string[] PeriodosCategorias { get; set; } = new string[5];

        // Márgenes y Resultados en los 5 períodos
        public decimal[] TotalIngresosFinancieros { get; set; } = new decimal[5];
        public decimal[] TotalGastosFinancieros { get; set; } = new decimal[5];
        public decimal[] MargenFinancieroBruto { get; set; } = new decimal[5];
        public decimal[] GastosAdministrativos { get; set; } = new decimal[5];
        public decimal[] ResultadoOperacionalNeto { get; set; } = new decimal[5];
        public decimal[] ResultadoPeriodo { get; set; } = new decimal[5];

        // Desglose del Último Período (para Gráficos Donut / Pie)
        public string PeriodoUltimoLabel { get; set; }
        public decimal TotalIngresosUltimo { get; set; }
        public decimal CarteraUltimo { get; set; }
        public decimal InversionesUltimo { get; set; }
        public decimal DisponibilidadesUltimo { get; set; }
        public decimal OtrosIngresosUltimo { get; set; }

        public decimal TotalGastosUltimo { get; set; }
        public decimal GastosPublicoUltimo { get; set; }
        public decimal GastosEntidadesUltimo { get; set; }
        public decimal OtrosGastosFinancierosUltimo { get; set; }
        public decimal GastosAdminUltimo { get; set; }
        public decimal GastosDeterioroUltimo { get; set; }

        // Variaciones entre períodos
        public string[] PeriodosComparacion { get; set; } = new string[4];
        public decimal[] VarMargenFinanciero { get; set; } = new decimal[4];
        public decimal[] VarResultadoNeto { get; set; } = new decimal[4];

        // Variaciones porcentuales para gráficos de evolución
        public decimal?[] VarIngresosPct { get; set; } = new decimal?[5];
        public decimal?[] VarGastosPct { get; set; } = new decimal?[5];
    }

    public class FilaEstadoResultadosItem
    {
        public int Orden { get; set; }
        public string Seccion { get; set; }
        public string Concepto { get; set; }
        public int Nivel { get; set; } // 0: Sección/Categoría, 1: Cuenta Detalle, 2: Subtotal, 3: Gran Total
        public bool EsNegrita { get; set; }
        public bool EsItalica { get; set; }
        public bool SinMontos { get; set; }
        public string CodigoContable { get; set; }

        // Montos en Millones de Colones para los 5 períodos
        public decimal[] Periodos { get; set; } = new decimal[5];

        // Variaciones
        public decimal VariacionAbsoluta { get; set; }
        public decimal VariacionRelativa { get; set; }

        // Análisis Vertical (% Representa respecto al Total de Ingresos Financieros)
        public decimal[] PorcentajesRepresenta { get; set; } = new decimal[5];

        public bool EsSeccion => Nivel == 0;
        public bool EsTotal => Nivel >= 2;
        public bool EsSubtotalItalica => Nivel == 2 && EsItalica;
        public bool EsGranTotal => Nivel == 3;
    }
}
