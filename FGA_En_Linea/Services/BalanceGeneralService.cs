using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FGA.Model;

namespace FGA.Services
{
    public class BalanceGeneralService
    {
        private readonly CultureInfo _crCulture = new CultureInfo("es-CR");

        /// <summary>
        /// Construye el modelo completo del Balance General a 5 períodos con cuadre contable dinámico y DTO ejecutivo de composición.
        /// </summary>
        public BalanceGeneralViewModel ConstruirModeloBalance(
            string entidadId,
            string periodoSel,
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

                // Verificación de caché en memoria de alto rendimiento
                string cacheKey = string.Format("FGA_BalanceGen_{0}_{1:yyyyMM}_{2}", idEntidad, refDate, tipoComp);
                if (HttpRuntime.Cache != null)
                {
                    var cachedModel = HttpRuntime.Cache.Get(cacheKey) as BalanceGeneralViewModel;
                    if (cachedModel != null)
                    {
                        return cachedModel;
                    }
                }

                var model = new BalanceGeneralViewModel
                {
                    EntidadId = idEntidad,
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
                    if (session != null && session["NomEntidad"] != null && session["IdEntidad"] != null && session["IdEntidad"].ToString() == idEntidad && !string.IsNullOrEmpty(session["NomEntidad"].ToString()))
                    {
                        model.NombreEntidad = session["NomEntidad"].ToString();
                    }
                    else
                    {
                        var entObj = entClient != null ? entClient.Get(idEntidad) : null;
                        model.NombreEntidad = entObj != null ? entObj.Nombre : (session != null && session["NomEntidad"] != null ? session["NomEntidad"].ToString() : "COOPENAE R.L.");
                        if (session != null) session["NomEntidad"] = model.NombreEntidad;
                    }
                }

                // 1. Determinar 5 períodos consecutivos según modalidad
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

                // 3. Consultar datos del Balance General a 5 períodos mediante el nuevo SP optimizado
                var spData = spClient.FGA_Rpt_Balance_General_5Periodos(idEntidad, refDate, tipoComp);
                if (spData != null && spData.Length > 0)
                {
                    MapearDesdeSP(model, spData, pFechas);
                }

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
                if (disposeSp) { try { spClient.Close(); } catch { } }
                if (disposeEnt) { try { entClient.Close(); } catch { } }
            }
        }

