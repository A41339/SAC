using DotNet.Highcharts;
using System.Collections.Generic;

namespace FGA.Model
{
    public class Modelo_PEEFF
    {
        public Modelo_PEEFF() {

            ddlCrecimiento = "1";
            ddlPlazo = "1";
            ddlTasa = "1";
            AlertaProyeccion = 1;
            Supuestos = false;
            Generar = false;
        }

        public string ddlTasa;
        public string ddlCrecimiento;
        public string ddlPlazo;
        public bool Supuestos;
        public bool Generar;

        public decimal CarteraAnual;
        public decimal Captaciones;
        public decimal Capital;
        public decimal GastoAdmin;
        public decimal GastoEstimac;
        public decimal ReservaVoluntaria;
        public decimal RecuperacionIncobrable;

        public decimal PCarteraAnual;
        public decimal PCaptaciones;
        public decimal PCapital;
        public decimal PGastoAdmin;
        public decimal PGastoEstimac;
        public decimal PReservaVoluntaria;
        public decimal PRecuperacionIncobrable;

        public decimal TasaCaptaciones;
        public decimal TasaCarteraVencida;
        public decimal TasaCarteraVigente;
        public decimal TasaInversiones;
        public decimal TasaNuevaColocacion;
        public decimal TasaObligaciones;

        public decimal PTasaCaptaciones;
        public decimal PTasaCarteraVencida;
        public decimal PTasaCarteraVigente;
        public decimal PTasaInversiones;
        public decimal PTasaNuevaColocacion;
        public decimal PTasaObligaciones;

        public int AlertaProyeccion;
        public string mensaje;
        public bool proyeccion;
        public Highcharts gMora;
        public Highcharts gCartera;
        public Highcharts gActivo_pasivo;
        public Highcharts gCapitalSocial;
        public Highcharts gObligacionPublico;
        public Highcharts gIngreso_gasto;
        public Highcharts gGastoUtilidad;
        public Highcharts gUtilidadPatrimonio;
    }
}