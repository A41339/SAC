using System;
using Microsoft.Reporting.WebForms;
using System.Collections.Generic;
using System.Security.Principal;
using System.Net;
using System.Linq;

namespace FGA.Reportes
{
    public partial class ReportView : System.Web.Mvc.ViewPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                RenderReportModels();
        }

        private void RenderReportModels()
        {
            try
            {
                if (Session["TipoReporte"] != null)
                {
                    ReportViewerRSFReports.Height = 1300;
                    var rptName = string.Empty;

                    switch (Session["TipoReporte"].ToString())
                    {
                        case "rpt_balance":
                            rptName = "Report_Balance";
                            break;
                        case "rpt_er":
                            rptName = "Report_ER";
                            break;
                        case "balanceCompleto":
                            rptName = "Report_Balance_ER";
                            break;
                        case "analisis_balance":
                            rptName = "Report_Balance_Analisis";
                            break;
                        case "analisis_er":
                            rptName = "Report_ER_Analisis";
                            break;
                        case "origen_aplicacion":
                            rptName = "Report_Origen_Aplic";
                            break;
                        case "origen_aplicacion_sf":
                            rptName = "Report_Origen_Aplic_SF";
                            break;
                        case "rpt_er_an_sf":
                            rptName = "Report_ER_AN_SF";
                            break;
                        case "rpt_er_sf":
                            rptName = "Report_ER_SF";
                            break;
                        case "rpt_balance_sf":
                            rptName = "Report_balance_SF";
                            break;
                        case "rpt_balance_an_sf":
                            rptName = "Report_balance_AN_SF";
                            break;
                        case "rpt_balance_proy":
                            rptName = "Report_Balance_Proy";
                            break;
                        case "rpt_er_proy":
                            rptName = "Report_ER_Proy";
                            break;
                        case "rpt_factura":
                            rptName = "Report_Factura";
                            break;
                        default:
                            break;
                    }

                    FGA_En_Linea.ParametrosService.ParametrosServiceClient param = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
                    var listParam = param.GetAll().ToList();
                    var ServidorReportes = listParam.Where(o => o.Llave == Utility.Utilitarios.Servidor_Reportes).Select(o => o.Valor).FirstOrDefault();
                    var CarpetaCorreos = listParam.Where(o => o.Llave == Utility.Utilitarios.Path_Reportes).Select(o => o.Valor).FirstOrDefault();
                    var Dominio = listParam.Where(o => o.Llave == Utility.Utilitarios.Dominio).Select(o => o.Valor).FirstOrDefault();
                    var UsuarioReportes = listParam.Where(o => o.Llave == Utility.Utilitarios.Usuario_Reportes).Select(o => o.Valor).FirstOrDefault();
                    var ContrasenaReportes = listParam.Where(o => o.Llave == Utility.Utilitarios.Clave_Dominio).Select(o => o.Valor).FirstOrDefault();

                    // confirm report properties (also setable in attributes)
                    ReportViewerRSFReports.ProcessingMode = ProcessingMode.Remote;
                    ReportViewerRSFReports.ZoomMode = ZoomMode.Percent;
                    ReportViewerRSFReports.ZoomPercent = 90;

                    // config variables
                    var reportServer = ServidorReportes;
                    var reportPath = "/" + CarpetaCorreos + "/";

                    // report setup
                    var serverReport = new ServerReport();
                    serverReport = ReportViewerRSFReports.ServerReport;
                    serverReport.ReportServerUrl = new Uri($@"http://{reportServer}/ReportServer");
                    serverReport.ReportPath = $@"{reportPath}{rptName}";

                    IReportServerCredentials irsc = new ReportServerCredentials(UsuarioReportes, ContrasenaReportes, Dominio);
                    serverReport.ReportServerCredentials = irsc;
                    ReportViewerRSFReports.ShowParameterPrompts = false;
                    ReportViewerRSFReports.AsyncRendering = true;

                    var parameters = new List<ReportParameter>();

                    if (rptName == "Report_Factura")
                    {
                        parameters.Add(new ReportParameter("Logo", this.Session["Logo"].ToString().Replace("https://www.ffc.co.cr/FFC", "http://10.171.1.26/ffc"), true));
                        parameters.Add(new ReportParameter("IDENTIDAD", Session["IdEntidad"].ToString()));
                        parameters.Add(new ReportParameter("TRIMESTRE", Session["Trimestre"].ToString()));
                        parameters.Add(new ReportParameter("ANNO", Session["Anno"].ToString()));
                    }
                    else
                    {
                        if (rptName == "Report_ER_Proy" || rptName == "Report_Balance_Proy")
                        {

                            DateTime feccorte = Utility.Utilitarios.ConvertirAFecha(this.Session["PeriodoI"].ToString());
                            //DateTime inicio = Session["Plazo"].ToString() == "1" ? feccorte : new DateTime(feccorte.Year, 1, 1);

                            parameters.Add(new ReportParameter("Logo", this.Session["Logo"].ToString().Replace("https://www.ffc.co.cr/FFC", "http://10.171.1.26/ffc"), true));
                            parameters.Add(new ReportParameter("IDENTIDAD", this.Session["IdEntidad"].ToString()));
                            parameters.Add(new ReportParameter("PERIODOINICIO", feccorte.ToString()));
                            parameters.Add(new ReportParameter("PERIODOCORTE", feccorte.ToString()));
                            parameters.Add(new ReportParameter("TIPOREPORTE", rptName == "Report_ER_Proy" ? "E" : "B"));
                            parameters.Add(new ReportParameter("TIPOPLAZO", Session["Plazo"].ToString()));
                        }
                        else
                        {

                            if (rptName == "Report_ER" || rptName == "Report_ER_Analisis")
                            {
                                parameters.Add(new ReportParameter("MENSUAL", this.Session["Mensual"].ToString()));
                            }

                            if (rptName == "Report_ER_AN_SF" || rptName == "Report_ER_SF" || rptName == "Report_balance_SF" || rptName == "Report_balance_AN_SF")
                            {
                                parameters.Add(new ReportParameter("periodo1", this.Session["Periodo1"].ToString()));
                            }
                            else
                            {
                                if (rptName != "Report_Origen_Aplic_SF")
                                {
                                    parameters.Add(new ReportParameter("Logo", this.Session["Logo"].ToString().Replace("https://www.ffc.co.cr/FFC", "http://10.171.1.26/ffc"), true));
                                    parameters.Add(new ReportParameter("IDENTIDAD", this.Session["IdEntidad"].ToString()));
                                }
                                else
                                {
                                    parameters.Add(new ReportParameter("IDENTIDAD", "SF"));
                                }
                                parameters.Add(new ReportParameter("PERIODO1", this.Session["Periodo1"].ToString()));
                                parameters.Add(new ReportParameter("PERIODO2", this.Session["Periodo2"].ToString()));
                                parameters.Add(new ReportParameter("PERIODO3", this.Session["Periodo3"].ToString()));
                            }
                        }
                    }

                    serverReport.SetParameters(parameters);
                    serverReport.Refresh();

                }
            }
            catch (Exception)
            {
            }
            finally
            {
            }
        }

        public class ReportServerCredentials : IReportServerCredentials
        {
            private string _userName;
            private string _password;
            private string _domain;

            public ReportServerCredentials(string userName, string password, string domain)
            {
                _userName = userName;
                _password = password;
                _domain = domain;
            }

            WindowsIdentity IReportServerCredentials.ImpersonationUser
            {
                get
                {
                    // Use default identity.
                    return null;
                }
            }

            ICredentials IReportServerCredentials.NetworkCredentials
            {
                get
                {
                    // Use default identity.
                    return new NetworkCredential(_userName, _password, _domain);
                }
            }

            public bool GetFormsCredentials(out Cookie authCookie, out string user, out string password, out string authority)
            {
                // Do not use forms credentials to authenticate.
                authCookie = null;
                user = password = authority = null;
                return false;
            }
        }
    }
}