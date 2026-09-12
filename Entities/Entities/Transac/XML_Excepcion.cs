namespace FGA.Models
{
    public class XML_Excepcion
    {
        public string IdArchivo_Id { get; set; }
      
        public string IdEntidad_Id { get; set; }

        public virtual TipoXML IdArchivo { get; set; }

        public virtual Entidad IdEntidad { get; set; }
    }
}
