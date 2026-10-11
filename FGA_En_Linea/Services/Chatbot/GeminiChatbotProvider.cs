using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace FGA.Services.Chatbot
{
    public class GeminiChatbotProvider : IChatbotAiProvider
    {
        public async Task<string> GenerarRespuestaAsync(string promptSistema, string historial, string preguntaUsuario)
        {
            string apiKey = ConfigurationManager.AppSettings["Chatbot_ApiKey"];
            string modelo = ConfigurationManager.AppSettings["Chatbot_Model"] ?? "gemini-2.5-flash";

            // Si no está en Web.config, intentar obtener de DBService (tabla dbo.Parametros)
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = ObtenerParametro("CHATBOT_API_KEY");
            }

            // Si hay API Key configurada, llamar a la API de Gemini
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    string respuestaApi = await LlamarGeminiApiAsync(apiKey, modelo, promptSistema, preguntaUsuario);
                    if (!string.IsNullOrWhiteSpace(respuestaApi))
                    {
                        return respuestaApi;
                    }
                }
                catch (Exception ex)
                {
                    // Si falla por red o cuota, recurrir al motor analítico local
                    System.Diagnostics.Trace.WriteLine("Error al invocar Gemini API: " + ex.Message);
                }
            }

            // Motor analítico local / Asistente FFC de contingencia
            return GenerarRespuestaMotorLocal(promptSistema, preguntaUsuario);
        }

        private async Task<string> LlamarGeminiApiAsync(string apiKey, string modelo, string promptSistema, string preguntaUsuario)
        {
            return await Task.Run(() =>
            {
                // Forzar TLS 1.2+
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                string url = string.Format("https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}", modelo, apiKey);

                var jsonRequest = new JObject
                {
                    ["system_instruction"] = new JObject
                    {
                        ["parts"] = new JArray
                        {
                            new JObject { ["text"] = promptSistema }
                        }
                    },
                    ["contents"] = new JArray
                    {
                        new JObject
                        {
                            ["role"] = "user",
                            ["parts"] = new JArray
                            {
                                new JObject { ["text"] = preguntaUsuario }
                            }
                        }
                    },
                    ["generationConfig"] = new JObject
                    {
                        ["temperature"] = 0.2,
                        ["maxOutputTokens"] = 1200
                    }
                };

                byte[] postBytes = Encoding.UTF8.GetBytes(jsonRequest.ToString());

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = postBytes.Length;
                request.Timeout = 15000; // 15 segundos timeout

                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string jsonResponse = reader.ReadToEnd();
                    var jObj = JObject.Parse(jsonResponse);
                    var candidates = jObj["candidates"] as JArray;
                    if (candidates != null && candidates.Count > 0)
                    {
                        var parts = candidates[0]?["content"]?["parts"] as JArray;
                        if (parts != null && parts.Count > 0)
                        {
                            return parts[0]?["text"]?.ToString();
                        }
                    }
                }

                return null;
            });
        }

        private string GenerarRespuestaMotorLocal(string promptSistema, string pregunta)
        {
            string p = (pregunta ?? "").ToLower().Trim();

            // 1. Si el prompt contiene evolución histórica mes a mes (ej. "últimos 3 meses")
            if (!string.IsNullOrEmpty(promptSistema) && promptSistema.Contains("EVOLUCIÓN HISTÓRICA MES A MES"))
            {
                string[] lines = promptSistema.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string refLine = lines.FirstOrDefault(l => l.IndexOf("Referencia utilizada:", StringComparison.OrdinalIgnoreCase) >= 0) ?? "";

                string palabraClave = null;
                if (p.Contains("suficiencia")) palabraClave = "Suficiencia Patrimonial";
                else if (p.Contains("eficiencia") || p.Contains("costo")) palabraClave = "Eficiencia Operativa";
                else if (p.Contains("mora") || p.Contains("vencid")) palabraClave = "Morosidad";
                else if (p.Contains("resultado") || p.Contains("excedente") || p.Contains("pérdida acumulada") || p.Contains("perdida acumulada")) palabraClave = "Resultado del período";
                else if (p.Contains("cartera") || p.Contains("crédito") || p.Contains("credito")) palabraClave = "Cartera de crédito";
                else if (p.Contains("obligacion") || p.Contains("público") || p.Contains("publico") || p.Contains("depósito") || p.Contains("deposito") || p.Contains("captaci")) palabraClave = "Obligaciones con el público";
                else if (p.Contains("margen financiero") || p.Contains("margen")) palabraClave = "Margen";
                else if (p.Contains("rentabilidad") || p.Contains("patrimonio")) palabraClave = "Rentabilidad";
                else if (p.Contains("pérdida esperada") || p.Contains("perdida esperada")) palabraClave = "Pérdida Esperada";
                else if (p.Contains("inversionista") || p.Contains("concentraci")) palabraClave = "Concentración";
                else if (p.Contains("disponibilidad") || p.Contains("vista")) palabraClave = "Disponibilidades";
                else if (p.Contains("activo")) palabraClave = "Activo";
                else if (p.Contains("roa")) palabraClave = "ROA";
                else if (p.Contains("roe")) palabraClave = "ROE";

                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(palabraClave))
                {
                    sb.AppendLine(string.Format("Tu evolución de **{0}** ({1}):", palabraClave, refLine.Replace("Referencia utilizada: ", "").Trim()));
                    sb.AppendLine();

                    string corteActual = "";
                    foreach (var line in lines)
                    {
                        if (line.Trim().StartsWith("▶ CORTE:"))
                        {
                            corteActual = line.Trim().Replace("▶ CORTE: ", "").TrimEnd(':');
                        }
                        else if (!string.IsNullOrEmpty(corteActual) && line.IndexOf(palabraClave, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            sb.AppendLine(string.Format("• **{0}**: {1}", corteActual, line.Trim().TrimStart('•', ' ')));
                        }
                    }
                    sb.AppendLine();
                    sb.AppendLine("¿Deseas que analicemos algún otro periodo o indicador?");
                    return sb.ToString();
                }
            }

            // 2. Si el prompt contiene datos oficiales consultados de DBService para corte específico
            if (!string.IsNullOrEmpty(promptSistema) && promptSistema.Contains("DATOS OFICIALES DE LA ENTIDAD"))
            {
                string[] lines = promptSistema.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var matchingLines = new System.Collections.Generic.List<string>();

                if (p.Contains("suficiencia")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Suficiencia", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("roa")) matchingLines.AddRange(lines.Where(l => l.IndexOf("ROA", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("roe")) matchingLines.AddRange(lines.Where(l => l.IndexOf("ROE", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("cartera")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Cartera", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("activo")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Activo", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("depósito") || p.Contains("deposito") || p.Contains("captaci")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Depósitos", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("mora") || p.Contains("vencid")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Mora", StringComparison.OrdinalIgnoreCase) >= 0 || l.IndexOf("Vencida", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("margen")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Margen", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("capital")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Capital Social", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("excedente") || p.Contains("resultado") || p.Contains("pérdida") || p.Contains("perdida")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Excedentes", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("liquidez")) matchingLines.AddRange(lines.Where(l => l.IndexOf("Liquidez", StringComparison.OrdinalIgnoreCase) >= 0));
                if (p.Contains("camel")) matchingLines.AddRange(lines.Where(l => l.IndexOf("CAMEL", StringComparison.OrdinalIgnoreCase) >= 0));

                if (matchingLines.Count > 0)
                {
                    var corteLine = lines.FirstOrDefault(l => l.IndexOf("Corte consultado", StringComparison.OrdinalIgnoreCase) >= 0) ?? "";
                    var distinctMatches = matchingLines.Distinct().Take(8).ToList();
                    var sb = new StringBuilder();
                    sb.AppendLine("Para tu consulta " + (!string.IsNullOrEmpty(corteLine) ? "(" + corteLine.Replace("Corte consultado: ", "").Trim() + "):" : ":"));
                    sb.AppendLine();
                    foreach (var m in distinctMatches)
                    {
                        sb.AppendLine(m.Trim());
                    }
                    sb.AppendLine();
                    sb.AppendLine("¿Deseas profundizar en algún otro indicador o comparar contra otra fecha?");
                    return sb.ToString();
                }
            }

            // Análisis semántico de preguntas frecuentes utilizando el contexto ya recopilado en promptSistema
            if (p.Contains("suficiencia") || p.Contains("patrimonial"))
            {
                return "Tu **Suficiencia Patrimonial** es un indicador clave de solvencia ante el FFC y los entes de supervisión. Representa la capacidad de absorción de pérdidas mediante el patrimonio técnico. Puedes revisar el valor exacto de tu entidad en la tarjeta 10 del Tablero principal.";
            }

            if (p.Contains("alerta") || p.Contains("bandeja"))
            {
                return "La **Bandeja de Alertas Financieras** monitorea variaciones abruptas entre periodos en tus activos, pasivos y morosidad respecto a los umbrales regulatorios. Puedes acceder al detalle completo pulsando la campana en la barra superior o desde el módulo de Alertas.";
            }

            if (p.Contains("pérdida") || p.Contains("perdida") || p.Contains("esperada"))
            {
                return "La **Pérdida Esperada** se calcula a partir de las matrices de transición crediticia según la probabilidad de incumplimiento y severidad. Refleja la provisión requerida estimada sobre tu cartera expuesta.";
            }

            if (p.Contains("fondeo") || p.Contains("financiamiento") || p.Contains("excel"))
            {
                return "En el módulo de **Estructura Financiera > Estructura de Fondeo** puedes consultar la composición de tus captaciones e inversionistas. Puedes exportar la información detallada pulsando el botón **Exportar Excel** ubicado en la esquina superior derecha.";
            }

            if (p.Contains("sbr") || p.Contains("evaluación") || p.Contains("autoevaluación") || p.Contains("remediación"))
            {
                return "El módulo de **Evaluación SBR (Supervisión Basada en Riesgos)** contiene la **Autoevaluación** institucional para medir brechas de riesgo y el **Plan de Remediación** para calendarizar acciones correctivas. Recuerda que este ejercicio es confidencial y formativo.";
            }

            if (p.Contains("indicador") || p.Contains("kpi") || p.Contains("resumen") || p.Contains("tablero") || p.Contains("como está mi") || p.Contains("como esta mi"))
            {
                return "En tu **Tablero Principal** tienes a tu disposición los 11 indicadores oficiales: Total Activo, Cartera Bruta, Cartera Neta, Depósitos Totales, Cartera Vencida, Margen de Intermediación, ROA, ROE, Capital Social, Suficiencia Patrimonial y Excedentes/Pérdidas. Puedes filtrar por cualquier periodo de corte en la barra de selección superior.";
            }

            return "¡Hola! Soy tu **Asistente Virtual Financiero del SAC**. Puedo ayudarte a interpretar tus 11 indicadores del tablero, explicarte tus alertas financieras, guiarte en el módulo de Evaluación SBR o consultar la estructura de fondeo. ¿En qué aspecto de tu entidad deseas que profundicemos hoy?";
        }

        private string ObtenerParametro(string llave)
        {
            try
            {
                var lista = Utility.WcfHelper.Execute<FGA_En_Linea.ParametrosService.ParametrosServiceClient, FGA.Models.Parametros[]>(
                    c => c.GetAll()
                );
                if (lista != null)
                {
                    var param = System.Linq.Enumerable.FirstOrDefault(lista, p => string.Equals(p.Llave, llave, StringComparison.OrdinalIgnoreCase));
                    return param?.Valor?.Trim();
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
