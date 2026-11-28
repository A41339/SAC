namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_Proyecciones_Liquidez_Result
    {
        [DataMember]
        public string IdEntidad { get; set; }

        [DataMember]
        public DateTime? PeriodoCorte { get; set; }

        [DataMember]
        public decimal SaldoCartera { get; set; }

        [DataMember]
        public decimal CrecimientoCarteraAnual { get; set; }

        [DataMember]
        public decimal SaldoCapitalSocial { get; set; }

        [DataMember]
        public decimal CrecimientoCapitalSocial { get; set; }

        [DataMember]
        public decimal OtrosIngresosPromedio { get; set; }

        [DataMember]
        public decimal EgresosFinancierosRatio { get; set; }

        [DataMember]
        public decimal GastosAdministrativosRatio { get; set; }

        [DataMember]
        public decimal SaldoTop20 { get; set; }

        [DataMember]
        public decimal FALTotal { get; set; }

        [DataMember]
        public decimal Disponibilidades110 { get; set; }

        [DataMember]
        public decimal Inversiones120 { get; set; }

        [DataMember]
        public decimal RecaudoCartera30d { get; set; }

        [DataMember]
        public decimal VencimientoInv30d { get; set; }

        [DataMember]
        public decimal VencimientoCDP30d { get; set; }
    }
}
