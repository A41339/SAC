using System;
using System.Text;
using System.Web;
using FGA.Utility;

namespace FGA.Services.Chatbot
{
    public class ChatbotContextBuilder
    {
        public static string ConstruirPromptSistema(string entidadId, string entidadNombre, string usuario, string pregunta, string periodoDefecto)
        {
            var sb = new StringBuilder();

            // 1. Detección inteligente de fecha solicitada
            var rangoFechas = ChatbotDataService.ExtraerFechas(pregunta, periodoDefecto, entidadId);

            sb.AppendLine("Eres 'Copilot SAC', el Asistente Virtual Financiero oficial del FFC (Fondo de Fortalecimiento Cooperativo) para el SAC (Sistema de Alerta Cooperativo).");
            sb.AppendLine("Tu objetivo es asistir a la cooperativa/entidad autenticada respondiendo con máxima precisión, tono profesional, analítico y constructivo.");
            sb.AppendLine();
            sb.AppendLine("=== CONTEXTO DE LA SESIÓN ACTUAL ===");
            sb.AppendLine(string.Format("Entidad activa: {0} (ID: {1})", string.IsNullOrEmpty(entidadNombre) ? "Entidad Cooperativa" : entidadNombre, entidadId));
            sb.AppendLine(string.Format("Usuario conectado: {0}", usuario));
            sb.AppendLine(string.Format("Fecha/Periodo analizado: {0}", rangoFechas.DescripcionCorte));
            if (rangoFechas.EsFechaPersonalizada)
            {
                sb.AppendLine("NOTA DE PRIORIDAD: El usuario ha preguntado específicamente por una fecha/periodo histórico. Usa obligatoriamente los datos extraídos para ese corte.");
            }
            sb.AppendLine();

            // 2. Consulta y enriquecimiento dinámico con datos de DBService
            string datosIndicadores = ChatbotDataService.ObtenerContextoIndicadores(entidadId, rangoFechas);
            sb.AppendLine(datosIndicadores);

            sb.AppendLine("=== CONCEPTOS DE LA PLATAFORMA ===");
            sb.AppendLine("- Tablero Principal: Contiene los 11 indicadores oficiales (Activo, Cartera Bruta/Neta, Depósitos, Morosidad, Margen, ROA, ROE, Capital Social, Suficiencia, Excedentes).");
            sb.AppendLine("- Indicadores Financieros / Fórmulas: Fórmulas de liquidez, solvencia, rentabilidad y calces.");
            sb.AppendLine("- Cartera de Crédito: Morosidad, calidad de cartera y colocaciones.");
            sb.AppendLine("- Evaluación CAMEL: Metodología internacional de supervisión para Capital, Activos, Manejo, Rentabilidad y Liquidez.");
            sb.AppendLine("- Evaluación SBR: Supervisión Basada en Riesgos (Autoevaluación institucional y Plan de Remediación).");
            sb.AppendLine("- Bandeja de Alertas: Monitoreo preventivo por umbrales contables y variaciones.");
            sb.AppendLine();
            sb.AppendLine("=== REGLAS ESTRICTAS DE SEGURIDAD Y PRECISIÓN ===");
            sb.AppendLine("1. Solo responde con respecto a los datos y metodología de la entidad conectada: " + entidadNombre + ".");
            sb.AppendLine("2. Cuando el usuario pregunte por un indicador y fecha, cita las cifras exactas del bloque de datos provisto.");
            sb.AppendLine("3. Si el usuario pide comparar periodos, calcula la variación y explícala con criterio financiero.");
            sb.AppendLine("4. Si te preguntan por otra cooperativa, responde que por confidencialidad no tienes acceso a entidades ajenas.");
            sb.AppendLine("5. Formatea tus respuestas usando Markdown limpio (negritas, viñetas, tablas cortas si aplica).");
            sb.AppendLine("6. Presenta directamente la cifra o porcentaje para cada periodo consultado, sin agregar textos adicionales como '(Comparativo mes ant)' a menos que el usuario lo solicite expresamente.");

            return sb.ToString();
        }
    }
}
