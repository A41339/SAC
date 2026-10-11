using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using FGA.Services.Chatbot;

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
        public async Task<JsonResult> Preguntar(string mensaje, string historial, string periodoContexto = null)
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

                // 3. Extracción de contexto de sesión seguro
                string entidadId = Env.GetUserInfo("identidad");
                if (string.IsNullOrEmpty(entidadId) && Session["IdEntidad"] != null)
                {
                    entidadId = Session["IdEntidad"].ToString();
                }

                string entidadNombre = Env.GetUserInfo("entidad");
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

                // Si aún no hay periodo o si la entidad está identificada, consultar su fecha de cierre oficial vía DBService
                if (!string.IsNullOrEmpty(entidadId))
                {
                    try
                    {
                        var fc = Utility.WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, DateTime>(
                            c => c.FGA_Consultar_FechaCierre(entidadId)
                        );
                        if (fc > new DateTime(2000, 1, 1))
                        {
                            // Si el periodo actual de sesión no existe o es mayor que el último cierre real con datos, anclar al cierre real
                            if (string.IsNullOrEmpty(periodo))
                            {
                                periodo = fc.ToShortDateString();
                            }
                            else if (DateTime.TryParse(periodo, out DateTime dtP) && dtP > fc)
                            {
                                periodo = fc.ToShortDateString();
                            }
                        }
                    }
                    catch { }
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
