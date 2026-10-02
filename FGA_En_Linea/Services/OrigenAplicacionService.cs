using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FGA.Model;

namespace FGA.Services
{
    public class OrigenAplicacionService
    {
        private readonly CultureInfo _crCulture = new CultureInfo("es-CR");
        private readonly BalanceGeneralService _balanceService = new BalanceGeneralService();

        public OrigenAplicacionViewModel ConstruirModeloOrigen(
            string entidadId,
            string periodoSel,
            string tipoCompSel,
            HttpSessionStateBase session,
            FGA_En_Linea.SPService.SPClient spClient = null,
            FGA_En_Linea.EntidadService.EntidadServiceClient entClient = null)
        {
            var idEntidad = string.IsNullOrEmpty(entidadId) ? (session["IdEntidad"] != null ? session["IdEntidad"].ToString() : "2") : entidadId;
            session["IdEntidad"] = idEntidad;

            var tipoComp = string.IsNullOrEmpty(tipoCompSel) ? (session["TipoComparacion"] != null ? session["TipoComparacion"].ToString() : "Interanual") : tipoCompSel;
            session["TipoComparacion"] = tipoComp;

            var periodoRef = string.IsNullOrEmpty(periodoSel) ? (session["Periodo2"] != null ? session["Periodo2"].ToString() : (session["Periodo"] != null ? session["Periodo"].ToString() : DateTime.Now.ToString("MM/yyyy"))) : periodoSel;

            // Obtenemos el Balance General modelado
            var balanceModel = _balanceService.ConstruirModeloBalance(idEntidad, periodoRef, tipoComp, session, spClient, entClient);
            session["Periodo2"] = balanceModel.PeriodoReferencia.ToShortDateString();

            var model = new OrigenAplicacionViewModel
            {
                EntidadId = balanceModel.EntidadId,
                EntidadNombre = balanceModel.NombreEntidad,
                TipoComparacion = tipoComp,
                LogoUrl = balanceModel.LogoUrl,
                FechaBase = balanceModel.PeriodoReferencia
            };

            // Determinar fechas base y referencia
            DateTime fechaBase = balanceModel.PeriodoReferencia;
            DateTime fechaRef;

            if (tipoComp.Equals("Mensual", StringComparison.OrdinalIgnoreCase))
            {
                fechaRef = fechaBase.AddMonths(-1);
                model.ComparacionTitulo = string.Format("Variación Mensual ({0:MMM yyyy} vs {1:MMM yyyy})", fechaBase, fechaRef);
            }
            else if (tipoComp.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
            {
                int currentQuarter = (fechaBase.Month - 1) / 3 + 1;
                int prevQuarterEndMonth = (currentQuarter - 1) * 3;
                if (prevQuarterEndMonth == 0)
                {
                    fechaRef = new DateTime(fechaBase.Year - 1, 12, 1);
                }
                else
                {
                    fechaRef = new DateTime(fechaBase.Year, prevQuarterEndMonth, 1);
                }
                model.ComparacionTitulo = string.Format("Variación Trimestral ({0:MMM yyyy} vs {1:MMM yyyy})", fechaBase, fechaRef);
            }
            else // Interanual
            {
                fechaRef = fechaBase.AddYears(-1);
                model.ComparacionTitulo = string.Format("Variación Interanual ({0:MMM yyyy} vs {1:MMM yyyy})", fechaBase, fechaRef);
            }

            model.FechaComparacion = fechaRef;

            // Mapear filas comparativas del Balance
            int idxRef = tipoComp.Equals("Interanual", StringComparison.OrdinalIgnoreCase) ? 1 : 3; // P2 o P4 (0-based)
            int idxBase = 4; // P5 siempre es el mes base

            var cuentasDetalleParaFlujo = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                BalanceGeneralConstants.CtaInversiones,
                BalanceGeneralConstants.CtaCartera,
                BalanceGeneralConstants.CtaDisponibilidades,
                BalanceGeneralConstants.CtaComisionesCobrar,
                BalanceGeneralConstants.CtaBienesRealizables,
                BalanceGeneralConstants.CtaParticipacionesEmpresas,
                BalanceGeneralConstants.CtaInmuebleMobiliarioEquipo,
                BalanceGeneralConstants.CtaOtrosActivos,
                BalanceGeneralConstants.CtaPropiedadesInversion,
                BalanceGeneralConstants.CtaObligacionesPublico,
                BalanceGeneralConstants.CtaObligacionesEntidades,
                BalanceGeneralConstants.CtaObligacionesBCCR,
                BalanceGeneralConstants.CtaOtrasCuentasPagar,
                BalanceGeneralConstants.CtaOtrosPasivos,
                BalanceGeneralConstants.CtaObligacionesSubordinadas,
                BalanceGeneralConstants.CtaObligacionesConvertibles,
                BalanceGeneralConstants.CtaObligacionesPreferentes,
                BalanceGeneralConstants.CtaAportesCapitalPagar,
                BalanceGeneralConstants.CtaCapitalSocial,
                BalanceGeneralConstants.CtaAportesNoCapitalizados,
                BalanceGeneralConstants.CtaAjustePatrimonio,
                BalanceGeneralConstants.CtaResultadoEjerciciosAnteriores,
                BalanceGeneralConstants.CtaReservasPatrimoniales
            };

            foreach (var bFila in balanceModel.Filas)
            {
                decimal mRef = (bFila.Periodos != null && bFila.Periodos.Length > idxRef) ? bFila.Periodos[idxRef] : 0m;
                decimal mBase = (bFila.Periodos != null && bFila.Periodos.Length > idxBase) ? bFila.Periodos[idxBase] : 0m;

                var compFila = new FilaBalanceComparativoItem
                {
                    Orden = bFila.Orden,
                    Seccion = bFila.Seccion,
                    Concepto = bFila.Concepto,
                    Nivel = bFila.Nivel,
                    EsNegrita = bFila.EsNegrita,
                    EsItalica = bFila.EsItalica,
                    SinMontos = bFila.SinMontos,
                    CodigoContable = bFila.CodigoContable,
                    MontoReferencia = mRef,
                    MontoBase = mBase
                };
                model.FilasBalance.Add(compFila);

                // Clasificar en Origen o Aplicación si es cuenta analítica de flujo
                bool esCuentaFlujo = (!string.IsNullOrEmpty(bFila.CodigoContable) && cuentasDetalleParaFlujo.Contains(bFila.CodigoContable))
                                     || bFila.Concepto.Trim().Equals("Resultados del periodo", StringComparison.OrdinalIgnoreCase);

                if (esCuentaFlujo && !bFila.SinMontos)
                {
                    decimal dif = compFila.Diferencia;
                    if (dif != 0m)
                    {
                        if (bFila.Seccion == BalanceGeneralConstants.SeccionActivo)
                        {
                            if (dif < 0m)
                            {
                                model.ItemsOrigen.Add(new ItemOrigenAplicacion
                                {
                                    Tipo = "Origen",
                                    Categoria = "Activo",
                                    Concepto = "Disminución en " + bFila.Concepto.Trim(),
                                    Monto = Math.Abs(dif)
                                });
                            }
                            else
                            {
                                model.ItemsAplicacion.Add(new ItemOrigenAplicacion
                                {
                                    Tipo = "Aplicación",
                                    Categoria = "Activo",
                                    Concepto = "Aumento en " + bFila.Concepto.Trim(),
                                    Monto = Math.Abs(dif)
                                });
                            }
                        }
                        else // Pasivo o Patrimonio
                        {
                            if (dif > 0m)
                            {
                                model.ItemsOrigen.Add(new ItemOrigenAplicacion
                                {
                                    Tipo = "Origen",
                                    Categoria = bFila.Seccion == BalanceGeneralConstants.SeccionPasivo ? "Pasivo" : "Patrimonio",
                                    Concepto = "Aumento en " + bFila.Concepto.Trim(),
                                    Monto = Math.Abs(dif)
                                });
                            }
                            else
                            {
                                model.ItemsAplicacion.Add(new ItemOrigenAplicacion
                                {
                                    Tipo = "Aplicación",
                                    Categoria = bFila.Seccion == BalanceGeneralConstants.SeccionPasivo ? "Pasivo" : "Patrimonio",
                                    Concepto = "Disminución en " + bFila.Concepto.Trim(),
                                    Monto = Math.Abs(dif)
                                });
                            }
                        }
                    }
                }
            }

            model.TotalOrigen = model.ItemsOrigen.Sum(x => x.Monto);
            model.TotalAplicacion = model.ItemsAplicacion.Sum(x => x.Monto);

            // Calcular porcentajes de participación
            if (model.TotalOrigen > 0m)
            {
                foreach (var item in model.ItemsOrigen)
                {
                    item.PorcentajeParticipacion = Math.Round((item.Monto / model.TotalOrigen) * 100m, 2);
                }
            }
            if (model.TotalAplicacion > 0m)
            {
                foreach (var item in model.ItemsAplicacion)
                {
                    item.PorcentajeParticipacion = Math.Round((item.Monto / model.TotalAplicacion) * 100m, 2);
                }
            }

            return model;
        }
    }
}
