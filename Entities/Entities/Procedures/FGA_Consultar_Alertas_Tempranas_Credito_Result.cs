namespace Entities.Entities.Procedures
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public partial class FGA_Consultar_Alertas_Tempranas_Credito_Result
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Tipo { get; set; }

        [DataMember]
        public string Formula { get; set; }

        [DataMember]
        public Nullable<decimal> ValorActual { get; set; }

        [DataMember]
        public Nullable<decimal> ValorAnterior { get; set; }

        [DataMember]
        public string LimiteTexto { get; set; }

        [DataMember]
        public Nullable<decimal> LimiteValor { get; set; }

        [DataMember]
        public string LimiteOperador { get; set; }

        [DataMember]
        public string Estado { get; set; }

        [DataMember]
        public Nullable<int> Orden { get; set; }
    }
}
