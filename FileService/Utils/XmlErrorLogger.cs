using FGA.Models;
using System;

namespace FileService.Utils
{
    public static class XmlErrorLogger
    {
        public static void AgregarError(string detalle, XML_Encabezado encabezado, SIContext db)
        {
            try
            {
                if (encabezado != null && (encabezado.IdEntidad == null || encabezado.IdEntidad.Ind_Validar))
                {
                    using (var errDb = new SIContext())
                    {
                        errDb.Database.CommandTimeout = 60;
                        var error = new XML_Errores();
                        error.IdEncabezado_Id = encabezado.Id;
                        error.Detalle = detalle;
                        errDb.XML_Errores.Add(error);
                        errDb.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
