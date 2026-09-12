using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FGA.Models;

namespace Concrete
{
    public class IndicadorRecienteDTO
    {
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Ind_Porcentaje { get; set; }
    }

    public class BCCR_IndicadoresRepository : FGA.Concrete.Repository<BCCR_Indicadores>
    {
        public BCCR_IndicadoresRepository()
        {
        }

        public override BCCR_Indicadores Get(string id)
        {
            long log = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == log);
        }

        public async Task<List<BCCR_Indicadores>> GetByType(int idType)
        {
            DateTime date = new DateTime(2016, 1, 1);
            return await DbSet.Where(o => o.Fecha >= date && o.IdIndicador == idType).Include("TipoIndicador").ToListAsync();
        }

        public async override Task<List<BCCR_Indicadores>> GetAll()
        {
            DateTime date = new DateTime(2016, 1, 1);
            return await DbSet.Where(o => o.Fecha >= date).ToListAsync();

        }

        public List<IndicadorRecienteDTO> GetUltimosValoresPorIndicador()
        {
            var maxFechasPorIndicador = DbSet
                .GroupBy(x => x.IdIndicador)
                .Select(g => new
                {
                    IdIndicador = g.Key,
                    MaxFecha = g.Max(x => x.Fecha)
                });

            var query = from indicador in DbSet
                        join maxFecha in maxFechasPorIndicador
                            on new { indicador.IdIndicador, indicador.Fecha } equals new { IdIndicador = maxFecha.IdIndicador, Fecha = maxFecha.MaxFecha }
                        join tipo in (context.Set<BCCR_TipoIndicador>())
                            on indicador.IdIndicador equals tipo.Id
                        select new IndicadorRecienteDTO
                        {
                            Nombre = tipo.Nombre,
                            Fecha = indicador.Fecha.Value,
                            Monto = indicador.Monto.Value,
                            Ind_Porcentaje = tipo.Ind_Porcentaje
                        };

            return query.ToList();
        }

    }
}