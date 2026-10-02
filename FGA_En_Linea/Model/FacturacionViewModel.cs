using System;
using System.Collections.Generic;
using System.Linq;

namespace FGA.Model
{
    public class FacturacionViewModel
    {
        public string IdEntidad { get; set; } = "";
        public string NomEntidad { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public int Anio { get; set; } = DateTime.Now.Year;
        public int Trimestre { get; set; } = 1;
        public string TrimestreTexto { get; set; } = "I Trimestre";
        public List<string> MesesNombres { get; set; } = new List<string>();
        public bool TieneDatos { get; set; } = false;

        public FacturacionPestanaModel FGD { get; set; } = new FacturacionPestanaModel();
        public FacturacionPestanaModel FFC { get; set; } = new FacturacionPestanaModel();
    }

    public class FacturacionPestanaModel
    {
        public string Codigo { get; set; } = ""; // "FGD" o "FFC"
        public string Titulo { get; set; } = "";
        public string Subtitulo { get; set; } = "";
        public decimal PorcentajeContribucion { get; set; } = 0;
        public decimal PagoTrimestral { get; set; } = 0;

        public List<FacturaRubroItem> Filas { get; set; } = new List<FacturaRubroItem>();

        // Indicadores analíticos de ahorrantes por mes
        public List<FacturaIndicadorMes> IndicadoresMeses { get; set; } = new List<FacturaIndicadorMes>();
    }

    public class FacturaRubroItem
    {
        public string Concepto { get; set; } = "";
        public decimal Mes1 { get; set; } = 0;
        public decimal Mes2 { get; set; } = 0;
        public decimal Mes3 { get; set; } = 0;
        public decimal Promedio { get; set; } = 0;

        public bool EsHeaderGrupo { get; set; } = false;
        public bool EsTotal { get; set; } = false;
        public bool EsDestacado { get; set; } = false;
        public bool EsPorcentaje { get; set; } = false;
        public bool EsEntero { get; set; } = false;
        public int IndentNivel { get; set; } = 0;
    }

    public class FacturaIndicadorMes
    {
        public string Periodo { get; set; } = "";
        public string MesNombre { get; set; } = "";
        public int CantidadTotalAhorrantes { get; set; } = 0;
        public int CantidadCubierto100 { get; set; } = 0;
        public decimal PorcCubierto100 { get; set; } = 0;
        public decimal PorcCubiertoDepositos { get; set; } = 0;
    }
}
