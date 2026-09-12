using System;
using System.Collections.Generic;
using System.Linq;
using FGA.Utility;

namespace Concrete
{
    public class XML_EncabezadoRepository : FGA.Concrete.Repository<FGA.Models.XML_Encabezado>
    {
        public XML_EncabezadoRepository()
        {
        }

        public override FGA.Models.XML_Encabezado Get(string id)
        {
            long enc = long.Parse(id);
            return DbSet.Include("IdEntidad")
                .Include("IdUsuario")
                .Include("IdArchivo")
                 .Include("IdEstado")
                .FirstOrDefault(o => o.Id == enc);
        }

        public List<FGA.Models.XML_Encabezado> GetByCompany(string idEntidad)
        {
            DateTime fecha = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

            return DbSet.Include("IdEntidad")
                .Include("IdUsuario")
                .Include("IdArchivo")
                 .Include("IdEstado")
                .Where(o => o.IdEntidad.Id == idEntidad).ToList(); 
        }

        public  List<FGA.Models.XML_Encabezado> GetMonthFiles(string idEntidad, DateTime periodo)
        {
            DateTime fecha = new DateTime(periodo.Year, periodo.Month, 1);

            return DbSet.Include("IdEntidad")
                .Include("IdUsuario")
                .Include("IdArchivo")
                 .Include("IdEstado").Where(o => o.IdEntidad.Id == idEntidad
            && o.Periodo == fecha
            && o.IdEstado.Id != Utilitarios.archivoEliminado).OrderBy(o => o.IdArchivo.Nombre).ToList(); 
        }

        public Boolean IsLoadingFile(string idEntidad, DateTime periodo)
        {
            return (DbSet.Where(o => o.IdEntidad.Id == idEntidad &&
                                           o.Periodo == periodo &&
                                           o.IdEstado.Id == Utilitarios.archivoCargado).Count() > 0 ? true : false);
        }

        public Boolean IsUpload(string idEntidad, string idArchivo, DateTime periodo)
        {
            Boolean isUpload = (DbSet.Where(o => o.IdEntidad.Id == idEntidad &&
                                               o.IdArchivo.Id == idArchivo &&
                                               o.Periodo == periodo &&
                                               o.IdEstado.Id == Utilitarios.archivoAceptado).Count() > 0 ? true : false);

            if (!isUpload) {
                List<FGA.Models.XML_Encabezado> files = DbSet
                .Where(o => o.IdEntidad.Id == idEntidad &&
                       o.IdArchivo.Id == idArchivo &&
                       o.Periodo == periodo &&
                       (o.IdEstado.Id != Utilitarios.archivoAceptado ||
                        o.IdEstado.Id != Utilitarios.archivoEliminado)).ToList();

                foreach (FGA.Models.XML_Encabezado enc in files)
                {
                    ArchivoEstadoRepository arces = new ArchivoEstadoRepository();
                    enc.IdEstado_Id = Utilitarios.archivoEliminado;
                    Update(enc);
                }
            }

            return isUpload;
        }
    }
}