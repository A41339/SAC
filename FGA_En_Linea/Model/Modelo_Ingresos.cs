using DotNet.Highcharts;
using System.Collections.Generic;

namespace FGA.Model
{
    public class Modelo_SF 
    {
        public Highcharts gIngresos;
        public List<Entities.Entities.Procedures.FGA_Consultar_Ingreso_SF_Result>  lIngresos;
        public Highcharts gGastos;
        public List<Entities.Entities.Procedures.FGA_Consultar_Gasto_SF_Result> lGastos;
        public Highcharts gComposicionBalance;
        public List<Entities.Entities.Procedures.FGA_Consultar_Composicion_Balance_SF_Result> lComposicionBalance;
    }
}