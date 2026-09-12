using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

namespace FGA.Utility
{
    public static class Utilitarios
    {
        public const int idBrecha = 16;
        public const int idBrechaM = 18;
        public const int idIRLBanda = 19;
        public const int idIRLBandaAcum = 20;
        public const int lineasCommit = 30;
        public const int maxUsuarios = 5;
        public const int monedaLocal = 1;
        public const int monedaDolares = 2;
        public const int perfilRiesgos = 1;
        public const int perfilPrudencial = 2;
        public const string estadoActivo = "A";
        public const string estadoInactivo = "I";
        public const string estadoBorrado = "B";
        public const string clientePendiente = "P";
        public const string Si = "S";
        public const string No = "N";
        public const string roleDirectorFGA = "6";
        public const string roleAdminEntidad = "5";
        public const string roleMaestroEntidad = "10";
        public const string Direccion_Correo = "Direccion_Correo";
        public const string Contrasena_Correo = "Contrasena_Correo";
        public const string Servidor_Correo = "Servidor_Correo";
        public const string Puerto_Correo = "Puerto_Correo";
        public const string Dominio = "Dominio";
        public const string Clave_Dominio = "Clave_Dominio";
        public const string Servidor_Reportes = "Servidor_Reportes";
        public const string Path_Reportes = "Path_Reportes";
        public const string Correo_Error_XML = "Correo_Error_XML";
        public const string Usuario_Reportes = "Usuario_Reportes";
        public const string Notificacion_Creacion_Usuarios = "Notificacion_Creacion_Usuarios";
        public const string Notificacion_Carga_Informes = "Notificacion_Carga_Informes";
        public const string Notificar_Entidades_Carga = "Notificar_Entidades_Carga";

        public const int archivoCargado = 1;
        public const int archivoValidado = 2;
        public const int archivoAceptado = 3;
        public const int archivoErrores = 4;
        public const int archivoEliminado = 12;
        public const int archivoPendiente = 13;
        public const int activos = 10000000;
        public const int pasivos = 20000000;
        public const int capitalPatrimonio = 30000000;
        public const int gastos = 40000000;
        public const int ingresos = 50000000;
        public const int opcionEvaluacion = 6000;

        public const string cuentaPasivos210 = "21000000";
        public const string entidadAdministradora = "0";
        public const int roleGerencial = 7;
        public const string nombreEntidadAdmin = "FFC";
        public const string entidadDefault = "2";
        public const string xml_contable_estado = "201";
        public const string xml_contable_datos_adicionales = "202";
        public const string xml_flujo_efectivo = "203";
        public const string xml_calce_plazo = "204";
        public const string xml_contable_brecha = "205";
        public const string xml_credito_deudor = "302";
        public const string xml_credito_oper_dirind = "303";
        public const string xml_credito_cuota_atrasada = "304";
        public const string xml_credito_garantia_operaciones = "305";
        public const string xml_inversiones_activas = "307";
        public const string xml_credito_bienes_realizables = "308";
        public const string xml_credito_cuentas_cobrar = "310";
        public const string xml_credito_informacion_oper_no_reportadas = "306";
        public const string xml_pasivos_cuentas_contables_210 = "2702";
        public const string xml_icl = "3301";
        public const string xml_suficiencia_patrimonial = "3401";
        public const string xml_indicadores_financieros = "3403";
        public const string xml_indicador_financiero = "3404";
        public const string xml_crediticio_deudores_1421 = "5101";
        public const string xml_crediticio_ingresoDeudores_1421 = "5102";
        public const string xml_crediticio_operaciones_1421 = "5103";
        public const string xml_crediticio_origenRecursos_1421 = "5104";
        public const string xml_crediticio_actividadEconomica_1421 = "5105";
        public const string xml_crediticio_naturalezaGasto_1421 = "5106";
        public const string xml_crediticio_cuentasCobrar_1421 = "5107";
        public const string xml_crediticio_creditosSindicados_1421 = "5108";
        public const string xml_crediticio_codeudores_1421 = "5109";
        public const string xml_crediticio_modificacion_1421 = "5110";
        public const string xml_crediticio_compradasRefinanciadas_1421 = "5111";
        public const string xml_crediticio_cambioClimatico_1421 = "5112";
        public const string xml_crediticio_garantiasOperacion_1421 = "5113";
        public const string xml_crediticio_operBienesRealizables_1421 = "5114";
        public const string xml_crediticio_fideicomiso_1421 = "5115";
        public const string xml_crediticio_bienesRealizablesNoReportadas_1421 = "5116";
        public const string xml_crediticio_bienesRealizables_1421 = "5117";
        public const string xml_crediticio_gravamenes_1421 = "5118";
        public const string xml_crediticio_cuentasPorCobrarNoAsociadas_1421 = "5119";
        public const string xml_crediticio_cuotasAtrasadas_1421 = "5120";
        public const string xml_crediticio_operacionesNoReportadas_1421 = "5121";
        public const string xml_crediticio_garantiasMobiliarias_1421 = "5122";
        public const string xml_crediticio_garantiasPolizas_1421 = "5201";
        public const string xml_crediticio_garantiasCartasCredito_1421 = "5202";
        public const string xml_crediticio_garantiasFacturasCedidas_1421 = "5203";
        public const string xml_crediticio_garantiasFiduciarias_1421 = "5204";
        public const string xml_crediticio_garantiasReales_1421 = "5205";
        public const string xml_crediticio_garantiasValores_1421 = "5206";

