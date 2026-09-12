using System;
using System.Data;
using System.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Entities.Entities.Transac
{
    public class WRiesgos : IDoc
    {

        protected const string ResourcesDirectory = @"C:\\CGPSINPE\";

        protected static Paragraph GetParagraphContainingPicture(DocX doc, int numImg)
        {
            int i = 0;

            foreach (var p in doc.Paragraphs)
            {
                var picture = p.Pictures.FirstOrDefault();
                if (picture != null)
                {
                    i += 1;
                    if (i == numImg)
                        return p;
                }
            }

            return null;
        }

        public void CreateDocument()
        {
            var templateDoc = DocX.Load(ResourcesDirectory + @"Pagare.docx") as DocX;
            if (templateDoc != null)
            {
                var invoice = CreateFromTemplate(templateDoc);
                invoice.SaveAs(ResourcesDirectory + @"CreateInvoice.docx");
            }
        }

        public DataTable GetDetailsData()
        {
            // Create Data to fill the invoice details table.
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new DataColumn[] { new DataColumn("Description"), new DataColumn("Amount") });

            dataTable.Rows.Add("Explorer 8698HD Terminal", "$149.95");
            dataTable.Rows.Add("MultiSwitch TV Connector", "$24.95");
            dataTable.Rows.Add("50 feets cable wires", "$22.49");
            dataTable.Rows.Add("Transit A449 Phone Modem", "$59.95");
            dataTable.Rows.Add("Toms Wi-Fi router", "$79.95");
            dataTable.Rows.Add("Toms Protect2000 Antivirus software", "$39.95");
            dataTable.Rows.Add("Installation (3h30)", "$154.49");

            return dataTable;
        }

        public DocX CreateFromTemplate(DocX templateDoc)
        {
            // Fill in the document custom properties.
            templateDoc.AddCustomProperty(new CustomProperty("Entidad", "Cooperativa Nacional de Educadores"));
            templateDoc.AddCustomProperty(new CustomProperty("Periodo", "Agosto 2020"));
            templateDoc.AddCustomProperty(new CustomProperty("Fecha", DateTime.Now.ToShortDateString()));
            templateDoc.AddCustomProperty(new CustomProperty("Analista", "Cynthia   cynthia@fgaconfia.com"));
            templateDoc.AddCustomProperty(new CustomProperty("GerenteTecnico", "Luis   luis@fgaconfia.com"));

            // Remove the default logo and add the new one.
            var paragraphWithDefaultLogo = GetParagraphContainingPicture(templateDoc, 2);
            if (paragraphWithDefaultLogo != null)
            {
                paragraphWithDefaultLogo.Pictures.First().Remove();
                var newLogo = templateDoc.AddImage(ResourcesDirectory + @"Logo.jpg");
                paragraphWithDefaultLogo.AppendPicture(newLogo.CreatePicture(45f, 135f));
            }

            return templateDoc;
        }

    }
}
