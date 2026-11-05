namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_Indicadores_Liquidez_Result
    {
        [DataMember]
        public Nullable<int> Orden { get; set; }

        [DataMember]
        public string Indicador { get; set; }

        [DataMember]
        public string Formula { get; set; }

        [DataMember]
        public string QueMide { get; set; }

        [DataMember]
        public Nullable<decimal> ValorP1 { get; set; }

        [DataMember]
        public Nullable<decimal> ValorP2 { get; set; }

        [DataMember]
        public Nullable<bool> EsPorcentaje { get; set; }
    }
}
