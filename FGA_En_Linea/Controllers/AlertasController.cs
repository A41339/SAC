using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Entities.Entities.Procedures;
using FGA.Models;
using FGA.Utility;

namespace FGA.Controllers
{
    public class AlertasController : BaseController
    {
        private readonly CultureInfo _crCulture = FGAConstants.CulturaCR;

        /// <summary>
        /// Vista principal: Bandeja / Centro de Notificaciones y Alertas Financieras
        /// </summary>
        public ActionResult Index(string entidad, string tipoAlerta, bool? soloNoLeidas)
        {
            try
            {
                string idEntidad = ResolveCurrentEntity(entidad, out bool esAdmin);
                var idEntidadUsuario = Env.GetUserInfo("identidad");

                ObtenerUmbralesEfectivos(idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado);
                ViewBag.UmbralPorc = umbralPorc;
                ViewBag.UmbralMonto = umbralMonto;
                ViewBag.ModoMonitoreo = modoMonitoreo;
                ViewBag.EsPersonalizado = esPersonalizado;
                ViewBag.EsAdmin = esAdmin;
                int idEntParsed = -1;
                int.TryParse(idEntidadUsuario, out idEntParsed);
                ViewBag.IdEntidadUsuario = idEntParsed;

                FGA_Obtener_Alertas_Financieras_Result[] rawAlertas = null;
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    // Si no existen alertas, intentar disparar una evaluación inicial del período actual
                    rawAlertas = spClient.FGA_Obtener_Alertas_Financieras(idEntidad, false, 100);
                    if (rawAlertas == null || rawAlertas.Length == 0)
                    {
                        try
                        {
                            rawAlertas = spClient.FGA_Generar_Alertas_Variacion_Financiera(idEntidad, null, "Acumulado", "Interanual");
                        }
                        catch
                        {
                            rawAlertas = new FGA_Obtener_Alertas_Financieras_Result[0];
                        }
                    }
                }
                finally
                {
                    spClient.SafeClose();
                }

                // Filtrar estrictamente por los umbrales vigentes configurados en Parámetros
                    var todos = (rawAlertas ?? new FGA_Obtener_Alertas_Financieras_Result[0])
                        .Where(a => Math.Abs(a.VariacionPorcentaje) >= umbralPorc && Math.Abs(a.VariacionMonto) >= umbralMonto)
                        .ToList();

                    var listAlertas = todos.ToList();

                    // Aplicar filtros de vista
                    if (!string.IsNullOrEmpty(tipoAlerta) && !tipoAlerta.Equals("TODAS", StringComparison.OrdinalIgnoreCase))
                    {
                        listAlertas = listAlertas.Where(a => a.TipoAlerta != null && a.TipoAlerta.Equals(tipoAlerta, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    if (soloNoLeidas.HasValue && soloNoLeidas.Value)
                    {
                        listAlertas = listAlertas.Where(a => !a.Leido).ToList();
                    }

                    ViewBag.EntidadSeleccionada = idEntidad;
                    ViewBag.TipoAlertaSeleccionado = tipoAlerta ?? "TODAS";
                    ViewBag.SoloNoLeidas = soloNoLeidas ?? false;

                    // Estadísticas para las tarjetas KPI superiores
                    ViewBag.TotalAlertas = todos.Count;
                    ViewBag.TotalNoLeidas = todos.Count(a => !a.Leido);
                    ViewBag.TotalCriticas = todos.Count(a => a.TipoAlerta != null && a.TipoAlerta.Equals("CRITICA", StringComparison.OrdinalIgnoreCase));
                    ViewBag.TotalAdvertencias = todos.Count(a => a.TipoAlerta != null && a.TipoAlerta.Equals("ADVERTENCIA", StringComparison.OrdinalIgnoreCase));

                    // Cargar entidades para el selector mediante helper centralizado con caché
                    ViewBag.EntidadesList = GetEntidadesCombo();

                    return View(listAlertas);
            }
            catch (Exception ex)
            {
                ViewBag.TotalAlertas = 0;
                ViewBag.TotalNoLeidas = 0;
                ViewBag.TotalCriticas = 0;
                ViewBag.TotalAdvertencias = 0;
                ViewBag.EntidadesList = new List<FGA.Models.Entidad>();
                ViewBag.EsAdmin = true;
                ViewBag.IdEntidadUsuario = -1;
                ViewBag.EsPersonalizado = false;
                ViewBag.ModoMonitoreo = FGAConstants.Alertas.ModoTodas;
                ViewBag.UmbralPorc = FGAConstants.Alertas.UmbralVariacionPorcGlobal;
                ViewBag.UmbralMonto = FGAConstants.Alertas.UmbralVariacionMontoGlobal;
                ViewBag.ErrorMessage = "Error al cargar las alertas financieras: " + ex.Message;
                return View(new List<FGA_Obtener_Alertas_Financieras_Result>());
            }
        }

        /// <summary>
        /// Endpoint AJAX para la campana del Navbar: retorna conteo de no leídas y las 6 alertas más recientes
        /// </summary>
        [HttpGet]
        public ActionResult GetResumenNavbar()
        {
            try
            {
                string idEntidad = Session["IdEntidad"] != null ? Session["IdEntidad"].ToString() : Env.GetUserInfo("identidad");
                if (string.IsNullOrEmpty(idEntidad)) idEntidad = "-1";

                ObtenerUmbralesConfigurados(out decimal umbralPorc, out decimal umbralMonto);

                using (var spClient = new FGA_En_Linea.SPService.SPClient())
                {
                    var rawAlertas = spClient.FGA_Obtener_Alertas_Financieras(idEntidad, false, 50);
                    if (rawAlertas == null || rawAlertas.Length == 0)
                    {
                        // Intentar autogenerar para el período actual si aún no hay registros
                        try
                        {
                            rawAlertas = spClient.FGA_Generar_Alertas_Variacion_Financiera(idEntidad, null, "Acumulado", "Interanual");
                        }
                        catch
                        {
                            rawAlertas = new FGA_Obtener_Alertas_Financieras_Result[0];
                        }
                    }

                    var list = (rawAlertas ?? new FGA_Obtener_Alertas_Financieras_Result[0])
                        .Where(a => Math.Abs(a.VariacionPorcentaje) >= umbralPorc && Math.Abs(a.VariacionMonto) >= umbralMonto)
                        .ToList();
                    int noLeidas = list.Count(a => !a.Leido);

                    var items = list.Take(6).Select(a => new
                    {
                        id = a.Id,
                        cuenta = a.Cuenta,
                        nombreCuenta = a.NombreCuenta,
                        tipo = a.TipoAlerta,
                        titulo = a.Titulo,
                        mensaje = a.Mensaje,
                        leido = a.Leido,
                        montoFormato = FormatearMontoColones(a.VariacionMonto),
                        porcFormato = (a.VariacionPorcentaje >= 0 ? "+" : "") + a.VariacionPorcentaje.ToString("N1", _crCulture) + "%",
                        esPositivo = a.VariacionMonto >= 0,
                        tiempoRelativo = ObtenerTiempoRelativo(a.FechaGeneracion),
                        entidad = a.NombreEntidad
                    }).ToList();

                    return Json(new
                    {
                        success = true,
                        totalNoLeidas = noLeidas,
                        total = list.Count,
                        alertas = items
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Marca una alerta individual como leída
        /// </summary>
        [HttpPost]
        public ActionResult MarcarLeida(int id)
        {
            try
            {
                string usuario = Env.GetUserInfo("name") ?? "SYSTEM";
                using (var spClient = new FGA_En_Linea.SPService.SPClient())
                {
                    int res = spClient.FGA_Marcar_Alerta_Leida(id, null, usuario);
                    return Json(new { success = true, afectados = res });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Marca todas las alertas de la entidad actual como leídas
        /// </summary>
        [HttpPost]
        public ActionResult MarcarTodasLeidas()
        {
            try
            {
                string idEntidad = Session["IdEntidad"] != null ? Session["IdEntidad"].ToString() : Env.GetUserInfo("identidad");
                if (string.IsNullOrEmpty(idEntidad)) idEntidad = "-1";
                string usuario = Env.GetUserInfo("name") ?? "SYSTEM";

                using (var spClient = new FGA_En_Linea.SPService.SPClient())
                {
                    int res = spClient.FGA_Marcar_Alerta_Leida(null, idEntidad, usuario);
                    return Json(new { success = true, afectados = res });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Dispara la detección y generación manual de alertas para un período y modalidad
        /// </summary>
        [HttpPost]
        public ActionResult EvaluarPeriodo(string entidadId, string periodo, string modalidad, string tipoComparacion)
        {
            try
            {
                string idEnt = string.IsNullOrEmpty(entidadId) ? GetSessionString("IdEntidad", "-1") : entidadId;
                DateTime? fecha = null;
                if (!string.IsNullOrEmpty(periodo))
                {
                    try { fecha = Utility.Utilitarios.ConvertirAFecha(periodo); } catch { }
                }

                string mod = string.IsNullOrEmpty(modalidad) ? "Acumulado" : modalidad;
                string tipoComp = string.IsNullOrEmpty(tipoComparacion) ? "Interanual" : tipoComparacion;

                ObtenerUmbralesEfectivos(idEnt, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado);

                FGA_Obtener_Alertas_Financieras_Result[] result = null;
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    result = spClient.FGA_Generar_Alertas_Variacion_Financiera(idEnt, fecha, mod, tipoComp);
                }
                finally
                {
                    spClient.SafeClose();
                }

                var filtradas = (result ?? new FGA_Obtener_Alertas_Financieras_Result[0])
                    .Where(a => Math.Abs(a.VariacionPorcentaje) >= umbralPorc && Math.Abs(a.VariacionMonto) >= umbralMonto)
                    .ToList();

                int total = filtradas.Count;
                string descModo = modoMonitoreo == "SOLO_WATCHLIST" ? " [Modo: Lista de Seguimiento]" : (modoMonitoreo == "EXCLUIR_BLACKLIST" ? " [Modo: Exclusión de Cuentas]" : "");
                return Json(new { success = true, totalGeneradas = total, message = string.Format("Se evaluaron las cuentas contables con los umbrales configurados (Variación ≥ {0:N1}%, Monto ≥ ₡{1:N0}){2}. Total de alertas vigentes: {3}", umbralPorc, umbralMonto, descModo, total) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private void ObtenerUmbralesConfigurados(out decimal umbralPorc, out decimal umbralMonto)
        {
            umbralPorc = FGAConstants.Alertas.UmbralVariacionPorcGlobal;
            umbralMonto = FGAConstants.Alertas.UmbralVariacionMontoGlobal;

            try
            {
                FGA.Models.Parametros[] lista = null;
                var paramClient = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
                try
                {
                    lista = paramClient.GetAll() ?? new FGA.Models.Parametros[0];
                }
                finally
                {
                    paramClient.SafeClose();
                }

                // Búsqueda flexible por llave o por descripción para el Porcentaje
                var pPorc = lista.FirstOrDefault(p => p.Llave != null && (
                    p.Llave.Trim().Equals("ALERTA_VARIACION_PORC", StringComparison.OrdinalIgnoreCase) ||
                    p.Llave.Trim().Equals("ALERTA_PORCENTAJE", StringComparison.OrdinalIgnoreCase) ||
                    p.Llave.Trim().Equals("ALERTA_PORC", StringComparison.OrdinalIgnoreCase) ||
                    p.Llave.Trim().Equals("ALERTA_VARIACION_PORCENTAJE", StringComparison.OrdinalIgnoreCase) ||
                    (p.Llave.IndexOf("ALERTA", StringComparison.OrdinalIgnoreCase) >= 0 && p.Llave.IndexOf("PORC", StringComparison.OrdinalIgnoreCase) >= 0)
                )) ?? lista.FirstOrDefault(p => p.Descripcion != null &&
                    p.Descripcion.IndexOf("alerta", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (p.Descripcion.IndexOf("porcent", StringComparison.OrdinalIgnoreCase) >= 0 || p.Descripcion.IndexOf("%", StringComparison.OrdinalIgnoreCase) >= 0));

                if (pPorc != null && !string.IsNullOrWhiteSpace(pPorc.Valor))
                {
                    umbralPorc = ParseFlexibleDecimal(pPorc.Valor, FGAConstants.Alertas.UmbralVariacionPorcGlobal);
                }

                // Búsqueda flexible por llave o por descripción para el Monto
                var pMonto = lista.FirstOrDefault(p => p.Llave != null && (
                    p.Llave.Trim().Equals("ALERTA_VARIACION_MONTO", StringComparison.OrdinalIgnoreCase) ||
                    p.Llave.Trim().Equals("ALERTA_MONTO", StringComparison.OrdinalIgnoreCase) ||
                    p.Llave.Trim().Equals("MONTO_ALERTA", StringComparison.OrdinalIgnoreCase) ||
                    (p.Llave.IndexOf("ALERTA", StringComparison.OrdinalIgnoreCase) >= 0 && p.Llave.IndexOf("MONTO", StringComparison.OrdinalIgnoreCase) >= 0)
                )) ?? lista.FirstOrDefault(p => p.Descripcion != null &&
                    p.Descripcion.IndexOf("alerta", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (p.Descripcion.IndexOf("monto", StringComparison.OrdinalIgnoreCase) >= 0 || p.Descripcion.IndexOf("₡", StringComparison.OrdinalIgnoreCase) >= 0));

                if (pMonto != null && !string.IsNullOrWhiteSpace(pMonto.Valor))
                {
                    umbralMonto = ParseFlexibleDecimal(pMonto.Valor, FGAConstants.Alertas.UmbralVariacionMontoGlobal);
                }
            }
            catch
            {
            }
        }

        private static decimal ParseFlexibleDecimal(string rawValue, decimal defaultValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue)) return defaultValue;

            string s = rawValue.Trim().Replace("₡", "").Replace("$", "").Replace("%", "").Replace(" ", "").Trim();
            if (string.IsNullOrEmpty(s)) return defaultValue;

            // Tratamiento de sufijo M (millones)
            if (s.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(0, s.Length - 1).Trim();
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mVal))
                    return mVal * 1000000m;
            }

            int lastComma = s.LastIndexOf(',');
            int lastDot = s.LastIndexOf('.');

            if (lastComma > -1 && lastDot > -1)
            {
                if (lastDot > lastComma)
                {
                    // Formato USA: 20,000,000.50 -> quitar comas
                    s = s.Replace(",", "");
                }
                else
                {
                    // Formato Español: 20.000.000,50 -> quitar puntos y cambiar coma a punto
                    s = s.Replace(".", "").Replace(",", ".");
                }
            }
            else if (lastComma > -1)
            {
                int commaCount = s.Count(c => c == ',');
                if (commaCount > 1)
                {
                    // Ej: 20,000,000 -> separadores de miles
                    s = s.Replace(",", "");
                }
                else
                {
                    int digitsAfter = s.Length - 1 - lastComma;
                    if (digitsAfter == 3 && s.Length > 4 && !s.StartsWith("0"))
                    {
                        // Ej: 20,000 -> miles
                        s = s.Replace(",", "");
                    }
                    else
                    {
                        // Ej: 25,00 -> decimal
                        s = s.Replace(",", ".");
                    }
                }
            }
            else if (lastDot > -1)
            {
                int dotCount = s.Count(c => c == '.');
                if (dotCount > 1)
                {
                    // Ej: 20.000.000 -> separadores de miles
                    s = s.Replace(".", "");
                }
                else
                {
                    int digitsAfter = s.Length - 1 - lastDot;
                    if (digitsAfter == 3 && s.Length >= 5 && defaultValue >= 1000m)
                    {
                        // Separador de miles en montos
                        s = s.Replace(".", "");
                    }
                }
            }

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result > 0 ? result : defaultValue;
            }

            return defaultValue;
        }

        #region Endpoints de Configuración de Alertas y Cuentas por Entidad

        /// <summary>
        /// Obtiene los umbrales configurados y modo de monitoreo para la entidad especificada
        /// </summary>
        [HttpGet]
        public ActionResult GetConfiguracionEntidad(string entidadId)
        {
            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                var roleId = 0;
                int.TryParse(Env.GetUserInfo("roleid"), out roleId);
                var idEntidadUsuario = Env.GetUserInfo("identidad");
                bool esAdmin = roleId == 1 || string.IsNullOrEmpty(idEntidadUsuario) || idEntidadUsuario == "-1" || idEntidadUsuario == "0";

                ObtenerUmbralesEfectivos(idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado, out decimal globalPorc, out decimal globalMonto, out DateTime? fechaModif, out string usuarioModif);

                var response = new AlertaConfiguracionResponse
                {
                    Success = true,
                    Message = "Configuración obtenida correctamente.",
                    EsAdmin = esAdmin,
                    EntidadId = idEntidad,
                    GlobalUmbralPorc = globalPorc,
                    GlobalUmbralMonto = globalMonto,
                    Config = new AlertaConfiguracionEntidadDTO
                    {
                        IdEntidad = idEntidad,
                        UmbralVariacionPorcentaje = umbralPorc,
                        UmbralVariacionMonto = umbralMonto,
                        ModoMonitoreo = modoMonitoreo,
                        EsPersonalizado = esPersonalizado,
                        Activo = esPersonalizado,
                        GlobalPorc = globalPorc,
                        GlobalMonto = globalMonto,
                        FechaModificacion = fechaModif.HasValue ? fechaModif.Value.ToString("dd/MM/yyyy HH:mm") : null,
                        UsuarioModificacion = usuarioModif
                    }
                };

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AlertaConfiguracionResponse
                {
                    Success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Guarda los umbrales y modo de monitoreo propios de una cooperativa
        /// </summary>
        [HttpPost]
        public ActionResult GuardarConfiguracionEntidad(string entidadId, decimal? umbralPorc, decimal? umbralMonto, string modoMonitoreo, bool? usarGlobales)
        {
            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                string usuario = Env.GetUserInfo("name") ?? "SYSTEM";
                bool activo = !(usarGlobales.HasValue && usarGlobales.Value);

                if (!activo)
                {
                    umbralPorc = null;
                    umbralMonto = null;
                    modoMonitoreo = "TODAS";
                }
                else
                {
                    if (!umbralPorc.HasValue || umbralPorc.Value <= 0) umbralPorc = 15.0m;
                    if (!umbralMonto.HasValue || umbralMonto.Value <= 0) umbralMonto = 10000000.0m;
                    if (string.IsNullOrEmpty(modoMonitoreo)) modoMonitoreo = "TODAS";
                }

                // 1. Intentar mediante el servicio WCF SPClient
                bool guardado = false;
                try
                {
                    var spClient = new FGA_En_Linea.SPService.SPClient();
                    try
                    {
                        var resSp = spClient.FGA_Guardar_Configuracion_Alerta_Entidad(idEntidad, umbralPorc, umbralMonto, modoMonitoreo, activo, usuario);
                        if (resSp != null)
                        {
                            guardado = true;
                        }
                    }
                    finally
                    {
                        spClient.SafeClose();
                    }
                }
                catch
                {
                    guardado = false;
                }

                // 2. Fallback directo ADO.NET si el servicio WCF no estuviera disponible
                if (!guardado)
                {
                    string connStr = ObtenerCadenaConexion();
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("[dbo].[FGA_Guardar_Configuracion_Alerta_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                            cmd.Parameters.AddWithValue("@UMBRAL_PORC", (object)umbralPorc ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@UMBRAL_MONTO", (object)umbralMonto ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@MODO_MONITOREO", modoMonitoreo);
                            cmd.Parameters.AddWithValue("@ACTIVO", activo);
                            cmd.Parameters.AddWithValue("@USUARIO", usuario);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return Json(new AlertaOperacionResponse
                {
                    Success = true,
                    Message = activo
                        ? "Parámetros de alerta personalizados guardados exitosamente para su entidad."
                        : "Se restablecieron los parámetros globales de la FFC exitosamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = "Error al guardar parámetros: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de cuentas monitoreadas o excluidas para la entidad especificada
        /// </summary>
        [HttpGet]
        public ActionResult GetCuentasMonitoreadas(string entidadId)
        {
            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                var lista = new List<AlertaCuentaMonitoreadaDTO>();

                // 1. Intentar mediante WCF SPClient
                bool obtenido = false;
                try
                {
                    var spClient = new FGA_En_Linea.SPService.SPClient();
                    try
                    {
                        var cuentasSp = spClient.FGA_Obtener_Cuentas_Monitoreadas_Entidad(idEntidad, true);
                        if (cuentasSp != null)
                        {
                            foreach (var c in cuentasSp)
                            {
                                lista.Add(new AlertaCuentaMonitoreadaDTO
                                {
                                    Id = c.Id,
                                    IdEntidad = c.IdEntidad,
                                    NombreEntidad = c.NombreEntidad,
                                    CuentaContable = c.Cuenta,
                                    NombreCuenta = c.NombreCuenta ?? "",
                                    Nivel = c.Nivel,
                                    TipoMonitoreo = c.TipoRegla,
                                    UmbralPorcPersonalizado = c.UmbralPorcPersonalizado,
                                    UmbralMontoPersonalizado = c.UmbralMontoPersonalizado,
                                    Activo = c.Activo,
                                    FechaRegistro = c.FechaRegistro.ToString("dd/MM/yyyy"),
                                    UsuarioRegistro = c.UsuarioRegistro
                                });
                            }
                            obtenido = true;
                        }
                    }
                    finally
                    {
                        spClient.SafeClose();
                    }
                }
                catch
                {
                    obtenido = false;
                }

                // 2. Fallback directo ADO.NET si WCF no estuviera disponible
                if (!obtenido)
                {
                    string connStr = ObtenerCadenaConexion();
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("[dbo].[FGA_Obtener_Cuentas_Monitoreadas_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                            cmd.Parameters.AddWithValue("@SOLO_ACTIVAS", true);

                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    lista.Add(new AlertaCuentaMonitoreadaDTO
                                    {
                                        Id = Convert.ToInt32(r["Id"]),
                                        IdEntidad = r["IdEntidad"].ToString(),
                                        NombreEntidad = r["NombreEntidad"] != DBNull.Value ? r["NombreEntidad"].ToString() : "",
                                        CuentaContable = r["Cuenta"].ToString(),
                                        NombreCuenta = r["NombreCuenta"] != DBNull.Value ? r["NombreCuenta"].ToString() : "",
                                        Nivel = r["Nivel"] != DBNull.Value ? Convert.ToInt32(r["Nivel"]) : 3,
                                        TipoMonitoreo = r["TipoRegla"].ToString(),
                                        UmbralPorcPersonalizado = r["UmbralPorcPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralPorcPersonalizado"]) : (decimal?)null,
                                        UmbralMontoPersonalizado = r["UmbralMontoPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralMontoPersonalizado"]) : (decimal?)null,
                                        Activo = r["Activo"] != DBNull.Value && Convert.ToBoolean(r["Activo"]),
                                        FechaRegistro = r["FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(r["FechaRegistro"]).ToString("dd/MM/yyyy") : "",
                                        UsuarioRegistro = r["UsuarioRegistro"] != DBNull.Value ? r["UsuarioRegistro"].ToString() : ""
                                    });
                                }
                            }
                        }
                    }
                }

                return Json(new AlertaCuentasMonitoreadasResponse
                {
                    Success = true,
                    Message = "Cuentas obtenidas correctamente.",
                    Cuentas = lista
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AlertaCuentasMonitoreadasResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Cuentas = new List<AlertaCuentaMonitoreadaDTO>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Búsqueda y autocompletado en el catálogo de cuentas contables
        /// </summary>
        [HttpGet]
        public ActionResult BuscarCuentasCatalogo(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new AlertaCatalogoCuentasResponse
                    {
                        Success = true,
                        Items = new List<AlertaCuentaCatalogoItemDTO>()
                    }, JsonRequestBehavior.AllowGet);
                }

                string q = query.Trim().ToLowerInvariant();
                List<AlertaCuentaCatalogoItemDTO> resultado = new List<AlertaCuentaCatalogoItemDTO>();

                // 1. Intentar vía CatalogoCuentaClient si está disponible
                try
                {
                    var catClient = new FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient();
                    try
                    {
                        var cuentas = catClient.GetAll();
                        if (cuentas != null && cuentas.Length > 0)
                        {
                            resultado = cuentas
                                .Where(c => c != null && !string.IsNullOrEmpty(c.Cuenta) &&
                                       (c.Cuenta.ToLowerInvariant().Contains(q) || (c.Nombre != null && c.Nombre.ToLowerInvariant().Contains(q))))
                                .OrderBy(c => c.Cuenta)
                                .Take(25)
                                .Select(c => new AlertaCuentaCatalogoItemDTO
                                {
                                    Cuenta = c.Cuenta != null ? c.Cuenta.Trim() : "",
                                    Descripcion = c.Nombre != null ? c.Nombre.Trim() : "",
                                    Nivel = c.Nivel ?? 3
                                })
                                .ToList();

                            if (resultado.Count > 0)
                            {
                                return Json(new AlertaCatalogoCuentasResponse
                                {
                                    Success = true,
                                    Items = resultado
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    finally
                    {
                        catClient.SafeClose();
                    }
                }
                catch { }

                // 2. Fallback vía base de datos directa
                string connStr = ObtenerCadenaConexion();
                if (!string.IsNullOrEmpty(connStr))
                {
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("SELECT TOP 25 CUENTA, NOMBRE, ISNULL(NIVEL, 3) AS NIVEL FROM dbo.CATALOGOCUENTA WHERE CUENTA LIKE @q OR NOMBRE LIKE @q ORDER BY CUENTA", conn))
                        {
                            cmd.Parameters.AddWithValue("@q", "%" + q + "%");
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    resultado.Add(new AlertaCuentaCatalogoItemDTO
                                    {
                                        Cuenta = reader["CUENTA"].ToString().Trim(),
                                        Descripcion = reader["NOMBRE"] != DBNull.Value ? reader["NOMBRE"].ToString().Trim() : "",
                                        Nivel = Convert.ToInt32(reader["NIVEL"])
                                    });
                                }
                            }
                        }
                    }
                }

                return Json(new AlertaCatalogoCuentasResponse
                {
                    Success = true,
                    Items = resultado
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AlertaCatalogoCuentasResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Items = new List<AlertaCuentaCatalogoItemDTO>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Agrega o actualiza una cuenta contable en la lista de monitoreo (Watchlist o Blacklist)
        /// </summary>
        [HttpPost]
        public ActionResult GuardarCuentaMonitoreada(string entidadId, string cuenta, string nombreCuenta, string tipoRegla, decimal? umbralPorc, decimal? umbralMonto)
        {
            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                if (string.IsNullOrWhiteSpace(cuenta))
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = "Debe especificar el código de la cuenta contable."
                    });
                }

                string usuario = Env.GetUserInfo("name") ?? "SYSTEM";
                string regla = string.IsNullOrEmpty(tipoRegla) ? "MONITOREAR" : tipoRegla;

                // 1. Intentar mediante WCF SPClient
                bool guardado = false;
                try
                {
                    var spClient = new FGA_En_Linea.SPService.SPClient();
                    try
                    {
                        var res = spClient.FGA_Guardar_Cuenta_Monitoreada_Entidad(idEntidad, cuenta.Trim(), nombreCuenta, regla, umbralPorc, umbralMonto, usuario);
                        if (res != null) guardado = true;
                    }
                    finally
                    {
                        spClient.SafeClose();
                    }
                }
                catch
                {
                    guardado = false;
                }

                // 2. Fallback directo ADO.NET
                if (!guardado)
                {
                    string connStr = ObtenerCadenaConexion();
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("[dbo].[FGA_Guardar_Cuenta_Monitoreada_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                            cmd.Parameters.AddWithValue("@CUENTA", cuenta.Trim());
                            cmd.Parameters.AddWithValue("@NOMBRE_CUENTA", (object)nombreCuenta ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TIPO_REGLA", regla);
                            cmd.Parameters.AddWithValue("@UMBRAL_PORC", (object)umbralPorc ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@UMBRAL_MONTO", (object)umbralMonto ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@USUARIO", usuario);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return Json(new AlertaOperacionResponse
                {
                    Success = true,
                    Message = "Cuenta registrada correctamente en la lista de monitoreo."
                });
            }
            catch (Exception ex)
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = "Error al guardar cuenta: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Retira una cuenta de la lista de monitoreo
        /// </summary>
        [HttpPost]
        public ActionResult EliminarCuentaMonitoreada(int id, string entidadId)
        {
            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                string usuario = Env.GetUserInfo("name") ?? "SYSTEM";

                // 1. Intentar mediante WCF SPClient
                bool eliminado = false;
                try
                {
                    var spClient = new FGA_En_Linea.SPService.SPClient();
                    try
                    {
                        spClient.FGA_Eliminar_Cuenta_Monitoreada_Entidad(id, idEntidad, usuario);
                        eliminado = true;
                    }
                    finally
                    {
                        spClient.SafeClose();
                    }
                }
                catch
                {
                    eliminado = false;
                }

                // 2. Fallback directo ADO.NET
                if (!eliminado)
                {
                    string connStr = ObtenerCadenaConexion();
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("[dbo].[FGA_Eliminar_Cuenta_Monitoreada_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID", id);
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                            cmd.Parameters.AddWithValue("@USUARIO", usuario);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return Json(new AlertaOperacionResponse
                {
                    Success = true,
                    Message = "Cuenta retirada de la lista de monitoreo exitosamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = "Error al eliminar cuenta: " + ex.Message
                });
            }
        }

        #endregion

        #region Helpers de Base de Datos y Parámetros Multi-Entidad

        private string ResolverEntidadAutorizada(string entidadId)
        {
            var roleId = 0;
            int.TryParse(Env.GetUserInfo("roleid"), out roleId);
            var idEntidadUsuario = Env.GetUserInfo("identidad");
            bool esAdmin = roleId == 1 || string.IsNullOrEmpty(idEntidadUsuario) || idEntidadUsuario == "-1" || idEntidadUsuario == "0";

            if (!esAdmin && !string.IsNullOrEmpty(idEntidadUsuario) && idEntidadUsuario != "-1" && idEntidadUsuario != "0")
            {
                return idEntidadUsuario;
            }

            if (!string.IsNullOrEmpty(entidadId) && entidadId != "-1" && entidadId != "0" && entidadId != "99")
            {
                return entidadId;
            }

            if (!string.IsNullOrEmpty(idEntidadUsuario) && idEntidadUsuario != "-1" && idEntidadUsuario != "0")
            {
                return idEntidadUsuario;
            }

            return "13";
        }

        private static string _cachedConnString = null;

        private string ObtenerCadenaConexion()
        {
            if (!string.IsNullOrEmpty(_cachedConnString)) return _cachedConnString;

            try
            {
                var cs = System.Configuration.ConfigurationManager.ConnectionStrings["FGAConnection"];
                if (cs != null && !string.IsNullOrEmpty(cs.ConnectionString))
                {
                    _cachedConnString = cs.ConnectionString;
                    return _cachedConnString;
                }
            }
            catch { }

            try
            {
                string dbServiceConfig = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\DBService\Web.config");
                if (System.IO.File.Exists(dbServiceConfig))
                {
                    var doc = new System.Xml.XmlDocument();
                    doc.Load(dbServiceConfig);
                    var node = doc.SelectSingleNode("//connectionStrings/add[@name='ContentConnectionString']");
                    if (node != null && node.Attributes["connectionString"] != null)
                    {
                        _cachedConnString = node.Attributes["connectionString"].Value;
                        return _cachedConnString;
                    }
                }
            }
            catch { }

            _cachedConnString = @"data source=10.171.1.26\FGASQLPROD;initial catalog=FGA;user id=sa;password=Estanoes1;MultipleActiveResultSets=True;";
            return _cachedConnString;
        }

        private void ObtenerUmbralesEfectivos(string idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado)
        {
            ObtenerUmbralesEfectivos(idEntidad, out umbralPorc, out umbralMonto, out modoMonitoreo, out esPersonalizado, out _, out _, out _, out _);
        }

        private void ObtenerUmbralesEfectivos(string idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado, out decimal globalPorc, out decimal globalMonto, out DateTime? fechaModif, out string usuarioModif)
        {
            ObtenerUmbralesConfigurados(out globalPorc, out globalMonto);
            umbralPorc = globalPorc;
            umbralMonto = globalMonto;
            modoMonitoreo = "TODAS";
            esPersonalizado = false;
            fechaModif = null;
            usuarioModif = null;

            if (string.IsNullOrEmpty(idEntidad) || idEntidad == "-1")
            {
                return;
            }

            // 1. Intentar obtener configuración mediante WCF SPClient
            bool obtenidoWcf = false;
            try
            {
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    var cfg = spClient.FGA_Obtener_Configuracion_Alerta_Entidad(idEntidad);
                    if (cfg != null && cfg.Length > 0)
                    {
                        var item = cfg[0];
                        umbralPorc = item.UmbralVariacionPorc;
                        umbralMonto = item.UmbralVariacionMonto;
                        modoMonitoreo = string.IsNullOrEmpty(item.ModoMonitoreo) ? "TODAS" : item.ModoMonitoreo;
                        esPersonalizado = item.EsPersonalizado;
                        fechaModif = item.FechaModificacion;
                        usuarioModif = item.UsuarioModificacion;
                        obtenidoWcf = true;
                    }
                }
                finally
                {
                    spClient.SafeClose();
                }
            }
            catch
            {
                obtenidoWcf = false;
            }

            if (obtenidoWcf) return;

            // 2. Fallback directo ADO.NET
            try
            {
                string connStr = ObtenerCadenaConexion();
                if (!string.IsNullOrEmpty(connStr))
                {
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("[dbo].[FGA_Obtener_Configuracion_Alerta_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);

                            using (var r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    if (r["UmbralVariacionPorc"] != DBNull.Value)
                                        umbralPorc = Convert.ToDecimal(r["UmbralVariacionPorc"]);
                                    if (r["UmbralVariacionMonto"] != DBNull.Value)
                                        umbralMonto = Convert.ToDecimal(r["UmbralVariacionMonto"]);
                                    if (r["ModoMonitoreo"] != DBNull.Value)
                                        modoMonitoreo = r["ModoMonitoreo"].ToString();
                                    if (r["EsPersonalizado"] != DBNull.Value)
                                        esPersonalizado = Convert.ToBoolean(r["EsPersonalizado"]);
                                    if (r["FechaModificacion"] != DBNull.Value)
                                        fechaModif = Convert.ToDateTime(r["FechaModificacion"]);
                                    if (r["UsuarioModificacion"] != DBNull.Value)
                                        usuarioModif = r["UsuarioModificacion"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback a los globales de forma transparente
            }
        }

        #endregion

        #region Helpers de Formato

        private string FormatearMontoColones(decimal monto)
        {
            decimal absMonto = Math.Abs(monto);
            string signo = monto >= 0 ? "+" : "-";

            if (absMonto >= 1000000m)
            {
                return string.Format("{0}₡{1:N2}M", signo, absMonto / 1000000m);
            }
            else if (absMonto >= 1000m)
            {
                return string.Format("{0}₡{1:N1}K", signo, absMonto / 1000m);
            }
            return string.Format("{0}₡{1:N2}", signo, absMonto);
        }

        private string ObtenerTiempoRelativo(DateTime fecha)
        {
            var diff = DateTime.Now - fecha;
            if (diff.TotalMinutes < 2) return "Hace un momento";
            if (diff.TotalMinutes < 60) return string.Format("Hace {0} min", (int)diff.TotalMinutes);
            if (diff.TotalHours < 24) return string.Format("Hace {0} h", (int)diff.TotalHours);
            if (diff.TotalDays < 2) return "Ayer";
            if (diff.TotalDays < 7) return string.Format("Hace {0} días", (int)diff.TotalDays);
            return fecha.ToString("dd/MM/yyyy", _crCulture);
        }

        #endregion
    }
}
