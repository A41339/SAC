using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Entities.Entities.Procedures;

public class SP : ISP
{
    private readonly FGAEntities sp = new FGAEntities();

    public List<FGA_Consultar_Grafico_Suficiencia_Result> FGA_Consultar_Grafico_Suficiencia(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF) {
        return sp.FGA_Consultar_Grafico_Suficiencia(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Sessiones_Result> FGA_Consultar_Sessiones(DateTime periodo)
    {
        return sp.FGA_Consultar_Sessiones(periodo).ToList();
    }

    public List<FGA_Consultar_Tipos_Indicadores_Result> FGA_Consultar_Tipos_Indicadores(string IdEntidad, string IdEntidadConsulta)
    {
        return sp.FGA_Consultar_Tipos_Indicadores(IdEntidad, IdEntidadConsulta).ToList();
    }

    public List<FGA_Consultar_Grafico_Indicador_Result> FGA_Consultar_Grafico_Indicador(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF, string iNDICADOR)
    {
        return sp.FGA_Consultar_Grafico_Indicador(iDENTIDAD, pERIODOI, pERIODOF, iNDICADOR).ToList();
    }

    public int FGA_Cierre_Mensual(string iDENTIDAD, DateTime? pERIODO)
    {
        return sp.FGA_Cierre_Mensual(iDENTIDAD, pERIODO);
    }

    public List<FGA_Consultar_Balance_Comprobacion_Result> FGA_Consultar_Balance_Comprobacion(string iDENTIDAD, DateTime? pERIODO)
    {
        return sp.FGA_Consultar_Balance_Comprobacion(iDENTIDAD, pERIODO).ToList();
    }
    public List<FGA_Consultar_Cartera_Credito_Result> FGA_Consultar_Cartera_Credito(string iDENTIDAD, DateTime? pERIODO,
        int pageSize, int skip, string search, ref int count)
    {
        var list = new List<FGA_Consultar_Cartera_Credito_Result>();
        if (!string.IsNullOrEmpty(search))
        {
            list = sp.FGA_Consultar_Cartera_Credito(iDENTIDAD, pERIODO).Where(o => o.DEUDOR.ToLower().Contains(search.ToLower())
                    || o.OPERACION.ToLower().Contains(search.ToLower())
                    || o.TIPOCARTERA.ToLower().Contains(search.ToLower())
                    || o.CATEGORIARIESGO.ToLower().Contains(search.ToLower()))
            .ToList();
        }
        else
            list = sp.FGA_Consultar_Cartera_Credito(iDENTIDAD, pERIODO).ToList();

        count = list.Count();
        return list.Skip(skip).Take(pageSize).ToList();
    }
    public List<FGA_Consultar_Maduracion_Cartera_Result> FGA_Consultar_Maduracion_Cartera(string iDENTIDAD, DateTime? pERIODO,
        int pageSize, int skip, string search, ref int count)
    {
        var list = new List<FGA_Consultar_Maduracion_Cartera_Result>();
        if (!string.IsNullOrEmpty(search))
        {
            list = list.Where(o => o.IDOPERACION.ToLower().Contains(search.ToLower())
                     || o.TIPOCARTERA.ToString().ToLower().Contains(search.ToLower())
                     || o.SALDO.ToString().ToLower().Contains(search.ToLower())
                     || o.PLAZORESTANTE.ToString().ToLower().Contains(search.ToLower())).ToList();
        }
        else
            list = sp.FGA_Consultar_Maduracion_Cartera(iDENTIDAD, pERIODO).ToList();


        count = list.Count();
        return list.Skip(skip).Take(pageSize).ToList();
    }
    public List<FGA_Consultar_Maduracion_Cartera_Det_Result> FGA_Consultar_Maduracion_Cartera_Det(string iDENTIDAD, DateTime? pERIODO)
    {
        return sp.FGA_Consultar_Maduracion_Cartera_Det(iDENTIDAD, pERIODO).ToList();
    }
    public List<FGA_Consultar_Monitor_Result> FGA_Consultar_Monitor()
    {
        return sp.FGA_Consultar_Monitor().ToList();
    }
    public List<FGA_Consultar_Riesgo_Liquidez_Result> FGA_Consultar_Riesgo_Liquidez(string iDENTIDAD, DateTime? pERIODO)
    {
        return sp.FGA_Consultar_Riesgo_Liquidez(iDENTIDAD, pERIODO).ToList();
    }
    public List<FGA_Consultar_Tasa_Ponderada_Result> FGA_Consultar_Tasa_Ponderada(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Consultar_Tasa_Ponderada(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }
    public int FGA_Ejecutar_Cierre()
    {
        return sp.FGA_Ejecutar_Cierre();
    }
    public List<FGA_Analisis_Horizontal_Result> FGA_Analisis_Horizontal(string iDENTIDAD, int iNDICADOR)
    {
        return sp.FGA_Analisis_Horizontal(iDENTIDAD, iNDICADOR).ToList();
    }
    public List<FGA_Analisis_Vertical_Result> FGA_Analisis_Vertical(string iDENTIDAD, int iNDICADOR)
    {
        return sp.FGA_Analisis_Vertical(iDENTIDAD, iNDICADOR).ToList();
    }
    public List<FGA_Consultar_Dashboard_Result> FGA_Consultar_Dashboard(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF)
    {
        return sp.FGA_Consultar_Dashboard(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }
    public DateTime FGA_Consultar_FechaCierre(string iDENTIDAD)
    {
        return sp.FGA_Consultar_FechaCierre(iDENTIDAD).First().Value;
    }
    public List<FGA_Consultar_Calific_Cartera_Result> FGA_Consultar_Calif_Cartera(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL, String cALIFICACION)
    {
        return sp.FGA_Consultar_Calific_Cartera(iDENTIDAD, pERIDOOINICIAL, pERIODOFINAL, cALIFICACION).ToList();
    }
    public List<FGA_Consultar_CalificE_Result> FGA_Consultar_CalificE(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL)
    {
        return sp.FGA_Consultar_CalificE(iDENTIDAD, pERIDOOINICIAL, pERIODOFINAL).ToList();
    }
    public List<FGA_Consultar_CarteraTotal_Result> FGA_Consultar_CarteraTotal(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL, int pMESES)
    {
        return sp.FGA_Consultar_CarteraTotal(iDENTIDAD, pERIDOOINICIAL, pERIODOFINAL, pMESES).ToList();
    }
    public List<FGA_Consultar_CuentasLiq_Result> FGA_Consultar_CuentasLiq(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL)
    {
        return sp.FGA_Consultar_CuentasLiq(iDENTIDAD, pERIDOOINICIAL, pERIODOFINAL).ToList();
    }
    public List<FGA_Consultar_RecuperacionActivos_Result> FGA_Consultar_RecuperacionActivos(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL)
    {
        return sp.FGA_Consultar_RecuperacionActivos(iDENTIDAD, pERIDOOINICIAL, pERIODOFINAL).ToList();
    }
    public List<FGA_Rpt_Balance_Result> FGA_Rpt_Balance(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3)
    {
        return sp.FGA_Rpt_Balance(iDENTIDAD, pERIODO1, pERIODO2, pERIODO3).ToList();
    }
    public List<FGA_Rpt_EstadoResultados_Result> FGA_Rpt_EstadoResultados(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3)
    {
        return sp.FGA_Rpt_EstadoResultados(iDENTIDAD, pERIODO1, pERIODO2, pERIODO3).ToList();
    }
    public List<string> FGA_Consultar_ArchivosPendientes(string iDENTIDAD)
    {
        return sp.FGA_Consultar_ArchivosPendientes(iDENTIDAD).ToList();
    }
    public List<FGA_Consultar_ArchivosCargados_Result> FGA_Consultar_ArchivosCargados(string iDENTIDAD, DateTime? pERIODO, bool pTipoEntidad)
    {
        return sp.FGA_Consultar_ArchivosCargados(iDENTIDAD, pERIODO, pTipoEntidad).ToList();
    }
    public List<FGA_Consultar_Matrices_Desmejora_Cartera_Result> FGA_Consultar_Matrices_Desmejora_Cartera(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL, DateTime? pERIODOINICIAL_COMP, DateTime? pERIODOFINAL_COMP)
    {
        return sp.FGA_Consultar_Matrices_Desmejora_Cartera(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL, pERIODOINICIAL_COMP, pERIODOFINAL_COMP).ToList();
    }
    public List<FGA_Consultar_Matrices_PerdidaEstimada_Result> FGA_Consultar_Matrices_PerdidaEstimada(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL)
    {
        return sp.FGA_Consultar_Matrices_PerdidaEstimada(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL).ToList();
    }
    public List<FGA_Consultar_Matrices_Variacion_Cartera_Result> FGA_Consultar_Matrices_Variacion_Cartera(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL)
    {
        return sp.FGA_Consultar_Matrices_Variacion_Cartera(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL).ToList();
    }
    public List<FGA_Consultar_Indicadores_Cartera_Result> FGA_Consultar_Indicadores_Cartera(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3)
    {
        return sp.FGA_Consultar_Indicadores_Cartera(iDENTIDAD, pERIODO1, pERIODO2, pERIODO3).ToList();
    }
    public List<FGA_Consultar_CAMEL_Result> FGA_Consultar_CAMEL(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3, string iNDRESUMEN)
    {
        return sp.FGA_Consultar_CAMEL(iDENTIDAD, pERIODO1, pERIODO2, pERIODO3, iNDRESUMEN).ToList();
    }
    public List<FGA_Consultar_Suficiencia_Result> FGA_Consultar_Suficiencia(string iDENTIDAD, DateTime? pERIODO, DateTime? pERIODO_COMPARAR, string iNDRESUMEN)
    {
        return sp.FGA_Consultar_Suficiencia(iDENTIDAD, pERIODO, pERIODO_COMPARAR, iNDRESUMEN).ToList();
    }
    public List<FGA_Consultar_Suficiencia_306_Result> FGA_Consultar_Suficiencia_306(string iDENTIDAD, DateTime? pERIODO, DateTime? pERIODO_COMPARAR, string iNDRESUMEN)
    {
        return sp.FGA_Consultar_Suficiencia_306(iDENTIDAD, pERIODO, pERIODO_COMPARAR, iNDRESUMEN).ToList();
    }
    public List<FGA_Consultar_Indicadores_Result> FGA_Consultar_Indicadores(string iDENTIDAD, int iNDICADOR)
    {
        return sp.FGA_Consultar_Indicadores(iDENTIDAD, iNDICADOR).ToList();
    }
    public List<FGA_Consultar_Otros_Indicadores_Result> FGA_Consultar_Otros_Indicadores(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3)
    {
        return sp.FGA_Consultar_Otros_Indicadores(iDENTIDAD, pERIODO1, pERIODO2, pERIODO3).ToList();
    }
    public List<FGA_Consultar_Calculos_Personalizados_Result> FGA_Consultar_Calculos_Personalizados(string iDENTIDAD, string idEntidadConsulta, DateTime? pERIODO, Nullable<System.DateTime> pERIODO_COMPARAR)
    {
        return sp.FGA_Consultar_Calculos_Personalizados(iDENTIDAD, pERIODO, pERIODO_COMPARAR, idEntidadConsulta).ToList();
    }
    public List<FGA_Consultar_Tipo_Cartera_Result> FGA_Consultar_Tipo_Cartera(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL, int cTIPOCARTERA)
    {
        return sp.FGA_Consultar_Tipo_Cartera(iDENTIDAD, pERIDOOINICIAL, pERIODOFINAL, cTIPOCARTERA).ToList();
    }
    public List<FGA_Consultar_Tipo_Cartera_E_Result> FGA_Consultar_Tipo_Cartera_E(string iDENTIDAD, DateTime pERIODOINICIAL, DateTime pERIODOFINAL)
    {
        return sp.FGA_Consultar_Tipo_Cartera_E(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL).ToList();
    }
    public List<FGA_Consultar_Modelo_Tasas_Result> FGA_Consultar_Modelo_Tasas(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL, int pOpcion)
    {
        return sp.FGA_Consultar_Modelo_Tasas(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL, pOpcion).ToList();
    }
    public List<FGA_Consultar_Modelo_Margen_Result> FGA_Consultar_Modelo_Margen(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL)
    {
        return sp.FGA_Consultar_Modelo_Margen(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL).ToList();
    }
    public List<FGA_Consultar_Graficos_Financieros_Result> FGA_Consultar_Graficos_Financieros(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3)
    {
        return sp.FGA_Consultar_Graficos_Financieros(iDENTIDAD, pERIODO1, pERIODO2, pERIODO3).ToList();
    }
    public List<FGA_Consultar_Balance_Comprobacion_Rango_Result> FGA_Consultar_Balance_Comprobacion_Rango(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL, bool? pISMENSUAL, bool? pINDMODELOTASAS)
    {
        return sp.FGA_Consultar_Balance_Comprobacion_Rango(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL, pISMENSUAL, pINDMODELOTASAS).ToList();
    }
    public List<FGA_Consultar_Variacion_CS_Result> FGA_Consultar_Variacion_CS(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL)
    {
        return sp.FGA_Consultar_Variacion_CS(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL).ToList();
    }
    public bool FGA_Valida_Permite_Cargar(string iDENTIDAD)
    {
        bool? valida = sp.FGA_Valida_Permite_Cargar(iDENTIDAD).First();
        return valida == null ? false : valida.Value;
    }

    public List<FGA_Consultar_Riesgo_Liquidez_Rango_Result> FGA_Consultar_Riesgo_Liquidez_Rango(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Consultar_Riesgo_Liquidez_Rango(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Credito_Categoria_Rango_Result> FGA_Consultar_Credito_Categoria_Rango(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Consultar_Credito_Categoria_Rango(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Gastos_Ingresos_Result> FGA_Consultar_Gastos_Ingresos(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Consultar_Gastos_Ingresos(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Grafico_Mora_Cartera_Result> FGA_Consultar_Grafico_Mora_Cartera(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Consultar_Grafico_Mora_Cartera(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Generar_Proyeccion_Result> FGA_Generar_Proyeccion(string iDENTIDAD, DateTime pERIODOCORTE, int pERIODOS_HISTORICOS, int pERIODOS_PROYECTAR, string cUENTA)
    {
        return sp.FGA_Generar_Proyeccion(iDENTIDAD, pERIODOCORTE, pERIODOS_HISTORICOS, pERIODOS_PROYECTAR, cUENTA).ToList();
    }

    public List<FGA_Consultar_Cuentas_Proyectar_Result> FGA_Consultar_Cuentas_Proyectar(string iDENTIDAD, bool iND_RESUMEN)
    {
        return sp.FGA_Consultar_Cuentas_Proyectar(iDENTIDAD, iND_RESUMEN).ToList();
    }

    public List<FGA_Consultar_Datos_Historicos_Result> FGA_Consultar_Datos_Historicos(string iDENTIDAD, DateTime pERIODOCORTE, int pERIODOS, string cUENTA)
    {
        return sp.FGA_Consultar_Datos_Historicos(iDENTIDAD, pERIODOCORTE, pERIODOS, cUENTA).ToList();
    }

    public List<string> FGA_Validar_Proyecciones_IRL(string iDENTIDAD, DateTime pERIODO)
    {
        return sp.FGA_Validar_Proyecciones_IRL(iDENTIDAD, pERIODO).ToList();
    }

    public List<FGA_Consultar_Estado_Proyeccion_Result> FGA_Consultar_Estado_Proyeccion(string iDENTIDAD, DateTime pERIODO)
    {
        return sp.FGA_Consultar_Estado_Proyeccion(iDENTIDAD, pERIODO).ToList();
    }

    public List<FGA_Consultar_IRL_Cuentas_Result> FGA_Consultar_IRL_Cuentas(string IdEntidad, DateTime Periodo,
        Decimal? Pond_Efectivo, Decimal? Pond_BCCR, Decimal? Pond_Ent_Finan, Decimal? Pond_Ent_Exterior, Decimal? Pond_Inv_BCCR,
        Decimal? Pond_Inv_SP_No_Financ, Decimal? Pond_Inv_Ots_Entidades)
    {
        return sp.FGA_Consultar_IRL_Cuentas(IdEntidad, Periodo, Pond_Efectivo, Pond_BCCR, Pond_Ent_Finan, Pond_Ent_Exterior,
            Pond_Inv_BCCR, Pond_Inv_SP_No_Financ, Pond_Inv_Ots_Entidades).ToList();
    }

    public List<FGA_Consultar_Matrices_Probabilidad_Result> FGA_Consultar_Matrices_Probabilidada(string iDENTIDAD, DateTime pERIODOINICIAL, DateTime pERIODOFINAL)
    {
        return sp.FGA_Consultar_Matrices_Probabilidad(iDENTIDAD, pERIODOINICIAL, pERIODOFINAL).ToList();
    }

    public void FGA_Borrar_Proyeccion(string IdEntidad, DateTime Periodo, string Cuenta)
    {
        sp.FGA_Borrar_Proyeccion(IdEntidad, Periodo, Cuenta);
    }

    public List<FGA_Consultar_Variacion_Interanual_SF_Result> FGA_Consultar_Variacion_Interanual_SF(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Consultar_Variacion_Interanual_SF(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Rpt_ER_SF_Result> FGA_Rpt_ER_SF(DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Rpt_ER_SF(pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Rpt_Balance_SF_Result> FGA_Rpt_Balance_SF(DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Rpt_Balance_SF(pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Ingreso_SF_Result> FGA_Consultar_Ingreso_SF(DateTime? pERIODO1)
    {
        return sp.FGA_Consultar_Ingreso_SF(pERIODO1).ToList();
    }

    public List<FGA_Consultar_Gasto_SF_Result> FGA_Consultar_Gasto_SF(DateTime? pERIODO1)
    {
        return sp.FGA_Consultar_Gasto_SF(pERIODO1).ToList();
    }

    public List<FGA_Consultar_Composicion_Balance_SF_Result> FGA_Consultar_Composicion_Balance_SF(DateTime? pERIODO1)
    {
        return sp.FGA_Consultar_Composicion_Balance_SF(pERIODO1).ToList();
    }

    public List<FGA_Consultar_Indicadores_Sugef_Result> FGA_Consultar_Indicadores_Sugef(Nullable<System.DateTime> pERIODOI, Nullable<System.DateTime> pERIODOF, int? iDINDICADOR)
    {
        return sp.FGA_Consultar_Indicadores_Sugef(pERIODOI, pERIODOF, iDINDICADOR).ToList();
    }

    public List<FGA_Consultar_Tipo_Cartera_Sector_Result> FGA_Consultar_Tipo_Cartera_Sector(DateTime? pERIODO, int idSector)
    {
        return sp.FGA_Consultar_Tipo_Cartera_Sector(idSector, pERIODO).ToList();
    }

    public List<FGA_Consultar_Variacion_Cartera_Sector_Result> FGA_Consultar_Variacion_Cartera_Sector(DateTime periodoInicial, DateTime periodoFinal, int idSector, int numMeses)
    {
        return sp.FGA_Consultar_Variacion_Cartera_Sector(idSector, periodoInicial, periodoFinal, numMeses).ToList();
    }


    public List<FGA_Consultar_Cartera_Rango_SF_Result> FGA_Consultar_Cartera_Rango_SF(DateTime? pERIODOI, DateTime? pERIODOF)
    {
        return sp.FGA_Consultar_Cartera_Rango_SF(pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Composicion_Cartera_Sector_Result> FGA_Consultar_Composicion_Cartera_Sector(DateTime periodo)
    {
        return sp.FGA_Consultar_Composicion_Cartera_Sector(periodo).ToList();
    }

    public List<FGA_Consultar_Composicion_Mora_Sector_Result> FGA_Consultar_Composicion_Mora_Sector(DateTime periodo)
    {
        return sp.FGA_Consultar_Composicion_Mora_Sector(periodo).ToList();
    }

    public List<FGA_Consultar_Indicadores_Formulas_Result> FGA_Consultar_Indicadores_Formulas(string iDENTIDAD, Nullable<System.DateTime> pERIODO, Nullable<System.DateTime> pERIODOCOMPARAR)
    {
        return sp.FGA_Consultar_Indicadores_Formulas(iDENTIDAD, pERIODO, pERIODOCOMPARAR).ToList();
    }

    public List<FGA_Generar_Proy_Matrices_Result> FGA_Generar_Proy_Matrices(int pERIODOSESTIMAR, int pERIODOSCRECIMIENTO, DateTime pERIODOINICIAL, DateTime pPERIODOFINAL, string iDENTIDAD, decimal Tasa)
    {
        return sp.FGA_Generar_Proy_Matrices(pERIODOSESTIMAR, pERIODOSCRECIMIENTO, pERIODOINICIAL, pPERIODOFINAL, iDENTIDAD, Tasa).ToList();
    }

    public List<FGA_CrecimientoSugerido_Result> FGA_CrecimientoSugerido(DateTime pERIODOCORTE, string iDENTIDAD)
    {
        return sp.FGA_CrecimientoSugerido(iDENTIDAD, pERIODOCORTE).ToList();
    }

    public List<FGA_Rpt_EEFF_Proy_Result> FGA_Rpt_EEFF_Proy(DateTime? pERIODOCORTE, string iDENTIDAD, string TipoReporte, int? TipoPlazo)
    {
        return sp.FGA_Rpt_EEFF_Proy(iDENTIDAD, pERIODOCORTE, TipoReporte, TipoPlazo).ToList();
    }

    public List<FGA_Consultar_Evaluacion_Result> FGA_ConsultarEvaluacion(string iDENTIDAD, int iDSUBCATEGORIA)
    {
        return sp.FGA_Consultar_Evaluacion(iDENTIDAD, iDSUBCATEGORIA).ToList();
    }

    public List<FGA_Consultar_Avance_Result> FGA_ConsultarAvance()
    {
        return sp.FGA_Consultar_Avance().ToList();
    }

    public List<FGA_Consultar_Avance_X_Categoria_Result> FGA_ConsultarAvance_X_Categoria(string iDENTIDAD, int? iDCATEGORIA)
    {
        return sp.FGA_Consultar_Avance_X_Categoria(iDENTIDAD, iDCATEGORIA).ToList();
    }

    public List<FGA_Consultar_Historial_X_Categoria_Result> FGA_ConsultarHistorial_X_Categoria(string iDENTIDAD, int? iDCATEGORIA)
    {
        return sp.FGA_Consultar_Historial_X_Categoria(iDENTIDAD, iDCATEGORIA).ToList();
    }

    public List<FGA_Consultar_Simulacion_Capital_Result> FGA_ConsultarSimulacionCapital(string iDENTIDAD, DateTime? pPERIODO, int? tIPOESCENARIO, decimal? pORC_CARTERA,
        decimal? pORC_INVER, decimal? pORC_ACTIVOS, decimal? pORC_DISPONIBILIDADES, decimal? pORC_INMUEBLES, decimal? pORC_POSICION, decimal? pORC_INCREMENTO,
        decimal? pORC_UOBA, decimal? pORC_CAPITAL, decimal? pORC_RETIRO, decimal? pIMCA, decimal? pORC_SUFICIENCIAOBJETIVO,
        decimal? pPorc_Legal, decimal? pPorc_Excedente, decimal? pPorc_Voluntarias, decimal? pPorc_Aportes, decimal? pPorc_Donaciones, decimal? pPorc_Revaluacion,
        ref decimal? vIMCA, ref decimal? vCAPITAL_BASE, ref decimal? vIMCA_CAPITAL, ref decimal? vIND_SUFICIENCIA,
        ref decimal? vCN1, ref decimal? vCCN1)
   {
        // Declarar variables para los parámetros de salida
        ObjectParameter iMCA = new ObjectParameter("IMCA", typeof(decimal)); 
        ObjectParameter cAPITAL_BASE = new ObjectParameter("CAPITAL_BASE", typeof(decimal));
        ObjectParameter iMCA_CAPITAL = new ObjectParameter("IMCA_CAPITAL", typeof(decimal));
        ObjectParameter iND_SUFICIENCIA = new ObjectParameter("IND_SUFICIENCIA", typeof(decimal));
        ObjectParameter cN1 = new ObjectParameter("CN1", typeof(decimal));
        ObjectParameter cCN1 = new ObjectParameter("CCN1", typeof(decimal));

        // Llamar al método
        var result = sp.FGA_Consultar_Simulacion_Capital(
            iDENTIDAD, pPERIODO, tIPOESCENARIO, pORC_CARTERA, pORC_INVER, pORC_ACTIVOS, pORC_DISPONIBILIDADES,
            pORC_INMUEBLES, pORC_POSICION, pORC_INCREMENTO, pORC_UOBA, pORC_CAPITAL, pORC_RETIRO, pORC_SUFICIENCIAOBJETIVO, pIMCA,
            pPorc_Legal, pPorc_Excedente, pPorc_Voluntarias, pPorc_Aportes, pPorc_Donaciones, pPorc_Revaluacion,
            iMCA, cAPITAL_BASE, iMCA_CAPITAL, iND_SUFICIENCIA, cN1, cCN1);

        return result.ToList();
    }

    public List<FGA_Consultar_Resultado_Evaluacion_Result> FGA_Consultar_Resultado_Evaluacion(string idEntidad, int idCategoria) {
        return sp.FGA_Consultar_Resultado_Evaluacion(idEntidad, idCategoria).ToList();
    }

    public List<FGA_Consultar_FacturacionFGD_Result> FGA_Consultar_FacturacionFGD(string idEntidad, int anno, int trimestre)
    {
        return sp.FGA_Consultar_FacturacionFGD(idEntidad, anno, trimestre).ToList();
    }

    public List<FGA_Consultar_Grafico_Oper_Categoria_Result> FGA_Consultar_Grafico_Oper_Categoria(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF)
    {
        return sp.FGA_Consultar_Grafico_Oper_Categoria(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Grafico_Oper_Etapa_Result> FGA_Consultar_Grafico_Oper_Etapa(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF)
    {
        return sp.FGA_Consultar_Grafico_Oper_Etapa(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Grafico_Oper_Segmento_Result> FGA_Consultar_Grafico_Oper_Segmento(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF)
    {
        return sp.FGA_Consultar_Grafico_Oper_Segmento(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }

    public List<FGA_Consultar_Grafico_Oper_EAD_Result> FGA_Consultar_Grafico_Oper_EAD(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF)
    {
        return sp.FGA_Consultar_Grafico_Oper_EAD(iDENTIDAD, pERIODOI, pERIODOF).ToList();
    }
}