using System.Data;
using Xceed.Words.NET;

namespace Entities.Entities.Transac
{
    public interface IDoc
    {
        void CreateDocument();

        DataTable GetDetailsData();

        DocX CreateFromTemplate(DocX templateDoc);
    }
}