        public const string xml_capital_social = "000";
        public const string sinCierre = "01012016";
        public const string A1 = "A1";
        public const string A2 = "A2";
        public const string B1 = "B1";
        public const string B2 = "B2";
        public const string C1 = "C1";
        public const string C2 = "C2";
        public const string D = "D";
        public const string E = "E";
        public const string isModified = "isModified";
        public const string show_YAxis = "ckc_configYAxis";
        public const string show_ColumnDetail = "ckc_configColumnDetail";
        public const string show_LineDetail = "ckc_configLineDetail";
        public const string show_DateEachN = "ckc_configDateEach";
        public const string show_MenuOculto = "ckc_configMenu";
        public const int SF = 8;

        public enum enum_indicadoresProyeccion
        {      
            activo_pasivo = 1,
            rentabilidad_patrimonio = 2,
            captaciones_pasivo = 3,
            margenFinanciero = 4,
            margen_activo = 5,
            gastos_utilidad = 6,
            cartera = 7,
            totalActivo = 8,
            totalPasivo = 9,
            obligacionesPublico = 10,
            capitalSocial = 11,
            totalIngresos = 12,
            totalGastos = 13,
            resultadoPeriodo = 14,
            gastos_utilidadBruta = 15,
            utilidad_patrimonio = 16,
        }

        public enum enum_proyeccion
        {
            AR = 1,
            Mensual = 2,
            Trimestral = 3,
            Semestral = 4,
            Anual = 5
        }

        public enum enum_tipoAlbum
        {
            galeria = 1,
            blog = 2,
            noticia = 3,
            video = 4,
            documento = 5
        }

        public enum enum_tipoReporte
        {
            rpt_balance = 1,
            rpt_er = 2,
            vertical = 3,
            horizontal = 4,
            balanceCompleto = 5,
            camel = 6,
            otros = 7,
            analisis_balance = 8,
            analisis_er = 9,
            rpt_er_actual = 10,
            origen_aplicacion = 11,
            modelo_tasas = 12,
            origen_aplicacion_sf = 13,
            rpt_er_an_sf = 14,
            rpt_er_sf = 15,
            rpt_balance_an_sf = 16,
            rpt_balance_sf = 17,
            rpt_balance_proy = 18,
            rpt_er_proy = 19,
            rpt_an_er_proy = 20,
            rpt_an_balance_proy = 21,
            rpt_factura = 22,
            rpt_factura_ffc = 23
        }

        public enum enum_Grafico14_21
        {
            TipoDeSegmento = 1,
            TipoDeCategoriaRiesgo = 2,
            SaldoPorEtapaDelCredito = 3,
            CantidadDeOperacionesPorEtapa = 4,
            SaldoPerdidaEsperadaVrsEAD = 5,
            VariacionMensualPerdidaEsperadaVrsSaldoEstimaciones = 6,
            PerdidaEsperadaPorTipoDeSegmento = 7,
            PerdidaEsperadaPorCategoriaDeRiesgo = 8,
            MontoDesembolsadoMensual = 9,
            TasaPonderadaPorSegmento = 10
        }

