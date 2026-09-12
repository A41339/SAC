using Entities.Entities.Procedures;
using System;

namespace FGA.Model
{
    public class Modelo_Simulacion
    {
        public Modelo_Simulacion() {
			Porc_Cartera = 0;
			Porc_Activos = 0;
			Porc_Capital = 0;
			Porc_Disponibilidades = 0;
			Porc_Incremento = 0;
			Porc_Inmuebles = 0;
			Porc_Inver = 0;
			Porc_Posicion = 0;
			Porc_Retiro = 0;
			Porc_Uoba = 0;
			Porc_Legal = 0;
			Porc_Excedentes = 0;
			Porc_Voluntarias = 0;
			Porc_Aportes = 0;
			Porc_Donaciones = 0;
			Porc_Revaluacion = 0;
			TipoEscenario = 1;
			Imca = 0;
			PImca = 0;
			CapitalBase = 0;
			ImcaCapital = 0;
			IndSuficiencia = 0;
			CN1 = 0;
			CCN1 = 0;
			result = null;
			Mostrar = false;
			SImca = 0;
			SCapitalBase = 0;
			SImcaCapital = 0;
			Porc_SuficienciaObjetivo = 14;
			RC = 0;
			RCAPITAL = 0;
			Perfil = 1;
		}

		public long Perfil;
		public bool Mostrar;
		public decimal Porc_SuficienciaObjetivo;
		public decimal RC;
		public decimal RCAPITAL;
		public string IdEntidad;
		public DateTime Periodo;
		public int TipoEscenario;
		public decimal Porc_Cartera;
		public decimal Porc_Inver;
		public decimal Porc_Activos;
		public decimal Porc_Disponibilidades;
		public decimal Porc_Inmuebles;
		public decimal Porc_Posicion;
		public decimal Porc_Incremento;
		public decimal Porc_Uoba;
		public decimal Porc_Capital;
		public decimal Porc_Retiro;
		public decimal Porc_Legal;
		public decimal Porc_Excedentes;
		public decimal Porc_Voluntarias;
		public decimal Porc_Aportes;
		public decimal Porc_Donaciones;
		public decimal Porc_Revaluacion;
		public decimal PImca;
		public decimal? SImca;
		public decimal? SCapitalBase;
		public decimal? SImcaCapital;
		public decimal? Imca;
		public decimal? CapitalBase;
		public decimal? ImcaCapital;
		public decimal? IndSuficiencia;
		public decimal? CN1;
		public decimal? CCN1;
		public string Mensaje;
		public FGA_Consultar_Simulacion_Capital_Result[] result;
	}
}