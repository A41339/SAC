namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_GAP_Contractual_Result
    {
        [DataMember]
        public Nullable<int> BandaId { get; set; }

        [DataMember]
        public string Banda { get; set; }

        [DataMember]
        public Nullable<decimal> Entradas { get; set; }

        [DataMember]
        public Nullable<decimal> Salidas { get; set; }

        [DataMember]
        public Nullable<decimal> BrechaBanda { get; set; }

        [DataMember]
        public Nullable<decimal> BrechaAcumulada { get; set; }
    }
}
