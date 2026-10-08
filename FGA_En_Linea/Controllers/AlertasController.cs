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

                // Si no es administrador institucional FFC, forzar estrictamente su propia entidad
                if (!esAdmin && !string.IsNullOrEmpty(idEntidadUsuario))
                {
                    idEntidad = idEntidadUsuario;
                }

                ObtenerUmbralesEfectivos(idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado);
                ViewBag.UmbralPorc = umbralPorc;
                ViewBag.UmbralMonto = umbralMonto;
                ViewBag.ModoMonitoreo = modoMonitoreo;
                ViewBag.EsPersonalizado = esPersonalizado;
                ViewBag.EsAdmin = esAdmin;
                int idEntParsed = -1;
                int.TryParse(idEntidad, out idEntParsed);
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

                // DEFENSA EN PROFUNDIDAD MULTI-TENANT:
                // Si el usuario es de una entidad (no admin), asegurar que bajo ninguna circunstancia se presenten alertas de otra entidad
                var alertasBase = (rawAlertas ?? new FGA_Obtener_Alertas_Financieras_Result[0]);
                if (!esAdmin && !string.IsNullOrEmpty(idEntidad) && idEntidad != "-1")
                {
                    alertasBase = alertasBase.Where(a => a.IdEntidad == idEntidad).ToArray();
                }

                // Filtrar estrictamente por los umbrales vigentes configurados en Parámetros
                var todos = alertasBase
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
                if (esAdmin)
                {
                    ViewBag.EntidadesList = GetEntidadesCombo();
                }
                else
                {
                    var combo = GetEntidadesCombo();
                    ViewBag.EntidadesList = combo != null ? combo.Where(e => e.Id == idEntidad).ToList() : new List<FGA.Models.Entidad>();
                }

                return View(listAlertas);
            }
            catch (Exception ex)
            {
                ViewBag.TotalAlertas = 0;
                ViewBag.TotalNoLeidas = 0;
                ViewBag.TotalCriticas = 0;
                ViewBag.TotalAdvertencias = 0;
                ViewBag.EntidadesList = new List<FGA.Models.Entidad>();
                ViewBag.EsAdmin = IsCurrentUserAdmin;
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
                string idEntidad = ResolveCurrentEntity(null, out bool esAdmin);
                if (!esAdmin && string.IsNullOrEmpty(idEntidad))
                {
                    idEntidad = Env.GetUserInfo("identidad");
                }
                if (string.IsNullOrEmpty(idEntidad)) idEntidad = "-1";

                ObtenerUmbralesConfigurados(out decimal umbralPorc, out decimal umbralMonto);

                FGA_Obtener_Alertas_Financieras_Result[] rawAlertas = null;
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    rawAlertas = spClient.FGA_Obtener_Alertas_Financieras(idEntidad, false, 50);
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
                }
                finally
                {
                    spClient.SafeClose();
                }

                var listRaw = (rawAlertas ?? new FGA_Obtener_Alertas_Financieras_Result[0]);
                if (!esAdmin && !string.IsNullOrEmpty(idEntidad) && idEntidad != "-1")
                {
                    listRaw = listRaw.Where(a => a.IdEntidad == idEntidad).ToArray();
                }

                var list = listRaw
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
                int res = 0;
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    res = spClient.FGA_Marcar_Alerta_Leida(id, null, usuario);
                }
                finally
                {
                    spClient.SafeClose();
                }
                return Json(new { success = true, afectados = res });
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
                string idEntidad = ResolveCurrentEntity(null, out bool esAdmin);
                if (!esAdmin && string.IsNullOrEmpty(idEntidad))
                {
                    idEntidad = Env.GetUserInfo("identidad");
                }
                if (string.IsNullOrEmpty(idEntidad)) idEntidad = "-1";
                string usuario = Env.GetUserInfo("name") ?? "SYSTEM";

                int res = 0;
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    res = spClient.FGA_Marcar_Alerta_Leida(null, idEntidad, usuario);
                }
                finally
                {
                    spClient.SafeClose();
                }
                return Json(new { success = true, afectados = res });
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
                string idEnt = ResolveCurrentEntity(entidadId, out bool esAdmin);
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
                bool esAdmin = IsCurrentUserAdmin;

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
                        AsegurarTablasAlertasPersonalizadas(conn);

                        bool spExitoso = false;
                        try
                        {
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
                                spExitoso = true;
                            }
                        }
                        catch
                        {
                            spExitoso = false;
                        }

                        if (!spExitoso)
                        {
                            string upsertSql = @"
                                IF EXISTS (SELECT 1 FROM dbo.AlertaParametrosEntidad WHERE IdEntidad = @idEntidad)
                                BEGIN
                                    UPDATE dbo.AlertaParametrosEntidad
                                    SET UmbralVariacionPorc = @umbralPorc,
                                        UmbralVariacionMonto = @umbralMonto,
                                        ModoMonitoreo = @modoMonitoreo,
                                        Activo = @activo,
                                        FechaModificacion = GETDATE(),
                                        UsuarioModificacion = @usuario
                                    WHERE IdEntidad = @idEntidad;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO dbo.AlertaParametrosEntidad (IdEntidad, UmbralVariacionPorc, UmbralVariacionMonto, ModoMonitoreo, Activo, FechaModificacion, UsuarioModificacion)
                                    VALUES (@idEntidad, @umbralPorc, @umbralMonto, @modoMonitoreo, @activo, GETDATE(), @usuario);
                                END";

                            using (var cmd = new SqlCommand(upsertSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                                cmd.Parameters.AddWithValue("@umbralPorc", (object)umbralPorc ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@umbralMonto", (object)umbralMonto ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@modoMonitoreo", modoMonitoreo);
                                cmd.Parameters.AddWithValue("@activo", activo);
                                cmd.Parameters.AddWithValue("@usuario", usuario);
                                cmd.ExecuteNonQuery();
                            }
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
            var lista = new List<AlertaCuentaMonitoreadaDTO>();
            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);

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
                                    TipoMonitoreo = (c.TipoRegla == "IGNORAR" || c.TipoRegla == "BLACKLIST") ? "BLACKLIST" : "WATCHLIST",
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
                    try
                    {
                        string connStr = ObtenerCadenaConexion();
                        if (!string.IsNullOrEmpty(connStr))
                        {
                            using (var conn = new SqlConnection(connStr))
                            {
                                conn.Open();
                                AsegurarTablasAlertasPersonalizadas(conn);

                                bool spExitoso = false;
                                try
                                {
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
                                                    TipoMonitoreo = (r["TipoRegla"].ToString() == "IGNORAR" || r["TipoRegla"].ToString() == "BLACKLIST") ? "BLACKLIST" : "WATCHLIST",
                                                    UmbralPorcPersonalizado = r["UmbralPorcPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralPorcPersonalizado"]) : (decimal?)null,
                                                    UmbralMontoPersonalizado = r["UmbralMontoPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralMontoPersonalizado"]) : (decimal?)null,
                                                    Activo = r["Activo"] != DBNull.Value && Convert.ToBoolean(r["Activo"]),
                                                    FechaRegistro = r["FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(r["FechaRegistro"]).ToString("dd/MM/yyyy") : "",
                                                    UsuarioRegistro = r["UsuarioRegistro"] != DBNull.Value ? r["UsuarioRegistro"].ToString() : ""
                                                });
                                            }
                                            spExitoso = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    spExitoso = false;
                                }

                                if (!spExitoso)
                                {
                                    try
                                    {
                                        string directSql = @"
                                            SELECT 
                                                acm.Id,
                                                acm.IdEntidad,
                                                ISNULL(e.Nombre, 'Entidad ' + acm.IdEntidad) AS NombreEntidad,
                                                acm.Cuenta,
                                                ISNULL(acm.NombreCuenta, ISNULL(c.NOMBRE, 'Cuenta ' + acm.Cuenta)) AS NombreCuenta,
                                                ISNULL(c.NIVEL, 3) AS Nivel,
                                                acm.TipoRegla,
                                                acm.UmbralPorcPersonalizado,
                                                acm.UmbralMontoPersonalizado,
                                                acm.Activo,
                                                acm.FechaRegistro,
                                                acm.UsuarioRegistro
                                            FROM dbo.AlertaCuentaMonitoreada acm WITH(NOLOCK)
                                            LEFT JOIN dbo.CATALOGOCUENTA c WITH(NOLOCK) ON acm.Cuenta = c.CUENTA
                                            LEFT JOIN dbo.Entidad e WITH(NOLOCK) ON acm.IdEntidad = e.Id
                                            WHERE (@idEntidad = '-1' OR acm.IdEntidad = @idEntidad)
                                              AND acm.Activo = 1
                                            ORDER BY acm.Cuenta";

                                        using (var cmd = new SqlCommand(directSql, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
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
                                                        TipoMonitoreo = (r["TipoRegla"].ToString() == "IGNORAR" || r["TipoRegla"].ToString() == "BLACKLIST") ? "BLACKLIST" : "WATCHLIST",
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
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
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
                    Success = true,
                    Message = ex.Message,
                    Cuentas = lista
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Búsqueda y autocompletado en el catálogo de cuentas contables
        /// </summary>
        [HttpGet]
        public ActionResult BuscarCuentasCatalogo(string query = null, string filtro = null, string entidadId = null)
        {
            try
            {
                string q = (query ?? filtro ?? "").Trim();
                if (string.IsNullOrWhiteSpace(q))
                {
                    return Json(new AlertaCatalogoCuentasResponse
                    {
                        Success = true,
                        Items = new List<AlertaCuentaCatalogoItemDTO>()
                    }, JsonRequestBehavior.AllowGet);
                }

                string qLower = q.ToLowerInvariant();
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
                                       (c.Cuenta.ToLowerInvariant().Contains(qLower) || (c.Nombre != null && c.Nombre.ToLowerInvariant().Contains(qLower))))
                                .OrderBy(c => c.Cuenta.StartsWith(q, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                                .ThenBy(c => c.Cuenta)
                                .Take(30)
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
                        AsegurarTablasAlertasPersonalizadas(conn);

                        string sqlCatalog = @"
                            SELECT TOP 30 
                                CUENTA, 
                                NOMBRE, 
                                CASE 
                                    WHEN LEN(RTRIM(CUENTA)) <= 2 THEN 1
                                    WHEN LEN(RTRIM(CUENTA)) <= 4 THEN 2
                                    WHEN LEN(RTRIM(CUENTA)) <= 6 THEN 3
                                    ELSE 4
                                END AS NIVEL 
                            FROM dbo.CATALOGOCUENTA WITH(NOLOCK) 
                            WHERE CUENTA LIKE @qLike OR NOMBRE LIKE @qLike 
                            ORDER BY 
                                CASE WHEN CUENTA LIKE @qStart THEN 0 WHEN NOMBRE LIKE @qStart THEN 1 ELSE 2 END,
                                CUENTA";

                        using (var cmd = new SqlCommand(sqlCatalog, conn))
                        {
                            cmd.Parameters.AddWithValue("@qLike", "%" + q + "%");
                            cmd.Parameters.AddWithValue("@qStart", q + "%");
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

                        // 3. Fallback adicional si no hubieron resultados en CATALOGOCUENTA:
                        // Buscar en SALIDA_BALANCE_COMPROBACION
                        if (resultado.Count == 0)
                        {
                            try
                            {
                                string sqlBal = @"
                                    SELECT DISTINCT TOP 30 
                                        CUENTA, 
                                        DESCRIPCION 
                                    FROM dbo.SALIDA_BALANCE_COMPROBACION WITH(NOLOCK) 
                                    WHERE (CUENTA LIKE @qLike OR DESCRIPCION LIKE @qLike)
                                      AND (@idEntidad IS NULL OR IDENTIDAD = @idEntidad)
                                    ORDER BY CUENTA";

                                using (var cmdBal = new SqlCommand(sqlBal, conn))
                                {
                                    cmdBal.Parameters.AddWithValue("@qLike", "%" + q + "%");
                                    cmdBal.Parameters.AddWithValue("@idEntidad", string.IsNullOrEmpty(entidadId) || entidadId == "-1" ? (object)DBNull.Value : entidadId);
                                    using (var rBal = cmdBal.ExecuteReader())
                                    {
                                        while (rBal.Read())
                                        {
                                            resultado.Add(new AlertaCuentaCatalogoItemDTO
                                            {
                                                Cuenta = rBal["CUENTA"].ToString().Trim(),
                                                Descripcion = rBal["DESCRIPCION"] != DBNull.Value ? rBal["DESCRIPCION"].ToString().Trim() : "",
                                                Nivel = 3
                                            });
                                        }
                                    }
                                }
                            }
                            catch { }
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
        public ActionResult GuardarCuentaMonitoreada(
            string entidadId,
            string cuentaContable = null,
            string cuenta = null,
            string nombreCuenta = null,
            string tipoMonitoreo = null,
            string tipoRegla = null,
            string justificacion = null,
            decimal? umbralPorc = null,
            decimal? umbralMonto = null)
        {
            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                string codCuenta = (cuentaContable ?? cuenta ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codCuenta))
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = "Debe especificar el código de la cuenta contable."
                    });
                }

                string usuario = Env.GetUserInfo("name") ?? "SYSTEM";
                string rawTipo = (tipoMonitoreo ?? tipoRegla ?? "WATCHLIST").Trim().ToUpperInvariant();
                string regla = (rawTipo == "BLACKLIST" || rawTipo == "IGNORAR") ? "IGNORAR" : "MONITOREAR";

                if (string.IsNullOrWhiteSpace(nombreCuenta))
                {
                    nombreCuenta = "Cuenta " + codCuenta;
                }

                // 1. Intentar mediante WCF SPClient
                bool guardado = false;
                try
                {
                    var spClient = new FGA_En_Linea.SPService.SPClient();
                    try
                    {
                        var res = spClient.FGA_Guardar_Cuenta_Monitoreada_Entidad(idEntidad, codCuenta, nombreCuenta, regla, umbralPorc, umbralMonto, usuario);
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
                        AsegurarTablasAlertasPersonalizadas(conn);

                        bool spExitoso = false;
                        try
                        {
                            using (var cmd = new SqlCommand("[dbo].[FGA_Guardar_Cuenta_Monitoreada_Entidad]", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                                cmd.Parameters.AddWithValue("@CUENTA", codCuenta);
                                cmd.Parameters.AddWithValue("@NOMBRE_CUENTA", (object)nombreCuenta ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@TIPO_REGLA", regla);
                                cmd.Parameters.AddWithValue("@UMBRAL_PORC", (object)umbralPorc ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@UMBRAL_MONTO", (object)umbralMonto ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@USUARIO", usuario);

                                cmd.ExecuteNonQuery();
                                spExitoso = true;
                            }
                        }
                        catch
                        {
                            spExitoso = false;
                        }

                        if (!spExitoso)
                        {
                            string upsertSql = @"
                                IF EXISTS (SELECT 1 FROM dbo.AlertaCuentaMonitoreada WHERE IdEntidad = @idEntidad AND Cuenta = @cuenta)
                                BEGIN
                                    UPDATE dbo.AlertaCuentaMonitoreada
                                    SET NombreCuenta = @nombre,
                                        TipoRegla = @tipo,
                                        UmbralPorcPersonalizado = @umbralPorc,
                                        UmbralMontoPersonalizado = @umbralMonto,
                                        Activo = 1,
                                        FechaRegistro = GETDATE(),
                                        UsuarioRegistro = @usuario
                                    WHERE IdEntidad = @idEntidad AND Cuenta = @cuenta;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO dbo.AlertaCuentaMonitoreada (IdEntidad, Cuenta, NombreCuenta, TipoRegla, UmbralPorcPersonalizado, UmbralMontoPersonalizado, Activo, FechaRegistro, UsuarioRegistro)
                                    VALUES (@idEntidad, @cuenta, @nombre, @tipo, @umbralPorc, @umbralMonto, 1, GETDATE(), @usuario);
                                END";

                            using (var cmd = new SqlCommand(upsertSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                                cmd.Parameters.AddWithValue("@cuenta", codCuenta);
                                cmd.Parameters.AddWithValue("@nombre", (object)nombreCuenta ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tipo", regla);
                                cmd.Parameters.AddWithValue("@umbralPorc", (object)umbralPorc ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@umbralMonto", (object)umbralMonto ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@usuario", usuario);
                                cmd.ExecuteNonQuery();
                            }
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
                        AsegurarTablasAlertasPersonalizadas(conn);

                        bool spExitoso = false;
                        try
                        {
                            using (var cmd = new SqlCommand("[dbo].[FGA_Eliminar_Cuenta_Monitoreada_Entidad]", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@ID", id);
                                cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                                cmd.Parameters.AddWithValue("@USUARIO", usuario);

                                cmd.ExecuteNonQuery();
                                spExitoso = true;
                            }
                        }
                        catch
                        {
                            spExitoso = false;
                        }

                        if (!spExitoso)
                        {
                            using (var cmd = new SqlCommand("UPDATE dbo.AlertaCuentaMonitoreada SET Activo = 0 WHERE Id = @id AND (@idEntidad = '-1' OR IdEntidad = @idEntidad)", conn))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                                cmd.ExecuteNonQuery();
                            }
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
            string idEntidad = ResolveCurrentEntity(entidadId, out bool esAdmin);
            if (!esAdmin)
            {
                return idEntidad;
            }

            if (!string.IsNullOrEmpty(entidadId) && entidadId != "-1" && entidadId != "0" && entidadId != "99")
            {
                return entidadId;
            }

            return idEntidad;
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

        private static bool _tablasAseguradas = false;
        private static readonly object _lockTablas = new object();

        private void AsegurarTablasAlertasPersonalizadas(SqlConnection conn)
        {
            if (_tablasAseguradas) return;
            lock (_lockTablas)
            {
                if (_tablasAseguradas) return;
                try
                {
                    string sqlCheck = @"
                        IF OBJECT_ID('dbo.AlertaParametrosEntidad', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.AlertaParametrosEntidad (
                                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                IdEntidad NVARCHAR(5) NOT NULL UNIQUE,
                                UmbralVariacionPorc DECIMAL(18,2) NULL,
                                UmbralVariacionMonto DECIMAL(18,2) NULL,
                                ModoMonitoreo NVARCHAR(20) NOT NULL DEFAULT 'TODAS',
                                Activo BIT NOT NULL DEFAULT 1,
                                FechaModificacion DATETIME NOT NULL DEFAULT GETDATE(),
                                UsuarioModificacion NVARCHAR(100) NULL
                            );
                        END

                        IF OBJECT_ID('dbo.AlertaCuentaMonitoreada', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.AlertaCuentaMonitoreada (
                                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                IdEntidad NVARCHAR(5) NOT NULL,
                                Cuenta NVARCHAR(50) NOT NULL,
                                NombreCuenta NVARCHAR(250) NULL,
                                TipoRegla NVARCHAR(20) NOT NULL DEFAULT 'MONITOREAR',
                                UmbralPorcPersonalizado DECIMAL(18,2) NULL,
                                UmbralMontoPersonalizado DECIMAL(18,2) NULL,
                                Activo BIT NOT NULL DEFAULT 1,
                                FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
                                UsuarioRegistro NVARCHAR(100) NULL,
                                CONSTRAINT UQ_AlertaCuenta_Entidad_Cuenta UNIQUE (IdEntidad, Cuenta)
                            );
                        END";

                    using (var cmd = new SqlCommand(sqlCheck, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    _tablasAseguradas = true;
                }
                catch { }
            }
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
                        AsegurarTablasAlertasPersonalizadas(conn);

                        bool spExitoso = false;
                        try
                        {
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
                                spExitoso = true;
                            }
                        }
                        catch
                        {
                            spExitoso = false;
                        }

                        if (!spExitoso)
                        {
                            try
                            {
                                using (var cmdDirect = new SqlCommand(@"
                                    SELECT 
                                        UmbralVariacionPorc, 
                                        UmbralVariacionMonto, 
                                        ModoMonitoreo, 
                                        Activo, 
                                        FechaModificacion, 
                                        UsuarioModificacion 
                                    FROM dbo.AlertaParametrosEntidad WITH(NOLOCK) 
                                    WHERE IdEntidad = @idEntidad AND Activo = 1", conn))
                                {
                                    cmdDirect.Parameters.AddWithValue("@idEntidad", idEntidad);
                                    using (var r = cmdDirect.ExecuteReader())
                                    {
                                        if (r.Read())
                                        {
                                            if (r["UmbralVariacionPorc"] != DBNull.Value)
                                                umbralPorc = Convert.ToDecimal(r["UmbralVariacionPorc"]);
                                            if (r["UmbralVariacionMonto"] != DBNull.Value)
                                                umbralMonto = Convert.ToDecimal(r["UmbralVariacionMonto"]);
                                            if (r["ModoMonitoreo"] != DBNull.Value)
                                                modoMonitoreo = r["ModoMonitoreo"].ToString();
                                            esPersonalizado = true;
                                            if (r["FechaModificacion"] != DBNull.Value)
                                                fechaModif = Convert.ToDateTime(r["FechaModificacion"]);
                                            if (r["UsuarioModificacion"] != DBNull.Value)
                                                usuarioModif = r["UsuarioModificacion"].ToString();
                                        }
                                    }
                                }
                            }
                            catch { }
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
