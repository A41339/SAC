using System;
using System.IO;
using System.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;
using FGA.Models;

namespace Entities.Entities.Transac
{
    public class PagareDoc
    {
        private const string ResourcesDirectory = @"C:\CGPSINPE\";
        private const string TemplateName = "Plantilla_Pagare.docx";

        public string Generate(Creditos_Solicitudes solicitud)
        {
            string templatePath = Path.Combine(ResourcesDirectory, TemplateName);
            
            // Si no existe la plantilla, lanzamos error o usamos una por defecto básica
            if (!System.IO.File.Exists(templatePath))
            {
                throw new FileNotFoundException("No se encontró la plantilla del pagaré en " + templatePath);
            }

            using (var doc = DocX.Load(templatePath))
            {
                // Reemplazamos marcadores de texto
                doc.ReplaceText("{ID_SOLICITUD}", solicitud.Id.ToString());
                doc.ReplaceText("{ENTIDAD_SOLICITANTE}", solicitud.EntidadSolicitante.Nombre);
                doc.ReplaceText("{MONTO}", solicitud.Mon_MontoSolicitado.ToString("N2"));
                doc.ReplaceText("{FECHA}", DateTime.Now.ToLongDateString());
                doc.ReplaceText("{PLAZO}", solicitud.Oferta.PlazoMeses.ToString());
                doc.ReplaceText("{TASA}", (solicitud.Oferta.TasaInteresAnual * 100).ToString("N2") + "%");

                // Generamos nombre de archivo único
                string fileName = $"Pagare_Sol_{solicitud.Id}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                string outputPath = Path.Combine(ResourcesDirectory, "Generados", fileName);

                // Asegurar que el directorio de salida existe
                string outputDir = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

                doc.SaveAs(outputPath);
                return outputPath;
            }
        }
    }
}
