using FGA.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Concrete
{
    public class ProyeccionRepository : FGA.Concrete.Repository<Proyeccion>
    {
        public ProyeccionRepository()
        {
        }

        public TipoProyeccion Existe(System.DateTime Periodo, string IdEntidad, string Cuenta)
        {
            Proyeccion proy = DbSet.FirstOrDefault(o => o.IdEntidad == IdEntidad && o.Periodo == Periodo && o.Cuenta == Cuenta);
            TipoProyeccion tipo = new TipoProyeccion();
            tipo.Id = (proy == null) ? 0 : proy.IdProyeccion;

            return tipo;
        }

        public override void Add(ref Proyeccion entity)
        {
            string Entidad = entity.IdEntidad;
            DateTime Periodo = entity.Periodo;
            string Cuenta = entity.Cuenta;

            List<Proyeccion> lista = DbSet.Where(o => o.IdEntidad == Entidad && o.Periodo == Periodo && o.Cuenta == Cuenta).ToList();
            foreach(var proy in lista) {
                DbSet.Remove(proy);
                context.SaveChanges();
            }

            base.Add(ref entity);
        }
    }
}