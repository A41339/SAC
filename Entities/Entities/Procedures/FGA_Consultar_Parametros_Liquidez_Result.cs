namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_Parametros_Liquidez_Result
    {
        [DataMember]
        public string IdEntidad { get; set; }

        [DataMember]
        public decimal RenovacionCDP_Historica { get; set; }

        [DataMember]
        public decimal DisminucionRecaudoManual { get; set; }

        [DataMember]
        public decimal DisponibilidadLineasMes { get; set; }

        [DataMember]
        public decimal PagoPrincipalDeudaMes { get; set; }

        [DataMember]
        public decimal CuponesMes { get; set; }

        [DataMember]
        public decimal EgresosFinancierosMes { get; set; }

        [DataMember]
        public decimal GastosAdministrativosMes { get; set; }

        [DataMember]
        public decimal SalidaTop20_Base { get; set; }

        [DataMember]
        public decimal CapitalSocial_Base { get; set; }

        [DataMember]
        public decimal OtrosIngresos_Base { get; set; }

        [DataMember]
        public string ProyectorVaR { get; set; }

        [DataMember]
        public string ProyectorCartera { get; set; }

        [DataMember]
        public string ProyectorCapital { get; set; }

        [DataMember]
        public string ProyectorOtrosIngresos { get; set; }

        [DataMember]
        public DateTime? FechaModificacion { get; set; }

        [DataMember]
        public string UsuarioModifica { get; set; }
    }
}
