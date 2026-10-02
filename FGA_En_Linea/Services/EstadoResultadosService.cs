using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FGA.Model;
using FGA.Utility;

namespace FGA.Services
{
    public class EstadoResultadosService
    {
        private readonly CultureInfo _crCulture = new CultureInfo("es-CR");

        public EstadoResultadosViewModel ConstruirModeloER(
            string entidadId,
            string periodoSel,
            string modalidadSel,
            string tipoCompSel,
            HttpSessionStateBase session,
            FGA_En_Linea.SPService.SPClient spClient = null,
            FGA_En_Linea.EntidadService.EntidadServiceClient entClient = null)
        {
            bool disposeSp = false;
            if (spClient == null)
            {
                spClient = new FGA_En_Linea.SPService.SPClient();
                disposeSp = true;
            }

            bool disposeEnt = false;
            if (entClient == null)
            {
                entClient = new FGA_En_Linea.EntidadService.EntidadServiceClient();
                disposeEnt = true;
            }

            try
            {
                var idEntidad = string.IsNullOrEmpty(entidadId) ? (session["IdEntidad"] != null ? session["IdEntidad"].ToString() : "2") : entidadId;
                session["IdEntidad"] = idEntidad;

                var modalidad = string.IsNullOrEmpty(modalidadSel) ? (session["Modalidad"] != null ? session["Modalidad"].ToString() : "Acumulado") : modalidadSel;
                session["Modalidad"] = modalidad;

                var tipoComp = string.IsNullOrEmpty(tipoCompSel) ? (session["TipoComparacion"] != null ? session["TipoComparacion"].ToString() : "Interanual") : tipoCompSel;
                session["TipoComparacion"] = tipoComp;

                var periodoRef = string.IsNullOrEmpty(periodoSel) ? (session["Periodo2"] != null ? session["Periodo2"].ToString() : (session["Periodo"] != null ? session["Periodo"].ToString() : DateTime.Now.ToString("MM/yyyy"))) : periodoSel;

                DateTime refDate;
                try
                {
                    refDate = Utility.Utilitarios.ConvertirAFecha(periodoRef);
                }
                catch
                {
                    refDate = DateTime.Now;
                }
                session["Periodo2"] = refDate.ToShortDateString();

                string cacheKey = string.Format("FGA_ER_{0}_{1:yyyyMM}_{2}_{3}", idEntidad, refDate, modalidad, tipoComp);
                if (HttpRuntime.Cache != null)
                {
                    var cachedModel = HttpRuntime.Cache.Get(cacheKey) as EstadoResultadosViewModel;
                    if (cachedModel != null)
                    {
                        return cachedModel;
                    }
                }

                var model = new EstadoResultadosViewModel
                {
                    EntidadId = idEntidad,
                    Modalidad = modalidad,
                    TipoComparacion = tipoComp,
                    PeriodoReferencia = refDate,
                    LogoUrl = session["Logo"] != null ? session["Logo"].ToString() : ""
                };

                // Nombre de la Entidad
                if (idEntidad == "-1")
                {
                    model.NombreEntidad = "TODAS LAS COOPERATIVAS";
                }
                else
                {
                    string resolvedName = null;
                    try
                    {
                        var rawLista = session != null ? session["AllEntidades"] as FGA.Models.Entidad[] : null;
                        if (rawLista != null)
                        {
                            var matched = rawLista.FirstOrDefault(o => o.Id == idEntidad);
                            if (matched != null && !string.IsNullOrEmpty(matched.Nombre))
                            {
                                resolvedName = matched.Nombre;
                            }
                        }
                        if (string.IsNullOrEmpty(resolvedName) && entClient != null)
                        {
                            var entObj = entClient.Get(idEntidad);
                            if (entObj != null && !string.IsNullOrEmpty(entObj.Nombre))
                            {
                                resolvedName = entObj.Nombre;
                            }
                        }
                    }
                    catch { }

                    if (string.IsNullOrEmpty(resolvedName))
                    {
                        resolvedName = (session != null && session["NomEntidad"] != null) ? session["NomEntidad"].ToString() : "ENTIDAD";
                    }

                    model.NombreEntidad = resolvedName;
                    if (session != null)
                    {
                        session["NomEntidad"] = resolvedName;
                        session["IdEntidad"] = idEntidad;
                    }
                }

                // 1. Determinar 5 períodos consecutivos
                DateTime[] pFechas = DeterminarFechasPeriodos(refDate, tipoComp, model);
                model.FechasPeriodos = pFechas;

                // 2. Encabezados descriptivos
                for (int i = 0; i < 5; i++)
                {
                    string mesNom = Utility.Utilitarios.toUpperFirstLetter(pFechas[i].ToString("MMM. yyyy", _crCulture));
                    if (tipoComp.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
                    {
                        if (pFechas[i].Month == 3) mesNom += " (I Trim)";
                        else if (pFechas[i].Month == 6) mesNom += " (II Trim)";
                        else if (pFechas[i].Month == 9) mesNom += " (III Trim)";
                        else if (pFechas[i].Month == 12) mesNom += " (IV Trim)";
                    }
                    model.EncabezadosPeriodos[i] = mesNom;
                }

                // 3. Consultar datos del Estado de Resultados a 5 períodos mediante el nuevo SP optimizado
                Entities.Entities.Procedures.FGA_Rpt_Estado_Resultados_5Periodos_Result[] spData = null;
                try
                {
                    spData = spClient.FGA_Rpt_Estado_Resultados_5Periodos(idEntidad, refDate, modalidad, tipoComp);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al invocar FGA_Rpt_Estado_Resultados_5Periodos: " + ex.Message);
                }

                if (spData != null && spData.Length > 0)
                {
                    MapearDesdeSP(model, spData);
                }
                else
                {
                    GenerarCatalogoEstructuradoDefault(model);
                }

                // 4. Calcular Análisis Vertical (% Representa)
                CalcularAnalisisVertical(model);

                // 5. Construir DTO de composición para Highcharts
                ConstruirDatosComposicion(model);

                if (HttpRuntime.Cache != null && model.Filas != null && model.Filas.Count > 0)
                {
                    HttpRuntime.Cache.Insert(
                        cacheKey,
                        model,
                        null,
                        DateTime.UtcNow.AddMinutes(10),
                        System.Web.Caching.Cache.NoSlidingExpiration);
                }

                return model;
            }
            finally
            {
                if (disposeSp) { spClient.SafeClose(); }
                if (disposeEnt) { entClient.SafeClose(); }
            }
        }

        private DateTime[] DeterminarFechasPeriodos(DateTime refDate, string tipoComp, EstadoResultadosViewModel model)
        {
            DateTime[] pFechas = new DateTime[5];
            int y = refDate.Year;
            int m = refDate.Month;

            if (tipoComp.Equals("Mensual", StringComparison.OrdinalIgnoreCase))
            {
                pFechas[4] = new DateTime(y, m, 1);
                pFechas[3] = pFechas[4].AddMonths(-1);
                pFechas[2] = pFechas[4].AddMonths(-2);
                pFechas[1] = pFechas[4].AddMonths(-3);
                pFechas[0] = pFechas[4].AddMonths(-4);
                model.ComparacionTitulo = "Variación Mensual (" + model.Modalidad + ")";
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 meses consecutivos)";
            }
            else if (tipoComp.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
            {
                int qMonth = ((m - 1) / 3 + 1) * 3;
                DateTime qCurrent = new DateTime(y, qMonth, 1);
                pFechas[4] = qCurrent;
                pFechas[3] = qCurrent.AddMonths(-3);
                pFechas[2] = qCurrent.AddMonths(-6);
                pFechas[1] = qCurrent.AddMonths(-9);
                pFechas[0] = qCurrent.AddMonths(-12);
                model.ComparacionTitulo = "Variación Trimestral (" + model.Modalidad + ")";
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 trimestres consecutivos)";
            }
            else // Interanual
            {
                pFechas[4] = new DateTime(y, m, 1);
                pFechas[3] = pFechas[4].AddYears(-1);
                pFechas[2] = pFechas[4].AddYears(-2);
                pFechas[1] = pFechas[4].AddYears(-3);
                pFechas[0] = pFechas[4].AddYears(-4);
                model.ComparacionTitulo = "Variación Interanual (" + model.Modalidad + ")";
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 años consecutivos)";
            }