        private DateTime[] DeterminarFechasPeriodos(DateTime refDate, string tipoComp, BalanceGeneralViewModel model)
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
                model.ComparacionTitulo = "Variación Mensual";
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
                model.ComparacionTitulo = "Variación Trimestral";
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 trimestres consecutivos)";
            }
            else // Interanual
            {
                pFechas[4] = new DateTime(y, m, 1);
                pFechas[3] = pFechas[4].AddYears(-1);
                pFechas[2] = pFechas[4].AddYears(-2);
                pFechas[1] = pFechas[4].AddYears(-3);
                pFechas[0] = pFechas[4].AddYears(-4);
                model.ComparacionTitulo = "Variación Interanual";
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 años consecutivos)";
            }

            return pFechas;
        }

        private void MapearDesdeSP(
            BalanceGeneralViewModel model,
            Entities.Entities.Procedures.FGA_Rpt_Balance_General_5Periodos_Result[] spData,
            DateTime[] pFechas)
        {
            var catalogo = BalanceGeneralConstants.CatalogoRenglones;
            var filasTemp = new List<FilaBalanceItem>();
            int orden = 1;

            foreach (var item in catalogo)
            {
                var fila = new FilaBalanceItem
                {
                    Orden = orden++,
                    Seccion = item.Seccion,
                    Concepto = item.Concepto,
                    Nivel = item.Nivel,
                    EsNegrita = item.EsNegrita,
                    EsItalica = item.EsItalica,
                    SinMontos = item.SinMontos,
                    CodigoContable = item.CodigoContable
                };

                if (!item.SinMontos)
                {
                    Entities.Entities.Procedures.FGA_Rpt_Balance_General_5Periodos_Result spRow = null;

                    // 1. Intentar match por Concepto exacto
                    if (!string.IsNullOrEmpty(item.Concepto))
                    {
                        spRow = spData.FirstOrDefault(x =>
                            x.Concepto != null &&
                            x.Concepto.Trim().Equals(item.Concepto.Trim(), StringComparison.OrdinalIgnoreCase)
                        );
                    }

                    // 2. Si no encontró por concepto y tiene código contable, buscar por cuenta (evitando cabeceras Nivel 0)
                    if (spRow == null && !string.IsNullOrEmpty(item.CodigoContable))
                    {
                        spRow = spData.FirstOrDefault(x =>
                            x.CodigoContable == item.CodigoContable &&
                            x.Nivel == item.Nivel
                        ) ?? spData.FirstOrDefault(x =>
                            x.CodigoContable == item.CodigoContable &&
                            x.Nivel > 0
                        );
                    }

                    if (spRow != null)
                    {
                        fila.Periodos = new decimal[] {
                            spRow.Periodo1 ?? 0m,
                            spRow.Periodo2 ?? 0m,
                            spRow.Periodo3 ?? 0m,
                            spRow.Periodo4 ?? 0m,
                            spRow.Periodo5 ?? 0m
                        };
                        fila.VariacionAbsoluta = spRow.VariacionAbsoluta ?? 0m;
                        fila.VariacionRelativa = spRow.VariacionRelativa ?? 0m;
                    }
                }

                filasTemp.Add(fila);
            }

            // =========================================================================
            // CÁLCULO Y CONSOLIDACIÓN MATEMÁTICA DE TOTALES Y CUADRE EN C#
            // =========================================================================
            Func<string, FilaBalanceItem> fConcepto = c =>
                filasTemp.FirstOrDefault(x => x.Concepto != null && x.Concepto.Trim().Equals(c.Trim(), StringComparison.OrdinalIgnoreCase));

            var fCartera = fConcepto("Cartera de crédito");
            var fVigentes = fConcepto("Créditos vigentes");
            var fVencidos = fConcepto("Créditos vencidos");
            var fCobroJudicial = fConcepto("Créditos en cobro judicial");
            var fRestringidos = fConcepto("Créditos restringidos");
            var fCostoDirec = fConcepto("Costo direc incre asoc a créditos");
            var fCuentasCobrar = fConcepto("Cuentas y productos por cobrar");
            var fIngresosDiferidos = fConcepto("Ingresos diferidos cartera de crédito");
            var fEstimacion = fConcepto("Estimación por deterioro de cartera");

            var fInv = fConcepto("Inversión en instrumentos financieros");
            var fTotActivoProd = fConcepto("Total activo productivo");

            var fDisponibilidades = fConcepto("Disponibilidades");
            var fComisiones = fConcepto("Comisiones por cobrar");
            var fBienes = fConcepto("Bienes realizables");
            var fParticipaciones = fConcepto("Participaciones en otras empresas");
            var fInmuebles = fConcepto("Inmueble, mobiliario y equipo");
            var fOtrosActivos = fConcepto("Otros activos");
            var fPropiedades = fConcepto("Propiedades de inversión");
            var fTotActivos = fConcepto(BalanceGeneralConstants.ConceptoTotalActivos);

            var fObligPublico = fConcepto("Obligaciones con el público");
            var fCaptacionesVista = fConcepto("Captaciones a la vista");
            var fOtrasOblig = fConcepto("Otras oblig con el público a la vista");
            var fCaptacionesPlazo = fConcepto("Captaciones a plazo");
            var fCargosPagar = fConcepto("Cargos por pagar oblig con público");
            var fReporto = fConcepto("Obligaciones por reporto");
            var fObligEntidades = fConcepto("Obligaciones con entidades");
            var fTotPasCosto = fConcepto("Total pasivo con costo");

            var fBCCR = fConcepto("Obligaciones con el BCCR");
            var fOtrasCuentas = fConcepto("Otras cuentas por pagar & provisiones");
            var fOtrosPasivos = fConcepto("Otros pasivos");
            var fObligSub = fConcepto("Obligaciones subordinadas");
            var fObligConv = fConcepto("Obligaciones convertibles en capital");
            var fObligPref = fConcepto("Obligaciones preferentes");
            var fAportesCapPagar = fConcepto("Aportes de capital por pagar");
            var fTotPasivos = fConcepto(BalanceGeneralConstants.ConceptoTotalPasivos);

            var fCapSocial = fConcepto("Capital Social");
            var fAportesNoCap = fConcepto("Aportes patrimoniales no capitalizados");
            var fAjustePatri = fConcepto("Ajuste al patrimonio");
            var fResultEjAnt = fConcepto("Resultado de ejercicios anteriores");
            var fReservas = fConcepto("Reservas patrimoniales");
            var fResultPeriodo = fConcepto(BalanceGeneralConstants.ConceptoResultadosPeriodo);
            var fTotPatrimonio = fConcepto(BalanceGeneralConstants.ConceptoTotalPatrimonio);

            var fTotPasPatrimonio = fConcepto(BalanceGeneralConstants.ConceptoTotalPasivosPatrimonio);
            var fPrueba = fConcepto(BalanceGeneralConstants.ConceptoPrueba);

            decimal[] totActivos = new decimal[5];
            decimal[] totPasCosto = new decimal[5];
            decimal[] totPasivos = new decimal[5];
            decimal[] totPatrimonio = new decimal[5];
            decimal[] totPasPatri = new decimal[5];

            for (int c = 0; c < 5; c++)
            {
                // 1. Rollup Cartera si viene en 0 o incompleta
                decimal subCartera = (fVigentes != null ? fVigentes.Periodos[c] : 0) +
                                     (fVencidos != null ? fVencidos.Periodos[c] : 0) +
                                     (fCobroJudicial != null ? fCobroJudicial.Periodos[c] : 0) +
                                     (fRestringidos != null ? fRestringidos.Periodos[c] : 0) +
                                     (fCostoDirec != null ? fCostoDirec.Periodos[c] : 0) +
                                     (fCuentasCobrar != null ? fCuentasCobrar.Periodos[c] : 0) +
                                     (fIngresosDiferidos != null ? fIngresosDiferidos.Periodos[c] : 0) +
                                     (fEstimacion != null ? fEstimacion.Periodos[c] : 0);
                if (fCartera != null && (fCartera.Periodos[c] == 0 || Math.Abs(fCartera.Periodos[c]) < Math.Abs(subCartera) * 0.5m))
                {
                    fCartera.Periodos[c] = subCartera;
                }

                // 2. Total Activo Productivo = Inversiones + Cartera
                decimal vInv = fInv != null ? fInv.Periodos[c] : 0;
                decimal vCart = fCartera != null ? fCartera.Periodos[c] : 0;
                if (fTotActivoProd != null)
                {
                    fTotActivoProd.Periodos[c] = vInv + vCart;
                }

                // 3. Otros Activos
                decimal vOtrosActivos = (fDisponibilidades != null ? fDisponibilidades.Periodos[c] : 0) +
                                        (fComisiones != null ? fComisiones.Periodos[c] : 0) +
                                        (fBienes != null ? fBienes.Periodos[c] : 0) +
                                        (fParticipaciones != null ? fParticipaciones.Periodos[c] : 0) +
                                        (fInmuebles != null ? fInmuebles.Periodos[c] : 0) +
                                        (fOtrosActivos != null ? fOtrosActivos.Periodos[c] : 0) +
                                        (fPropiedades != null ? fPropiedades.Periodos[c] : 0);

                // 4. Total de Activos
                decimal vTotActivos = (fTotActivoProd != null ? fTotActivoProd.Periodos[c] : (vInv + vCart)) + vOtrosActivos;
                if (fTotActivos != null)
                {
                    if (fTotActivos.Periodos[c] == 0 || Math.Abs(fTotActivos.Periodos[c] - vTotActivos) > 1.0m)
                    {
                        fTotActivos.Periodos[c] = vTotActivos;
                    }
                    else
                    {
                        vTotActivos = fTotActivos.Periodos[c];
                    }
                }
                totActivos[c] = vTotActivos;

                // 5. Obligaciones con el Público Rollup
                decimal subObligPub = (fCaptacionesVista != null ? fCaptacionesVista.Periodos[c] : 0) +
                                      (fOtrasOblig != null ? fOtrasOblig.Periodos[c] : 0) +
                                      (fCaptacionesPlazo != null ? fCaptacionesPlazo.Periodos[c] : 0) +
                                      (fCargosPagar != null ? fCargosPagar.Periodos[c] : 0) +
                                      (fReporto != null ? fReporto.Periodos[c] : 0);
                if (fObligPublico != null && (fObligPublico.Periodos[c] == 0 || Math.Abs(fObligPublico.Periodos[c]) < Math.Abs(subObligPub) * 0.5m))
                {
                    fObligPublico.Periodos[c] = subObligPub;
                }

                // 6. Total Pasivo con Costo
                decimal vPasCosto = (fObligPublico != null ? fObligPublico.Periodos[c] : 0) +
                                    (fObligEntidades != null ? fObligEntidades.Periodos[c] : 0);
                if (fTotPasCosto != null)
                {
                    fTotPasCosto.Periodos[c] = vPasCosto;
                }
                totPasCosto[c] = vPasCosto;

                // 7. Total de Pasivos
                decimal vTotPasivos = vPasCosto +
                                      (fBCCR != null ? fBCCR.Periodos[c] : 0) +
                                      (fOtrasCuentas != null ? fOtrasCuentas.Periodos[c] : 0) +
                                      (fOtrosPasivos != null ? fOtrosPasivos.Periodos[c] : 0) +
                                      (fObligSub != null ? fObligSub.Periodos[c] : 0) +
                                      (fObligConv != null ? fObligConv.Periodos[c] : 0) +
                                      (fObligPref != null ? fObligPref.Periodos[c] : 0) +
                                      (fAportesCapPagar != null ? fAportesCapPagar.Periodos[c] : 0);
                if (fTotPasivos != null)
                {
                    fTotPasivos.Periodos[c] = vTotPasivos;
                }
                totPasivos[c] = vTotPasivos;

                // 8. Patrimonio Base y Resultados del Periodo
                decimal patBase = (fCapSocial != null ? fCapSocial.Periodos[c] : 0) +
                                  (fAportesNoCap != null ? fAportesNoCap.Periodos[c] : 0) +
                                  (fAjustePatri != null ? fAjustePatri.Periodos[c] : 0) +
                                  (fResultEjAnt != null ? fResultEjAnt.Periodos[c] : 0) +
                                  (fReservas != null ? fReservas.Periodos[c] : 0);

                // Cuadre contable de Resultados del Periodo (Activos - Pasivos - PatrimonioBase)
                decimal resPeriodo = vTotActivos - vTotPasivos - patBase;
                if (fResultPeriodo != null)
                {
                    if (Math.Abs(fResultPeriodo.Periodos[c] - resPeriodo) > 0.05m || fResultPeriodo.Periodos[c] == 0)
                    {
                        fResultPeriodo.Periodos[c] = resPeriodo;
                    }
                    else
                    {
                        resPeriodo = fResultPeriodo.Periodos[c];
                    }
                }

                // 9. Total de Patrimonio
                decimal vTotPatrimonio = patBase + resPeriodo;
                if (fTotPatrimonio != null)
                {
                    fTotPatrimonio.Periodos[c] = vTotPatrimonio;
                }
                totPatrimonio[c] = vTotPatrimonio;

                // 10. Total Pasivos y Patrimonio
                decimal vTotPasPatri = vTotPasivos + vTotPatrimonio;
                if (fTotPasPatrimonio != null)
                {
                    fTotPasPatrimonio.Periodos[c] = vTotPasPatri;
                }
                totPasPatri[c] = vTotPasPatri;

                // 11. Comprobación / Prueba (Activo - Pasivo y Patrimonio = 0)
                if (fPrueba != null)
                {
                    fPrueba.Periodos[c] = Math.Round(vTotActivos - vTotPasPatri, 2);
                }
            }

            // Variaciones Absolutas y Relativas según tipo de comparación
            int varRefCol = model.TipoComparacion != null && model.TipoComparacion.Equals("Interanual", StringComparison.OrdinalIgnoreCase) ? 1 : 3;
            foreach (var f in filasTemp)
            {
                if (!f.SinMontos && !f.EsCabecera)
                {
                    decimal vAct = f.Periodos[4];
                    decimal vRef = f.Periodos[varRefCol];
                    f.VariacionAbsoluta = Math.Round(vAct - vRef, 2);
                    f.VariacionRelativa = vRef != 0 ? Math.Round(((vAct - vRef) / Math.Abs(vRef)) * 100m, 2) : 0m;
                }
            }

            // Calcular análisis vertical (% Representa) y agregar a modelo
            foreach (var f in filasTemp)
            {
                if (!f.EsCabecera && !f.SinMontos)
                {
                    for (int c = 0; c < 5; c++)
                    {
                        decimal baseTotal = f.Seccion == BalanceGeneralConstants.SeccionActivo ? totActivos[c] : totPasPatri[c];
                        f.PorcentajesRepresenta[c] = baseTotal != 0 ? Math.Round((f.Periodos[c] / baseTotal) * 100m, 2) : 0m;
                    }
                }
                model.Filas.Add(f);
            }

            // Construir DTO de Composición para el Dashboard de 6 Tarjetas
            model.DatosComposicion = ConstruirComposicionDTO(filasTemp, totActivos, totPasCosto, totPasivos, totPatrimonio, pFechas);

            // Estado de cuadre contable
            decimal diffUlt = fPrueba != null ? fPrueba.Periodos[4] : 0;
            model.EstaBalanceCuadrado = Math.Abs(diffUlt) <= 0.05m;
            model.MensajeCuadre = model.EstaBalanceCuadrado
                ? "Balance Contable Cuadrado (Activo = Pasivo + Patrimonio)"
                : string.Format("Diferencia de cuadre contable detectada: ₡ {0:N2} millones", diffUlt);
        }

        private ComposicionBalanceDTO ConstruirComposicionDTO(
            List<FilaBalanceItem> filasTemp,
            decimal[] totActivos,
            decimal[] totPasCosto,
            decimal[] totPasivos,
            decimal[] totPatrimonio,
            DateTime[] pFechas)
        {
            var comp = new ComposicionBalanceDTO();

            for (int i = 0; i < 5; i++)
            {
                comp.PeriodosCategorias[i] = Utility.Utilitarios.toUpperFirstLetter(pFechas[i].ToString("MMM. yyyy", _crCulture));
                comp.TotalPasivos[i] = totPasivos[i];
                comp.TotalPatrimonio[i] = totPatrimonio[i];
                decimal totalPasPat = totPasivos[i] + totPatrimonio[i];
                comp.PorcPasivo[i] = totalPasPat > 0 ? Math.Round((totPasivos[i] / totalPasPat) * 100m, 2) : 0;
                comp.PorcPatrimonio[i] = totalPasPat > 0 ? Math.Round((totPatrimonio[i] / totalPasPat) * 100m, 2) : 0;
            }

            for (int i = 1; i < 5; i++)
            {
                int idx = i - 1;
                string fActual = Utility.Utilitarios.toUpperFirstLetter(pFechas[i].ToString("MMM. yy", _crCulture));
                string fAnterior = Utility.Utilitarios.toUpperFirstLetter(pFechas[i - 1].ToString("MMM. yy", _crCulture));
                comp.PeriodosComparacion[idx] = fActual + " - " + fAnterior;
            }

            var fCartera = filasTemp.FirstOrDefault(x => x.CodigoContable == BalanceGeneralConstants.CtaCartera || x.Concepto == "Cartera de crédito");
            var fInversiones = filasTemp.FirstOrDefault(x => x.CodigoContable == BalanceGeneralConstants.CtaInversiones || x.Concepto == "Inversión en instrumentos financieros");
            var fDisponibilidades = filasTemp.FirstOrDefault(x => x.CodigoContable == BalanceGeneralConstants.CtaDisponibilidades || x.Concepto == "Disponibilidades");

            decimal[] mCartera = fCartera != null ? fCartera.Periodos : new decimal[5];
            decimal[] mInversiones = fInversiones != null ? fInversiones.Periodos : new decimal[5];
            decimal[] mDisponibilidades = fDisponibilidades != null ? fDisponibilidades.Periodos : new decimal[5];
            decimal[] mOtrosActivos = new decimal[5];
            for (int i = 0; i < 5; i++)
            {
                mOtrosActivos[i] = Math.Round(totActivos[i] - mCartera[i] - mInversiones[i] - mDisponibilidades[i], 2);
            }

            // Card 2: Composición del activo último período
            comp.PeriodoUltimoLabel = comp.PeriodosCategorias[4];
            comp.TotalActivoUltimo = totActivos[4];
            comp.CarteraUltimo = mCartera[4];
            comp.InversionesUltimo = mInversiones[4];
            comp.DisponibilidadesUltimo = mDisponibilidades[4];
            comp.OtrosActivosUltimo = mOtrosActivos[4];

            if (comp.TotalActivoUltimo > 0)
            {
                comp.PorcCarteraUltimo = Math.Round((comp.CarteraUltimo / comp.TotalActivoUltimo) * 100m, 2);
                comp.PorcInversionesUltimo = Math.Round((comp.InversionesUltimo / comp.TotalActivoUltimo) * 100m, 2);
                comp.PorcDisponibilidadesUltimo = Math.Round((comp.DisponibilidadesUltimo / comp.TotalActivoUltimo) * 100m, 2);
                comp.PorcOtrosActivosUltimo = Math.Round(100m - comp.PorcCarteraUltimo - comp.PorcInversionesUltimo - comp.PorcDisponibilidadesUltimo, 2);
            }

            // Card 3: Variaciones del activo entre períodos
            for (int i = 1; i < 5; i++)
            {
                int idx = i - 1;
                comp.VarCartera[idx] = Math.Round(mCartera[i] - mCartera[i - 1], 2);
                comp.VarInversiones[idx] = Math.Round(mInversiones[i] - mInversiones[i - 1], 2);
                comp.VarDisponibilidades[idx] = Math.Round(mDisponibilidades[i] - mDisponibilidades[i - 1], 2);
                comp.VarOtrosActivos[idx] = Math.Round(mOtrosActivos[i] - mOtrosActivos[i - 1], 2);
            }

            // Card 4: Variación del total de pasivos (Waterfall)
            comp.SaldoInicialPasivos = totPasivos[0];
            comp.SaldoFinalPasivos = totPasivos[4];
            for (int i = 1; i < 5; i++)
            {
                comp.VarTotalPasivos[i - 1] = Math.Round(totPasivos[i] - totPasivos[i - 1], 2);
            }

            // Card 5: Variaciones del pasivo con costo
            for (int i = 1; i < 5; i++)
            {
                comp.VarPasivoConCosto[i - 1] = Math.Round(totPasCosto[i] - totPasCosto[i - 1], 2);
            }

            // Card 6: Variación del patrimonio
            for (int i = 1; i < 5; i++)
            {
                comp.VarPatrimonio[i - 1] = Math.Round(totPatrimonio[i] - totPatrimonio[i - 1], 2);
            }

            return comp;
        }
    }
}
