using System;
using System.Collections.Generic;

namespace FGA.Services
{
    public class CatalogoRenglonBalanceItem
    {
        public string Seccion { get; set; }
        public string Concepto { get; set; }
        public int Nivel { get; set; }
        public bool EsNegrita { get; set; }
        public bool EsItalica { get; set; }
        public bool SinMontos { get; set; }
        public string CodigoContable { get; set; }
        public int Id { get; set; }
    }

    public static class BalanceGeneralConstants
    {
        #region Códigos Contables SUGEF
        public const string CtaActivoTotal = "10000000";
        public const string CtaDisponibilidades = "11000000";
        public const string CtaInversiones = "12000000";
        public const string CtaCartera = "13000000";
        public const string CtaCreditosVigentes = "13100000";
        public const string CtaCreditosVencidos = "13200000";
        public const string CtaCreditosCobroJudicial = "13300000";
        public const string CtaCreditosRestringidos = "13400000";
        public const string CtaCostoDirecCreditos = "13600000";
        public const string CtaIngresosDiferidos = "13700000";
        public const string CtaCuentasProductosCobrar = "13800000";
        public const string CtaEstimacionDeterioro = "13900000";
        public const string CtaComisionesCobrar = "14000000";
        public const string CtaBienesRealizables = "15000000";
        public const string CtaParticipacionesEmpresas = "16000000";
        public const string CtaInmuebleMobiliarioEquipo = "17000000";
        public const string CtaOtrosActivos = "18000000";
        public const string CtaPropiedadesInversion = "19000000";

        public const string CtaPasivoTotal = "20000000";
        public const string CtaObligacionesPublico = "21000000";
        public const string CtaCaptacionesVista = "21100000";
        public const string CtaOtrasObligacionesVista = "21200000";
        public const string CtaCaptacionesPlazo = "21300000";
        public const string CtaObligacionesReporto = "21500000";
        public const string CtaCargosPagarPublico = "21900000";
        public const string CtaObligacionesEntidades = "23000000";
        public const string CtaObligacionesBCCR = "22000000";
        public const string CtaOtrasCuentasPagar = "24000000";
        public const string CtaOtrosPasivos = "25000000";
        public const string CtaObligacionesSubordinadas = "26000000";
        public const string CtaObligacionesConvertibles = "27000000";
        public const string CtaObligacionesPreferentes = "28000000";
        public const string CtaAportesCapitalPagar = "29000000";

        public const string CtaPatrimonioTotal = "30000000";
        public const string CtaCapitalSocial = "31000000";
        public const string CtaAportesNoCapitalizados = "32000000";
        public const string CtaAjustePatrimonio = "33000000";
        public const string CtaResultadoEjerciciosAnteriores = "34000000";
        public const string CtaReservasPatrimoniales = "35000000";
        #endregion

        #region Secciones y Conceptos
        public const string SeccionActivo = "ACTIVO";
        public const string SeccionPasivo = "PASIVO";
        public const string SeccionPatrimonio = "PATRIMONIO";
        public const string SeccionTotal = "TOTAL";

        public const string ConceptoTotalActivos = "Total de activos";
        public const string ConceptoTotalPasivos = "Total de pasivos";
        public const string ConceptoTotalPatrimonio = "Total de patrimonio";
        public const string ConceptoTotalPasivosPatrimonio = "Total pasivos y patrimonio";
        public const string ConceptoResultadosPeriodo = "Resultados del periodo";
        public const string ConceptoPrueba = "Prueba";
        #endregion

        #region Paleta de Colores Institucionales FFC
        public const string ColorAzulPrimario  = "#0071AD"; // Azul principal FFC
        public const string ColorNaranja       = "#FE7235"; // Naranja FFC
        public const string ColorCelesteMedio  = "#6DB5CB"; // Celeste medio FFC
        public const string ColorCelesteClaro  = "#B7D8DF"; // Celeste claro FFC
        public const string ColorGrisPrincipal = "#565656"; // Gris principal FFC
        public const string ColorGrisAuxiliar  = "#959595"; // Gris auxiliar FFC
        public const string ColorGrisClaro     = "#CFCFCF"; // Gris claro FFC
        public const string ColorAzulOscuro    = "#003F6B"; // Azul oscuro derivado FFC
        // Semánticos (no de marca, para estados del sistema)
        public const string ColorVerdeExito    = "#22c55e"; // Verde éxito (solo alertas sistema)
        public const string ColorRojoAlerta    = "#dc2626"; // Rojo alerta (solo errores sistema)
        public const string ColorBordeSuave    = "#e2e8f0"; // Borde neutro UI
        #endregion