            return pFechas;
        }

        private void MapearDesdeSP(
            EstadoResultadosViewModel model,
            Entities.Entities.Procedures.FGA_Rpt_Estado_Resultados_5Periodos_Result[] spData)
        {
            model.Filas = new List<FilaEstadoResultadosItem>();
            foreach (var r in spData)
            {
                var fila = new FilaEstadoResultadosItem
                {
                    Orden = r.Orden ?? 0,
                    Seccion = r.Seccion ?? "",
                    Concepto = r.Concepto ?? "",
                    Nivel = r.Nivel ?? 1,
                    EsNegrita = r.EsNegrita ?? false,
                    EsItalica = (r.Nivel == 2),
                    SinMontos = (r.Nivel == 0),
                    CodigoContable = r.CodigoContable ?? "",
                    Periodos = new decimal[] {
                        r.Periodo1 ?? 0m,
                        r.Periodo2 ?? 0m,
                        r.Periodo3 ?? 0m,
                        r.Periodo4 ?? 0m,
                        r.Periodo5 ?? 0m
                    },
                    VariacionAbsoluta = r.VariacionAbsoluta ?? 0m,
                    VariacionRelativa = r.VariacionRelativa ?? 0m
                };
                model.Filas.Add(fila);
            }
        }

        private void GenerarCatalogoEstructuradoDefault(EstadoResultadosViewModel model)
        {
            // Catálogo por defecto si la base de datos aún no tiene registros en el SP
            var conceptos = new (string Seccion, string Concepto, int Nivel, bool EsNegrita, string Codigo)[]
            {
                ("INGRESOS FINANCIEROS", "Ingresos financieros", 0, true, ""),
                ("INGRESOS FINANCIEROS", "Ingresos financieros por disponibilidades", 1, false, "51100000"),
                ("INGRESOS FINANCIEROS", "Ingresos financieros por instrum. financieros", 1, false, "51200000"),
                ("INGRESOS FINANCIEROS", "Productos por cartera de crédito vigente", 1, false, "51300000"),
                ("INGRESOS FINANCIEROS", "Productos por cartera de crédito vencida", 1, false, "51400000"),
                ("INGRESOS FINANCIEROS", "Otros ingresos financieros", 1, false, "51900000"),
                ("INGRESOS FINANCIEROS", "Total de ingresos financieros", 2, true, ""),

                ("GASTOS FINANCIEROS", "Gastos financieros", 0, true, ""),
                ("GASTOS FINANCIEROS", "Gastos financieros por oblig. con público", 1, false, "41100000"),
                ("GASTOS FINANCIEROS", "Gastos financieros por oblig. con entidades", 1, false, "41300000"),
                ("GASTOS FINANCIEROS", "Gastos financieros cuentas por pagar diversas", 1, false, "41400000"),
                ("GASTOS FINANCIEROS", "Gastos por obligaciones subordinadas", 1, false, "41600000"),
                ("GASTOS FINANCIEROS", "Otros gastos financieros", 1, false, "41900000"),
                ("GASTOS FINANCIEROS", "Total de gastos financieros", 2, true, ""),

                ("RESULTADO FINANCIERO", "Ingresos por recuperación de activos", 1, false, "52000000"),
                ("RESULTADO FINANCIERO", "Por estimación de deterioro de activos", 1, false, "42000000"),
                ("RESULTADO FINANCIERO", "Resultado financiero bruto", 2, true, ""),

                ("MONEDA EXTRANJERA", "Ganancias por DF y UD", 1, false, "51800000"),
                ("MONEDA EXTRANJERA", "Pérdidas por DC y UD", 1, false, "41800000"),
                ("MONEDA EXTRANJERA", "Resultado por DC y UD", 2, true, ""),

                ("OPERACION", "Total otros ingresos de operación", 1, false, "53000000"),
                ("OPERACION", "Total otros gastos de operación", 1, false, "43000000"),
                ("OPERACION", "Resultado operacional bruto", 2, true, ""),

                ("GASTOS ADMINISTRATIVOS", "Gastos administrativos", 0, true, ""),
                ("GASTOS ADMINISTRATIVOS", "Por gastos de personal", 1, false, "44100000"),
                ("GASTOS ADMINISTRATIVOS", "Por otros gastos de administración", 1, false, "44200000"),
                ("GASTOS ADMINISTRATIVOS", "Total gastos administrativos", 2, true, ""),

                ("RESULTADO NETO", "Resultado operacional neto", 2, true, ""),
                ("RESULTADO NETO", "DIS de impuesto y participación sobre utilidad", 1, false, "55000000"),
                ("RESULTADO NETO", "Participaciones sobre la utilidad", 1, false, "45000000"),
                ("RESULTADO NETO", "Resultado del periodo", 3, true, "")
            };

            model.Filas = new List<FilaEstadoResultadosItem>();
            int orden = 1;
            foreach (var c in conceptos)
            {
                model.Filas.Add(new FilaEstadoResultadosItem
                {
                    Orden = orden++,
                    Seccion = c.Seccion,
                    Concepto = c.Concepto,
                    Nivel = c.Nivel,
                    EsNegrita = c.EsNegrita,
                    EsItalica = (c.Nivel == 2),
                    SinMontos = (c.Nivel == 0),
                    CodigoContable = c.Codigo,
                    Periodos = new decimal[5]
                });
            }
        }

