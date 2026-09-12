using System.Collections.Generic;

namespace FGA.Models
{
    public class SolicitudView
    {
        public IList<FileModel> files;
        public IList<Tarea> tareas;
        public SolicitudCambio solicitud;
    }
}