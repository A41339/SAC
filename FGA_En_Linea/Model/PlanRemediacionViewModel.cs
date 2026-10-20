using System;

namespace FGA.Model
{
    public class PlanRemediacionMetadata
    {
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string NombreArchivoOriginal { get; set; }
        public DateTime FechaCarga { get; set; }
        public string UsuarioCarga { get; set; }
        public long TamanioBytes { get; set; }
        public string Notas { get; set; }
    }

    public class AdminPlanRemediacionViewModel
    {
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string ContactoEntidad { get; set; }
        public bool TienePlan { get; set; }
        public string NombreArchivoOriginal { get; set; }
        public DateTime? FechaCarga { get; set; }
        public string UsuarioCarga { get; set; }
        public long TamanioBytes { get; set; }
        public string TamanioFormato { get; set; }
        public string Notas { get; set; }
    }
}
