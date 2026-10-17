using FGA.Models;
using System.Xml;

namespace FileService.Processors
{
    public interface IXmlProcessor
    {
        void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null);
    }
}
