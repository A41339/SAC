using System;
using System.Collections.Generic;
using System.ServiceModel;
using Entities.Entities.Procedures;

[ServiceContract]
public interface ISP
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    void FGA_Borrar_Proyeccion(string IdEntidad, DateTime Periodo, string Cuenta);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Tipos_Indicadores_Result> FGA_Consultar_Tipos_Indicadores(string IdEntidad, string IdEntidadConsulta);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Grafico_Indicador_Result> FGA_Consultar_Grafico_Indicador(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF, string iNDICADOR);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Monitor_Result> FGA_Consultar_Monitor();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Sessiones_Result> FGA_Consultar_Sessiones(DateTime pERIODO);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    int FGA_Cierre_Mensual(string iDENTIDAD, Nullable<System.DateTime> pERIODO);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Balance_Comprobacion_Result> FGA_Consultar_Balance_Comprobacion(string iDENTIDAD, Nullable<System.DateTime> pERIODO);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Cartera_Credito_Result> FGA_Consultar_Cartera_Credito(string iDENTIDAD, Nullable<System.DateTime> pERIODO,
        int pageSize, int skip, string search, ref int count);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Maduracion_Cartera_Result> FGA_Consultar_Maduracion_Cartera(string iDENTIDAD, Nullable<System.DateTime> pERIODO,
        int pageSize, int skip, string search, ref int count);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Maduracion_Cartera_Det_Result> FGA_Consultar_Maduracion_Cartera_Det(string iDENTIDAD, Nullable<System.DateTime> pERIODO);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Riesgo_Liquidez_Result> FGA_Consultar_Riesgo_Liquidez(string iDENTIDAD, Nullable<System.DateTime> pERIODO);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Tasa_Ponderada_Result> FGA_Consultar_Tasa_Ponderada(string iDENTIDAD, Nullable<System.DateTime> pERIODOI, Nullable<System.DateTime> pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_CAMEL_Result> FGA_Consultar_CAMEL(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3, string iNDRESUMEN);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Suficiencia_Result> FGA_Consultar_Suficiencia(string iDENTIDAD, Nullable<System.DateTime> pERIODO, Nullable<System.DateTime> pERIODO_COMPARAR, string iNDRESUMEN);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Suficiencia_306_Result> FGA_Consultar_Suficiencia_306(string iDENTIDAD, Nullable<System.DateTime> pERIODO, Nullable<System.DateTime> pERIODO_COMPARAR, string iNDRESUMEN);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Indicadores_Result> FGA_Consultar_Indicadores(string iDENTIDAD, int iNDICADOR);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Otros_Indicadores_Result> FGA_Consultar_Otros_Indicadores(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Indicadores_Cartera_Result> FGA_Consultar_Indicadores_Cartera(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Calculos_Personalizados_Result> FGA_Consultar_Calculos_Personalizados(string iDENTIDAD, string idEntidadConsulta, DateTime? pERIODO, Nullable<System.DateTime> pERIODO_COMPARAR);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Analisis_Horizontal_Result> FGA_Analisis_Horizontal(string iDENTIDAD, int iNDICADOR);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Analisis_Vertical_Result> FGA_Analisis_Vertical(string iDENTIDAD, int iNDICADOR);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Dashboard_Result> FGA_Consultar_Dashboard(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    DateTime FGA_Consultar_FechaCierre(string iDENTIDAD);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    int FGA_Ejecutar_Cierre();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Calific_Cartera_Result> FGA_Consultar_Calif_Cartera(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL, String cALIFICACION);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_CalificE_Result> FGA_Consultar_CalificE(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<string> FGA_Consultar_ArchivosPendientes(string iDENTIDAD);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Tipo_Cartera_Result> FGA_Consultar_Tipo_Cartera(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL, int cTIPOCARTERA);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Tipo_Cartera_E_Result> FGA_Consultar_Tipo_Cartera_E(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_CarteraTotal_Result> FGA_Consultar_CarteraTotal(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL, int pMESES);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_CuentasLiq_Result> FGA_Consultar_CuentasLiq(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_RecuperacionActivos_Result> FGA_Consultar_RecuperacionActivos(string iDENTIDAD, DateTime pERIDOOINICIAL, DateTime pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Rpt_Balance_Result> FGA_Rpt_Balance(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Rpt_EstadoResultados_Result> FGA_Rpt_EstadoResultados(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_ArchivosCargados_Result> FGA_Consultar_ArchivosCargados(string iDENTIDAD, DateTime? pERIODO, bool pTipoEntidad);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Matrices_Desmejora_Cartera_Result> FGA_Consultar_Matrices_Desmejora_Cartera(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL, DateTime? pERIODOINICIAL_COMP, DateTime? pERIODOFINAL_COMP);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Matrices_PerdidaEstimada_Result> FGA_Consultar_Matrices_PerdidaEstimada(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Matrices_Variacion_Cartera_Result> FGA_Consultar_Matrices_Variacion_Cartera(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Modelo_Tasas_Result> FGA_Consultar_Modelo_Tasas(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL, int pOpcion);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Modelo_Margen_Result> FGA_Consultar_Modelo_Margen(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Variacion_CS_Result> FGA_Consultar_Variacion_CS(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Balance_Comprobacion_Rango_Result> FGA_Consultar_Balance_Comprobacion_Rango(string iDENTIDAD, DateTime? pERIODOINICIAL, DateTime? pERIODOFINAL, bool? pISMENSUAL, bool? pINDMODELOTASAS);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Graficos_Financieros_Result> FGA_Consultar_Graficos_Financieros(string iDENTIDAD, DateTime? pERIODO1, DateTime? pERIODO2, DateTime? pERIODO3);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Variacion_Interanual_SF_Result> FGA_Consultar_Variacion_Interanual_SF(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    bool FGA_Valida_Permite_Cargar(string iDENTIDAD);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Riesgo_Liquidez_Rango_Result> FGA_Consultar_Riesgo_Liquidez_Rango(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Credito_Categoria_Rango_Result> FGA_Consultar_Credito_Categoria_Rango(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Gastos_Ingresos_Result> FGA_Consultar_Gastos_Ingresos(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Grafico_Mora_Cartera_Result> FGA_Consultar_Grafico_Mora_Cartera(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Grafico_Suficiencia_Result> FGA_Consultar_Grafico_Suficiencia(string iDENTIDAD, DateTime? pERIODOI, DateTime? pERIODOF);

    /*IRL*/
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Generar_Proyeccion_Result> FGA_Generar_Proyeccion(string iDENTIDAD, DateTime pERIODOCORTE, int pERIODOS_HISTORICOS, int pERIODOS_PROYECTAR, string cUENTA);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Cuentas_Proyectar_Result> FGA_Consultar_Cuentas_Proyectar(string iDENTIDAD, bool iND_RESUMEN);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Datos_Historicos_Result> FGA_Consultar_Datos_Historicos(string iDENTIDAD, DateTime pERIODOCORTE, int pERIODOS, string cUENTA);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<string> FGA_Validar_Proyecciones_IRL(string iDENTIDAD, DateTime pERIODO);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Estado_Proyeccion_Result> FGA_Consultar_Estado_Proyeccion(string iDENTIDAD, DateTime pERIODO);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_IRL_Cuentas_Result> FGA_Consultar_IRL_Cuentas(string IdEntidad, DateTime Periodo,
        Decimal? Pond_Efectivo, Decimal? Pond_BCCR, Decimal? Pond_Ent_Finan, Decimal? Pond_Ent_Exterior, Decimal? Pond_Inv_BCCR,
        Decimal? Pond_Inv_SP_No_Financ, Decimal? Pond_Inv_Ots_Entidades);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Matrices_Probabilidad_Result> FGA_Consultar_Matrices_Probabilidada(string iDENTIDAD, DateTime pERIODOINICIAL, DateTime pERIODOFINAL);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Rpt_ER_SF_Result> FGA_Rpt_ER_SF(DateTime? pERIODOI, DateTime? pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Rpt_Balance_SF_Result> FGA_Rpt_Balance_SF(DateTime? pERIODOI, DateTime? pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Ingreso_SF_Result> FGA_Consultar_Ingreso_SF(DateTime? pERIODO1);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Gasto_SF_Result> FGA_Consultar_Gasto_SF(DateTime? pERIODO1);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Composicion_Balance_SF_Result> FGA_Consultar_Composicion_Balance_SF(DateTime? pERIODO1);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Indicadores_Sugef_Result> FGA_Consultar_Indicadores_Sugef(Nullable<System.DateTime> pERIODOI, Nullable<System.DateTime> pERIODOF, int? iNDICADOR);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Tipo_Cartera_Sector_Result> FGA_Consultar_Tipo_Cartera_Sector(Nullable<System.DateTime> pERIODO, int idSector);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Variacion_Cartera_Sector_Result> FGA_Consultar_Variacion_Cartera_Sector(DateTime periodoInicial, DateTime periodoFinal, int idSector, int numMeses);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Composicion_Cartera_Sector_Result> FGA_Consultar_Composicion_Cartera_Sector(DateTime periodo);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Composicion_Mora_Sector_Result> FGA_Consultar_Composicion_Mora_Sector(DateTime periodo);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Cartera_Rango_SF_Result> FGA_Consultar_Cartera_Rango_SF(Nullable<System.DateTime> pERIODOI, Nullable<System.DateTime> pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Indicadores_Formulas_Result> FGA_Consultar_Indicadores_Formulas(string iDENTIDAD, Nullable<System.DateTime> pERIODO, Nullable<System.DateTime> pERIODOCOMPARAR);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Generar_Proy_Matrices_Result> FGA_Generar_Proy_Matrices(int pERIODOSESTIMAR, int pERIODOSCRECIMIENTO, DateTime pERIODOINICIAL, DateTime pPERIODOFINAL, string iDENTIDAD, decimal Tasa);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_CrecimientoSugerido_Result> FGA_CrecimientoSugerido(DateTime pERIODOCORTE, string iDENTIDAD);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Rpt_EEFF_Proy_Result> FGA_Rpt_EEFF_Proy(DateTime? pERIODOCORTE, string iDENTIDAD, string TipoReporte, int? TipoPlazo);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Evaluacion_Result> FGA_ConsultarEvaluacion(string iDENTIDAD, int iDSUBCATEGORIA);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Avance_Result> FGA_ConsultarAvance();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Avance_X_Categoria_Result> FGA_ConsultarAvance_X_Categoria(string iDENTIDAD, int? iDCATEGORIA);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Historial_X_Categoria_Result> FGA_ConsultarHistorial_X_Categoria(string iDENTIDAD, int? iDCATEGORIA);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Simulacion_Capital_Result> FGA_ConsultarSimulacionCapital(string iDENTIDAD, DateTime? pPERIODO, int? tIPOESCENARIO, decimal? pORC_CARTERA,
        decimal? pORC_INVER, decimal? pORC_ACTIVOS, decimal? pORC_DISPONIBILIDADES, decimal? pORC_INMUEBLES, decimal? pORC_POSICION, decimal? pORC_INCREMENTO,
        decimal? pORC_UOBA, decimal? pORC_CAPITAL, decimal? pORC_RETIRO, decimal? pIMCA, decimal? pORC_SUFICIENCIAOBJETIVO,
        decimal? pPorc_Legal, decimal? pPorc_Excedente, decimal? pPorc_Voluntarias, decimal? pPorc_Aportes, decimal? pPorc_Donaciones, decimal? pPorc_Revaluacion,
        ref decimal? vIMCA, ref decimal? vCAPITAL_BASE, ref decimal? vIMCA_CAPITAL, ref decimal? vIND_SUFICIENCIA,
        ref decimal? vCN1, ref decimal? vCCN1);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Resultado_Evaluacion_Result> FGA_Consultar_Resultado_Evaluacion(string idEntidad, int idCategoria);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_FacturacionFGD_Result> FGA_Consultar_FacturacionFGD(string idEntidad, int anno, int trimestre);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Grafico_Oper_Categoria_Result> FGA_Consultar_Grafico_Oper_Categoria(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Grafico_Oper_Etapa_Result> FGA_Consultar_Grafico_Oper_Etapa(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Grafico_Oper_Segmento_Result> FGA_Consultar_Grafico_Oper_Segmento(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA_Consultar_Grafico_Oper_EAD_Result> FGA_Consultar_Grafico_Oper_EAD(string iDENTIDAD, DateTime pERIODOI, DateTime pERIODOF);

}