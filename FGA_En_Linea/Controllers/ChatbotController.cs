using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FGA.Services.Chatbot;
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
        public async Task<JsonResult> Preguntar(string mensaje, string historial, string periodoContexto = null, string entidadContexto = null)
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

                if (!string.IsNullOrWhiteSpace(entidadContexto) && entidadContexto != "0" && entidadContexto != "-1")
                {
                    entidadId = entidadContexto.Trim();
                }

                if (string.IsNullOrWhiteSpace(entidadId) && Session["IdEntidad"] != null)
                {
                    string sId = Session["IdEntidad"].ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(sId) && sId != "0" && sId != "-1")
                    {
                        entidadId = sId;
                    }
                }

                if (string.IsNullOrWhiteSpace(entidadId))
                {
                    string uEnt = Env.GetUserInfo("identidad");
                    if (!string.IsNullOrWhiteSpace(uEnt) && uEnt != "0" && uEnt != "-1")
                    {
                        entidadId = uEnt.Trim();
                    }
                }

                // Si es un usuario Administrador FFC o no hay cooperativa fija en sesión, obtener la primera entidad activa válida
                if (string.IsNullOrWhiteSpace(entidadId) || entidadId == "0" || entidadId == "-1")
                {
                    try
                    {
                        var listaEnt = Utility.WcfHelper.Execute<EntidadServiceClient, FGA.Models.Entidad[]>(
                            c => c.GetAll()
                        );
                        var primera = listaEnt?.FirstOrDefault(e => e.Id != "0" && e.Id != "-1" && e.Activo == true);
                        entidadId = primera != null ? primera.Id : "2";
                    }
                    catch
                    {
                        entidadId = "2";
                    }
                }

                Session["IdEntidad"] = entidadId;

                // Nombre de la Entidad
                string entidadNombre = Session["NomEntidad"]?.ToString();
                if (string.IsNullOrWhiteSpace(entidadNombre) || entidadNombre.Equals("FGA", StringComparison.OrdinalIgnoreCase) || entidadNombre.Equals("FFC", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var entObj = Utility.WcfHelper.Execute<EntidadServiceClient, FGA.Models.Entidad>(
                            c => c.Get(entidadId)
                        );
                        if (entObj != null && !string.IsNullOrWhiteSpace(entObj.Nombre))
                        {
                            entidadNombre = entObj.Nombre;
                            Session["NomEntidad"] = entidadNombre;
                        }
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(entidadNombre))
                {
                    entidadNombre = Env.GetUserInfo("entidad") ?? "Entidad Cooperativa";
                }

                string usuario = Env.GetUserInfo("name");

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
                        int uDia = DateTime.DaysInMonth(mesCorte.Year, mesCorte.Month);
                        maxFechaDatos = new DateTime(mesCorte.Year, mesCorte.Month, uDia);
                    }
                }
                catch { }

                // Si no hay periodo, o si el periodo recibido excede la fecha máxima con datos, o es del año actual desfasado (>= 2025)
                if (maxFechaDatos > DateTime.MinValue)
                {
                    if (string.IsNullOrEmpty(periodo))
                    {
                        periodo = maxFechaDatos.ToString("dd/MM/yyyy");
                    }
                    else if (DateTime.TryParse(periodo, out DateTime dtP))
                    {
                        if (dtP > maxFechaDatos || (dtP.Year >= 2025 && maxFechaDatos.Year < 2025))
                        {
                            periodo = maxFechaDatos.ToString("dd/MM/yyyy");
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(periodo) || (DateTime.TryParse(periodo, out DateTime dtFallback) && dtFallback.Year >= 2025))
                    {
                        periodo = "31/12/2024";
                    }
                }

                // 4. Construcción del Prompt de Sistema con contexto dinámico de fecha e indicadores
                string promptSistema = ChatbotContextBuilder.ConstruirPromptSistema(entidadId, entidadNombre, usuario, mensaje, periodo);

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
