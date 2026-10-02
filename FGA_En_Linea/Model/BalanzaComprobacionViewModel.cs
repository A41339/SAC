using System;
using System.Collections.Generic;

namespace FGA.Model
{
    public class BalanzaComprobacionViewModel
    {
        public string EntidadId { get; set; }
        public string NombreEntidad { get; set; }
        public string LogoUrl { get; set; }
        public string Modalidad { get; set; } = "Acumulado"; // "Acumulado" o "Mensual"
        public string TipoComparacion { get; set; } = "Interanual"; // "Interanual", "Trimestral", "Mensual"
        public DateTime PeriodoReferencia { get; set; }

        public string[] EncabezadosPeriodos { get; set; } = new string[5];
        public DateTime[] FechasPeriodos { get; set; } = new DateTime[5];

        public string ComparacionTitulo { get; set; }
        public string ComparacionSubtitulo { get; set; }

        public List<FilaBalanzaItem> Filas { get; set; } = new List<FilaBalanzaItem>();

        public ComposicionBalanzaDTO DatosComposicion { get; set; } = new ComposicionBalanzaDTO();

        public bool EstaCuadrada { get; set; } = true;
        public string MensajeCuadre { get; set; } = "Balanza Contable Cuadrada";
    }

    public class FilaBalanzaItem
    {
        public int Orden { get; set; }
        public string Cuenta { get; set; }
        public string Nombre { get; set; }
        public int Nivel { get; set; } // 1: Clase, 2: Grupo, 3: Subgrupo, 4: Cuenta Detalle
        public string Padre { get; set; }
        public bool EsPadre { get; set; }

        // Montos en Millones de Colones para los 5 períodos
        public decimal[] Periodos { get; set; } = new decimal[5];

        // Variaciones
        public decimal VariacionAbsoluta { get; set; }
        public decimal VariacionRelativa { get; set; }

        // Análisis Vertical (% Representa respecto a la Clase mayor o Total Activo)
        public decimal[] PorcentajesRepresenta { get; set; } = new decimal[5];

        // Ayudantes de diseño y vista
        public bool EsClase => Nivel == 1;
        public bool EsGrupo => Nivel == 2;
        public bool EsSubgrupo => Nivel == 3;
        public bool EsDetalle => Nivel >= 4;
        public bool EsFilaMayor => Nivel <= 2;
    }

    public class ComposicionBalanzaDTO
    {
        public string[] PeriodosCategorias { get; set; } = new string[5];

        // Totales por Grandes Clases en el último período (Slot 4)
        public decimal TotalActivoUltimo { get; set; }
        public decimal TotalPasivoUltimo { get; set; }
        public decimal TotalPatrimonioUltimo { get; set; }
        public decimal TotalIngresosUltimo { get; set; }
        public decimal TotalGastosUltimo { get; set; }

        // Porcentajes de representación sobre Activo Total
        public decimal PorcActivo { get; set; }
        public decimal PorcPasivo { get; set; }
        public decimal PorcPatrimonio { get; set; }

        // Series temporales a 5 períodos para gráficos de tendencias
        public decimal[] SerieActivos { get; set; } = new decimal[5];
        public decimal[] SeriePasivos { get; set; } = new decimal[5];
        public decimal[] SeriePatrimonio { get; set; } = new decimal[5];
        public decimal[] SerieIngresos { get; set; } = new decimal[5];
        public decimal[] SerieGastos { get; set; } = new decimal[5];

        // Variaciones del último período vs período anterior
        public decimal VarActivo { get; set; }
        public decimal VarPasivo { get; set; }
        public decimal VarPatrimonio { get; set; }
        public decimal VarIngresos { get; set; }
        public decimal VarGastos { get; set; }
    }
}
