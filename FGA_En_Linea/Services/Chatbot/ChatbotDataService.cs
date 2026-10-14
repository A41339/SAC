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
                        resultado.CortesHistoricos.Add(new DateTime(mesDt.Year, mesDt.Month, 1));
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
                        var dt = new DateTime(ano, mes, 1);
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
                        var dt = new DateTime(ano, mes, 1);
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
                    resultado.FechaComparar = new DateTime(resultado.FechaCorte.AddMonths(-1).Year, resultado.FechaCorte.AddMonths(-1).Month, 1);
                    resultado.EsFechaPersonalizada = true;
                    resultado.DescripcionCorte = string.Format("{0:MM/yyyy} (corte mensual contra {1:MM/yyyy})", resultado.FechaCorte, resultado.FechaComparar);
                    return resultado;
                }
            }

            // 4. Fallback por defecto: Fecha de referencia activa
            resultado.FechaCorte = fechaReferencia;
            resultado.FechaComparar = new DateTime(fechaReferencia.AddMonths(-1).Year, fechaReferencia.AddMonths(-1).Month, 1);
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

        public static DateTime ParsearFechaPeriodoSegura(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return DateTime.MinValue;
            string limpia = texto.Trim();

            if (DateTime.TryParseExact(limpia, new[] { "MM/yyyy", "M/yyyy", "MM-yyyy", "M-yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMM" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtExact))
            {
                return new DateTime(dtExact.Year, dtExact.Month, 1);
            }

            if (DateTime.TryParse(limpia, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dtParsed))
            {
                return new DateTime(dtParsed.Year, dtParsed.Month, 1);
            }

            return DateTime.MinValue;
        }

        private static DateTime ResolverFechaReferencia(string periodoSesionDefecto, string idEntidad = null)
        {
            DateTime? fechaCierreReal = null;

            // 1. Consultar fecha de cierre contable oficial de la entidad vía DBService
            if (!string.IsNullOrWhiteSpace(idEntidad) && idEntidad != "0" && idEntidad != "-1")
            {
                try
                {
                    DateTime fc = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, DateTime>(
                        c => c.FGA_Consultar_FechaCierre(idEntidad)
                    );
                    if (fc > new DateTime(2000, 1, 1))
                    {
                        // FGA_Consultar_FechaCierre retorna DATEADD(MONTH, 1, MAX(PERIODO))
                        // Por lo tanto, el último corte contable cerrado con datos reales es fc.AddMonths(-1)
                        DateTime mesCorte = fc.AddMonths(-1);
                        fechaCierreReal = new DateTime(mesCorte.Year, mesCorte.Month, 1);
                    }
                }
                catch { }
            }

            // 2. Parsear el periodo recibido (de sesión o selector)
            DateTime refDate = ParsearFechaPeriodoSegura(periodoSesionDefecto);

            // 3. Si no hay periodo válido, tomar el cierre real o fecha activa
            if (refDate == DateTime.MinValue)
            {
                refDate = fechaCierreReal.HasValue ? fechaCierreReal.Value : new DateTime(2026, 8, 1);
            }

            // 4. Si la fecha supera el cierre real con datos de la entidad, anclar al cierre real
            if (fechaCierreReal.HasValue && refDate > fechaCierreReal.Value)
            {
                refDate = fechaCierreReal.Value;
            }

            return new DateTime(refDate.Year, refDate.Month, 1);
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
                    DateTime compMesAnt = new DateTime(corte.AddMonths(-1).Year, corte.AddMonths(-1).Month, 1);
                    DateTime corteDia1 = new DateTime(corte.Year, corte.Month, 1);

                    sb.AppendLine(string.Format("▶ CORTE: {0:MM/yyyy}:", corteDia1));

                    try
                    {
                        var dashMes = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Dashboard_Result[]>(
                            c => c.FGA_Consultar_Dashboard(idEntidad, compMesAnt, corteDia1)
                        );

                        // Suficiencia Patrimonial con soporte multi-fuente (FGA_Consultar_Dashboard / FGA_Consultar_Grafico_Suficiencia / CAMEL)
                        AgregarSuficienciaConFallback(sb, dashMes, idEntidad, compMesAnt, corteDia1);

                        if (dashMes != null && dashMes.Length > 0)
                        {
                            // Eficiencia y Mora
                            AgregarIndicadorDashboard(sb, dashMes, 8118, "Eficiencia Operativa (Costo Administrativo)", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8022, "Morosidad mayor a 90 días", true, corteDia1);

                            // Indicadores SUGEF 3-06 (Perfil 2 / Cooperativas no prudenciales)
                            AgregarIndicadorDashboard(sb, dashMes, 9100, "Apalancamiento (SUGEF 3-06)", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8991, "Capital Nivel 1 - CN1", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8992, "Capital Común Nivel 1 - CCN1", true, corteDia1);

                            // Cuentas de Balance (en Millones)
                            AgregarCuentaBalanceConFallback(sb, dashMes, 8400, "Resultado del período acumulado", idEntidad, compMesAnt, corteDia1);
                            AgregarCuentaBalanceConFallback(sb, dashMes, 8130, "Cartera de crédito", idEntidad, compMesAnt, corteDia1);
                            AgregarCuentaBalanceConFallback(sb, dashMes, 8210, "Obligaciones con el público", idEntidad, compMesAnt, corteDia1);
                            AgregarCuentaBalanceConFallback(sb, dashMes, 310, "Capital Social", idEntidad, compMesAnt, corteDia1);
                            AgregarCuentaBalanceConFallback(sb, dashMes, 300, "Patrimonio Total", idEntidad, compMesAnt, corteDia1);
                            AgregarCuentaBalanceConFallback(sb, dashMes, 100, "Activo Total", idEntidad, compMesAnt, corteDia1);

                            // Indicadores Complementarios del Tablero
                            AgregarIndicadorDashboard(sb, dashMes, 8045, "Margen financiero / Activos", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8038, "Margen Financiero Promedio", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8112, "Rentabilidad sobre Patrimonio Promedio (ROE)", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8127, "Utilidad o pérdida acumulada trimestral", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8030, "Pérdida Esperada Cartera", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8020, "Concentración de los 20 inversionistas", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8053, "Disponibilidades / Captaciones a la vista", true, corteDia1);
                            AgregarIndicadorDashboard(sb, dashMes, 8060, "Activo Productivo / Pasivo con Costo", true, corteDia1);
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine("  (Error al consultar mes vía DBService: " + ex.Message + ")");
                    }
                    sb.AppendLine();
                }

                AgregarAlertasYFondeo(sb, idEntidad, rango.FechaComparar, rango.FechaCorte);
                return sb.ToString();
            }

            // CASO B: Corte Único o Comparación entre Dos Fechas
            DateTime corteB = new DateTime(rango.FechaCorte.Year, rango.FechaCorte.Month, 1);
            DateTime compB = new DateTime(rango.FechaComparar.Year, rango.FechaComparar.Month, 1);
            sb.AppendLine(string.Format("Corte consultado: {0:MM/yyyy} (Periodo comparativo: {1:MM/yyyy})", corteB, compB));
            sb.AppendLine();

            // 1. Indicadores del Tablero Oficial (FGA_Consultar_Dashboard)
            try
            {
                var dashResults = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Dashboard_Result[]>(
                    c => c.FGA_Consultar_Dashboard(idEntidad, compB, corteB)
                );

                sb.AppendLine("--- [1. INDICADORES DEL TABLERO PRINCIPAL] ---");

                // Suficiencia Patrimonial con respaldo multi-fuente
                AgregarSuficienciaConFallback(sb, dashResults, idEntidad, compB, corteB);

                if (dashResults != null && dashResults.Length > 0)
                {
                    // Eficiencia y Mora
                    AgregarIndicadorDashboard(sb, dashResults, 8118, "Eficiencia Operativa (Costo Administrativo)", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8022, "Morosidad mayor a 90 días", true, corteB);

                    // Indicadores SUGEF 3-06
                    AgregarIndicadorDashboard(sb, dashResults, 9100, "Apalancamiento (SUGEF 3-06)", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8991, "Capital Nivel 1 - CN1", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8992, "Capital Común Nivel 1 - CCN1", true, corteB);

                    // Cuentas de Balance (en Millones)
                    AgregarCuentaBalanceConFallback(sb, dashResults, 8400, "Resultado del período acumulado", idEntidad, compB, corteB);
                    AgregarCuentaBalanceConFallback(sb, dashResults, 8130, "Cartera de crédito", idEntidad, compB, corteB);
                    AgregarCuentaBalanceConFallback(sb, dashResults, 8210, "Obligaciones con el público", idEntidad, compB, corteB);
                    AgregarCuentaBalanceConFallback(sb, dashResults, 310, "Capital Social", idEntidad, compB, corteB);
                    AgregarCuentaBalanceConFallback(sb, dashResults, 300, "Patrimonio Total", idEntidad, compB, corteB);
                    AgregarCuentaBalanceConFallback(sb, dashResults, 100, "Activo Total", idEntidad, compB, corteB);

                    // Indicadores Complementarios del Tablero
                    AgregarIndicadorDashboard(sb, dashResults, 8045, "Margen financiero / Activos", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8038, "Margen Financiero Promedio", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8112, "Rentabilidad sobre Patrimonio Promedio (ROE)", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8127, "Utilidad o pérdida acumulada trimestral", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8030, "Pérdida Esperada Cartera", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8020, "Concentración de los 20 inversionistas", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8053, "Disponibilidades / Captaciones a la vista", true, corteB);
                    AgregarIndicadorDashboard(sb, dashResults, 8060, "Activo Productivo / Pasivo con Costo", true, corteB);
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

            // 5. Alertas y Fondeo
            AgregarAlertasYFondeo(sb, idEntidad, compB, corteB);

            return sb.ToString();
        }

        private static void AgregarAlertasYFondeo(StringBuilder sb, string idEntidad, DateTime comp, DateTime corte)
        {
            if (string.IsNullOrWhiteSpace(idEntidad)) return;

            // Alertas Financieras Activas (FGA_Obtener_Alertas_Financieras)
            try
            {
                var alertas = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Obtener_Alertas_Financieras_Result[]>(
                    c => c.FGA_Obtener_Alertas_Financieras(idEntidad, false, 20)
                );

                sb.AppendLine("--- [BANDEJA DE ALERTAS ACTIVAS DE LA ENTIDAD] ---");
                if (alertas != null && alertas.Length > 0)
                {
                    sb.AppendLine(string.Format("Total de alertas registradas en el sistema: {0}", alertas.Length));
                    foreach (var al in alertas.Take(6))
                    {
                        sb.AppendLine(string.Format("• [{0}] {1} (Cuenta: {2}): Variación {3:N2}% ({4:N2} M). Período: {5:MM/yyyy}",
                            al.TipoAlerta ?? "ALERTA",
                            al.NombreCuenta?.Trim() ?? al.Titulo?.Trim() ?? "Cuenta contable",
                            al.Cuenta?.Trim() ?? "N/A",
                            al.VariacionPorcentaje,
                            al.VariacionMonto / 1000000m,
                            al.PeriodoActual));
                    }
                }
                else
                {
                    sb.AppendLine("• La entidad no presenta alertas financieras activas ni desviaciones contables por umbrales en este momento.");
                }
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                sb.AppendLine("Aviso: No se pudieron consultar alertas: " + ex.Message);
            }

            // Concentración de Fondeo y Ahorrantes (FGA_Consultar_Concentracion_Ahorrantes)
            try
            {
                var ahorrantes = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Concentracion_Ahorrantes_Result[]>(
                    c => c.FGA_Consultar_Concentracion_Ahorrantes(idEntidad, comp, corte)
                );

                if (ahorrantes != null && ahorrantes.Length > 0)
                {
                    sb.AppendLine("--- [CONCENTRACIÓN DE FONDEO Y AHORRANTES] ---");
                    var ult = ahorrantes.LastOrDefault();
                    if (ult != null)
                    {
                        if (ult.PorcTop10.HasValue) sb.AppendLine(string.Format("• Concentración 10 Mayores Ahorrantes: {0:N2}%", ult.PorcTop10.Value));
                        if (ult.PorcTop20.HasValue) sb.AppendLine(string.Format("• Concentración 20 Mayores Ahorrantes: {0:N2}%", ult.PorcTop20.Value));
                        if (ult.MontoTotal.HasValue && ult.MontoTotal.Value > 0)
                        {
                            sb.AppendLine(string.Format("• Captaciones Totales de Ahorrantes: ₡{0:N2} M", ult.MontoTotal.Value / 1000000m));
                        }
                    }
                    sb.AppendLine();
                }
            }
            catch { }
        }

        /// <summary>
        /// Intenta obtener el saldo de una cuenta de balance (Cartera, Obligaciones, Resultado)
        /// desde FGA_Consultar_Dashboard (ID interno). Si el valor es 0, hace fallback a
        /// FGA_Consultar_Balance_Comprobacion consultando directamente la cuenta contable.
        /// </summary>
        private static void AgregarCuentaBalanceConFallback(
            StringBuilder sb,
            FGA_Consultar_Dashboard_Result[] results,
            int dashboardId,
            string nombre,
            string idEntidad,
            DateTime compMesAnt,
            DateTime corte)
        {
            decimal valActual = 0;
            bool encontrado = false;

            // Normalización de IDs entre esquema moderno (serie 8000) y tradicional
            int idTrad = dashboardId;
            int idMod = dashboardId;
            if (dashboardId == 8130 || dashboardId == 130) { idTrad = 130; idMod = 8130; }
            else if (dashboardId == 8210 || dashboardId == 210) { idTrad = 210; idMod = 8210; }
            else if (dashboardId == 8400 || dashboardId == 400) { idTrad = 400; idMod = 8400; }
            else if (dashboardId == 310 || dashboardId == 31000000) { idTrad = 310; idMod = 310; }
            else if (dashboardId == 300 || dashboardId == 30000000) { idTrad = 300; idMod = 300; }
            else if (dashboardId == 100 || dashboardId == 10000000) { idTrad = 100; idMod = 100; }

            // 1. Intentar desde FGA_Consultar_Dashboard
            if (results != null && results.Length > 0)
            {
                var matches = results.Where(r => r.ID == idMod || r.ID == idTrad).ToList();
                if (matches.Count > 0)
                {
                    // Buscar coincidencia que coincida con el mes y año de corte
                    var matchCorte = matches.FirstOrDefault(r => r.PERIODO.HasValue && r.PERIODO.Value.Year == corte.Year && r.PERIODO.Value.Month == corte.Month);
                    if (matchCorte != null && (matchCorte.MONTO.GetValueOrDefault() != 0 || matchCorte.CALIFICACION.GetValueOrDefault() != 0))
                    {
                        valActual = matchCorte.MONTO.GetValueOrDefault() != 0 ? matchCorte.MONTO.Value : matchCorte.CALIFICACION.GetValueOrDefault();
                        encontrado = true;
                    }
                    else
                    {
                        var ultimo = matches.OrderBy(r => r.PERIODO ?? DateTime.MinValue).Last();
                        decimal cand = ultimo.MONTO.GetValueOrDefault() != 0 ? ultimo.MONTO.Value : ultimo.CALIFICACION.GetValueOrDefault();
                        if (cand != 0)
                        {
                            valActual = cand;
                            encontrado = true;
                        }
                    }
                }
            }

            // 2. Fallback: FGA_Consultar_Balance_Comprobacion si el dashboard devolvio 0
            if (!encontrado && !string.IsNullOrWhiteSpace(idEntidad))
            {
                // Mapeo dashboardId a cuenta contable exacta (SUGEF)
                string cuentaContable = null;
                if (idTrad == 130) cuentaContable = "13000000"; // Cartera de credito
                else if (idTrad == 210) cuentaContable = "21000000"; // Obligaciones con el publico
                else if (idTrad == 400) cuentaContable = "50000000"; // Resultado del periodo (ingresos)
                else if (idTrad == 310) cuentaContable = "31000000"; // Capital Social
                else if (idTrad == 300) cuentaContable = "30000000"; // Patrimonio Total
                else if (idTrad == 100) cuentaContable = "10000000"; // Activo Total

                if (!string.IsNullOrWhiteSpace(cuentaContable))
                {
                    try
                    {
                        var balRes = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Balance_Comprobacion_Result[]>(
                            c => c.FGA_Consultar_Balance_Comprobacion(idEntidad, corte)
                        );
                        if (balRes != null && balRes.Length > 0)
                        {
                            // Caso especial: Resultado del período = Ingresos (50000000) - Gastos (40000000)
                            if (idTrad == 400)
                            {
                                var ing = balRes.FirstOrDefault(r => r.Cuenta != null && r.Cuenta.Trim() == "50000000");
                                var gas = balRes.FirstOrDefault(r => r.Cuenta != null && r.Cuenta.Trim() == "40000000");
                                decimal resultado = (ing != null ? ing.SaldoFinal : 0) - (gas != null ? gas.SaldoFinal : 0);
                                if (resultado != 0)
                                {
                                    valActual = Math.Abs(resultado) / 1000000m;
                                    encontrado = true;
                                }
                            }
                            else
                            {
                                // Buscar cuenta exacta (igual que el Dashboard SQL: WHERE CUENTA = '21000000')
                                var cuentaExacta = balRes.FirstOrDefault(r =>
                                    r.Cuenta != null && r.Cuenta.Trim() == cuentaContable && r.SaldoFinal != 0);

                                // Si no hay exacta, intentar con subcuentas de primer nivel (prefijo de 2 dígitos)
                                if (cuentaExacta == null)
                                {
                                    string prefijo = cuentaContable.Substring(0, 2);
                                    decimal suma = balRes
                                        .Where(r => r.Cuenta != null && r.Cuenta.Trim().StartsWith(prefijo) && r.SaldoFinal != 0)
                                        .Sum(r => r.SaldoFinal);
                                    if (suma != 0)
                                    {
                                        valActual = Math.Abs(suma) / 1000000m;
                                        encontrado = true;
                                    }
                                }
                                else
                                {
                                    valActual = Math.Abs(cuentaExacta.SaldoFinal) / 1000000m;
                                    encontrado = true;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            // 3. Fallback secundario: FGA_Consultar_CAMEL buscando por nombre similar
            if (!encontrado && !string.IsNullOrWhiteSpace(idEntidad))
            {
                try
                {
                    var camel = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_CAMEL_Result[]>(
                        c => c.FGA_Consultar_CAMEL(idEntidad, compMesAnt, corte, DateTime.Now, "S")
                    );
                    if (camel != null && camel.Length > 0)
                    {
                        string kw = nombre.Split(' ')[0];
                        var match = camel.FirstOrDefault(cm =>
                            cm.NOMBRE != null &&
                            cm.NOMBRE.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 &&
                            cm.MONTO_2 != 0);
                        if (match != null)
                        {
                            valActual = Math.Abs(match.MONTO_2) / 1000000m;
                            encontrado = true;
                        }
                    }
                }
                catch { }
            }

            string sActual = string.Format("₡{0:N2} M", valActual);
            sb.AppendLine(string.Format("  • {0}: {1}", nombre, sActual));
        }

        private static void AgregarSuficienciaConFallback(StringBuilder sb, FGA_Consultar_Dashboard_Result[] results, string idEntidad, DateTime compMesAnt, DateTime corte)
        {
            decimal valActual = 0;
            bool encontradoEnDashboard = false;

            if (results != null && results.Length > 0)
            {
                var matches = results.Where(r => r.ID == 8001 || r.ID == 1).ToList();
                if (matches.Count > 0)
                {
                    var matchCorte = matches.FirstOrDefault(r => r.PERIODO.HasValue && r.PERIODO.Value.Year == corte.Year && r.PERIODO.Value.Month == corte.Month);
                    if (matchCorte != null && matchCorte.MONTO.HasValue && matchCorte.MONTO.Value > 0)
                    {
                        valActual = matchCorte.MONTO.Value;
                        encontradoEnDashboard = true;
                    }
                    else
                    {
                        var ultimo = matches.OrderBy(r => r.PERIODO ?? DateTime.MinValue).Last();
                        if (ultimo.MONTO.HasValue && ultimo.MONTO.Value > 0)
                        {
                            valActual = ultimo.MONTO.Value;
                            encontradoEnDashboard = true;
                        }
                    }
                }
            }

            // Si FGA_Consultar_Dashboard arrojó 0 o no lo encontró, consultar vía FGA_Consultar_Grafico_Suficiencia
            if (!encontradoEnDashboard && !string.IsNullOrWhiteSpace(idEntidad))
            {
                try
                {
                    var takSuf = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Grafico_Suficiencia_Result[]>(
                        c => c.FGA_Consultar_Grafico_Suficiencia(idEntidad, compMesAnt, corte)
                    );
                    if (takSuf != null && takSuf.Length > 0)
                    {
                        var matchActual = takSuf.FirstOrDefault(s => s.Nombre != null && s.Nombre.Trim().Equals("Suficiencia", StringComparison.OrdinalIgnoreCase) && s.Monto.HasValue && s.Monto.Value > 0);
                        if (matchActual != null)
                        {
                            valActual = matchActual.Monto.Value;
                            encontradoEnDashboard = true;
                        }
                    }
                }
                catch { }
            }

            // Si aún da 0, consultar FGA_Consultar_CAMEL (ID 1 o nombre Suficiencia)
            if (!encontradoEnDashboard && !string.IsNullOrWhiteSpace(idEntidad))
            {
                try
                {
                    var camel = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_CAMEL_Result[]>(
                        c => c.FGA_Consultar_CAMEL(idEntidad, compMesAnt, corte, DateTime.Now, "S")
                    );
                    if (camel != null && camel.Length > 0)
                    {
                        var matchCamel = camel.FirstOrDefault(cm => (cm.IDRPT_CAMEL == 1 || (cm.NOMBRE != null && cm.NOMBRE.IndexOf("SUFICIENCIA", StringComparison.OrdinalIgnoreCase) >= 0)) && cm.MONTO_2 > 0);
                        if (matchCamel != null)
                        {
                            valActual = matchCamel.MONTO_2;
                            encontradoEnDashboard = true;
                        }
                    }
                }
                catch { }
            }

            // Si aún da 0, consultar FGA_Consultar_Indicadores_Formulas
            if (!encontradoEnDashboard && !string.IsNullOrWhiteSpace(idEntidad))
            {
                try
                {
                    var formulas = WcfHelper.Execute<FGA_En_Linea.SPService.SPClient, FGA_Consultar_Indicadores_Formulas_Result[]>(
                        c => c.FGA_Consultar_Indicadores_Formulas(idEntidad, corte, compMesAnt)
                    );
                    if (formulas != null && formulas.Length > 0)
                    {
                        var matchForm = formulas.FirstOrDefault(f => f.NOMBRE != null && f.NOMBRE.IndexOf("SUFICIENCIA", StringComparison.OrdinalIgnoreCase) >= 0 && f.RESULTADO.HasValue && f.RESULTADO.Value > 0);
                        if (matchForm != null)
                        {
                            valActual = matchForm.RESULTADO.Value;
                            encontradoEnDashboard = true;
                        }
                    }
                }
                catch { }
            }

            string sActual = string.Format("{0:N2}%", valActual);
            sb.AppendLine(string.Format("  • Suficiencia Patrimonial: {0}", sActual));
        }

        private static void AgregarIndicadorDashboard(StringBuilder sb, FGA_Consultar_Dashboard_Result[] results, int id, string nombre, bool esPorcentaje, DateTime? corte = null)
        {
            if (results == null || results.Length == 0) return;
            var matches = results.Where(r => r.ID == id).ToList();
            if (matches.Count == 0) return;

            decimal valActual = 0;

            if (corte.HasValue)
            {
                var matchCorte = matches.FirstOrDefault(r => r.PERIODO.HasValue && r.PERIODO.Value.Year == corte.Value.Year && r.PERIODO.Value.Month == corte.Value.Month);
                if (matchCorte != null)
                {
                    valActual = matchCorte.MONTO ?? matchCorte.CALIFICACION ?? 0;
                }
                else
                {
                    valActual = matches.Last().MONTO ?? matches.Last().CALIFICACION ?? 0;
                }
            }
            else
            {
                valActual = matches.Last().MONTO ?? matches.Last().CALIFICACION ?? 0;
            }

            string sActual = esPorcentaje ? string.Format("{0:N2}%", valActual) : string.Format("₡{0:N2}", valActual);
            sb.AppendLine(string.Format("  • {0}: {1}", nombre, sActual));
        }

        private static string FormatearNumero(decimal? num, bool esPct)
        {
            if (!num.HasValue) return "N/D";
            return esPct ? string.Format("{0:N2}%", num.Value) : string.Format("₡{0:N2}", num.Value);
        }
    }
}
