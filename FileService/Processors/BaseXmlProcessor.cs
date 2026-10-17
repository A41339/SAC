using FGA.Models;
using System.Data.Entity;
using System.Linq;
using System.Xml;

namespace FileService.Processors
{
    public abstract class BaseXmlProcessor : IXmlProcessor
    {
        public abstract void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null);

        protected void GuardarLote(SIContext db, XML_Encabezado encabezado)
        {
            db.SaveChanges();
            foreach (var entry in db.ChangeTracker.Entries().Where(e => e.Entity != encabezado).ToList())
            {
                entry.State = EntityState.Detached;
            }
        }
    }

    public class NoOpXmlProcessor : IXmlProcessor
    {
        public void Procesar(XmlReader reader, XML_Encabezado encabezado, SIContext db, long? contableEstadosId = null)
        {
            // Archivos sin lógica de inserción activa en FileService (comportamiento original de case vacío)
            encabezado.Cantidad = 0;
            db.Entry(encabezado).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
