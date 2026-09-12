using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Threading.Tasks;
using FGA.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FGA_WinService
{
    public class BCCRResponse
    {
        [JsonProperty("estado")]
        public bool Estado { get; set; }

        [JsonProperty("mensaje")]
        public string Mensaje { get; set; }

        [JsonProperty("datos")]
        public List<BCCRDatoIndicador> Datos { get; set; }
    }

    public class BCCRDatoIndicador
    {
        [JsonProperty("codigoIndicador")]
        public string CodigoIndicador { get; set; }

        [JsonProperty("nombreIndicador")]
        public string NombreIndicador { get; set; }

        [JsonProperty("series")]
        public List<BCCRSerie> Series { get; set; }
    }

    public class BCCRSerie
    {
        [JsonProperty("fecha")]
        public DateTime Fecha { get; set; }

        [JsonProperty("valorDatoPorPeriodo")]
        public decimal ValorDatoPorPeriodo { get; set; }

        [JsonProperty("valor")]
        private decimal ValorAlias { set { ValorDatoPorPeriodo = value; } }

        [JsonProperty("numValor")]
        private decimal NumValorAlias { set { ValorDatoPorPeriodo = value; } }

        [JsonProperty("monto")]
        private decimal MontoAlias { set { ValorDatoPorPeriodo = value; } }
    }

    public partial class Monitor : ServiceBase
    {
        private static readonly HttpClient httpClient;
        private readonly string urlBCCR = "https://apim.bccr.fi.cr/SDDE/api/Bccr.Ge.SDDE.Publico.Indicadores.API";

        static Monitor()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                System.Net.ServicePointManager.DefaultConnectionLimit = 20;

                var handler = new HttpClientHandler
                {
                    UseProxy = true,
                    UseDefaultCredentials = true,
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };

                httpClient = new HttpClient(handler);
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) FGA_WinService/1.0");
            }
            catch
            {
                httpClient = new HttpClient();
            }
        }

        private readonly String minutes = ConfigurationManager.AppSettings["Minutes"];
        private readonly String token = ConfigurationManager.AppSettings["Token"];

        private readonly String TC = ConfigurationManager.AppSettings["TC"];
        private readonly String Libor = ConfigurationManager.AppSettings["Libor"];
        private readonly String TBP = ConfigurationManager.AppSettings["TBP"];
        private readonly String ipcVariacion = ConfigurationManager.AppSettings["IPCVariacion"];
        private readonly String IPC = ConfigurationManager.AppSettings["IPC"];
       
        public Monitor()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            monitor.Interval = int.Parse(minutes) * 20 * 1000;
            monitor.Enabled = true;
            monitor.Start();
        }

        protected override void OnStop()
        {
            monitor.Enabled = false;
        }

        private void Notificar(DateTime periodo)
        {
            using (var db = new SIContext())
            {
                try
                {
                    var listParam = db.Parametros.AsNoTracking().ToList();
                    var ServidorCorreo = listParam.Where(o => o.Llave == FGA.Utility.Utilitarios.Servidor_Correo).Select(o => o.Valor).FirstOrDefault();
                    var CuentaCorreo = listParam.Where(o => o.Llave == FGA.Utility.Utilitarios.Direccion_Correo).Select(o => o.Valor).FirstOrDefault();
                    var PasswordCorreo = listParam.Where(o => o.Llave == FGA.Utility.Utilitarios.Contrasena_Correo).Select(o => o.Valor).FirstOrDefault();
                    var CorreoCarga = listParam.Where(o => o.Llave == FGA.Utility.Utilitarios.Correo_Error_XML).Select(i => i.Valor).FirstOrDefault();
                    List<Entidad> listaEntidades = db.Entidades.AsNoTracking().Where(o => o.Contacto != string.Empty && o.Activo == true).ToList();
                    string mensaje = string.Empty;

                    try
                    {

                        Calendario calendario = db.Calendarios.FirstOrDefault(o => o.Periodo == periodo);

                       
                        if (calendario != null)
                        {

                            mensaje = "Estimad@ usuario:<br /><br />" +
                                        "El Fondo de Fortalecimiento Cooperativo le informa que ya se encuentra habilitada la carga correspondiente al mes en curso." +
                                        "<br /><br />" +
                                        "Si ya realizó el envío, favor omitir este mensaje." +
                                        "<br /><br />" +
                                        "Si requiere asesoría o tiene alguna consulta, puede contactar a las analistas de riesgo:" +
                                        "<br /><br />" +
                                        "Cinthya Salazar <a href='mailto:csalazar@ffc.co.cr'>csalazar@ffc.co.cr</a>" +
                                        "<br />" +
                                        "Viviana Zumbado <a href='mailto:vzumbado@ffc.co.cr'>vzumbado@ffc.co.cr</a>" +
                                        "<br /><br />" +
                                        "También puede comunicarse al teléfono 2257-1111." +
                                        "<br /><br />";


                            if ((calendario.Enviado == false && calendario.DiaNotificacion == DateTime.Now.Day && DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 9) ||
                               (calendario.Enviado == true && calendario.Ind_Vencido == false && calendario.DiaLimite == DateTime.Now.Day && DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 9))
                            {
                                //List<Usuario> listaUsuarios = db.Usuarios.Where(o => o.Estado_Usuario_Id == FGA.Utility.Utilitarios.estadoActivo).ToList();
                                //string periodoCarga = FGA.Utility.Utilitarios.toUpperFirstLetter(periodo.ToString("MMMM-yyyy"));
                                
                                foreach (Entidad ent in listaEntidades)
                                {
                                    using (SP.SPClient sp = new SP.SPClient())
                                    {
                                        try
                                        {
                                            var fechaCierre = sp.FGA_Consultar_FechaCierre(ent.Id);
                                            if (fechaCierre <= periodo)
                                            {
                                                MailSend.Email.EnviarCorreoImagenes("Notificación de Carga", mensaje,
                                                ent.Contacto, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            RegistrarError("Notificar_SP_FechaCierre", ex);
                                        }
                                    }
                                }
                               
                                if (calendario.DiaLimite == DateTime.Now.Day)
                                {
                                    db.Calendarios.Remove(calendario);
                                    db.SaveChanges();
                                }
                                else {
                                    calendario.Enviado = true;
                                    db.Entry(calendario).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }                            
                        }
                    }
                    catch (System.Net.Mail.SmtpException smtpExc)
                    {
                        db.Logs.Add(new Log
                        {
                            Action = "Correo",
                            Controller = "Correo",
                            Fecha = DateTime.Now,
                            IdUsuario_Id = 11,
                            Mensaje = smtpExc.StatusCode.ToString()
                        });
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        RegistrarError("Notificar_Calendario", ex);
                    }

                    try
                    {
                        DateTime primerDia = new DateTime(periodo.Year, periodo.Month, 1);
                        Calendario reporte = db.Calendarios.Where(o => o.Periodo >= primerDia && o.Enviado == true && o.Ind_Vencido == true).FirstOrDefault();

                        if (reporte != null && reporte.DiaLimite + 1 == DateTime.Now.Day && DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 9)
                        {
                            listaEntidades = db.Entidades.Where(o => o.Activo == true && o.Id != "0").ToList();
                            string periodoCarga = FGA.Utility.Utilitarios.toUpperFirstLetter(periodo.ToString("MMMM-yyyy"));

                            foreach (Entidad ent in listaEntidades)
                            {
                                using (SP.SPClient sp = new SP.SPClient())
                                {
                                    try
                                    {
                                        var fechaCierre = sp.FGA_Consultar_FechaCierre(ent.Id);
                                        if (fechaCierre <= periodo)
                                        {
                                            mensaje = "Estimad@ usuario: FFC <br /><br /> La entidad " + ent.Nombre + ", está pendiente de completar la carga correspondiente al periodo "
                                             + FGA.Utility.Utilitarios.toUpperFirstLetter(primerDia.ToString("MMMM yyyy")) + ".";

                                            MailSend.Email.EnviarCorreoImagenes("Notificación de carga pendiente", mensaje, CorreoCarga, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        RegistrarError("Notificar_PendienteEntidad", ex);
                                    }
                                }
                            }

                            db.Calendarios.Remove(reporte);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarError("Notificar_Pendientes", ex);
                    }

                    try
                    {
                        Notificaciones notificacion = db.Notificaciones.Where(o => o.Enviado == false).FirstOrDefault();
                        if (notificacion != null)
                        {
                            listaEntidades = db.Entidades.Where(o => o.Activo == true).ToList();

                            foreach (Entidad ent in listaEntidades)
                            {
                                try
                                {
                                    MailSend.Email.EnviarCorreoImagenes(notificacion.Asunto, notificacion.Texto, ent.Contacto, ServidorCorreo, CuentaCorreo, CuentaCorreo, PasswordCorreo);
                                }
                                catch (Exception ex)
                                {
                                    RegistrarError("Notificar_EnvioIndividual", ex);
                                }
                            }

                            notificacion.Enviado = true;
                            db.Entry(notificacion).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarError("Notificar_General", ex);
                    }
                }
                catch (Exception ex)
                {
                    RegistrarError("Notificar_Raiz", ex);
                }
            }
        }

        private void RegistrarError(string accion, Exception ex)
        {
            string mensajeError = $"[Monitor Error - {accion}] {ex.Message}" + (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "");
            try
            {
                using (var db = new SIContext())
                {
                    db.Logs.Add(new Log
                    {
                        Action = accion,
                        Controller = "BCCR_Monitor",
                        Fecha = DateTime.Now,
                        IdUsuario_Id = 11,
                        Mensaje = mensajeError.Length > 4000 ? mensajeError.Substring(0, 4000) : mensajeError
                    });
                    db.SaveChanges();
                }
            }
            catch
            {
                try
                {
                    System.Diagnostics.EventLog.WriteEntry("Application", mensajeError, System.Diagnostics.EventLogEntryType.Error);
                }
                catch { }
            }
        }

        private List<IndicadorEconomico> ObtenerIndicadoresEconomicos(string indicador, DateTime fechaInicio, DateTime fechaFin)
        {
            return Task.Run(() => ObtenerIndicadoresEconomicosAsync(indicador, fechaInicio, fechaFin)).GetAwaiter().GetResult();
        }

        private async Task<List<IndicadorEconomico>> ObtenerIndicadoresEconomicosAsync(
    string indicador,
    DateTime fechaInicio,
    DateTime fechaFin)
        {
            if (string.IsNullOrWhiteSpace(indicador))
                return new List<IndicadorEconomico>();

            if (indicador.Contains(","))
                indicador = indicador.Split(',').Last().Trim();

            // NO sobreescribir fechaInicio
            fechaInicio = fechaFin;

            if (fechaFin > DateTime.Today)
                fechaFin = DateTime.Today;

            if (fechaInicio > fechaFin)
                fechaInicio = fechaFin;

            string fechaInicioStr = fechaInicio.ToString(
                "yyyy/MM/dd",
                System.Globalization.CultureInfo.InvariantCulture);

            string fechaFinStr = fechaFin.ToString(
                "yyyy/MM/dd",
                System.Globalization.CultureInfo.InvariantCulture);

            string url =
                $"{urlBCCR}/indicadoresEconomicos/{indicador}/series" +
                $"?fechaInicio={Uri.EscapeDataString(fechaInicioStr)}" +
                $"&fechaFin={Uri.EscapeDataString(fechaFinStr)}" +
                $"&idioma=ES";

            const int maxRetries = 4;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", token.Trim());
                        }

                        request.Headers.Accept.Clear();
                        request.Headers.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response =
                            await httpClient.SendAsync(request).ConfigureAwait(false);

                        string json =
                            await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (!response.IsSuccessStatusCode)
                        {
                            int statusCode = (int)response.StatusCode;

                            if (attempt < maxRetries &&
                                (statusCode >= 500 || statusCode == 429))
                            {
                                await Task.Delay(2000 * attempt).ConfigureAwait(false);
                                continue;
                            }

                            throw new Exception(
                                $"Error BCCR {statusCode} [URL: {url}]: {json}");
                        }

                        if (string.IsNullOrWhiteSpace(json))
                            return new List<IndicadorEconomico>();

                        var resp =
                            JsonConvert.DeserializeObject<BCCRResponse>(json);

                        if (resp?.Datos == null)
                            return new List<IndicadorEconomico>();

                        return resp.Datos
                            .Where(x => x.Series != null)
                            .SelectMany(x => x.Series)
                            .Where(x => x.Fecha != DateTime.MinValue)
                            .Select(x => new IndicadorEconomico
                            {
                                fecha = x.Fecha,
                                monto = x.ValorDatoPorPeriodo
                            })
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
                    if (attempt >= maxRetries)
                    {
                        RegistrarError(
                            $"ObtenerIndicador_{indicador}",
                            ex);

                        return new List<IndicadorEconomico>();
                    }

                    await Task.Delay(1000 * attempt).ConfigureAwait(false);
                }
            }

            return new List<IndicadorEconomico>();
        }
        private void ProcesarTC(DateTime fechaFinal)
        {
            using (var db = new SIContext())
            {
                Hist_TipoCambio ult = db.Hist_TipoCambio.OrderByDescending(o => o.Fecha).FirstOrDefault();
                Hist_TipoCambio ult12M;
                List<Hist_TipoCambio> lista12M;
                Decimal monto = 0;
                Decimal monto12M = 1;
                Double montoP12M = 1;
                Double fluctuacion = 0;
                Double crecimiento = 0;
                DateTime fechaInicial = new DateTime(2026, 01, 01);
                DateTime fecha12M = DateTime.Now;

                if (ult != null)
                {
                    monto = ult.Monto;
                    fechaInicial = ult.Fecha;
                    if (fechaInicial < new DateTime(2026, 01, 01))
                        fechaInicial = new DateTime(2026, 01, 01);
                }

                DateTime inicioMesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                while (fechaInicial <= fechaFinal)
                {
                    DateTime primerDia = new DateTime(fechaInicial.Year, fechaInicial.Month, 1);
                    DateTime ultimoDia = new DateTime(fechaInicial.Year, fechaInicial.Month, DateTime.DaysInMonth(fechaInicial.Year, fechaInicial.Month));

                    var registroExistente = db.Hist_TipoCambio.FirstOrDefault(o => o.Fecha == ultimoDia);

                    if (registroExistente != null && registroExistente.Monto > 0 && primerDia < inicioMesActual)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    List<IndicadorEconomico> listaValores = ObtenerIndicadoresEconomicos(TC, primerDia, ultimoDia);

                    if (listaValores.Count() == 0)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    monto = listaValores.OrderByDescending(o => o.fecha).FirstOrDefault().monto;

                    fecha12M = ultimoDia.AddMonths(-12);
                    ult12M = db.Hist_TipoCambio.FirstOrDefault(o => o.Fecha == fecha12M);

                    if (ult12M != null)
                        monto12M = ult12M.Monto;

                    if (monto12M != 1)
                    {
                        crecimiento = Convert.ToDouble(monto / monto12M);
                        try
                        {
                            crecimiento = Math.Log(crecimiento);
                        }
                        catch (Exception)
                        {
                            crecimiento = 0;
                        }

                        try
                        {
                            montoP12M = (Convert.ToDouble(db.Hist_TipoCambio.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).Sum(o => (decimal?)o.Desviacion) ?? 0) + crecimiento) / 12;
                            lista12M = db.Hist_TipoCambio.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).ToList();
                            foreach (Hist_TipoCambio detalle in lista12M)
                                fluctuacion += (Convert.ToDouble(detalle.Desviacion) - montoP12M) * (Convert.ToDouble(detalle.Desviacion) - montoP12M);

                            fluctuacion += (crecimiento - montoP12M) * (crecimiento - montoP12M);
                            fluctuacion = fluctuacion / 11;
                            fluctuacion = Math.Sqrt(fluctuacion);
                            fluctuacion = fluctuacion * 2.33 * Convert.ToDouble(monto);
                        }
                        catch (Exception)
                        {
                            fluctuacion = 0;
                        }
                    }

                    if (registroExistente != null)
                    {
                        registroExistente.Monto = monto;
                        registroExistente.Desviacion = Convert.ToDecimal(crecimiento);
                        registroExistente.Fluctuacion = Convert.ToDecimal(fluctuacion);
                        db.Entry(registroExistente).State = EntityState.Modified;
                    }
                    else
                    {
                        db.Hist_TipoCambio.Add(new Hist_TipoCambio
                        {
                            Fecha = ultimoDia,
                            Monto = monto,
                            Desviacion = Convert.ToDecimal(crecimiento),
                            Fluctuacion = Convert.ToDecimal(fluctuacion)
                        });
                    }

                    db.SaveChanges();
                    monto12M = 1;
                    crecimiento = 0;
                    fluctuacion = 0;
                    fechaInicial = fechaInicial.AddMonths(1);
                }
            }
        }

        private void ProcesarLibor(DateTime fechaFinal)
        {
            using (var db = new SIContext())
            {
                Hist_Libor ult = db.Hist_Libor.OrderByDescending(o => o.Fecha).FirstOrDefault();
                Hist_Libor ult12M;
                List<Hist_Libor> lista12M;
                Decimal monto = 0;
                Decimal monto12M = 1;
                Double montoP12M = 1;
                Double fluctuacion = 0;
                Double crecimiento = 0;
                DateTime fechaInicial = new DateTime(2026, 01, 01);
                DateTime fecha1M = DateTime.Now;
                DateTime fecha12M = DateTime.Now;

                if (ult != null)
                {
                    monto = ult.Monto;
                    fechaInicial = ult.Fecha;
                    if (fechaInicial < new DateTime(2026, 01, 01))
                        fechaInicial = new DateTime(2026, 01, 01);
                }

                DateTime inicioMesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                while (fechaInicial <= fechaFinal)
                {
                    DateTime primerDia = new DateTime(fechaInicial.Year, fechaInicial.Month, 1);
                    DateTime ultimoDia = new DateTime(fechaInicial.Year, fechaInicial.Month, DateTime.DaysInMonth(fechaInicial.Year, fechaInicial.Month));

                    var registroExistente = db.Hist_Libor.FirstOrDefault(o => o.Fecha == ultimoDia);

                    if (registroExistente != null && registroExistente.Monto > 0 && primerDia < inicioMesActual)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    List<IndicadorEconomico> listaValores = ObtenerIndicadoresEconomicos(Libor, primerDia, ultimoDia);
                    fluctuacion = 0;

                    if (listaValores.Count() == 0)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    monto = listaValores.OrderByDescending(o => o.fecha).FirstOrDefault().monto;

                    fecha1M = new DateTime(ultimoDia.AddMonths(-1).Year, ultimoDia.AddMonths(-1).Month, DateTime.DaysInMonth(ultimoDia.AddMonths(-1).Year, ultimoDia.AddMonths(-1).Month));
                    fecha12M = new DateTime(ultimoDia.AddMonths(-11).Year, ultimoDia.AddMonths(-11).Month, DateTime.DaysInMonth(ultimoDia.AddMonths(-11).Year, ultimoDia.AddMonths(-11).Month));

                    ult12M = db.Hist_Libor.FirstOrDefault(o => o.Fecha == fecha1M);

                    if (ult12M != null)
                        monto12M = ult12M.Monto;

                    if (monto12M != 1)
                    {
                        crecimiento = Convert.ToDouble(monto / monto12M);
                        try
                        {
                            crecimiento = Math.Log(crecimiento);
                        }
                        catch (Exception)
                        {
                            crecimiento = 0;
                        }

                        try
                        {
                            montoP12M = (Convert.ToDouble(db.Hist_Libor.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).Sum(o => (decimal?)o.Desviacion) ?? 0) + crecimiento) / 11;
                            lista12M = db.Hist_Libor.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).ToList();
                            foreach (Hist_Libor detalle in lista12M)
                                fluctuacion += (Convert.ToDouble(detalle.Desviacion) - montoP12M) * (Convert.ToDouble(detalle.Desviacion) - montoP12M);

                            fluctuacion += (crecimiento - montoP12M) * (crecimiento - montoP12M);
                            fluctuacion = fluctuacion * 12;
                            fluctuacion = Math.Sqrt(fluctuacion / 10);
                            fluctuacion = fluctuacion * 2.33 * Convert.ToDouble(monto);
                        }
                        catch (Exception)
                        {
                            fluctuacion = 0;
                        }
                    }

                    if (registroExistente != null)
                    {
                        registroExistente.Monto = monto;
                        registroExistente.Desviacion = Convert.ToDecimal(crecimiento);
                        registroExistente.Fluctuacion = Convert.ToDecimal(fluctuacion / 100);
                        db.Entry(registroExistente).State = EntityState.Modified;
                    }
                    else
                    {
                        db.Hist_Libor.Add(new Hist_Libor
                        {
                            Fecha = ultimoDia,
                            Monto = monto,
                            Desviacion = Convert.ToDecimal(crecimiento),
                            Fluctuacion = Convert.ToDecimal(fluctuacion / 100)
                        });
                    }

                    db.SaveChanges();
                    monto12M = 1;
                    crecimiento = 0;
                    fluctuacion = 0;
                    fechaInicial = fechaInicial.AddMonths(1);
                }
            }
        }

        private void ProcesarTBP(DateTime fechaFinal)
        {
            using (var db = new SIContext())
            {
                Hist_TBP ult = db.Hist_TBP.OrderByDescending(o => o.Fecha).FirstOrDefault();
                Hist_TBP ult12M;
                List<Hist_TBP> lista12M;
                Decimal monto = 0;
                Decimal monto12M = 1;
                Double montoP12M = 1;
                Double fluctuacion = 0;
                Double crecimiento = 0;
                DateTime fechaInicial = new DateTime(2026, 01, 01);
                DateTime fecha1M = DateTime.Now;
                DateTime fecha12M = DateTime.Now;

                if (ult != null)
                {
                    monto = ult.Monto;
                    fechaInicial = ult.Fecha;
                    if (fechaInicial < new DateTime(2026, 01, 01))
                        fechaInicial = new DateTime(2026, 01, 01);
                }

                DateTime inicioMesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                while (fechaInicial <= fechaFinal)
                {
                    DateTime primerDia = new DateTime(fechaInicial.Year, fechaInicial.Month, 1);
                    DateTime ultimoDia = new DateTime(fechaInicial.Year, fechaInicial.Month, DateTime.DaysInMonth(fechaInicial.Year, fechaInicial.Month));

                    var registroExistente = db.Hist_TBP.FirstOrDefault(o => o.Fecha == ultimoDia);

                    if (registroExistente != null && registroExistente.Monto > 0 && primerDia < inicioMesActual)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    List<IndicadorEconomico> listaValores = ObtenerIndicadoresEconomicos(TBP, primerDia, ultimoDia);
                    fluctuacion = 0;

                    if (listaValores.Count() == 0)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    monto = listaValores.OrderByDescending(o => o.fecha).FirstOrDefault().monto;

                    fecha1M = new DateTime(ultimoDia.AddMonths(-1).Year, ultimoDia.AddMonths(-1).Month, DateTime.DaysInMonth(ultimoDia.AddMonths(-1).Year, ultimoDia.AddMonths(-1).Month));
                    fecha12M = new DateTime(ultimoDia.AddMonths(-11).Year, ultimoDia.AddMonths(-11).Month, DateTime.DaysInMonth(ultimoDia.AddMonths(-11).Year, ultimoDia.AddMonths(-11).Month));
                    ult12M = db.Hist_TBP.FirstOrDefault(o => o.Fecha == fecha1M);

                    if (ult12M != null)
                        monto12M = ult12M.Monto;

                    if (monto12M != 1)
                    {
                        crecimiento = Convert.ToDouble(monto / monto12M);
                        try
                        {
                            crecimiento = Math.Log(crecimiento);
                        }
                        catch (Exception)
                        {
                            crecimiento = 0;
                        }

                        try
                        {
                            montoP12M = (Convert.ToDouble(db.Hist_TBP.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).Sum(o => (decimal?)o.Desviacion) ?? 0) + crecimiento) / 11;
                            lista12M = db.Hist_TBP.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).ToList();
                            foreach (Hist_TBP detalle in lista12M)
                                fluctuacion += (Convert.ToDouble(detalle.Desviacion) - montoP12M) * (Convert.ToDouble(detalle.Desviacion) - montoP12M);

                            fluctuacion += (crecimiento - montoP12M) * (crecimiento - montoP12M);
                            fluctuacion = fluctuacion * 1.2;
                            fluctuacion = Math.Sqrt(fluctuacion);
                            fluctuacion = fluctuacion * 2.33 * Convert.ToDouble(monto);
                        }
                        catch (Exception)
                        {
                            fluctuacion = 0;
                        }
                    }

                    if (registroExistente != null)
                    {
                        registroExistente.Monto = monto;
                        registroExistente.Desviacion = Convert.ToDecimal(crecimiento);
                        registroExistente.Fluctuacion = Convert.ToDecimal(fluctuacion / 100);
                        db.Entry(registroExistente).State = EntityState.Modified;
                    }
                    else
                    {
                        db.Hist_TBP.Add(new Hist_TBP
                        {
                            Fecha = ultimoDia,
                            Monto = monto,
                            Desviacion = Convert.ToDecimal(crecimiento),
                            Fluctuacion = Convert.ToDecimal(fluctuacion / 100)
                        });
                    }

                    db.SaveChanges();
                    monto12M = 1;
                    crecimiento = 0;
                    fluctuacion = 0;
                    fechaInicial = fechaInicial.AddMonths(1);
                }
            }
        }

        private void ProcesarIPC(DateTime fechaFinal)
        {
            using (var db = new SIContext())
            {
                Hist_IPC ult = db.Hist_IPC.OrderByDescending(o => o.Fecha).FirstOrDefault();
                decimal monto = 0;
                decimal variacion = 0;
                DateTime fechaInicial = new DateTime(2016, 01, 01);
                Hist_TBP ult12M;
                List<Hist_TBP> lista12M;
                decimal monto12M = 1;
                double montoP12M = 1;
                double fluctuacion = 0;
                double crecimiento = 0;
                DateTime fecha1M = DateTime.Now;
                DateTime fecha12M = DateTime.Now;

                if (ult != null)
                {
                    monto = ult.Monto;
                    fechaInicial = ult.Fecha.AddMonths(-6);
                    if (fechaInicial < new DateTime(2016, 01, 01))
                        fechaInicial = new DateTime(2016, 01, 01);
                }

                DateTime inicioMesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                while (fechaInicial <= fechaFinal)
                {
                    DateTime primerDia = new DateTime(fechaInicial.Year, fechaInicial.Month, 1);
                    DateTime ultimoDia = new DateTime(fechaInicial.Year, fechaInicial.Month, DateTime.DaysInMonth(fechaInicial.Year, fechaInicial.Month));

                    var registroExistente = db.Hist_IPC.FirstOrDefault(o => o.Fecha == ultimoDia);

                    if (registroExistente != null && registroExistente.Monto > 0 && primerDia < inicioMesActual)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    List<IndicadorEconomico> listaValores = ObtenerIndicadoresEconomicos(IPC, primerDia, ultimoDia);
                    fluctuacion = 0;

                    if (listaValores.Count() == 0)
                    {
                        fechaInicial = fechaInicial.AddMonths(1);
                        continue;
                    }

                    monto = listaValores.OrderByDescending(o => o.fecha).FirstOrDefault().monto;

                    fecha1M = new DateTime(ultimoDia.AddMonths(-1).Year, ultimoDia.AddMonths(-1).Month, DateTime.DaysInMonth(ultimoDia.AddMonths(-1).Year, ultimoDia.AddMonths(-1).Month));
                    fecha12M = new DateTime(ultimoDia.AddMonths(-11).Year, ultimoDia.AddMonths(-11).Month, DateTime.DaysInMonth(ultimoDia.AddMonths(-11).Year, ultimoDia.AddMonths(-11).Month));
                    ult12M = db.Hist_TBP.FirstOrDefault(o => o.Fecha == fecha1M);

                    if (ult12M != null)
                        monto12M = ult12M.Monto;

                    if (monto12M != 1)
                    {
                        crecimiento = Convert.ToDouble(monto / monto12M);
                        try
                        {
                            crecimiento = Math.Log(crecimiento);
                        }
                        catch (Exception)
                        {
                            crecimiento = 0;
                        }

                        try
                        {
                            montoP12M = (Convert.ToDouble(db.Hist_TBP.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).Sum(o => (decimal?)o.Desviacion) ?? 0) + crecimiento) / 11;
                            lista12M = db.Hist_TBP.Where(o => o.Fecha > fecha12M && o.Fecha < ultimoDia).ToList();
                            foreach (Hist_TBP detalle in lista12M)
                                fluctuacion += (Convert.ToDouble(detalle.Desviacion) - montoP12M) * (Convert.ToDouble(detalle.Desviacion) - montoP12M);

                            fluctuacion += (crecimiento - montoP12M) * (crecimiento - montoP12M);
                            fluctuacion = fluctuacion * 1.2;
                            fluctuacion = Math.Sqrt(fluctuacion);
                            fluctuacion = fluctuacion * 2.33 * Convert.ToDouble(monto);
                        }
                        catch (Exception)
                        {
                            fluctuacion = 0;
                        }
                    }

                    listaValores = ObtenerIndicadoresEconomicos(ipcVariacion, primerDia, ultimoDia);

                    if (listaValores.Count() > 0)
                        variacion = listaValores.OrderByDescending(o => o.fecha).FirstOrDefault().monto;

                    if (registroExistente != null)
                    {
                        registroExistente.Monto = monto;
                        registroExistente.Desviacion = Convert.ToDecimal(crecimiento);
                        registroExistente.Fluctuacion = Convert.ToDecimal(fluctuacion / 100);
                        registroExistente.VariacionInter = variacion;
                        db.Entry(registroExistente).State = EntityState.Modified;
                    }
                    else
                    {
                        db.Hist_IPC.Add(new Hist_IPC
                        {
                            Fecha = ultimoDia,
                            Monto = monto,
                            Desviacion = Convert.ToDecimal(crecimiento),
                            Fluctuacion = Convert.ToDecimal(fluctuacion / 100),
                            VariacionInter = variacion
                        });
                    }

                    db.SaveChanges();
                    monto12M = 1;
                    crecimiento = 0;
                    fluctuacion = 0;
                    fechaInicial = fechaInicial.AddMonths(1);
                }
            }
        }

        private void ProcesarIndicadorBCCR(BCCR_TipoIndicador tipo, DateTime fechaFinal)
        {
            using (var db = new SIContext())
            {
                var ult = db.BCCR_Indicadores
                        .Where(x => x.IdIndicador == tipo.Id)
                        .OrderByDescending(x => x.Fecha)
                        .FirstOrDefault();

                DateTime fechaInicial = (ult is null ? new DateTime(2016, 01, 01) : ult.Fecha.Value);
                if (fechaInicial < new DateTime(2016, 01, 01))
                    fechaInicial = new DateTime(2016, 01, 01);

                DateTime inicioMesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                while (fechaInicial <= fechaFinal)
                {
                    try
                    {
                        DateTime primerDia = new DateTime(fechaInicial.Year, fechaInicial.Month, 1);
                        DateTime ultimoDia = new DateTime(fechaInicial.Year, fechaInicial.Month, DateTime.DaysInMonth(fechaInicial.Year, fechaInicial.Month));

                        var existente = db.BCCR_Indicadores.FirstOrDefault(x => x.IdIndicador == tipo.Id && x.Fecha == ultimoDia);
                        if (existente != null && existente.Monto.HasValue && existente.Monto.Value > 0 && primerDia < inicioMesActual)
                        {
                            fechaInicial = fechaInicial.AddMonths(1);
                            continue;
                        }

                        List<IndicadorEconomico> listaValores = ObtenerIndicadoresEconomicos(tipo.Id.ToString(), primerDia, ultimoDia);

                        if (listaValores.Count > 0)
                        {
                            var valor = listaValores.OrderByDescending(v => v.fecha).First();

                            if (existente != null)
                            {
                                existente.Monto = valor.monto;
                                db.Entry(existente).State = EntityState.Modified;
                            }
                            else
                            {
                                var nuevo = new BCCR_Indicadores
                                {
                                    IdIndicador = tipo.Id,
                                    Fecha = ultimoDia,
                                    Monto = valor.monto
                                };
                                db.BCCR_Indicadores.Add(nuevo);
                            }
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarError($"ProcesarIndicadorBCCR_{tipo.Id}", ex);
                    }
                    fechaInicial = fechaInicial.AddMonths(1);
                }
            }
        }

        private void Monitor_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.monitor.Stop();
            DateTime ahora = DateTime.Now;
            DateTime periodoNotif = ahora.AddMonths(-1);

            DateTime fechaFinalProceso = new DateTime(periodoNotif.Year, periodoNotif.Month, DateTime.DaysInMonth(periodoNotif.Year, periodoNotif.Month));

            try
            {
                try
                {
                    this.ProcesarTC(fechaFinalProceso);
                    this.ProcesarLibor(fechaFinalProceso);
                    this.ProcesarTBP(fechaFinalProceso);
                    this.ProcesarIPC(fechaFinalProceso);
                }
                catch (Exception ex)
                {
                    RegistrarError("ProcesarIndicadores", ex);
                }

               
                DateTime dateNotif = new DateTime(periodoNotif.Year, periodoNotif.Month, 1);

                try
                {
                    this.Notificar(dateNotif);
                }
                catch (Exception ex)
                {
                    RegistrarError("Notificar", ex);
                }
            }
            catch (Exception ex)
            {
                RegistrarError("Monitor_Tick", ex);
            }
            finally
            {
                this.monitor.Start();
            }

            try
            {
                using (var db = new SIContext())
                {
                    var listaIndicadores = db.BCCR_TipoIndicador.ToList();
                    foreach (var indicador in listaIndicadores)
                        this.ProcesarIndicadorBCCR(indicador, fechaFinalProceso);
                }
            }
            catch (Exception ex)
            {
                RegistrarError("ProcesarIndicadoresGenericos", ex);
            }
        }
    }
}