using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Entities.Entities.Procedures;
using FGA.Models;
using FGA.Utility;
using FGA.Services;

namespace FGA.Controllers
{
    public class AlertasController : BaseController
    {
        private readonly CultureInfo _crCulture = FGAConstants.CulturaCR;
        private readonly AlertasDataService _dataService = new AlertasDataService();

        /// <summary>
        /// Vista principal: Bandeja / Centro de Notificaciones y Alertas Financieras
        /// </summary>
        public ActionResult Index(string entidad, string tipoAlerta, bool? soloNoLeidas)
        {
            if (!Env.IsAlertasEnabled())
            {
                return RedirectToAction("Index", "Home");
            }

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

                // Conteo de cuentas en lista de monitoreo para la entidad actual
                int cuentasMonitoreadasCount = ObtenerCantidadCuentasMonitoreadas(idEntidad);
                ViewBag.CuentasMonitoreadasCount = cuentasMonitoreadasCount;

                FGA_Obtener_Alertas_Financieras_Result[] rawAlertas = null;
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    // Por arquitectura multi-tenant y default institucional:
                    // Se consultan las alertas financieras vigentes ya calificadas para las cuentas monitoreadas.
                    // No se autogeneran alertas para cuentas no autorizadas por la entidad.
                    rawAlertas = spClient.FGA_Obtener_Alertas_Financieras(idEntidad, false, 100);
                }
                catch
                {
                    rawAlertas = new FGA_Obtener_Alertas_Financieras_Result[0];
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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new
                {
                    success = false,
                    habilitado = false,
                    totalNoLeidas = 0,
                    total = 0,
                    alertas = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }

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
                }
                catch
                {
                    rawAlertas = new FGA_Obtener_Alertas_Financieras_Result[0];
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
                    entidad = a.NombreEntidad,
                    periodoActual = a.PeriodoActual.ToString("MM/yyyy"),
                    periodoAnterior = a.PeriodoAnterior.ToString("MM/yyyy")
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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new { success = false, message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema." });
            }

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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new { success = false, message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema." });
            }

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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema."
                });
            }

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

                // Si la entidad está en modo SOLO_WATCHLIST (por defecto) y no tiene cuentas configuradas en su Lista de Monitoreo
                if ((modoMonitoreo == FGAConstants.Alertas.ModoSoloWatchlist || string.IsNullOrEmpty(modoMonitoreo)) && idEnt != "-1")
                {
                    int totalCuentas = ObtenerCantidadCuentasMonitoreadas(idEnt);
                    if (totalCuentas == 0)
                    {
                        return Json(new AlertaOperacionResponse
                        {
                            Success = true,
                            TotalGeneradas = 0,
                            Message = "Su entidad no tiene cuentas registradas en su Lista de Monitoreo. Por defecto el FFC no genera alertas automáticas hasta que configure las cuentas que desea supervisar en 'Configurar Entidad' -> pestaña 'Cuentas a Monitorear'."
                        });
                    }
                }

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
                var response = new AlertaOperacionResponse
                {
                    Success = true,
                    TotalGeneradas = total,
                    Message = string.Format("Se evaluaron las cuentas contables con los umbrales configurados (Variación ≥ {0:N1}%, Monto ≥ ₡{1:N0}){2}. Total de alertas vigentes: {3}", umbralPorc, umbralMonto, descModo, total)
                };
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = FGAConstants.Alertas.MsgErrorEvaluacion + ": " + ex.Message
                });
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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new AlertaConfiguracionResponse
                {
                    Success = false,
                    Message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema."
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                bool esAdmin = IsCurrentUserAdmin;

                ObtenerUmbralesEfectivos(idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado, out decimal globalPorc, out decimal globalMonto, out DateTime? fechaModif, out string usuarioModif);

                var response = new AlertaConfiguracionResponse
                {
                    Success = true,
                    Message = FGAConstants.Alertas.MsgConfiguracionObtenida,
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
                        FechaModificacion = fechaModif.HasValue ? fechaModif.Value.ToString(FGAConstants.Formatos.FechaCorta + " HH:mm") : null,
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
        public ActionResult GuardarConfiguracionEntidad(
            string entidadId,
            decimal? umbralPorc = null,
            decimal? umbralPorcentaje = null,
            decimal? umbralMonto = null,
            string modoMonitoreo = null,
            bool? activo = null,
            bool? usarGlobales = null)
        {
            if (!Env.IsAlertasEnabled())
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema."
                });
            }

            try
            {
                string idEntidad = ResolverEntidadConfigurable(entidadId);
                if (!FGAConstants.Entidades.EsEntidadEspecifica(idEntidad))
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = FGAConstants.Alertas.MsgErrorEntidadRequerida
                    });
                }

                string usuario = Env.GetUserInfo(FGAConstants.Sesion.Nombre) ?? FGAConstants.Alertas.UsuarioDefecto;
                bool esActivo = activo.HasValue ? activo.Value : !(usarGlobales.HasValue && usarGlobales.Value);

                decimal? porcFinal = umbralPorc ?? umbralPorcentaje;
                decimal? montoFinal = umbralMonto;
                string modoFinal = string.IsNullOrEmpty(modoMonitoreo) ? FGAConstants.Alertas.ModoDefecto : modoMonitoreo.Trim().ToUpperInvariant();

                if (!esActivo)
                {
                    porcFinal = null;
                    montoFinal = null;
                    modoFinal = FGAConstants.Alertas.ModoDefecto;
                }
                else
                {
                    if (!porcFinal.HasValue || porcFinal.Value <= 0) porcFinal = FGAConstants.Alertas.UmbralVariacionPorcGlobal;
                    if (!montoFinal.HasValue || montoFinal.Value <= 0) montoFinal = FGAConstants.Alertas.UmbralVariacionMontoGlobal;
                    if (modoFinal != FGAConstants.Alertas.ModoTodas && modoFinal != FGAConstants.Alertas.ModoSoloWatchlist && modoFinal != FGAConstants.Alertas.ModoExcluirBlacklist)
                    {
                        modoFinal = FGAConstants.Alertas.ModoDefecto;
                    }
                }

                bool guardado = _dataService.GuardarConfiguracionEntidad(idEntidad, porcFinal, montoFinal, modoFinal, esActivo, usuario);
                if (!guardado)
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = "No fue posible guardar la configuración de alertas en este momento."
                    });
                }

                string msg = esActivo
                    ? FGAConstants.Alertas.MsgConfiguracionGuardada
                    : FGAConstants.Alertas.MsgConfiguracionGlobalRestablecida;

                return Json(new AlertaOperacionResponse
                {
                    Success = true,
                    Message = msg
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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new AlertaCuentasMonitoreadasResponse
                {
                    Success = false,
                    Message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema.",
                    Cuentas = new List<AlertaCuentaMonitoreadaDTO>()
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                var lista = _dataService.ObtenerCuentasMonitoreadas(idEntidad);

                return Json(new AlertaCuentasMonitoreadasResponse
                {
                    Success = true,
                    Message = "Cuentas obtenidas correctamente.",
                    Cuentas = lista
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("GetCuentasMonitoreadas error: " + ex.Message);
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
        public ActionResult BuscarCuentasCatalogo(string query = null, string filtro = null, string entidadId = null)
        {
            if (!Env.IsAlertasEnabled())
            {
                return Json(new AlertaCatalogoCuentasResponse
                {
                    Success = false,
                    Message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema.",
                    Items = new List<AlertaCuentaCatalogoItemDTO>()
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string q = (query ?? filtro ?? "").Trim();
                var resultado = _dataService.BuscarCuentasCatalogo(q);

                return Json(new AlertaCatalogoCuentasResponse
                {
                    Success = true,
                    Message = "OK",
                    Items = resultado
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("BuscarCuentasCatalogo error: " + ex.Message);
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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema."
                });
            }

            try
            {
                string idEntidad = ResolverEntidadConfigurable(entidadId);
                if (!FGAConstants.Entidades.EsEntidadEspecifica(idEntidad))
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = FGAConstants.Alertas.MsgErrorEntidadRequerida
                    });
                }

                string codCuenta = (cuentaContable ?? cuenta ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codCuenta))
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = FGAConstants.Alertas.MsgErrorCuentaRequerida
                    });
                }

                string usuario = Env.GetUserInfo(FGAConstants.Sesion.Nombre) ?? FGAConstants.Alertas.UsuarioDefecto;
                string rawTipo = (tipoMonitoreo ?? tipoRegla ?? FGAConstants.Alertas.ReglaWatchlist).Trim().ToUpperInvariant();
                string regla = (rawTipo == FGAConstants.Alertas.ReglaBlacklist || rawTipo == FGAConstants.Alertas.ReglaIgnorar)
                    ? FGAConstants.Alertas.ReglaIgnorar
                    : FGAConstants.Alertas.ReglaMonitorear;

                if (string.IsNullOrWhiteSpace(nombreCuenta))
                {
                    nombreCuenta = "Cuenta " + codCuenta;
                }

                bool guardado = _dataService.GuardarCuentaMonitoreada(idEntidad, codCuenta, nombreCuenta, regla, umbralPorc, umbralMonto, usuario);
                if (!guardado)
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = "No fue posible registrar la cuenta monitoreada en este momento."
                    });
                }

                return Json(new AlertaOperacionResponse
                {
                    Success = true,
                    Message = FGAConstants.Alertas.MsgCuentaRegistrada
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
            if (!Env.IsAlertasEnabled())
            {
                return Json(new AlertaOperacionResponse
                {
                    Success = false,
                    Message = "El módulo de alertas se encuentra deshabilitado por configuración del sistema."
                });
            }

            try
            {
                string idEntidad = ResolverEntidadAutorizada(entidadId);
                string usuario = Env.GetUserInfo(FGAConstants.Sesion.Nombre) ?? FGAConstants.Alertas.UsuarioDefecto;

                bool eliminado = _dataService.EliminarCuentaMonitoreada(id, idEntidad, usuario);
                if (!eliminado)
                {
                    return Json(new AlertaOperacionResponse
                    {
                        Success = false,
                        Message = "No fue posible retirar la cuenta de monitoreo en este momento."
                    });
                }

                return Json(new AlertaOperacionResponse
                {
                    Success = true,
                    Message = FGAConstants.Alertas.MsgCuentaRemovida
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

            if (FGAConstants.Entidades.EsEntidadEspecifica(entidadId))
            {
                return entidadId;
            }

            return idEntidad;
        }

        private string ResolverEntidadConfigurable(string entidadId)
        {
            string idEntidad = ResolverEntidadAutorizada(entidadId);
            if (!FGAConstants.Entidades.EsEntidadEspecifica(idEntidad))
            {
                string idSesion = Session?[FGAConstants.Sesion.IdEntidad]?.ToString();
                if (FGAConstants.Entidades.EsEntidadEspecifica(idSesion))
                {
                    idEntidad = idSesion;
                }
                else
                {
                    var usrObj = Session?[FGAConstants.Sesion.CurrentUserObj] as Usuario;
                    idEntidad = usrObj?.Entidad_Usuario_Id ?? Env.GetUserInfo(FGAConstants.Sesion.Entidad);
                }
            }
            return idEntidad;
        }

        #region Servicios de Datos y Configuración (Delegados a AlertasDataService)

        private int ObtenerCantidadCuentasMonitoreadas(string idEntidad)
        {
            return _dataService.ObtenerCantidadCuentasMonitoreadas(idEntidad);
        }

        private void ObtenerUmbralesEfectivos(string idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado)
        {
            _dataService.ObtenerUmbralesEfectivos(idEntidad, out _, out _, out umbralPorc, out umbralMonto, out modoMonitoreo, out esPersonalizado, out _, out _);
        }

        private void ObtenerUmbralesEfectivos(string idEntidad, out decimal umbralPorc, out decimal umbralMonto, out string modoMonitoreo, out bool esPersonalizado, out decimal globalPorc, out decimal globalMonto, out DateTime? fechaModif, out string usuarioModif)
        {
            _dataService.ObtenerUmbralesEfectivos(idEntidad, out globalPorc, out globalMonto, out umbralPorc, out umbralMonto, out modoMonitoreo, out esPersonalizado, out fechaModif, out usuarioModif);
        }

        #endregion

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
