using System;
using System.Web.Mvc;
using static FGA.Utility.Utilitarios;

namespace FGA.Controllers
{
    public class FacturacionController : BaseController
    {
        public ActionResult Index()
        {
            Load();
            Session["TipoReporte"] = null;
            return View();
        }

        public ActionResult Buscar(String Entidades, String Anio, String Trimestre) {

            Session["IdEntidad"] = Entidades;
            Session["Anno"] = Anio;
            Session["Trimestre"] = Trimestre;
            Session["TipoReporte"] = enum_tipoReporte.rpt_factura;
            Session["TipoReporte2"] = enum_tipoReporte.rpt_factura_ffc;

            Load();
            return View("Index");
        }
    }
}