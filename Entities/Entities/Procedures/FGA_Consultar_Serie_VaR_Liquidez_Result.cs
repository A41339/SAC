namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_Serie_VaR_Liquidez_Result
    {
        [DataMember]
        public Nullable<System.DateTime> Periodo { get; set; }

        [DataMember]
        public Nullable<decimal> SaldoAhorroVista { get; set; }
    }
}
