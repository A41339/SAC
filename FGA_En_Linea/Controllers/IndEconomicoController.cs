using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FGA.Enum;
using FGA.Models;

namespace FGA.Controllers
{
    public class IndEconomicoController : BaseController
    {
        private void Init(object selectedValue = null)
        {
            this.Load();
            var indicadores = tipos.GetAll()
                           .OrderBy(x => x.Nombre)
                           .ToList();

            // Insertar la opción "Seleccione una opción" al inicio
            indicadores.Insert(0, new BCCR_TipoIndicador
            {
                Id = -1,
                Nombre = "Todos"
            });

            var sel = selectedValue ?? Session["Indicador"] ?? -1;
            ViewBag.Indicadores = new SelectList(indicadores, "Id", "Nombre", sel);
        }


        public ActionResult Index()
        {
            Init();
            return View(GetList());
        }

        [HttpGet]
        public IEnumerable<BCCR_Indicadores> GetList()
        {
            if (Session["Indicador"] != null)
            {
                var lista = indicador.GetByType(int.Parse(Session["Indicador"].ToString())).ToList();
                return lista;
            }
            return null;
        }

        private void LoadBuscar(int codIndicador)
        {
            if (codIndicador == -1)
                Session["Indicador"] = null;
            else
                Session["Indicador"] = codIndicador;
            Init(codIndicador);
        }

        [AllowAnonymous]
        public ActionResult Buscar(int? Indicadores = null)
        {
            int val = Indicadores ?? -1;
            LoadBuscar(val);
            return View("Index", GetList());
        }

        [HttpGet]
        public ActionResult GetGrid()
        {
            try
            {
                if (Session["Indicador"] != null)
                {
                    var porcentaje = tipos.Get(Session["Indicador"].ToString()).Ind_Porcentaje;
                    var tak = indicador.GetByType(int.Parse(Session["Indicador"].ToString())).OrderByDescending(o => o.Fecha);
                    var result = from c in tak
                                 select new string[] {
                             Convert.ToString(c.Fecha.Value.ToShortDateString()),
                             Utility.Utilitarios.ConvertirAString(c.Monto.Value) + (porcentaje == "S" ? "%" : ""),
                            };
                    return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { aaData = new[] { "", "" } }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetUltimos()
        {
            try
            {
                var tak = indicador.GetUltimosValoresPorIndicador();
                var result = from c in tak
                             select new string[] {
                                      Convert.ToString(c.Nombre),
                             Convert.ToString(c.Fecha.ToShortDateString()),
                             Utility.Utilitarios.ConvertirAString(c.Monto) + (c.Ind_Porcentaje == "S" ? "%" : ""),
                            };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
            }

            return null;
        }

        private readonly FGA_En_Linea.BCCR_TipoIndicadorService.BCCR_TipoIndicadorServiceClient tipos = new FGA_En_Linea.BCCR_TipoIndicadorService.BCCR_TipoIndicadorServiceClient();
        private readonly FGA_En_Linea.BCCR_IndicadoresService.BCCR_IndicadoresServiceClient indicador = new FGA_En_Linea.BCCR_IndicadoresService.BCCR_IndicadoresServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tipos.Close();
                indicador.Close();
            }
            base.Dispose(disposing);
        }
    }
}