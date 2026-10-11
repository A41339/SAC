using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Entities.Entities.Procedures;
using FGA.Utility;

namespace FGA.Services.Chatbot
{
    public class RangoFechasConsulta
    {
        public DateTime FechaCorte { get; set; }
        public DateTime FechaComparar { get; set; }
        public bool EsFechaPersonalizada { get; set; }
        public bool EsSerieHistorica { get; set; }
        public int CantidadMesesSerie { get; set; }
        public List<DateTime> CortesHistoricos { get; set; } = new List<DateTime>();
        public string DescripcionCorte { get; set; }
    }

    public class ChatbotDataService
    {
        private static readonly Dictionary<string, int> MesesMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "enero", 1 }, { "ene", 1 },
            { "febrero", 2 }, { "feb", 2 },
            { "marzo", 3 }, { "mar", 3 },
            { "abril", 4 }, { "abr", 4 },
            { "mayo", 5 }, { "may", 5 },
            { "junio", 6 }, { "jun", 6 },
            { "julio", 7 }, { "jul", 7 },
            { "agosto", 8 }, { "ago", 8 },
            { "setiembre", 9 }, { "septiembre", 9 }, { "set", 9 }, { "sep", 9 },
            { "octubre", 10 }, { "oct", 10 },
            { "noviembre", 11 }, { "nov", 11 },
            { "diciembre", 12 }, { "dic", 12 }
        };

        public static RangoFechasConsulta ExtraerFechas(string pregunta, string periodoSesionDefecto, string idEntidad = null)
        {
            var resultado = new RangoFechasConsulta();

            // 1. Obtener la Fecha de Referencia base a partir del último cierre de la entidad o sesión
            DateTime fechaReferencia = ResolverFechaReferencia(periodoSesionDefecto, idEntidad);

            if (!string.IsNullOrWhiteSpace(pregunta))
            {
                string p = pregunta.Trim();

                // 2. Detección de expresiones de tiempo relativo ("últimos N meses", "último trimestre", etc.)
                int mesesRelativos = DetectarMesesRelativos(p);
                if (mesesRelativos > 0)
                {
                    resultado.EsFechaPersonalizada = true;
                    resultado.EsSerieHistorica = true;
                    resultado.CantidadMesesSerie = mesesRelativos;

                    // Generar la lista de cortes mensuales en orden cronológico (del más antiguo al más reciente)
                    for (int i = mesesRelativos - 1; i >= 0; i--)
                    {
                        DateTime mesDt = fechaReferencia.AddMonths(-i);
                        int ultimoDia = DateTime.DaysInMonth(mesDt.Year, mesDt.Month);
                        resultado.CortesHistoricos.Add(new DateTime(mesDt.Year, mesDt.Month, ultimoDia));
                    }

                    resultado.FechaCorte = fechaReferencia;
                    resultado.FechaComparar = resultado.CortesHistoricos.First();
                    resultado.DescripcionCorte = string.Format("Últimos {0} meses con base en la fecha de referencia {1:MM/yyyy} ({2:MM/yyyy} a {1:MM/yyyy})",
                        mesesRelativos, fechaReferencia, resultado.CortesHistoricos.First());

                    return resultado;
                }

                // 3. Detección de fechas específicas explícitas (Mes + Año o MM/yyyy)
                var fechasEncontradas = new List<DateTime>();

                // Patrón Nombre de mes + Año
                var regexMesAno = new Regex(@"(?i)\b(enero|febrero|marzo|abril|mayo|junio|julio|agosto|setiembre|septiembre|octubre|noviembre|diciembre|ene|feb|mar|abr|may|jun|jul|ago|set|sep|oct|nov|dic)\b\s*(?:del?|de)?\s*\b(20\d\d)\b");
                var matchesMesAno = regexMesAno.Matches(p);
                foreach (Match m in matchesMesAno)
                {
                    string mesStr = m.Groups[1].Value.ToLower();
                    if (MesesMap.TryGetValue(mesStr, out int mes) && int.TryParse(m.Groups[2].Value, out int ano))
                    {
                        int ultimoDia = DateTime.DaysInMonth(ano, mes);
                        var dt = new DateTime(ano, mes, ultimoDia);
                        if (!fechasEncontradas.Contains(dt))
                        {
                            fechasEncontradas.Add(dt);
                        }
                    }
                }

                // Patrón numérico MM/yyyy
                var regexNum = new Regex(@"\b(0?[1-9]|1[0-2])[\/\-](20\d\d)\b");
                var matchesNum = regexNum.Matches(p);
                foreach (Match m in matchesNum)
                {
                    if (int.TryParse(m.Groups[1].Value, out int mes) && int.TryParse(m.Groups[2].Value, out int ano))
                    {
                        int ultimoDia = DateTime.DaysInMonth(ano, mes);
                        var dt = new DateTime(ano, mes, ultimoDia);
                        if (!fechasEncontradas.Contains(dt))
                        {
                            fechasEncontradas.Add(dt);
                        }
                    }
                }

                if (fechasEncontradas.Count >= 2)
                {
                    fechasEncontradas = fechasEncontradas.OrderBy(f => f).ToList();
                    resultado.FechaComparar = fechasEncontradas[0];
                    resultado.FechaCorte = fechasEncontradas[1];
                    resultado.EsFechaPersonalizada = true;
                    resultado.DescripcionCorte = string.Format("{0:MM/yyyy} comparado con {1:MM/yyyy}", resultado.FechaCorte, resultado.FechaComparar);
                    return resultado;
                }
                else if (fechasEncontradas.Count == 1)
                {
                    resultado.FechaCorte = fechasEncontradas[0];
                    resultado.FechaComparar = resultado.FechaCorte.AddYears(-1);
                    resultado.EsFechaPersonalizada = true;
                    resultado.DescripcionCorte = string.Format("{0:MM/yyyy} (corte interanual contra {1:MM/yyyy})", resultado.FechaCorte, resultado.FechaComparar);
                    return resultado;
                }
            }

            // 4. Fallback por defecto: Fecha de referencia activa
            resultado.FechaCorte = fechaReferencia;
            resultado.FechaComparar = fechaReferencia.AddYears(-1);
            resultado.EsFechaPersonalizada = false;
            resultado.DescripcionCorte = string.Format("{0:MM/yyyy} (periodo de referencia activo)", resultado.FechaCorte);

            return resultado;
        }

        private static int DetectarMesesRelativos(string pregunta)
        {
            // Patrón numérico: "últimos 3 meses", "ultimos 3 meses", "3 meses", "en los 3 meses"
            var regexNum = new Regex(@"(?i)(?:últimos?|ultimos?|en\s+los?|hace)?\s*(\d{1,2})\s*(?:meses|mes)");
            var matchNum = regexNum.Match(pregunta);
            if (matchNum.Success && int.TryParse(matchNum.Groups[1].Value, out int n) && n >= 1 && n <= 36)
            {
                return n;
            }

            // Patrones semánticos en lenguaje natural
            if (Regex.IsMatch(pregunta, @"(?i)\b(?:un|1)\s+mes\b|último\s+mes|ultimo\s+mes")) return 1;
            if (Regex.IsMatch(pregunta, @"(?i)\b(?:dos|2)\s+meses\b")) return 2;
            if (Regex.IsMatch(pregunta, @"(?i)\b(?:tres|3)\s+meses\b|trimestre|trimestral")) return 3;
            if (Regex.IsMatch(pregunta, @"(?i)\b(?:cuatro|4)\s+meses\b")) return 4;
            if (Regex.IsMatch(pregunta, @"(?i)\b(?:cinco|5)\s+meses\b")) return 5;
            if (Regex.IsMatch(pregunta, @"(?i)\b(?:seis|6)\s+meses\b|semestre|semestral")) return 6;
            if (Regex.IsMatch(pregunta, @"(?i)\b(?:doce|12)\s+meses\b|último\s+año|ultimo\s+ano|anual")) return 12;

            return 0;
        }

        private static DateTime ResolverFechaReferencia(string periodoSesionDefecto, string idEntidad = null)
        {
            DateTime? fechaCierreReal = null;

            // 1. Consultar fecha de cierre contable oficial de la entidad vía DBService
            if (!string.IsNullOrWhiteSpace(idEntidad))
            {
                try
                {
                    DateTime fc = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, DateTime>(
                        c => c.FGA_Consultar_FechaCierre(idEntidad)
                    );
                    if (fc > new DateTime(2000, 1, 1))
                    {
                        fechaCierreReal = fc;
                    }
                }
                catch { }
            }

            DateTime refDate = DateTime.MinValue;

            // 2. Parsear el periodo recibido (de sesión o selector)
            if (!string.IsNullOrWhiteSpace(periodoSesionDefecto))
            {
                string limpia = periodoSesionDefecto.Trim();

                if (DateTime.TryParse(limpia, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dtParsed))
                {
                    refDate = dtParsed;
                }
                else if (DateTime.TryParseExact(limpia, new[] { "MM/yyyy", "M/yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                         CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtExact))
                {
                    refDate = dtExact;
                }
            }

            // 3. Si no hay periodo válido, tomar el cierre real o fecha actual
            if (refDate == DateTime.MinValue)
            {
                refDate = fechaCierreReal.HasValue ? fechaCierreReal.Value : DateTime.Today;
            }

            // 4. Protección contra fechas futuras o desfasadas (si la fecha es mayor que el cierre real con datos)
            if (fechaCierreReal.HasValue && refDate > fechaCierreReal.Value)
            {
                refDate = fechaCierreReal.Value;
            }

            // Ajustar al último día del mes
            int ultimoDia = DateTime.DaysInMonth(refDate.Year, refDate.Month);
            return new DateTime(refDate.Year, refDate.Month, ultimoDia);
        }

        public static string ObtenerContextoIndicadores(string idEntidad, RangoFechasConsulta rango)
        {
            if (string.IsNullOrWhiteSpace(idEntidad))
            {
                return "No hay identificador de entidad disponible para consultar indicadores.";
            }

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== DATOS OFICIALES DE LA ENTIDAD CONSULTADOS VÍA DBSERVICE ===");
            sb.AppendLine("Referencia utilizada: " + rango.DescripcionCorte);
            sb.AppendLine();

            // CASO A: Serie Histórica Relativa (ej. "últimos 3 meses")
            if (rango.EsSerieHistorica && rango.CortesHistoricos.Count > 0)
            {
                sb.AppendLine(string.Format("--- [EVOLUCIÓN HISTÓRICA MES A MES: ÚLTIMOS {0} MESES] ---", rango.CortesHistoricos.Count));

                foreach (var corte in rango.CortesHistoricos)
                {
                    DateTime compMesAnt = corte.AddMonths(-1);
                    int uDia = DateTime.DaysInMonth(compMesAnt.Year, compMesAnt.Month);
                    compMesAnt = new DateTime(compMesAnt.Year, compMesAnt.Month, uDia);

                    sb.AppendLine(string.Format("▶ CORTE: {0:MM/yyyy} ({0:dd/MM/yyyy}):", corte));

                    try
                    {
                        var dashMes = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Dashboard_Result[]>(
                            c => c.FGA_Consultar_Dashboard(idEntidad, compMesAnt, corte)
                        );

                        if (dashMes != null && dashMes.Length > 0)
                        {
                            // 3 KPIs Principales
                            AgregarIndicadorDashboard(sb, dashMes, 8001, "Suficiencia Patrimonial", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8118, "Eficiencia Operativa (Costo Administrativo)", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8022, "Morosidad mayor a 90 días", true);

                            // 3 Cuentas Principales (en Millones)
                            AgregarIndicadorDashboard(sb, dashMes, 8400, "Resultado del período acumulado", false);
                            AgregarIndicadorDashboard(sb, dashMes, 8130, "Cartera de crédito", false);
                            AgregarIndicadorDashboard(sb, dashMes, 8210, "Obligaciones con el público", false);

                            // Indicadores Complementarios del Tablero
                            AgregarIndicadorDashboard(sb, dashMes, 8045, "Margen financiero / Activos", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8038, "Margen Financiero Promedio", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8112, "Rentabilidad nominal / Patrimonio promedio", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8127, "Utilidad o pérdida acumulada trimestral", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8030, "Pérdida Esperada Cartera", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8020, "Concentración de los 20 inversionistas", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8053, "Disponibilidades / Captaciones a la vista", true);
                            AgregarIndicadorDashboard(sb, dashMes, 8060, "Activo Productivo / Pasivo con Costo", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine("  (Error al consultar mes vía DBService: " + ex.Message + ")");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }

            // CASO B: Corte Único o Comparación entre Dos Fechas
            sb.AppendLine(string.Format("Corte consultado: {0:dd/MM/yyyy} (Periodo comparativo: {1:dd/MM/yyyy})", rango.FechaCorte, rango.FechaComparar));
            sb.AppendLine();

            // 1. Indicadores del Tablero Oficial (FGA_Consultar_Dashboard)
            try
            {
                var dashResults = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Dashboard_Result[]>(
                    c => c.FGA_Consultar_Dashboard(idEntidad, rango.FechaComparar, rango.FechaCorte)
                );

                if (dashResults != null && dashResults.Length > 0)
                {
                    sb.AppendLine("--- [1. INDICADORES DEL TABLERO PRINCIPAL] ---");
                    // 3 KPIs Principales
                    AgregarIndicadorDashboard(sb, dashResults, 8001, "Suficiencia Patrimonial", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8118, "Eficiencia Operativa (Costo Administrativo)", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8022, "Morosidad mayor a 90 días", true);

                    // 3 Cuentas Principales (en Millones)
                    AgregarIndicadorDashboard(sb, dashResults, 8400, "Resultado del período acumulado", false);
                    AgregarIndicadorDashboard(sb, dashResults, 8130, "Cartera de crédito", false);
                    AgregarIndicadorDashboard(sb, dashResults, 8210, "Obligaciones con el público", false);

                    // Indicadores Complementarios del Tablero
                    AgregarIndicadorDashboard(sb, dashResults, 8045, "Margen financiero / Activos", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8038, "Margen Financiero Promedio", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8112, "Rentabilidad nominal / Patrimonio promedio", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8127, "Utilidad o pérdida acumulada trimestral", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8030, "Pérdida Esperada Cartera", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8020, "Concentración de los 20 inversionistas", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8053, "Disponibilidades / Captaciones a la vista", true);
                    AgregarIndicadorDashboard(sb, dashResults, 8060, "Activo Productivo / Pasivo con Costo", true);
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Aviso: No se pudieron extraer datos del Dashboard vía DBService: " + ex.Message);
            }

            // 2. Indicadores Financieros y Fórmulas de la Herramienta (FGA_Consultar_Indicadores_Formulas)
            try
            {
                var formulas = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Indicadores_Formulas_Result[]>(
                    c => c.FGA_Consultar_Indicadores_Formulas(idEntidad, rango.FechaCorte, rango.FechaComparar)
                );

                if (formulas != null && formulas.Length > 0)
                {
                    sb.AppendLine("--- [2. INDICADORES FINANCIEROS Y FÓRMULAS DEL SISTEMA] ---");
                    foreach (var f in formulas.Take(25))
                    {
                        string valActual = FormatearNumero(f.RESULTADO, f.IND_PORCENTAJE == true);
                        string valComp = FormatearNumero(f.RESULTADO_COMPARAR, f.IND_PORCENTAJE == true);
                        sb.AppendLine(string.Format("• [{0}] {1}: {2} (Anterior: {3})", f.TIPO?.Trim() ?? "General", f.NOMBRE?.Trim(), valActual, valComp));
                    }
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Aviso: No se pudieron extraer fórmulas de indicadores: " + ex.Message);
            }

            // 3. Indicadores de Cartera de Crédito (FGA_Consultar_Indicadores_Cartera)
            try
            {
                var carteraInds = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Indicadores_Cartera_Result[]>(
                    c => c.FGA_Consultar_Indicadores_Cartera(idEntidad, rango.FechaComparar, rango.FechaCorte, DateTime.Now)
                );

                if (carteraInds != null && carteraInds.Length > 0)
                {
                    sb.AppendLine("--- [3. INDICADORES DE CARTERA DE CRÉDITO] ---");
                    foreach (var ci in carteraInds.Take(10))
                    {
                        string val1 = FormatearNumero(ci.monto_1, ci.ind_porcentaje == true);
                        string val2 = FormatearNumero(ci.monto_2, ci.ind_porcentaje == true);
                        sb.AppendLine(string.Format("• {0}: Actual {1} | Anterior {2}", ci.nombreindicador?.Trim(), val2, val1));
                    }
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Aviso: No se pudieron extraer indicadores de cartera: " + ex.Message);
            }

            // 4. Indicadores Metodología CAMEL (FGA_Consultar_CAMEL)
            try
            {
                var camelInds = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_CAMEL_Result[]>(
                    c => c.FGA_Consultar_CAMEL(idEntidad, rango.FechaComparar, rango.FechaCorte, DateTime.Now, "S")
                );

                if (camelInds != null && camelInds.Length > 0)
                {
                    sb.AppendLine("--- [4. EVALUACIÓN Y RATIOS CAMEL] ---");
                    foreach (var cm in camelInds.Take(12))
                    {
                        string v1 = FormatearNumero(cm.MONTO_2, cm.IND_PORCENTAJE == "S");
                        string vAnt = FormatearNumero(cm.MONTO_1, cm.IND_PORCENTAJE == "S");
                        sb.AppendLine(string.Format("• CAMEL {0}: {1} (Anterior: {2})", cm.NOMBRE?.Trim(), v1, vAnt));
                    }
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Aviso: No se pudieron extraer datos CAMEL: " + ex.Message);
            }

            return sb.ToString();
        }

        private static void AgregarIndicadorDashboard(StringBuilder sb, FGA_Consultar_Dashboard_Result[] results, int id, string nombre, bool esPorcentaje)
        {
            var matches = results.Where(r => r.ID == id).ToList();
            if (matches.Count == 0) return;

            decimal valActual = 0;
            decimal valComp = 0;

            if (matches.Count > 1)
            {
                var ordenados = matches.Where(r => r.PERIODO.HasValue).OrderBy(r => r.PERIODO).ToList();
                valComp = ordenados.Count > 0 ? (ordenados.First().MONTO ?? 0) : (matches.First().MONTO ?? 0);
                valActual = ordenados.Count > 0 ? (ordenados.Last().MONTO ?? 0) : (matches.Last().MONTO ?? 0);
            }
            else
            {
                valActual = matches[0].MONTO ?? 0;
                valComp = matches[0].CALIFICACION ?? valActual;
            }

            string sActual = esPorcentaje ? string.Format("{0:N2}%", valActual) : string.Format("₡{0:N2}", valActual);
            string sComp = esPorcentaje ? string.Format("{0:N2}%", valComp) : string.Format("₡{0:N2}", valComp);

            sb.AppendLine(string.Format("  • {0}: {1} (Comparativo mes ant: {2})", nombre, sActual, sComp));
        }

        private static string FormatearNumero(decimal? num, bool esPct)
        {
            if (!num.HasValue) return "N/D";
            return esPct ? string.Format("{0:N2}%", num.Value) : string.Format("₡{0:N2}", num.Value);
        }
    }
}
