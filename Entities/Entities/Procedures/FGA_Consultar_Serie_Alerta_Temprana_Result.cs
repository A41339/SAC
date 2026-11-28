namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_Serie_Alerta_Temprana_Result
    {
        [DataMember]
        public string MesTexto { get; set; }

        [DataMember]
        public Nullable<System.DateTime> Periodo { get; set; }

        [DataMember]
        public Nullable<decimal> Valor { get; set; }
    }
}
