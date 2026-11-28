namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_Calculo_VaR_Liquidez_Result
    {
        [DataMember]
        public string IdEntidad { get; set; }

        [DataMember]
        public DateTime? PeriodoCorte { get; set; }

        [DataMember]
        public int ObservacionesHistoricas { get; set; }

        [DataMember]
        public decimal SaldoAhorroVista { get; set; }

        [DataMember]
        public decimal SaldoCaptacionesVencidas { get; set; }

        [DataMember]
        public decimal DesviacionEstandar { get; set; }

        [DataMember]
        public decimal VaR95_Monto { get; set; }

        [DataMember]
        public decimal VaR99_Monto { get; set; }

        [DataMember]
        public decimal Corrida95_Monto { get; set; }

        [DataMember]
        public decimal Corrida99_Monto { get; set; }

        [DataMember]
        public decimal PctSalida95 { get; set; }

        [DataMember]
        public decimal PctSalida99 { get; set; }

        [DataMember]
        public decimal MaxSalidaHistorica { get; set; }

        [DataMember]
        public decimal PctMaxSalida { get; set; }
    }
}