        #region Catálogo Estructurado de Renglones del Balance
        public static readonly List<CatalogoRenglonBalanceItem> CatalogoRenglones = new List<CatalogoRenglonBalanceItem>
        {
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Activo", Nivel = 0, EsNegrita = true, EsItalica = false, SinMontos = true, CodigoContable = "", Id = 0 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Activo productivo", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = true, CodigoContable = "", Id = 0 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Inversión en instrumentos financieros", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaInversiones, Id = 1 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Cartera de crédito", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCartera, Id = 2 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Créditos vigentes", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCreditosVigentes, Id = 3 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Créditos vencidos", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCreditosVencidos, Id = 4 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Créditos en cobro judicial", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCreditosCobroJudicial, Id = 5 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Créditos restringidos", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCreditosRestringidos, Id = 6 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Costo direc incre asoc a créditos", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCostoDirecCreditos, Id = 7 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Cuentas y productos por cobrar", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCuentasProductosCobrar, Id = 9 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Ingresos diferidos cartera de crédito", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaIngresosDiferidos, Id = 8 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "  Estimación por deterioro de cartera", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaEstimacionDeterioro, Id = 10 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Total activo productivo", Nivel = 3, EsNegrita = true, EsItalica = true, SinMontos = false, CodigoContable = "", Id = 11 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Disponibilidades", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaDisponibilidades, Id = 12 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Comisiones por cobrar", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaComisionesCobrar, Id = 13 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Bienes realizables", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaBienesRealizables, Id = 14 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Participaciones en otras empresas", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaParticipacionesEmpresas, Id = 15 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Inmueble, mobiliario y equipo", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaInmuebleMobiliarioEquipo, Id = 16 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Otros activos", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaOtrosActivos, Id = 17 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = "Propiedades de inversión", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaPropiedadesInversion, Id = 18 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionActivo, Concepto = ConceptoTotalActivos, Nivel = 3, EsNegrita = true, EsItalica = false, SinMontos = false, CodigoContable = CtaActivoTotal, Id = 19 },

            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Pasivos", Nivel = 0, EsNegrita = true, EsItalica = false, SinMontos = true, CodigoContable = "", Id = 0 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Pasivo con costo", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = true, CodigoContable = "", Id = 0 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Obligaciones con el público", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaObligacionesPublico, Id = 20 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "  Captaciones a la vista", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCaptacionesVista, Id = 21 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "  Otras oblig con el público a la vista", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaOtrasObligacionesVista, Id = 22 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "  Captaciones a plazo", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCaptacionesPlazo, Id = 23 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "  Cargos por pagar oblig con público", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCargosPagarPublico, Id = 24 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "  Obligaciones por reporto", Nivel = 2, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaObligacionesReporto, Id = 245 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Obligaciones con entidades", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaObligacionesEntidades, Id = 25 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Total pasivo con costo", Nivel = 3, EsNegrita = true, EsItalica = true, SinMontos = false, CodigoContable = "", Id = 26 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Obligaciones con el BCCR", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaObligacionesBCCR, Id = 27 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Otras cuentas por pagar & provisiones", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaOtrasCuentasPagar, Id = 28 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Otros pasivos", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaOtrosPasivos, Id = 29 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Obligaciones subordinadas", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaObligacionesSubordinadas, Id = 30 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Obligaciones convertibles en capital", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaObligacionesConvertibles, Id = 31 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Obligaciones preferentes", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaObligacionesPreferentes, Id = 32 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = "Aportes de capital por pagar", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaAportesCapitalPagar, Id = 33 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPasivo, Concepto = ConceptoTotalPasivos, Nivel = 3, EsNegrita = true, EsItalica = false, SinMontos = false, CodigoContable = CtaPasivoTotal, Id = 34 },

            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = "Patrimonio", Nivel = 0, EsNegrita = true, EsItalica = false, SinMontos = true, CodigoContable = "", Id = 0 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = "Capital Social", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaCapitalSocial, Id = 35 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = "Aportes patrimoniales no capitalizados", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaAportesNoCapitalizados, Id = 36 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = "Ajuste al patrimonio", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaAjustePatrimonio, Id = 37 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = "Resultado de ejercicios anteriores", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaResultadoEjerciciosAnteriores, Id = 38 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = "Reservas patrimoniales", Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = CtaReservasPatrimoniales, Id = 39 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = ConceptoResultadosPeriodo, Nivel = 1, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = "", Id = 40 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionPatrimonio, Concepto = ConceptoTotalPatrimonio, Nivel = 3, EsNegrita = true, EsItalica = false, SinMontos = false, CodigoContable = CtaPatrimonioTotal, Id = 41 },

            new CatalogoRenglonBalanceItem { Seccion = SeccionTotal, Concepto = ConceptoTotalPasivosPatrimonio, Nivel = 3, EsNegrita = true, EsItalica = false, SinMontos = false, CodigoContable = "", Id = 42 },
            new CatalogoRenglonBalanceItem { Seccion = SeccionTotal, Concepto = ConceptoPrueba, Nivel = 3, EsNegrita = false, EsItalica = false, SinMontos = false, CodigoContable = "", Id = 999 }
        };
        #endregion
    }
}
