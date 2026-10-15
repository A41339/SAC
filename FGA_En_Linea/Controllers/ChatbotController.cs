using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FGA.Services.Chatbot;
using FGA.Utility;
using FGA_En_Linea.EntidadService;

namespace FGA.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IChatbotAiProvider _aiProvider;

        public ChatbotController()
        {
            _aiProvider = new GeminiChatbotProvider();
        }

        public ChatbotController(IChatbotAiProvider aiProvider)
        {
            _aiProvider = aiProvider ?? new GeminiChatbotProvider();
        }

        [HttpPost]
        public async Task<JsonResult> Preguntar(string mensaje, string historial, string periodoContexto = null, string entidadContexto = null, string pantallaContexto = null)
        {
            try
            {
                // 1. Verificación del Interruptor / Parámetro de activación
                if (!Env.IsChatbotEnabled())
                {
                    return Json(new
                    {
                        success = false,
                        respuesta = "El Asistente Virtual se encuentra actualmente desactivado por configuración del sistema."
                    });
                }

                // 2. Validación de entrada
                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    return Json(new
                    {
                        success = false,
                        respuesta = "Por favor ingresa una pregunta o consulta."
                    });
                }

                // 3. Extracción de contexto de entidad seguro (Aislamiento de Cooperativa / Admin FFC)
                string entidadId = null;
                string entidadNombre = null;

                // Obtener catálogo de entidades activas para mapeo por ID y por NOMBRE
                FGA.Models.Entidad[] listaEnt = null;
                try
                {
                    listaEnt = Utility.WcfHelper.Execute<EntidadServiceClient, FGA.Models.Entidad[]>(
                        c => c.GetAll()
                    );
                }
                catch (Exception exEnt)
                {
                    System.Diagnostics.Trace.TraceWarning("ChatbotController: Error al consultar catálogo de entidades WCF: " + exEnt.Message);
                }

                // DETECCIÓN POR NOMBRE: Si el usuario menciona una cooperativa en su consulta (ej. "Coopealianza", "Credecoop", etc.)
                if (listaEnt != null && listaEnt.Length > 0 && !string.IsNullOrWhiteSpace(mensaje))
                {
                    string msgNorm = mensaje.ToLower();
                    foreach (var e in listaEnt.Where(x => x.Activo == true && FGAConstants.Entidades.EsEntidadEspecifica(x.Id)))
                    {
                        string nom = (e.Nombre ?? "").Trim().ToLower();
                        if (nom.Length >= 4 && msgNorm.Contains(nom))
                        {
                            entidadId = e.Id;
                            entidadNombre = e.Nombre.Trim();
                            break;
                        }

                        // Buscar también palabras distintivas del nombre (ej. "alianza" para Coopealianza, "credecoop", etc.)
                        string palLimpia = nom.Replace("cooperativa", "").Replace("coope", "").Trim();
                        if (palLimpia.Length >= 4 && msgNorm.Contains(palLimpia))
                        {
                            entidadId = e.Id;
                            entidadNombre = e.Nombre.Trim();
                            break;
                        }
                    }
                }

                // Si no se mencionó por nombre, priorizar el selector activo del frontend
                if (string.IsNullOrWhiteSpace(entidadId) && FGAConstants.Entidades.EsEntidadEspecifica(entidadContexto))
                {
                    entidadId = entidadContexto.Trim();
                }

                // Si no, verificar Session["IdEntidad"]
                if (string.IsNullOrWhiteSpace(entidadId) && Session?[FGAConstants.Sesion.IdEntidad] != null)
                {
                    string sId = Session[FGAConstants.Sesion.IdEntidad].ToString().Trim();
                    if (FGAConstants.Entidades.EsEntidadEspecifica(sId))
                    {
                        entidadId = sId;
                    }
                }

                // Si no, verificar Env.GetUserInfo("identidad")
                if (string.IsNullOrWhiteSpace(entidadId))
                {
                    string uEnt = Env.GetUserInfo(FGAConstants.Sesion.Entidad);
                    if (FGAConstants.Entidades.EsEntidadEspecifica(uEnt))
                    {
                        entidadId = uEnt.Trim();
                    }
                }

                // Fallback: si aún es nulo o filtro global (admin FFC sin entidad seleccionada), tomar la primera entidad activa válida con datos
                if (FGAConstants.Entidades.EsFiltroTodas(entidadId))
                {
                    var primera = listaEnt?.FirstOrDefault(e => FGAConstants.Entidades.EsEntidadEspecifica(e.Id) && e.Activo == true);
                    entidadId = primera != null ? primera.Id : FGAConstants.Alertas.EntidadDefecto;
                }

                Session[FGAConstants.Sesion.IdEntidad] = entidadId;

                // Nombre oficial de la entidad
                if (string.IsNullOrWhiteSpace(entidadNombre))
                {
                    entidadNombre = Session?[FGAConstants.Sesion.NomEntidad]?.ToString();
                    if (string.IsNullOrWhiteSpace(entidadNombre) || entidadNombre.Equals("FGA", StringComparison.OrdinalIgnoreCase) || entidadNombre.Equals(FGAConstants.Entidades.NombreAdministradora, StringComparison.OrdinalIgnoreCase))
                    {
                        var entObj = listaEnt?.FirstOrDefault(e => e.Id == entidadId);
                        if (entObj != null && !string.IsNullOrWhiteSpace(entObj.Nombre))
                        {
                            entidadNombre = entObj.Nombre.Trim();
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(entidadNombre))
                {
                    entidadNombre = Env.GetUserInfo(FGAConstants.Sesion.Entidad) ?? "Entidad Cooperativa";
                }
                Session[FGAConstants.Sesion.NomEntidad] = entidadNombre;

                string usuario = Env.GetUserInfo(FGAConstants.Sesion.Nombre);

                // Periodo prioritario: enviado explícitamente desde la UI de la pantalla activa
                string periodo = !string.IsNullOrWhiteSpace(periodoContexto) ? periodoContexto.Trim() : null;

                if (string.IsNullOrEmpty(periodo))
                {
                    periodo = Session["Periodo2"]?.ToString() 
                           ?? Session["Periodo"]?.ToString() 
                           ?? Session["Periodo1"]?.ToString() 
                           ?? "";
                }

                // Consultar fecha de cierre oficial de la entidad vía DBService para anclar a datos contables existentes
                DateTime maxFechaDatos = DateTime.MinValue;
                try
                {
                    var fc = Utility.WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, DateTime>(
                        c => c.FGA_Consultar_FechaCierre(entidadId)
                    );
                    if (fc > new DateTime(2000, 1, 1))
                    {
                        // FGA_Consultar_FechaCierre retorna DATEADD(MONTH, 1, MAX(PERIODO)), por lo que el último mes cerrado con datos es fc.AddMonths(-1)
                        DateTime mesCorte = fc.AddMonths(-1);
                        maxFechaDatos = new DateTime(mesCorte.Year, mesCorte.Month, 1);
                    }
                }
                catch (Exception exFc)
                {
                    System.Diagnostics.Trace.TraceWarning("ChatbotController: Error al consultar fecha de cierre: " + exFc.Message);
                }

                if (maxFechaDatos == DateTime.MinValue)
                {
                    maxFechaDatos = new DateTime(2026, 8, 1);
                }

                // Parsear periodo de forma segura reconociendo formatos MM/yyyy
                DateTime dtPeriodoParsed = ChatbotDataService.ParsearFechaPeriodoSegura(periodo);

                // Si no hay periodo o la fecha solicitada supera la fecha de cierre oficial de la entidad, anclar al último corte con datos
                if (dtPeriodoParsed == DateTime.MinValue || dtPeriodoParsed > maxFechaDatos)
                {
                    periodo = maxFechaDatos.ToString("dd/MM/yyyy");
                }

                // 4. Construcción del Prompt de Sistema con contexto dinámico de fecha e indicadores
                string promptSistema = ChatbotContextBuilder.ConstruirPromptSistema(entidadId, entidadNombre, usuario, mensaje, periodo, pantallaContexto);

                // 5. Invocación del proveedor de IA
                string respuesta = await _aiProvider.GenerarRespuestaAsync(promptSistema, historial, mensaje.Trim());

                if (string.IsNullOrWhiteSpace(respuesta))
                {
                    respuesta = "No pude generar una respuesta en este momento. Por favor intenta de nuevo en unos instantes.";
                }

                return Json(new
                {
                    success = true,
                    respuesta = respuesta,
                    entidad = entidadNombre
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    respuesta = "Ocurrió un inconveniente al procesar tu solicitud: " + ex.Message
                });
            }
        }

        [HttpGet]
        public JsonResult Estado()
        {
            bool habilitado = Env.IsChatbotEnabled();
            string entidad = Env.GetUserInfo("entidad");

            return Json(new
            {
                habilitado = habilitado,
                entidad = entidad
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