        public enum enum_tipoGrafico
        {
            carteraTotal = 1,
            variacionCartera = 2,
            calificacionCartera = 3,
            creditoCalificacionE = 4,
            cuentasLiquidadas = 5,
            recuperacionActivos = 6,
            tipoCartera = 7,
            tipoCarteraCalificacionE = 8,
            variacionCapitalSocial = 9,
            comparacionRecuperacionLiquidadas = 10,
            variacionCarteraMensual = 11,
            composicionSFNTipoCartera = 12,
            concentracionRangoMora = 13,
            composicionRangoMora = 14,
            tasaPonderada = 15,
            concentracionMora = 16,
            concentracionMoraAgrupado = 17
        }

        public enum enum_secciones
        {
            quienes_somos = 1,
            afiliados = 2,
            videos = 3,
            contactenos = 4,
            galeria = 5,
            blog = 6,
            ubicacion = 7,
            noticias = 8,
            conozca = 9,
            banner = 10,
            parametros = 11,
            piePagina = 12,
            unase = 13,
            popUp
        }

        public static string ObtenerCategoriaRiesgo(string idCategoria)
        {
            string resultado = "A1";
            switch (idCategoria)
            {
                case "1":
                    resultado = "A1";
                    break;
                case "2":
                    resultado = "A2";
                    break;
                case "3":
                    resultado = "B1";
                    break;
                case "4":
                    resultado = "B2";
                    break;
                case "5":
                    resultado = "C1";
                    break;
                case "6":
                    resultado = "C2";
                    break;
                case "7":
                    resultado = "D";
                    break;
                case "8":
                    resultado = "E";
                    break;
                default:
                    break;
            }

            return resultado;
        }

        public static string ObtenerIdArchivo(string idArchivo)
        {

            if (idArchivo == "5101")  //deudores
            {
                idArchivo = "302";
            }
            else if (idArchivo == "5103") //operaciones
            {
                idArchivo = "303";
            }
            else if (idArchivo == "5120") //cuotasAtrasadas
            {
                idArchivo = "304";
            }

            return idArchivo;
        }

        public static decimal GetSubCategoriaPorcentaje(int subCategoria, List<Entities.Entities.Procedures.FGA_Consultar_Evaluacion_Result> eval)
        {
            decimal porcentaje = 0;

            try
            {
                var detalle = eval.Where(o => o.IdSubCategoria == subCategoria).FirstOrDefault();
                if (detalle != null)
                {
                    porcentaje = detalle.Avance_SubCategoria.Value;

                    if (detalle.Confirmada.Contains(";" + detalle.IdSubCategoria.ToString() + ";") == false && porcentaje == 100)
                        porcentaje = 99;
                }
            }
            catch (Exception)
            {
            }

            return porcentaje;
        }

        public static string GetPorcentaje(int subCategoria, List<Entities.Entities.Procedures.FGA_Consultar_Evaluacion_Result> eval)
        {
            decimal porcentaje = 0;
            string avance = "0.0%";

            try
            {
                var detalle = eval.Where(o => o.IdSubCategoria == subCategoria).FirstOrDefault();
                if (detalle != null)
                {
                    avance = detalle.Avance_General.Value.ToString() + "%";
                    porcentaje = detalle.Avance_General.Value;

                    if (detalle.Confirmada.Contains(";" + detalle.IdSubCategoria.ToString() + ";") == false && porcentaje == 100)
                        avance = "99%. Pendiente de envío";
                }
            }
            catch (Exception)
            {
            }

            return avance;
        }

        public static string ConvertirAString(decimal numero)
        {
            string resultado;

            try
            {
                resultado = string.Format("{0:0,0.00}", numero);
            }
            catch (Exception)
            {
                try
                {
                    resultado = string.Format("{0:0.0,00}", numero);
                }
                catch (Exception)
                {
                    throw new Exception("El valor: " + numero + " no es un número.");
                }
            }

            if (resultado.StartsWith("0") && resultado.Length > 3)
                resultado = resultado.Substring(1);

            if (resultado.StartsWith("-0") && resultado.Length > 4)
                resultado = resultado.Replace("-0", "-");

            resultado = resultado.Replace(",", "_");
            resultado = resultado.Replace(".", ",");
            resultado = resultado.Replace("_", ".");
            return resultado;
        }

