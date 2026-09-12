using System.ComponentModel.DataAnnotations;
using System;

namespace FGA.Models
{
    public class CatalogoCuenta
    {
		[Key()]
		public String Cuenta { get; set; }
		public String Nombre { get; set; }
		public int? Nivel { get; set; }
		public string Padre { get; set; }
		public bool Ind_Financiero { get; set; }
		public bool Ind_Recuperacion { get; set; }
		public bool Ind_CapitalSocial { get; set; }
		public bool Ind_CuentasLiquidadas { get; set; }
		public bool Ind_OtrosPasivos { get; set; }
		public bool Ind_OtrosActivos { get; set; }
		public bool Ind_CarteraTotal { get; set; }
		public bool Ind_Proyectar { get; set; }

	}
}