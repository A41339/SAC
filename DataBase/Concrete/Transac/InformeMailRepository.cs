using FGA.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Concrete
{
    public class InformeMailRepository : FGA.Concrete.Repository<FGA.Models.InformeMail>
    {
        public InformeMailRepository()
        {
        }

        public List<DateTime> ListaPeriodos(string idEntidad)
        {
            return DbSet
                .Where(o => o.IdEntidad_Id == idEntidad) // Filtrar por idEntidad
                .Select(o => o.Periodo.Year) // Solo obtener el año
                .Distinct()
                .OrderByDescending(o => o) // Ordenar los años
                .AsEnumerable() // Cambiar a Enumerable para procesar en memoria
                .Select(year => new DateTime(year, 1, 1)) // Crear DateTime en memoria con el año y enero
                .ToList();
        }

        public List<InformeMail> GetInformes(string idEntidad, DateTime Periodo)
        {
            int anio = Periodo.Year; // Extraer el año del periodo
            return DbSet
                .Include("IdInforme")
                .Where(o => o.IdEntidad_Id == idEntidad && o.Periodo.Year == anio) // Comparar solo el año
                .OrderByDescending(o => o.Periodo) // Ordenar descendente por la fecha completa
                .ToList();
        }

        public override InformeMail Get(string id)
        {
            int informe = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == informe);
        }
    }
}