        public static string ConvertirAStringNDecimales(decimal numero)
        {
            string resultado;

            try
            {
                resultado = string.Format("{0:0,0.00000}", numero);
            }
            catch (Exception)
            {
                try
                {
                    resultado = string.Format("{0:0.0,00000}", numero);
                }
                catch (Exception)
                {
                    throw new Exception("El valor: " + numero + " no es un número.");
                }
            }

            resultado = resultado.Replace(",", "_");
            resultado = resultado.Replace(".", ",");
            resultado = resultado.Replace("_", ".");
            return resultado;
        }

        public static decimal ConvertirADecimal(string numero, string detalle = "")
        {
            numero = numero.Trim();
            numero = numero.Replace(" ", "");

            try
            {
                if (!numero.StartsWith("-"))
                    numero = "0" + numero;

                numero.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                return decimal.Parse(numero, NumberStyles.Any, CultureInfo.InvariantCulture);

            }
            catch (Exception)
            {
                throw new Exception(detalle + " " + numero + ". No puede ser convertido a número.");
            }
        }

        public static DateTime ConvertirAFecha(string fecha)
        {
            try
            {
                fecha = fecha.Replace("\n", String.Empty);
                fecha = fecha.Replace("\r", String.Empty);
                fecha = fecha.Replace("\t", String.Empty);
                fecha = fecha.Trim();
            }
            catch (Exception)
            {
            }

            DateTime resultado;
            if (fecha.Length < 8)
                fecha = "01/" + fecha;

            if (fecha.Contains(" "))
                fecha = fecha.Substring(0, fecha.IndexOf(" "));

            try
            {
                fecha = fecha.Replace("/", "").Replace("-", "");
                resultado = DateTime.ParseExact(fecha, "ddMMyyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new Exception("El valor: " + fecha + " no es una fecha. Debe estar en formato Dia/Mes/Año. Ejemplo: 30/12/2019");
            }

            return resultado;
        }

        public static DateTime ConvertirAFechaNula(string fecha)
        {
            try
            {
                fecha = fecha.Replace("\n", String.Empty);
                fecha = fecha.Replace("\r", String.Empty);
                fecha = fecha.Replace("\t", String.Empty);
                fecha = fecha.Trim();
            }
            catch (Exception)
            {
            }

            DateTime resultado;

            if (string.IsNullOrEmpty(fecha))
            {
                return DateTime.MinValue;
            }

            if (fecha.Length < 8)
                fecha = "01/" + fecha;

            try
            {
                fecha = fecha.Replace("/", "").Replace("-", "");
                resultado = DateTime.ParseExact(fecha, "ddMMyyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new Exception("El valor: " + fecha + " no es una fecha. Debe estar en formato Dia/Mes/Año. Ejemplo: 30/12/2019");
            }

            return resultado;
        }
        public static int ConvertirAEntero(string numero)
        {
            int resultado;
            try
            {
                if (numero.StartsWith(".") || string.IsNullOrEmpty(numero))
                    numero = "0" + numero;

                resultado = int.Parse(numero);
            }
            catch (Exception)
            {
                throw new Exception("El valor: " + numero + " no es un número.");
            }

            return resultado;
        }

        public static string toUpperFirstLetter(string valor)
        {
            return valor.Substring(0, 1).ToUpper() + valor.Substring(1).ToLower();
        }

        public static string GetDirectoryName(FGA.Models.Album album)
        {
            String carpeta = album.Id.ToString();

            if (carpeta.Length > 20)
                carpeta = carpeta.Substring(0, 20);

            return carpeta;
        }
        public static string GetEstadoArchivo(int estado)
        {
            string nombreEstado;
            switch (estado)
            {
                case archivoCargado:
                    nombreEstado = "Cargado";
                    break;
                case archivoValidado:
                    nombreEstado = "Validado";
                    break;
                default:
                    nombreEstado = "Aceptado";
                    break;
            }
            return nombreEstado;
        }

        public static string GetMoneda(int moneda)
        {
            string nombreEstado;
            switch (moneda)
            {
                case 1:
                    nombreEstado = "Colones";
                    break;
                case 2:
                    nombreEstado = "Dólares";
                    break;
                case 3:
                    nombreEstado = "Euros";
                    break;
                default:
                    nombreEstado = "Inválida";
                    break;
            }

            return nombreEstado;
        }

        public static string GetMonth(int month)
        {
            try
            {
                DateTimeFormatInfo dtinfo = new CultureInfo("es-ES", false).DateTimeFormat;
                return toUpperFirstLetter(dtinfo.GetMonthName(month));
            }
            catch
            {

            }
            return "Mes inválido";

        }

        public static string GetEstadoCliente(string estado)
        {
            string nombreEstado;

            switch (estado)
            {
                case estadoActivo:
                    nombreEstado = "Activo";
                    break;
                case estadoInactivo:
                    nombreEstado = "Inactivo";
                    break;
                default:
                    nombreEstado = "Pendiente confirmación";
                    break;
            }
            return nombreEstado;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static string GetFileName(string code)
        {
            string fileName;

            switch (code)
            {
                case xml_indicadores_financieros:
                    fileName = "xml_indicadores_financieros";
                    break;
                case xml_indicador_financiero:
                    fileName = "xml_indicadores_financieros";
                    break;
                case xml_suficiencia_patrimonial:
                    fileName = "xml_suficiencia_patrimonial";
                    break;
                case xml_contable_estado:
                    fileName = "xml_contable_estado";
                    break;
                case xml_credito_bienes_realizables:
                    fileName = "xml_credito_bienes_realizables";
                    break;
                case xml_credito_cuentas_cobrar:
                    fileName = "xml_credito_cuentas_cobrar";
                    break;
                case xml_credito_cuota_atrasada:
                    fileName = "xml_credito_cuota_atrasada";
                    break;
                case xml_credito_deudor:
                    fileName = "xml_credito_deudor";
                    break;
                case xml_credito_garantia_operaciones:
                    fileName = "xml_credito_garantia_operaciones";
                    break;
                case xml_credito_informacion_oper_no_reportadas:
                    fileName = "xml_credito_informacion_oper_no_reportadas";
                    break;
                case xml_credito_oper_dirind:
                    fileName = "xml_credito_oper_dirinds";
                    break;
                case xml_icl:
                    fileName = "xml_icl";
                    break;
                case xml_inversiones_activas:
                    fileName = "xml_inversiones_activas";
                    break;
                case xml_pasivos_cuentas_contables_210:
                    fileName = "xml_pasivos_cuentas_contables_210";
                    break;
                case xml_calce_plazo:
                    fileName = "xml_calce_plazo";
                    break;
                case xml_contable_brecha:
                    fileName = "xml_contable_brecha";
                    break;
                case xml_contable_datos_adicionales:
                    fileName = "xml_contable_datos_adicionales";
                    break;
                case xml_flujo_efectivo:
                    fileName = "xml_flujo_efectivo";
                    break;
                case xml_crediticio_deudores_1421:
                    fileName = "xml_credito_deudor";
                    break;
                case xml_crediticio_operaciones_1421:
                    fileName = "xml_credito_oper_dirinds";
                    break;
                case xml_crediticio_garantiasOperacion_1421:
                    fileName = "xml_credito_garantiasOperacion";
                    break;
                case xml_crediticio_bienesRealizablesNoReportadas_1421:
                    fileName = "xml_credito_bienesRealizablesNoReportadas";
                    break;
                case xml_crediticio_bienesRealizables_1421:
                    fileName = "xml_credito_bienesRealizables";
                    break;
                case xml_crediticio_cuentasPorCobrarNoAsociadas_1421:
                    fileName = "xml_credito_cuentasPorCobrarNoAsociada";
                    break;
                case xml_crediticio_cuotasAtrasadas_1421:
                    fileName = "xml_credito_cuota_atrasada";
                    break;
                case xml_crediticio_operacionesNoReportadas_1421:
                    fileName = "xml_credito_operacionesNoReportadas";
                    break;
                case xml_crediticio_garantiasPolizas_1421:
                    fileName = "xml_credito_garantiasPolizas";
                    break;
                case xml_crediticio_garantiasCartasCredito_1421:
                    fileName = "xml_credito_garantiasCartasCredito";
                    break;
                case xml_crediticio_garantiasFacturasCedidas_1421:
                    fileName = "xml_credito_garantiasFacturasCedidas";
                    break;
                case xml_crediticio_garantiasFiduciarias_1421:
                    fileName = "xml_credito_garantiasFiduciarias";
                    break;
                case xml_crediticio_garantiasReales_1421:
                    fileName = "xml_credito_garantiasReales";
                    break;
                case xml_crediticio_garantiasValores_1421:
                    fileName = "xml_credito_garantiasValores";
                    break;
                case xml_crediticio_ingresoDeudores_1421:
                    fileName = "xml_credito_ingresoDeudores";
                    break;
                case xml_crediticio_origenRecursos_1421:
                    fileName = "xml_credito_origenRecursos";
                    break;
                case xml_crediticio_actividadEconomica_1421:
                    fileName = "xml_credito_actividadEconomica";
                    break;
                case xml_crediticio_naturalezaGasto_1421:
                    fileName = "xml_credito_naturalezaGasto";
                    break;
                case xml_crediticio_cuentasCobrar_1421:
                    fileName = "xml_credito_cuentasCobrar";
                    break;
                case xml_crediticio_creditosSindicados_1421:
                    fileName = "xml_credito_creditosSindicados";
                    break;
                case xml_crediticio_codeudores_1421:
                    fileName = "xml_credito_codeudores";
                    break;
                case xml_crediticio_modificacion_1421:
                    fileName = "xml_credito_modificacion";
                    break;
                case xml_crediticio_compradasRefinanciadas_1421:
                    fileName = "xml_credito_compradasRefinanciadas";
                    break;
                case xml_crediticio_cambioClimatico_1421:
                    fileName = "xml_credito_cambioClimatico";
                    break;
                case xml_crediticio_operBienesRealizables_1421:
                    fileName = "xml_credito_operBienesRealizables";
                    break;
                case xml_crediticio_fideicomiso_1421:
                    fileName = "xml_credito_fideicomiso";
                    break;
                case xml_crediticio_gravamenes_1421:
                    fileName = "xml_credito_gravamenes";
                    break;
                case xml_crediticio_garantiasMobiliarias_1421:
                    fileName = "xml_credito_garantiasMobiliarias";
                    break;
                default:
                    fileName = "";
                    break;
            }

            return fileName;
        }

        public static string GetToolBar(int responsable, int estado, int id, int idLogueado, string controller, int idLider, bool indCosmetico)
        {
            string toolbar = string.Empty;
            string tooltip = FGA.Models.Estado.GetTooltip(estado + 1);
            toolbar = "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Consultar\" href=\"" + controller + "/Details/" + id + "\"><i class=\"btn btn-info icon fa fa-search\"></i></a>";

            if (responsable == idLogueado && estado != (int)FGA.Enum.Enum_EstadoSolicitud.Finalizado)
            {
                if (estado == (int)Enum.Enum_EstadoSolicitud.Registrado || estado == (int)Enum.Enum_EstadoSolicitud.Pruebas)
                    toolbar += "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Rechazar\" href=\"javascript:confirmDelete('" + id + "')\"><i class=\"btn btn-danger icon fa fa-trash\"></i></a>";

                if (estado == (int)FGA.Enum.Enum_EstadoSolicitud.Rechazado)
                    toolbar += "<a data-toggle=\"tooltip\" data-id=\"" + id + "\" data-placement=\"top\" title=\"Aprobar\" class=\"btn_aceptar\"><i class=\"btn btn-success icon fa fa-check\"></i></a>";
                else
                {
                    toolbar += "<a data-toggle=\"tooltip\" data-id=\"" + id + "\" data-placement=\"top\" title=\"Enviar a " + tooltip + "\"class=\"btn_aceptar\"><i class=\"btn btn-success icon fa fa-check\"></i></a>";

                    if (estado != (int)Enum.Enum_EstadoSolicitud.Registrado)
                        toolbar += "<a data-toggle=\"tooltip\" data-id=\"" + id + "\" data-placement=\"top\" title=\"Tareas\" href=\"javascript:addTask('" + id + "', 'Tareas de " + FGA.Models.Estado.GetTooltip(estado) + "')\"><i class=\"btn btn-warning icon fa fa-edit\"></i></a>";
                }
            }
            else if (idLider == idLogueado && indCosmetico && estado == (int)Enum.Enum_EstadoSolicitud.Registrado)
                toolbar += "<a data-toggle=\"tooltip\" data-id=\"" + id + "\" data-placement=\"top\" title=\"Enviar a " + tooltip + "\"class=\"btn_aceptar\"><i class=\"btn btn-success icon fa fa-check\"></i></a>";

            return toolbar;
        }
    }
}