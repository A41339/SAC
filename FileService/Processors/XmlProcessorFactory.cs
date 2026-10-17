using FGA.Utility;
using System.Collections.Generic;

namespace FileService.Processors
{
    public class XmlProcessorFactory
    {
        private readonly Dictionary<string, IXmlProcessor> _processors = new Dictionary<string, IXmlProcessor>();

        public XmlProcessorFactory()
        {
            var pasivos = new Pasivos210Processor();
            Registrar(Utilitarios.xml_pasivos_cuentas_contables_210, pasivos);

            var credOper = new CreditoOperacionProcessor();
            Registrar(Utilitarios.xml_credito_oper_dirind, credOper);
            Registrar(Utilitarios.xml_crediticio_operaciones_1421, credOper);

            var credDeudor = new CreditoDeudorProcessor();
            Registrar(Utilitarios.xml_credito_deudor, credDeudor);
            Registrar(Utilitarios.xml_crediticio_deudores_1421, credDeudor);

            var cuotaAtrasada = new CreditoCuotaAtrasadaProcessor();
            Registrar(Utilitarios.xml_credito_cuota_atrasada, cuotaAtrasada);
            Registrar(Utilitarios.xml_crediticio_cuotasAtrasadas_1421, cuotaAtrasada);

            Registrar(Utilitarios.xml_suficiencia_patrimonial, new SuficienciaPatrimonialProcessor());
            Registrar(Utilitarios.xml_icl, new IclProcessor());
            Registrar(Utilitarios.xml_indicadores_financieros, new IndicadoresFinancierosProcessor());
            Registrar(Utilitarios.xml_calce_plazo, new CalcePlazoProcessor());
            Registrar(Utilitarios.xml_contable_brecha, new ContableBrechaProcessor());
            Registrar(Utilitarios.xml_contable_datos_adicionales, new ContableDatosProcessor());
            Registrar(Utilitarios.xml_flujo_efectivo, new FlujoEfectivoProcessor());
            Registrar(Utilitarios.xml_capital_social, new CapitalSocialProcessor());
            Registrar(Utilitarios.xml_inversiones_activas, new InversionActivaProcessor());

            // Archivos sin lógica de inserción (comportamiento original de case vacío)
            var noop = new NoOpXmlProcessor();
            Registrar(Utilitarios.xml_indicador_financiero, noop);
            Registrar(Utilitarios.xml_credito_garantia_operaciones, noop);
            Registrar(Utilitarios.xml_credito_cuentas_cobrar, noop);
            Registrar(Utilitarios.xml_credito_bienes_realizables, noop);
            Registrar(Utilitarios.xml_credito_informacion_oper_no_reportadas, noop);
            Registrar(Utilitarios.xml_crediticio_bienesRealizablesNoReportadas_1421, noop);
            Registrar(Utilitarios.xml_crediticio_bienesRealizables_1421, noop);
            Registrar(Utilitarios.xml_crediticio_cuentasPorCobrarNoAsociadas_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasCartasCredito_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasFacturasCedidas_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasFiduciarias_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasPolizas_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasOperacion_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasReales_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasValores_1421, noop);
            Registrar(Utilitarios.xml_crediticio_operacionesNoReportadas_1421, noop);
            Registrar(Utilitarios.xml_crediticio_ingresoDeudores_1421, noop);
            Registrar(Utilitarios.xml_crediticio_origenRecursos_1421, noop);
            Registrar(Utilitarios.xml_crediticio_actividadEconomica_1421, noop);
            Registrar(Utilitarios.xml_crediticio_naturalezaGasto_1421, noop);
            Registrar(Utilitarios.xml_crediticio_cuentasCobrar_1421, noop);
            Registrar(Utilitarios.xml_crediticio_creditosSindicados_1421, noop);
            Registrar(Utilitarios.xml_crediticio_codeudores_1421, noop);
            Registrar(Utilitarios.xml_crediticio_modificacion_1421, noop);
            Registrar(Utilitarios.xml_crediticio_compradasRefinanciadas_1421, noop);
            Registrar(Utilitarios.xml_crediticio_cambioClimatico_1421, noop);
            Registrar(Utilitarios.xml_crediticio_operBienesRealizables_1421, noop);
            Registrar(Utilitarios.xml_crediticio_fideicomiso_1421, noop);
            Registrar(Utilitarios.xml_crediticio_gravamenes_1421, noop);
            Registrar(Utilitarios.xml_crediticio_garantiasMobiliarias_1421, noop);
        }

        public void Registrar(string tipoArchivoId, IXmlProcessor processor)
        {
            if (!string.IsNullOrEmpty(tipoArchivoId) && processor != null)
            {
                _processors[tipoArchivoId] = processor;
            }
        }

        public IXmlProcessor ObtenerProcesador(string tipoArchivoId)
        {
            if (string.IsNullOrEmpty(tipoArchivoId)) return null;
            IXmlProcessor processor;
            _processors.TryGetValue(tipoArchivoId, out processor);
            return processor;
        }

        public bool ExisteProcesador(string tipoArchivoId)
        {
            if (string.IsNullOrEmpty(tipoArchivoId)) return false;
            return _processors.ContainsKey(tipoArchivoId);
        }
    }
}
