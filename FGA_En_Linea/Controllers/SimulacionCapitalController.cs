using FGA.Model;
using System;
using System.Data;
using System.Web.Mvc;
using Entities.Entities.Procedures;
using System.IO;
using FGA.Models;
using System.Linq;

namespace FGA.Controllers
{
    public class SimulacionCapitalController : BaseController
    {
        public ActionResult Index()
        {
            Load();
            var fechaCorte = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString()).AddMonths(-1);
            Session["PeriodoI"] = fechaCorte.ToShortDateString();
           
            Model.Modelo_Simulacion simulacion = new Modelo_Simulacion();
            return View(simulacion);
        }

        public PartialViewResult ViewSimulacion(String pIdEntidad, DateTime pPeriodo, int pTipoEscenario, decimal pPorc_Cartera,
            decimal pPorc_Inver, decimal pPorc_Activos, decimal pPorc_Disponibilidades, decimal pPorc_Inmuebles, decimal pPorc_Posicion,
            decimal pPorc_Incremento, decimal pPorc_Uoba, decimal pPorc_Capital, decimal pPorc_Retiro, decimal pImca, decimal pSuficienciaObjetivo,
            decimal pPorc_Legal, decimal pPorc_Excedente, decimal pPorc_Voluntarias, decimal pPorc_Aportes, decimal pPorc_Donaciones, decimal pPorc_Revaluacion)
        {

            Session["PeriodoI"] = pPeriodo;
            Session["IdEntidad"] = pIdEntidad;
            Load();
            Entidad objEntidad = ent.GetAll().Where(o=>o.Id== pIdEntidad).FirstOrDefault();
            Model.Modelo_Simulacion simulacion = new Modelo_Simulacion();
            simulacion.Perfil = objEntidad.Perfil_Entidad_Id;
            simulacion.IdEntidad = pIdEntidad;
            simulacion.Periodo = pPeriodo;
            simulacion.TipoEscenario = pTipoEscenario;
            simulacion.Porc_Cartera = pPorc_Cartera;
            simulacion.Porc_Inver = pPorc_Inver;
            simulacion.Porc_Activos = pPorc_Activos;
            simulacion.Porc_Disponibilidades = pPorc_Disponibilidades;
            simulacion.Porc_Inmuebles = pPorc_Inmuebles;
            simulacion.Porc_Posicion = pPorc_Posicion;
            simulacion.Porc_Incremento = pPorc_Incremento;
            simulacion.Porc_Uoba = pPorc_Uoba;
            simulacion.Porc_Capital = pPorc_Capital;
            simulacion.Porc_Retiro = pPorc_Retiro;
            simulacion.Porc_Legal = pPorc_Legal;
            simulacion.Porc_Excedentes = pPorc_Excedente;
            simulacion.Porc_Voluntarias = pPorc_Voluntarias;
            simulacion.Porc_Aportes = pPorc_Aportes;
            simulacion.Porc_Donaciones = pPorc_Donaciones;
            simulacion.Porc_Revaluacion = pPorc_Revaluacion;
            simulacion.PImca = pImca;
            simulacion.Porc_SuficienciaObjetivo = pSuficienciaObjetivo;
            simulacion.Mostrar = true;
           
            simulacion.result = sp.FGA_ConsultarSimulacionCapital(simulacion.IdEntidad, simulacion.Periodo, simulacion.TipoEscenario, simulacion.Porc_Cartera,
            simulacion.Porc_Inver, simulacion.Porc_Activos, simulacion.Porc_Disponibilidades, simulacion.Porc_Inmuebles,
            simulacion.Porc_Posicion, simulacion.Porc_Incremento, simulacion.Porc_Uoba, simulacion.Porc_Capital, simulacion.Porc_Retiro,
            simulacion.PImca, simulacion.Porc_SuficienciaObjetivo, pPorc_Legal, pPorc_Excedente, pPorc_Voluntarias, pPorc_Aportes, pPorc_Donaciones, pPorc_Revaluacion,
            ref simulacion.Imca, ref simulacion.CapitalBase, ref simulacion.ImcaCapital, ref simulacion.IndSuficiencia,
            ref simulacion.CN1, ref simulacion.CCN1);

            if (simulacion.result != null && simulacion.result.Length > 0)
            {
                simulacion.SImca = simulacion.result[0].SIMCA;
                simulacion.SImcaCapital = simulacion.result[0].SIMCA_CAPITAL;
                simulacion.SCapitalBase = simulacion.result[0].SCAPITAL_BASE;
                simulacion.Imca = simulacion.result[0].IMCA;
                simulacion.CapitalBase = simulacion.result[0].CAPITAL_BASE;
                simulacion.ImcaCapital = simulacion.result[0].IMCA_CAPITAL;
                simulacion.IndSuficiencia = simulacion.result[0].IND_SUFICIENCIA;
                simulacion.CN1 = simulacion.result[0].CN1;
                simulacion.CCN1 = simulacion.result[0].CCN1;
            }

            Session["Info"] = simulacion;
            return PartialView("_Simulacion", simulacion);
        }


        [AllowAnonymous]
        public ActionResult LoadReportAsync()
        {
            return GetSimulacion();
        }


        public ZipResult GetSimulacion()
        {
            DataTable sheet = new DataTable("Report");
            string path = System.Configuration.ConfigurationManager.AppSettings["TempExcel"].ToString();

            Model.Modelo_Simulacion simulacion = (Model.Modelo_Simulacion)Session["Info"];
            FGA_Consultar_Simulacion_Capital_Result[] tak = simulacion.result;

            sheet.Columns.Add("Cuenta", typeof(string));
            sheet.Columns.Add("Periodo " + simulacion.Periodo.ToShortDateString(), typeof(string));
            sheet.Columns.Add("Simulacion", typeof(string));
            foreach (var detalle in tak)
            {
                string[] fila = { detalle.NOMBRE.ToString(), 
                                ((detalle.VALPERIODO.Value.ToString("N2")) + (detalle.IND_PORCENTAJE == "S" ? "%" : "" )),
                                ((detalle.VALSIMULADO.Value.ToString("N2")) + (detalle.IND_PORCENTAJE == "S" ? "%" : "" ))};
                sheet.Rows.Add(fila);               
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(sheet);
            string fullPath = Path.Combine(path, "Simulacion " + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + Env.GetUserInfo("userid").ToString() + ".xls");
            CreateExcelFile.CreateExcelDocument(ds, fullPath, includeAutoFilter: true);
            return DownloadResult(fullPath);
        }

        private ZipResult DownloadResult(string path)
        {
            ZipResult zip = new ZipResult();
            zip.AddFile(FileModel.Decode(path));
            return zip;
        }

        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.Close();
                sp.Close();
            }
            base.Dispose(disposing);
        }
    }
}