        private void CalcularAnalisisVertical(EstadoResultadosViewModel model)
        {
            if (model.Filas == null || model.Filas.Count == 0) return;

            // Base para el cálculo vertical: Total de Ingresos Financieros
            var filaIngresosFin = model.Filas.FirstOrDefault(f => f.Concepto.Trim().Equals("Total de ingresos financieros", StringComparison.OrdinalIgnoreCase));
            decimal[] baseIngresos = new decimal[5];
            for (int i = 0; i < 5; i++)
            {
                baseIngresos[i] = (filaIngresosFin != null && filaIngresosFin.Periodos[i] != 0m) ? Math.Abs(filaIngresosFin.Periodos[i]) : 0m;
            }

            foreach (var fila in model.Filas)
            {
                if (fila.SinMontos) continue;
                for (int i = 0; i < 5; i++)
                {
                    if (baseIngresos[i] > 0m)
                    {
                        fila.PorcentajesRepresenta[i] = Math.Round((fila.Periodos[i] / baseIngresos[i]) * 100m, 1);
                    }
                    else
                    {
                        fila.PorcentajesRepresenta[i] = 0m;
                    }
                }
            }
        }

        private void ConstruirDatosComposicion(EstadoResultadosViewModel model)
        {
            var comp = new ComposicionERDTO();
            comp.PeriodosCategorias = (string[])model.EncabezadosPeriodos.Clone();
            comp.PeriodosComparacion = new string[] {
                model.EncabezadosPeriodos[1],
                model.EncabezadosPeriodos[2],
                model.EncabezadosPeriodos[3],
                model.EncabezadosPeriodos[4]
            };
            comp.PeriodoUltimoLabel = model.EncabezadosPeriodos[4];

            Func<string, decimal[]> getValores = (concepto) =>
            {
                var f = model.Filas.FirstOrDefault(x => x.Concepto != null && 
                    (x.Concepto.Trim().Equals(concepto, StringComparison.OrdinalIgnoreCase) ||
                     x.Concepto.Trim().IndexOf(concepto, StringComparison.OrdinalIgnoreCase) >= 0));
                return f != null ? f.Periodos : new decimal[5];
            };

            comp.TotalIngresosFinancieros = getValores("Total de ingresos financieros");
            comp.TotalGastosFinancieros = getValores("Total de gastos financieros");
            comp.MargenFinancieroBruto = getValores("Resultado financiero bruto");
            comp.GastosAdministrativos = getValores("Total gastos administrativos");
            comp.ResultadoOperacionalNeto = getValores("Resultado operacional neto");
            comp.ResultadoPeriodo = getValores("Resultado del periodo");

            // Desglose de Ingresos del Último Período
            var vCarteraVig = getValores("Productos por cartera de crédito vigente");
            var vCarteraVen = getValores("Productos por cartera de crédito vencida");
            comp.CarteraUltimo = vCarteraVig[4] + vCarteraVen[4];

            var vInv = getValores("Ingresos financieros por instrum. financieros");
            comp.InversionesUltimo = vInv[4];

            var vDisp = getValores("Ingresos financieros por disponibilidades");
            comp.DisponibilidadesUltimo = vDisp[4];

            var vOtrosIng = getValores("Otros ingresos financieros");
            comp.OtrosIngresosUltimo = vOtrosIng[4];

            decimal totalIng = comp.TotalIngresosFinancieros[4];
            if (totalIng > 0)
            {
                comp.TotalIngresosUltimo = totalIng;
                if (comp.OtrosIngresosUltimo == 0 && (totalIng - comp.CarteraUltimo - comp.InversionesUltimo - comp.DisponibilidadesUltimo) > 0)
                {
                    comp.OtrosIngresosUltimo = totalIng - comp.CarteraUltimo - comp.InversionesUltimo - comp.DisponibilidadesUltimo;
                }
            }
            else
            {
                comp.TotalIngresosUltimo = comp.CarteraUltimo + comp.InversionesUltimo + comp.DisponibilidadesUltimo + comp.OtrosIngresosUltimo;
            }

            // Desglose de Gastos del Último Período
            var vPub = getValores("Gastos financieros por oblig. con público");
            comp.GastosPublicoUltimo = Math.Abs(vPub[4]);

            var vEnt = getValores("Gastos financieros por oblig. con entidades");
            comp.GastosEntidadesUltimo = Math.Abs(vEnt[4]);

            var vCuentasDiv = getValores("Gastos financieros cuentas por pagar diversas");
            var vSubord = getValores("Gastos por obligaciones subordinadas");
            var vOtrosGastosFin = getValores("Otros gastos financieros");
            comp.OtrosGastosFinancierosUltimo = Math.Abs(vCuentasDiv[4]) + Math.Abs(vSubord[4]) + Math.Abs(vOtrosGastosFin[4]);

            decimal totalGas = Math.Abs(comp.TotalGastosFinancieros[4]);
            if (totalGas > 0)
            {
                comp.TotalGastosUltimo = totalGas;
                if (comp.OtrosGastosFinancierosUltimo == 0 && (totalGas - comp.GastosPublicoUltimo - comp.GastosEntidadesUltimo) > 0)
                {
                    comp.OtrosGastosFinancierosUltimo = totalGas - comp.GastosPublicoUltimo - comp.GastosEntidadesUltimo;
                }
            }
            else
            {
                comp.TotalGastosUltimo = comp.GastosPublicoUltimo + comp.GastosEntidadesUltimo + comp.OtrosGastosFinancierosUltimo;
            }

            var vAdmin = getValores("Total gastos administrativos");
            comp.GastosAdminUltimo = Math.Abs(vAdmin[4]);

            var vDet = getValores("Por estimación de deterioro de activos");
            comp.GastosDeterioroUltimo = Math.Abs(vDet[4]);

            // Variaciones del margen y resultado
            for (int i = 0; i < 4; i++)
            {
                comp.VarMargenFinanciero[i] = comp.MargenFinancieroBruto[i + 1] - comp.MargenFinancieroBruto[i];
                comp.VarResultadoNeto[i] = comp.ResultadoPeriodo[i + 1] - comp.ResultadoPeriodo[i];
            }

            // Variaciones porcentuales para gráficos de evolución (5 períodos)
            comp.VarIngresosPct = new decimal?[5];
            comp.VarGastosPct = new decimal?[5];
            comp.VarIngresosPct[0] = null;
            comp.VarGastosPct[0] = null;

            for (int i = 1; i < 5; i++)
            {
                decimal prevIng = comp.TotalIngresosFinancieros[i - 1];
                decimal currIng = comp.TotalIngresosFinancieros[i];
                if (prevIng != 0)
                {
                    comp.VarIngresosPct[i] = Math.Round(((currIng - prevIng) / Math.Abs(prevIng)) * 100m, 1);
                }
                else
                {
                    comp.VarIngresosPct[i] = 0m;
                }

                decimal prevGas = Math.Abs(comp.TotalGastosFinancieros[i - 1]);
                decimal currGas = Math.Abs(comp.TotalGastosFinancieros[i]);
                if (prevGas != 0)
                {
                    comp.VarGastosPct[i] = Math.Round(((currGas - prevGas) / prevGas) * 100m, 1);
                }
                else
                {
                    comp.VarGastosPct[i] = 0m;
                }
            }

            model.DatosComposicion = comp;
        }
    }
}
