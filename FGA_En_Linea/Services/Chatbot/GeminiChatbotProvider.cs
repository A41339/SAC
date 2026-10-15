using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static string NormalizarTexto(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt)) return "";
            string s = txt.ToLower().Trim();
            s = s.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
            return s;
        }

        private string GenerarRespuestaMotorLocal(string promptSistema, string pregunta)
        {
            string p = NormalizarTexto(pregunta);

            // 0. Si el prompt contiene consulta específica de cuenta contable (ej. "cuenta 31000000")
            if (!string.IsNullOrEmpty(promptSistema) && promptSistema.Contains("CONSULTA ESPECÍFICA DE LA CUENTA CONTABLE SOLICITADA"))
            {
                return FormatearRespuestaCuentaContable(promptSistema);
            }

            // 1. Si el prompt contiene evolución histórica mes a mes (ej. "últimos 3 meses")
            if (!string.IsNullOrEmpty(promptSistema) && promptSistema.Contains("EVOLUCIÓN HISTÓRICA MES A MES"))
            {
                return FormatearRespuestaEvolucionHistorica(promptSistema, p);
            }

            // Aislar EXCLUSIVAMENTE el bloque de datos oficiales para evitar fugas de instrucciones internas del sistema
            string bloqueDatos = promptSistema ?? "";
            int idxConceptos = bloqueDatos.IndexOf("=== CONCEPTOS CLAVE", StringComparison.OrdinalIgnoreCase);
            if (idxConceptos >= 0)
            {
                bloqueDatos = bloqueDatos.Substring(0, idxConceptos);
            }

            // Extraer cortes
            string corteStr = "el periodo consultado";
            string compStr = "el mes anterior";
            var mCorte = Regex.Match(bloqueDatos, @"Corte consultado:\s*([0-9]{2}/[0-9]{4})");
            if (mCorte.Success) corteStr = mCorte.Groups[1].Value;
            var mComp = Regex.Match(bloqueDatos, @"Periodo comparativo:\s*([0-9]{2}/[0-9]{4})");
            if (mComp.Success) compStr = mComp.Groups[1].Value;

            // Extraer mapa de indicadores parseados
            var indicadores = ParsearIndicadoresBloque(bloqueDatos);

            // 2. Detección de indicador específico solicitado por el usuario
            // Caso SUFICIENCIA PATRIMONIAL
            if (p.Contains("suficiencia"))
            {
                return ResponderSuficiencia(indicadores, corteStr, compStr, p);
            }

            // Caso MOROSIDAD
            if (p.Contains("mora") || p.Contains("moro") || p.Contains("vencid"))
            {
                return ResponderMorosidad(indicadores, corteStr, compStr, p);
            }

            // Caso CARTERA DE CRÉDITO
            if (p.Contains("cartera") || p.Contains("credito"))
            {
                return ResponderCartera(indicadores, corteStr, compStr, p);
            }

            // Caso OBLIGACIONES CON EL PÚBLICO / DEPÓSITOS
            if (p.Contains("obligaci") || p.Contains("publico") || p.Contains("deposito") || p.Contains("captaci"))
            {
                return ResponderObligaciones(indicadores, corteStr, compStr, p);
            }

            // Caso CAPITAL SOCIAL
            if (p.Contains("capital social") || (p.Contains("capital") && !p.Contains("nivel") && !p.Contains("comun")))
            {
                return ResponderCapitalSocial(indicadores, corteStr, compStr, p);
            }

            // Caso RESULTADO DEL PERÍODO / EXCEDENTES / UTILIDAD
            if (p.Contains("resultado") || p.Contains("excedente") || p.Contains("utilidad") || p.Contains("perdida"))
            {
                return ResponderResultado(indicadores, corteStr, compStr, p);
            }

            // Caso EFICIENCIA OPERATIVA / COSTO ADMINISTRATIVO
            if (p.Contains("eficiencia") || p.Contains("costo administrativo") || p.Contains("gasto administrativo"))
            {
                return ResponderEficiencia(indicadores, corteStr, compStr, p);
            }

            // Caso APALANCAMIENTO SUGEF 3-06
            if (p.Contains("apalancamiento"))
            {
                return ResponderApalancamiento(indicadores, corteStr, compStr, p);
            }

            // Caso PATRIMONIO TOTAL
            if (p.Contains("patrimonio"))
            {
                return ResponderPatrimonio(indicadores, corteStr, compStr, p);
            }

            // Caso ACTIVO TOTAL
            if (p.Contains("activo total") || p.Contains("activo"))
            {
                return ResponderActivo(indicadores, corteStr, compStr, p);
            }

            // Caso BANDEJA DE ALERTAS
            if (p.Contains("alerta") || p.Contains("bandeja"))
            {
                return ResponderAlertas(bloqueDatos, corteStr);
            }

            // Caso CONCENTRACIÓN DE FONDEO / MAYORES AHORRANTES
            if (p.Contains("fondeo") || p.Contains("concentraci") || p.Contains("ahorrante") || p.Contains("inversionista"))
            {
                return ResponderConcentracion(bloqueDatos, corteStr);
            }

            // Caso RESUMEN GENERAL / TABLERO / ESTADO DE LA ENTIDAD
            if (p.Contains("indicador") || p.Contains("resumen") || p.Contains("tablero") || p.Contains("como esta") || p.Contains("situac") || p.Contains("general") || p.Contains("cifra") || p.Contains("dato"))
            {
                return ResponderResumenGeneral(indicadores, corteStr, compStr);
            }

            // Caso PREGUNTAS METODOLÓGICAS O CONCEPTUALES
            if (p.Contains("sbr") || p.Contains("supervision basada") || p.Contains("remediacion"))
            {
                return "La metodología **SBR (Supervisión Basada en Riesgos)** evalúa el perfil de riesgo institucional a partir de dos dimensiones: los **Riesgos Inherentes** (Crédito, Liquidez, Mercado, Operacional, Gobernanza) y la efectividad de los **Mitigadores y Controles Internos**. Si se detectan brechas, se formaliza un **Plan de Remediación** con causas raíz y acciones correctivas calendarizadas.\n\n[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]\n[Sugerencia: ¿Cuál es el índice de morosidad mayor a 90 días?]";
            }

            if (p.Contains("camel"))
            {
                return "La evaluación **CAMEL** analiza la solidez de la cooperativa en 5 dimensiones fundamentales:\n• **C (Capital):** Solvencia y suficiencia patrimonial para absorber pérdidas.\n• **A (Activos):** Calidad crediticia de la cartera, morosidad y coberturas.\n• **M (Manejo/Management):** Eficiencia operativa y gobierno institucional.\n• **E (Excedentes/Earnings):** Capacidad de generación de excedentes y rentabilidad.\n• **L (Liquidez):** Calce de plazos y disponibilidad para cubrir retiros.\n\n[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]\n[Sugerencia: ¿Cómo cerró la morosidad mayor a 90 días?]\n[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]";
            }

            // Saludo por defecto
            return "¡Hola! Soy tu **Asistente SAC**. Estoy a tu disposición para informarte con precisión oficial sobre tus indicadores financieros, cartera, obligaciones, morosidad, alertas y solvencia patrimonial. ¿Qué indicador o consulta deseas revisar?\n\n[Sugerencia: ¿Cómo cerró mi suficiencia patrimonial y cuál es su tendencia?]\n[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]\n[Sugerencia: Resumen de indicadores del tablero]";
        }

        private class IndicadorItem
        {
            public string Nombre { get; set; }
            public string ValorActualStr { get; set; }
            public decimal ValorActual { get; set; }
            public string ValorAnteriorStr { get; set; }
            public decimal? ValorAnterior { get; set; }
            public bool EsPorcentaje { get; set; }
        }

        private static decimal ParsearDecimalSeguro(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return 0;
            string limpio = Regex.Replace(str, @"[^\d,\.\-]", "").Trim();
            if (string.IsNullOrEmpty(limpio)) return 0;

            int lastDot = limpio.LastIndexOf('.');
            int lastComma = limpio.LastIndexOf(',');

            if (lastDot >= 0 && lastComma >= 0)
            {
                if (lastDot > lastComma)
                {
                    // Formato 139,775.19 -> quitar comas
                    limpio = limpio.Replace(",", "");
                }
                else
                {
                    // Formato 139.775,19 -> quitar puntos y cambiar coma por punto
                    limpio = limpio.Replace(".", "").Replace(",", ".");
                }
            }
            else if (lastComma >= 0)
            {
                // Solo coma: ej 19,85
                limpio = limpio.Replace(",", ".");
            }

            decimal res;
            if (decimal.TryParse(limpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out res))
            {
                return res;
            }
            return 0;
        }

        private Dictionary<string, IndicadorItem> ParsearIndicadoresBloque(string bloqueDatos)
        {
            var dic = new Dictionary<string, IndicadorItem>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(bloqueDatos)) return dic;

            string[] lines = bloqueDatos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (!line.StartsWith("•") && !line.StartsWith("-")) continue;
                line = line.TrimStart('•', '-', ' ').Trim();

                int idxColon = line.IndexOf(':');
                if (idxColon <= 0) continue;

                string nombre = line.Substring(0, idxColon).Trim();
                if (nombre.StartsWith("["))
                {
                    int idxCierre = nombre.IndexOf(']');
                    if (idxCierre >= 0 && idxCierre < nombre.Length - 1)
                    {
                        nombre = nombre.Substring(idxCierre + 1).Trim();
                    }
                }
                if (nombre.StartsWith("CAMEL", StringComparison.OrdinalIgnoreCase))
                {
                    nombre = nombre.Substring(5).Trim('-', ' ');
                }

                string resto = line.Substring(idxColon + 1).Trim();
                string valorActualStr = resto;
                string valorAntStr = null;

                var mAnt = Regex.Match(resto, @"\((?:Anterior|Comp)[^:]*:\s*([^)]+)\)", RegexOptions.IgnoreCase);
                if (mAnt.Success)
                {
                    valorAntStr = mAnt.Groups[1].Value.Trim();
                    valorActualStr = resto.Substring(0, mAnt.Index).Trim();
                }

                bool esPct = valorActualStr.Contains("%");
                decimal valActNum = ParsearDecimalSeguro(valorActualStr);
                decimal? valAntNum = !string.IsNullOrEmpty(valorAntStr) ? ParsearDecimalSeguro(valorAntStr) : (decimal?)null;

                var item = new IndicadorItem
                {
                    Nombre = nombre,
                    ValorActualStr = valorActualStr,
                    ValorActual = valActNum,
                    ValorAnteriorStr = valorAntStr,
                    ValorAnterior = valAntNum,
                    EsPorcentaje = esPct
                };

                if (!dic.ContainsKey(nombre) || (dic[nombre].ValorActual == 0 && valActNum != 0))
                {
                    dic[nombre] = item;
                }
            }
            return dic;
        }

        private IndicadorItem BuscarIndicador(Dictionary<string, IndicadorItem> dic, string termino)
        {
            if (dic == null || dic.Count == 0) return null;
            if (dic.ContainsKey(termino)) return dic[termino];

            string normTermino = NormalizarTexto(termino);
            foreach (var kvp in dic)
            {
                if (NormalizarTexto(kvp.Key).IndexOf(normTermino, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        private string ResponderSuficiencia(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Suficiencia Patrimonial") ?? BuscarIndicador(indicadores, "Suficiencia");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("Tu **Suficiencia Patrimonial** cerró en **{0}** para el corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis de tendencia:**");

                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    string signo = dif >= 0 ? "+" : "";
                    string sentido = dif > 0 ? "al alza (favorable)" : (dif < 0 ? "a la baja" : "estable");
                    sb.AppendLine(string.Format("• **Evolución:** Muestra una variación de **{0}{1:N2} puntos porcentuales** {2} respecto al mes anterior (**{3}** en {4}).", signo, dif, sentido, item.ValorAnteriorStr, compStr));
                }
                else
                {
                    sb.AppendLine(string.Format("• **Evolución:** Mantiene su registro oficial en **{0}** para el corte evaluado.", item.ValorActualStr));
                }

                if (item.ValorActual >= 10m)
                {
                    decimal holgura = item.ValorActual - 10m;
                    sb.AppendLine(string.Format("• **Posición regulatoria:** La entidad supera ampliamente el requerimiento normativo mínimo del **10,00%** normado por la SUGEF (Acuerdo SUGEF 3-06), ubicándose en una posición de adecuada solidez patrimonial con una holgura de **+{0:N2} p.p.**", holgura));
                }
                else
                {
                    sb.AppendLine("• **Posición regulatoria:** Se encuentra por debajo del requerimiento normativo mínimo del **10,00%** de la SUGEF, requiriendo seguimiento patrimonial prioritario.");
                }
            }
            else
            {
                sb.AppendLine("La **Suficiencia Patrimonial** es el ratio prudencial que mide la solvencia y solidez patrimonial de la entidad ante la SUGEF y el FFC, exigiendo un mínimo regulatorio del 10%.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la morosidad mayor a 90 días?]");
            sb.AppendLine("[Sugerencia: Ver evolución de los últimos 3 meses]");

            return sb.ToString();
        }

        private string ResponderMorosidad(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Morosidad mayor a 90") ?? BuscarIndicador(indicadores, "Morosidad") ?? BuscarIndicador(indicadores, "Vencida");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("La **Morosidad mayor a 90 días** cerró en **{0}** para el corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis de tendencia:**");

                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    string signo = dif >= 0 ? "+" : "";
                    string sentido = dif < 0 ? "favorable (reducción del atraso)" : (dif > 0 ? "aumento en la cartera atrasada" : "estable");
                    sb.AppendLine(string.Format("• **Evolución:** Muestra una variación de **{0}{1:N2} puntos porcentuales** ({2}) respecto al periodo anterior (**{3}** en {4}).", signo, dif, sentido, item.ValorAnteriorStr, compStr));
                }
                else
                {
                    sb.AppendLine(string.Format("• **Corte analizado:** Se registra en **{0}** al cierre de {1}.", item.ValorActualStr, corteStr));
                }

                if (item.ValorActual <= 3m)
                {
                    sb.AppendLine("• **Límite prudencial:** Se encuentra dentro del rango prudencial aceptable fijado por la SUGEF (máximo recomendado del **3,00%**), reflejando una sana calidad crediticia.");
                }
                else
                {
                    sb.AppendLine("• **Límite prudencial:** Supera el umbral prudencial del **3,00%** de la SUGEF, por lo que se recomienda intensificar las gestiones de cobranza y recuperación.");
                }
            }
            else
            {
                sb.AppendLine("El indicador de **Morosidad mayor a 90 días** refleja la proporción de créditos con atrasos superiores a 90 días respecto a la cartera total, con un umbral regulatorio recomendado no mayor al 3,00%.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
            sb.AppendLine("[Sugerencia: Ver alertas financieras]");

            return sb.ToString();
        }

        private string ResponderCartera(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Cartera de crédito") ?? BuscarIndicador(indicadores, "Cartera");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("El saldo de la **Cartera de crédito** cerró en **{0}** al corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis de tendencia:**");

                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    decimal porcVar = (dif / item.ValorAnterior.Value) * 100m;
                    string signo = dif >= 0 ? "+" : "";
                    string signoPorc = porcVar >= 0 ? "+" : "";
                    string sentido = dif > 0 ? "crecimiento neto de colocaciones" : (dif < 0 ? "amortización neta o contracción" : "saldo estable");
                    sb.AppendLine(string.Format("• **Evolución:** Registra una variación de **{0}₡{1:N2} M** ({2}{3:N2}%) respecto al mes anterior (**{4}** en {5}), reflejando un {6}.", signo, Math.Abs(dif), signoPorc, porcVar, item.ValorAnteriorStr, compStr, sentido));
                }
                sb.AppendLine("• **Importancia estratégica:** Constituye el principal activo productivo generador de ingresos de la entidad.");
            }
            else
            {
                sb.AppendLine("La **Cartera de crédito** representa la totalidad de los financiamientos otorgados por la cooperativa a sus asociados.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la morosidad mayor a 90 días?]");
            sb.AppendLine("[Sugerencia: ¿Cuánto tenemos en obligaciones con el público?]");
            sb.AppendLine("[Sugerencia: Ver evolución de los últimos 3 meses]");

            return sb.ToString();
        }

        private string ResponderObligaciones(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Obligaciones con el público") ?? BuscarIndicador(indicadores, "Obligaciones") ?? BuscarIndicador(indicadores, "Depósitos");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("Las **Obligaciones con el público** (captaciones y depósitos de asociados) cerraron en **{0}** al corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis de tendencia:**");

                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    decimal porcVar = (dif / item.ValorAnterior.Value) * 100m;
                    string signo = dif >= 0 ? "+" : "";
                    string signoPorc = porcVar >= 0 ? "+" : "";
                    string sentido = dif > 0 ? "captación neta positiva" : (dif < 0 ? "vencimiento de captaciones" : "saldo estable");
                    sb.AppendLine(string.Format("• **Evolución:** Variación de **{0}₡{1:N2} M** ({2}{3:N2}%) respecto al mes anterior (**{4}** en {5}), indicando {6}.", signo, Math.Abs(dif), signoPorc, porcVar, item.ValorAnteriorStr, compStr, sentido));
                }
                sb.AppendLine("• **Estabilidad de pasivos:** Es la principal fuente de fondeo institucional para la intermediación financiera.");
            }
            else
            {
                sb.AppendLine("Las **Obligaciones con el público** corresponden al total de recursos captados de asociados en ahorros a la vista, plazo fijo y aportaciones contractuales.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cuál es la concentración de los 10 mayores ahorrantes?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
            sb.AppendLine("[Sugerencia: Ver evolución de los últimos 3 meses]");

            return sb.ToString();
        }

        private string ResponderCapitalSocial(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Capital Social");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("El **Capital Social** de la entidad cerró en **{0}** al corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis de tendencia:**");

                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    decimal porcVar = (dif / item.ValorAnterior.Value) * 100m;
                    string signo = dif >= 0 ? "+" : "";
                    string signoPorc = porcVar >= 0 ? "+" : "";
                    sb.AppendLine(string.Format("• **Evolución:** Muestra una variación de **{0}₡{1:N2} M** ({2}{3:N2}%) respecto a {4} (**{5}**).", signo, Math.Abs(dif), signoPorc, porcVar, compStr, item.ValorAnteriorStr));
                }
                sb.AppendLine("• **Respaldo institucional:** Representa la aportación directa de los asociados y constituye la base estructural del patrimonio cooperativo.");
            }
            else
            {
                sb.AppendLine("El **Capital Social** representa el patrimonio fundacional aportado por los asociados de la cooperativa.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo del patrimonio total?]");
            sb.AppendLine("[Sugerencia: Ver evolución de los últimos 3 meses]");

            return sb.ToString();
        }

        private string ResponderResultado(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Resultado del período acumulado") ?? BuscarIndicador(indicadores, "Resultado") ?? BuscarIndicador(indicadores, "Excedentes");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("El **Resultado del período acumulado** cerró en **{0}** al corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis:**");
                if (item.ValorActual >= 0)
                {
                    sb.AppendLine("• **Desempeño:** La entidad genera **excedentes acumulados positivos** en el ejercicio económico analizado.");
                }
                else
                {
                    sb.AppendLine("• **Desempeño:** La entidad registra pérdidas acumuladas en el ejercicio económico analizado.");
                }
                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value != 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    string signo = dif >= 0 ? "+" : "";
                    sb.AppendLine(string.Format("• **Evolución:** Variación de **{0}₡{1:N2} M** respecto al periodo comparativo ({2}).", signo, Math.Abs(dif), item.ValorAnteriorStr ?? compStr));
                }
            }
            else
            {
                sb.AppendLine("El **Resultado del período acumulado** refleja la utilidad o pérdida neta generada durante el ejercicio fiscal en curso.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cuál es la eficiencia operativa?]");
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");

            return sb.ToString();
        }

        private string ResponderEficiencia(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Eficiencia Operativa");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("La **Eficiencia Operativa (Costo Administrativo)** cerró en **{0}** al corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis de tendencia:**");
                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    string signo = dif >= 0 ? "+" : "";
                    string sentido = dif < 0 ? "mejora (reducción de costos)" : (dif > 0 ? "aumento en la carga de gastos" : "estable");
                    sb.AppendLine(string.Format("• **Evolución:** Variación de **{0}{1:N2} p.p.** ({2}) respecto al periodo anterior (**{3}** en {4}).", signo, dif, sentido, item.ValorAnteriorStr, compStr));
                }
                sb.AppendLine("• **Interpretación:** Mide la proporción de gastos operativos sobre el activo total; un porcentaje menor refleja mayor optimización en la estructura de costos.");
            }
            else
            {
                sb.AppendLine("La **Eficiencia Operativa** evalúa los gastos administrativos incurridos para gestionar el volumen de activos de la cooperativa.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cuál fue el resultado del período acumulado?]");
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");

            return sb.ToString();
        }

        private string ResponderApalancamiento(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Apalancamiento");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("El ratio de **Apalancamiento (SUGEF 3-06)** cerró en **{0}** para el corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis:**");
                sb.AppendLine("• **Normativa prudencial:** Mide el nivel de respaldo que provee el capital común y aportaciones institucionales sobre los activos totales y contingentes según el Acuerdo SUGEF 3-06.");
            }
            else
            {
                sb.AppendLine("El ratio de **Apalancamiento SUGEF 3-06** relaciona la exposición total de activos con el capital base de la entidad.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo del patrimonio total?]");

            return sb.ToString();
        }

        private string ResponderPatrimonio(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Patrimonio Total") ?? BuscarIndicador(indicadores, "Patrimonio");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("El **Patrimonio Total** de la entidad cerró en **{0}** al corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis:**");
                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    string signo = dif >= 0 ? "+" : "";
                    sb.AppendLine(string.Format("• **Evolución:** Variación de **{0}₡{1:N2} M** respecto a {2} (**{3}**).", signo, Math.Abs(dif), compStr, item.ValorAnteriorStr));
                }
                sb.AppendLine("• **Respaldo institucional:** Agrupa el capital social, reservas patrimoniales y excedentes acumulados.");
            }
            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo del capital social?]");
            return sb.ToString();
        }

        private string ResponderActivo(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr, string pregunta)
        {
            var item = BuscarIndicador(indicadores, "Activo Total") ?? BuscarIndicador(indicadores, "Activo");

            var sb = new StringBuilder();
            if (item != null)
            {
                sb.AppendLine(string.Format("El **Activo Total** de la entidad cerró en **{0}** al corte de **{1}**.", item.ValorActualStr, corteStr));
                sb.AppendLine();
                sb.AppendLine("**Análisis:**");
                if (item.ValorAnterior.HasValue && item.ValorAnterior.Value > 0)
                {
                    decimal dif = item.ValorActual - item.ValorAnterior.Value;
                    string signo = dif >= 0 ? "+" : "";
                    sb.AppendLine(string.Format("• **Evolución:** Variación de **{0}₡{1:N2} M** respecto a {2} (**{3}**).", signo, Math.Abs(dif), compStr, item.ValorAnteriorStr));
                }
                sb.AppendLine("• **Composición principal:** Integrado mayoritariamente por la cartera de crédito y las disponibilidades líquidas.");
            }
            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
            sb.AppendLine("[Sugerencia: ¿Cuánto tenemos en obligaciones con el público?]");
            return sb.ToString();
        }

        private string ResponderAlertas(string bloqueDatos, string corteStr)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Bandeja de **Alertas Financieras** de la entidad ({0}):", corteStr));
            sb.AppendLine();

            string[] lines = bloqueDatos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            bool enBloqueAlertas = false;
            var lineasAlertas = new List<string>();

            foreach (var l in lines)
            {
                if (l.Contains("BANDEJA DE ALERTAS ACTIVAS"))
                {
                    enBloqueAlertas = true;
                    continue;
                }
                if (enBloqueAlertas)
                {
                    if (l.StartsWith("---") || l.StartsWith("===")) break;
                    if (l.Trim().StartsWith("•"))
                    {
                        lineasAlertas.Add(l.Trim());
                    }
                }
            }

            if (lineasAlertas.Count > 0 && !lineasAlertas[0].Contains("no presenta alertas"))
            {
                foreach (var al in lineasAlertas.Take(5))
                {
                    sb.AppendLine(al);
                }
            }
            else
            {
                sb.AppendLine("• La entidad no presenta alertas financieras activas ni desviaciones contables por umbrales en este momento.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la morosidad mayor a 90 días?]");
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");

            return sb.ToString();
        }

        private string ResponderConcentracion(string bloqueDatos, string corteStr)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Estructura de **Concentración de Fondeo y Ahorrantes** ({0}):", corteStr));
            sb.AppendLine();

            string[] lines = bloqueDatos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            bool enBloqueFondeo = false;
            var lineasFondeo = new List<string>();

            foreach (var l in lines)
            {
                if (l.Contains("CONCENTRACIÓN DE FONDEO Y AHORRANTES"))
                {
                    enBloqueFondeo = true;
                    continue;
                }
                if (enBloqueFondeo)
                {
                    if (l.StartsWith("---") || l.StartsWith("===")) break;
                    if (l.Trim().StartsWith("•"))
                    {
                        lineasFondeo.Add(l.Trim());
                    }
                }
            }

            if (lineasFondeo.Count > 0)
            {
                foreach (var lf in lineasFondeo)
                {
                    sb.AppendLine(lf);
                }
            }
            else
            {
                sb.AppendLine("• No se registran concentraciones críticas de ahorrantes en el periodo evaluado.");
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cuánto tenemos en obligaciones con el público?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");

            return sb.ToString();
        }

        private string ResponderResumenGeneral(Dictionary<string, IndicadorItem> indicadores, string corteStr, string compStr)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Resumen oficial de los principales indicadores de la entidad para el corte **{0}** (comparado con {1}):", corteStr, compStr));
            sb.AppendLine();

            string[] principales = new[]
            {
                "Suficiencia Patrimonial",
                "Cartera de crédito",
                "Obligaciones con el público",
                "Morosidad mayor a 90 días",
                "Resultado del período acumulado",
                "Capital Social"
            };

            foreach (var p in principales)
            {
                var item = BuscarIndicador(indicadores, p);
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.ValorAnteriorStr))
                    {
                        sb.AppendLine(string.Format("• **{0}:** {1} (Anterior: {2})", item.Nombre, item.ValorActualStr, item.ValorAnteriorStr));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("• **{0}:** {1}", item.Nombre, item.ValorActualStr));
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cómo cerró mi suficiencia patrimonial y cuál es su tendencia?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
            sb.AppendLine("[Sugerencia: Ver evolución de los últimos 3 meses]");

            return sb.ToString();
        }

        private string FormatearRespuestaCuentaContable(string promptSistema)
        {
            var sb = new StringBuilder();
            var mCta = Regex.Match(promptSistema, @"Cuenta consultada por el usuario:\s*([^\r\n]+)");
            string descCta = mCta.Success ? mCta.Groups[1].Value.Trim() : "Cuenta Contable";

            sb.AppendLine(string.Format("Saldos oficiales en el Balance de Comprobación para **{0}**:", descCta));
            sb.AppendLine();

            var matches = Regex.Matches(promptSistema, @"•\s*([0-9]{2}/[0-9]{4}):\s*(₡[0-9\.,]+\s*M)");
            decimal primerMonto = 0;
            decimal ultimoMonto = 0;
            int count = 0;

            foreach (Match m in matches)
            {
                string periodo = m.Groups[1].Value;
                string montoStr = m.Groups[2].Value;
                sb.AppendLine(string.Format("• **{0}**: {1}", periodo, montoStr));

                string numLimpio = m.Groups[2].Value.Replace("₡", "").Replace("M", "").Replace(" ", "").Trim();
                decimal val = ParsearDecimalSeguro(numLimpio);
                if (count == 0) primerMonto = val;
                ultimoMonto = val;
                count++;
            }

            if (count >= 2 && primerMonto > 0)
            {
                decimal dif = ultimoMonto - primerMonto;
                decimal pct = (dif / primerMonto) * 100m;
                string signo = dif >= 0 ? "+" : "";
                sb.AppendLine();
                sb.AppendLine(string.Format("**Variación neta del periodo analizado:** {0}₡{1:N2} M ({2}{3:N2}%).", signo, Math.Abs(dif), signo, pct));
            }

            sb.AppendLine();
            sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
            sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
            sb.AppendLine("[Sugerencia: Ver obligaciones con el público]");

            return sb.ToString();
        }

        private string FormatearRespuestaEvolucionHistorica(string promptSistema, string p)
        {
            string[] lines = promptSistema.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string refLine = lines.FirstOrDefault(l => l.IndexOf("Referencia utilizada:", StringComparison.OrdinalIgnoreCase) >= 0) ?? "";

            string palabraClave = null;
            if (p.Contains("suficiencia")) palabraClave = "Suficiencia Patrimonial";
            else if (p.Contains("eficiencia") || p.Contains("costo")) palabraClave = "Eficiencia Operativa";
            else if (p.Contains("mora") || p.Contains("vencid")) palabraClave = "Morosidad";
            else if (p.Contains("resultado") || p.Contains("excedente") || p.Contains("pérdida") || p.Contains("perdida")) palabraClave = "Resultado del período";
            else if (p.Contains("cartera") || p.Contains("crédito") || p.Contains("credito")) palabraClave = "Cartera de crédito";
            else if (p.Contains("obligacion") || p.Contains("público") || p.Contains("publico") || p.Contains("depósito") || p.Contains("deposito") || p.Contains("captaci")) palabraClave = "Obligaciones con el público";
            else if (p.Contains("capital")) palabraClave = "Capital Social";
            else if (p.Contains("activo")) palabraClave = "Activo Total";
            else if (p.Contains("patrimonio")) palabraClave = "Patrimonio Total";

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(palabraClave))
            {
                sb.AppendLine(string.Format("Tu evolución de **{0}** ({1}):", palabraClave, refLine.Replace("Referencia utilizada: ", "").Trim()));
                sb.AppendLine();

                string corteActual = "";
                decimal primerValor = 0;
                decimal ultimoValor = 0;
                int count = 0;
                bool esPct = false;

                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("▶ CORTE:"))
                    {
                        corteActual = line.Trim().Replace("▶ CORTE: ", "").TrimEnd(':');
                    }
                    else if (!string.IsNullOrEmpty(corteActual) && line.IndexOf(palabraClave, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string limpia = Regex.Replace(line.Trim().TrimStart('•', ' '), @"\s*\(Comparativo[^\)]*\)", "", RegexOptions.IgnoreCase);
                        sb.AppendLine(string.Format("• **{0}**: {1}", corteActual, limpia));

                        var mNum = Regex.Match(limpia, @":\s*(.+)");
                        if (mNum.Success)
                        {
                            string rawVal = mNum.Groups[1].Value.Trim();
                            if (rawVal.Contains("%")) esPct = true;
                            decimal num = ParsearDecimalSeguro(rawVal);
                            if (count == 0) primerValor = num;
                            ultimoValor = num;
                            count++;
                        }
                    }
                }

                if (count >= 2 && primerValor > 0)
                {
                    decimal dif = ultimoValor - primerValor;
                    string signo = dif >= 0 ? "+" : "";
                    sb.AppendLine();
                    if (esPct)
                    {
                        sb.AppendLine(string.Format("**Variación neta del periodo analizado:** {0}{1:N2} puntos porcentuales.", signo, dif));
                    }
                    else
                    {
                        decimal pct = (dif / primerValor) * 100m;
                        sb.AppendLine(string.Format("**Variación neta del periodo analizado:** {0}₡{1:N2} M ({2}{3:N2}%).", signo, Math.Abs(dif), signo, pct));
                    }
                }

                sb.AppendLine();
                sb.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
                sb.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
                sb.AppendLine("[Sugerencia: Ver obligaciones con el público]");
                return sb.ToString();
            }

            // Resumen general mes a mes
            var sbGen = new StringBuilder();
            sbGen.AppendLine(string.Format("Resumen histórico de indicadores ({0}):", refLine.Replace("Referencia utilizada: ", "").Trim()));
            sbGen.AppendLine();
            string corteActualGen = "";
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("▶ CORTE:"))
                {
                    corteActualGen = line.Trim().Replace("▶ CORTE: ", "").TrimEnd(':');
                    sbGen.AppendLine(string.Format("• **{0}**:", corteActualGen));
                }
                else if (!string.IsNullOrEmpty(corteActualGen) && (line.Contains("Suficiencia") || line.Contains("Cartera de crédito") || line.Contains("Obligaciones") || line.Contains("Resultado")))
                {
                    string limpia = Regex.Replace(line.Trim().TrimStart('•', ' '), @"\s*\(Comparativo[^\)]*\)", "", RegexOptions.IgnoreCase);
                    sbGen.AppendLine("   - " + limpia);
                }
            }
            sbGen.AppendLine();
            sbGen.AppendLine("[Sugerencia: ¿Cómo cerró la suficiencia patrimonial?]");
            sbGen.AppendLine("[Sugerencia: ¿Cuál es el saldo de la cartera de crédito?]");
            return sbGen.ToString();
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
