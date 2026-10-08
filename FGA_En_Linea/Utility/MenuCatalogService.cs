using System;
using System.Collections.Generic;

namespace FGA.Utility
{
    public class CardMeta
    {
        public string Descripcion { get; set; } = "";
        public string Icono { get; set; } = "fa fa-file-text-o";
        public string ColorFondoIcono { get; set; } = "#e6f1f8";
        public string ColorIcono { get; set; } = "#0071AD";
    }

    public class ModuloMeta
    {
        public string Subtitulo { get; set; } = "";
        public string NotaPie { get; set; } = "";
    }

    public class ColorPair
    {
        public string Fondo { get; set; }
        public string Icono { get; set; }
    }

    public static class MenuCatalogService
    {
        // Paleta dinámica de 12 colores institucionales FFC (fondo pastel suave + ícono saturado)
        public static readonly ColorPair[] Palette = new[]
        {
            new ColorPair { Fondo = "#e0f2fe", Icono = "#0071AD" }, // Azul Corporativo FFC
            new ColorPair { Fondo = "#fff1eb", Icono = "#FE7235" }, // Naranja Institucional FFC
            new ColorPair { Fondo = "#ebf6f9", Icono = "#6DB5CB" }, // Celeste Medio FFC
            new ColorPair { Fondo = "#e6ecf1", Icono = "#003F6B" }, // Azul Oscuro FFC
            new ColorPair { Fondo = "#eef3f9", Icono = "#2F5597" }, // Azul Marino Institucional FFC
            new ColorPair { Fondo = "#e0f2fe", Icono = "#0284c7" }, // Celeste Primario FFC
            new ColorPair { Fondo = "#e8eef8", Icono = "#1e40af" }, // Azul Royal FFC
            new ColorPair { Fondo = "#f0f7f9", Icono = "#0071AD" }, // Celeste Claro FFC
            new ColorPair { Fondo = "#eef3f9", Icono = "#2F5597" }, // Azul Profundo FFC
            new ColorPair { Fondo = "#fff1eb", Icono = "#FE7235" }, // Naranja FFC
            new ColorPair { Fondo = "#ebf6f9", Icono = "#6DB5CB" }, // Celeste FFC
            new ColorPair { Fondo = "#e0f2fe", Icono = "#0284c7" }  // Celeste FFC
        };

        public static ColorPair GetPaletteColor(string key, int index = 0)
        {
            if (string.IsNullOrEmpty(key))
                return Palette[Math.Abs(index) % Palette.Length];

            int hash = 0;
            foreach (char c in key)
            {
                hash = (hash * 31 + c);
            }
            int selected = Math.Abs(hash + index) % Palette.Length;
            return Palette[selected];
        }

        // Metadatos de módulos raíz (por texto o ID)
        private static readonly Dictionary<string, ModuloMeta> Modulos = new Dictionary<string, ModuloMeta>(StringComparer.OrdinalIgnoreCase)
        {
            ["Estructura Financiera"] = new ModuloMeta
            {
                Subtitulo = "Consulta y an\u00e1lisis de la informaci\u00f3n financiera de la entidad",
                NotaPie = "Nota: Toda la informaci\u00f3n se presenta en millones de colones y corresponde al per\u00edodo de referencia seleccionado."
            },
            ["Información Financiera"] = new ModuloMeta
            {
                Subtitulo = "Consulta y an\u00e1lisis de la informaci\u00f3n financiera de la entidad",
                NotaPie = "Nota: Toda la informaci\u00f3n se presenta en millones de colones y corresponde al per\u00edodo de referencia seleccionado."
            },
            ["Informacion Financiera"] = new ModuloMeta
            {
                Subtitulo = "Consulta y an\u00e1lisis de la informaci\u00f3n financiera de la entidad",
                NotaPie = "Nota: Toda la informaci\u00f3n se presenta en millones de colones y corresponde al per\u00edodo de referencia seleccionado."
            },
            ["Cartera"] = new ModuloMeta
            {
                Subtitulo = "An\u00e1lisis de colocaci\u00f3n, morosidad y concentraci\u00f3n de la cartera de cr\u00e9dito",
                NotaPie = "Nota: Informaci\u00f3n calculada con base en los saldos y categor\u00edas de riesgo registradas."
            },
            ["Cartera de Cr\u00e9dito"] = new ModuloMeta
            {
                Subtitulo = "An\u00e1lisis de colocaci\u00f3n, morosidad y concentraci\u00f3n de la cartera de cr\u00e9dito",
                NotaPie = "Nota: Informaci\u00f3n calculada con base en los saldos y categor\u00edas de riesgo registradas."
            },
            ["Cartera de Credito"] = new ModuloMeta
            {
                Subtitulo = "An\u00e1lisis de colocaci\u00f3n, morosidad y concentraci\u00f3n de la cartera de cr\u00e9dito",
                NotaPie = "Nota: Informaci\u00f3n calculada con base en los saldos y categor\u00edas de riesgo registradas."
            },
            ["Cr\u00e9ditos"] = new ModuloMeta
            {
                Subtitulo = "Plataforma de negociaci\u00f3n y gesti\u00f3n de cartera crediticia interinstitucional",
                NotaPie = "Nota: Las ofertas y solicitudes registradas son de car\u00e1cter confidencial."
            },
            ["Creditos"] = new ModuloMeta
            {
                Subtitulo = "Plataforma de negociaci\u00f3n y gesti\u00f3n de cartera crediticia interinstitucional",
                NotaPie = "Nota: Las ofertas y solicitudes registradas son de car\u00e1cter confidencial."
            },
            ["Riesgos"] = new ModuloMeta
            {
                Subtitulo = "Monitoreo de matrices de transici\u00f3n, liquidez y l\u00edmites prudenciales",
                NotaPie = "Nota: Indicadores evaluados seg\u00fan la normativa prudencial vigente."
            },
            ["Gobierno Corporativo"] = new ModuloMeta
            {
                Subtitulo = "Evaluaci\u00f3n y seguimiento de la Supervisi\u00f3n Basada en Riesgos (SBR)",
                NotaPie = "Nota: Los resultados de autoevaluaci\u00f3n se procesan seg\u00fan la metodolog\u00eda SUGEF."
            },
            ["Evaluacion"] = new ModuloMeta
            {
                Subtitulo = "Autoevaluaci\u00f3n de los aspectos del Reglamento SUGEF 24-22",
                NotaPie = "Nota: Los resultados y ponderaciones de la autoevaluaci\u00f3n se procesan conforme a las directrices del Reglamento SUGEF 24-22 para la Supervisi\u00f3n Basada en Riesgos."
            },
            ["Evaluación"] = new ModuloMeta
            {
                Subtitulo = "Autoevaluaci\u00f3n de los aspectos del Reglamento SUGEF 24-22",
                NotaPie = "Nota: Los resultados y ponderaciones de la autoevaluaci\u00f3n se procesan conforme a las directrices del Reglamento SUGEF 24-22 para la Supervisi\u00f3n Basada en Riesgos."
            },
            ["Evaluación SBR"] = new ModuloMeta
            {
                Subtitulo = "Autoevaluaci\u00f3n de los aspectos del Reglamento SUGEF 24-22",
                NotaPie = "Nota: Los resultados y ponderaciones de la autoevaluaci\u00f3n se procesan conforme a las directrices del Reglamento SUGEF 24-22 para la Supervisi\u00f3n Basada en Riesgos."
            },
            ["Evaluacion SBR"] = new ModuloMeta
            {
                Subtitulo = "Autoevaluaci\u00f3n de los aspectos del Reglamento SUGEF 24-22",
                NotaPie = "Nota: Los resultados y ponderaciones de la autoevaluaci\u00f3n se procesan conforme a las directrices del Reglamento SUGEF 24-22 para la Supervisi\u00f3n Basada en Riesgos."
            },
            ["SBR"] = new ModuloMeta
            {
                Subtitulo = "Autoevaluaci\u00f3n de los aspectos del Reglamento SUGEF 24-22",
                NotaPie = "Nota: Los resultados y ponderaciones de la autoevaluaci\u00f3n se procesan conforme a las directrices del Reglamento SUGEF 24-22 para la Supervisi\u00f3n Basada en Riesgos."
            },
            ["Mantenimientos"] = new ModuloMeta
            {
                Subtitulo = "Gestión de catálogos, fórmulas, entidades, parámetros y roles del sistema",
                NotaPie = "Nota: Los cambios y configuraciones quedan debidamente registrados en la bitácora del sistema."
            },
            ["Mantenimiento"] = new ModuloMeta
            {
                Subtitulo = "Gestión de catálogos, fórmulas, entidades, parámetros y roles del sistema",
                NotaPie = "Nota: Los cambios y configuraciones quedan debidamente registrados en la bitácora del sistema."
            },
            ["Perfiles"] = new ModuloMeta
            {
                Subtitulo = "Gestión de usuarios, perfiles y seguridad de acceso al sistema",
                NotaPie = "Nota: Los cambios de contraseñas y permisos de usuario quedan auditados en la bitácora de seguridad."
            },
            ["Control de Accesos"] = new ModuloMeta
            {
                Subtitulo = "Gestión de usuarios, perfiles y seguridad de acceso al sistema",
                NotaPie = "Nota: Los cambios de contraseñas y permisos de usuario quedan auditados en la bitácora de seguridad."
            },
            ["Control de Acceso"] = new ModuloMeta
            {
                Subtitulo = "Gestión de usuarios, perfiles y seguridad de acceso al sistema",
                NotaPie = "Nota: Los cambios de contraseñas y permisos de usuario quedan auditados en la bitácora de seguridad."
            },
            ["Administracion"] = new ModuloMeta
            {
                Subtitulo = "Gestión de usuarios, roles, parámetros y configuraciones del sistema",
                NotaPie = "Nota: Los cambios en configuraciones quedan auditados en la bitácora de seguridad."
            },
            ["Administración"] = new ModuloMeta
            {
                Subtitulo = "Gestión de usuarios, roles, parámetros y configuraciones del sistema",
                NotaPie = "Nota: Los cambios en configuraciones quedan auditados en la bitácora de seguridad."
            },
            ["Seguridad"] = new ModuloMeta
            {
                Subtitulo = "Monitoreo de sesiones activas y auditoría de accesos a la plataforma",
                NotaPie = "Nota: Los registros de sesión se actualizan automáticamente en tiempo real."
            }
        };

        // Metadatos de tarjetas hijas (por coincidencia de URL o texto)
        private static readonly Dictionary<string, CardMeta> Tarjetas = new Dictionary<string, CardMeta>(StringComparer.OrdinalIgnoreCase)
        {
            ["Autoevaluación"] = new CardMeta
            {
                Descripcion = "Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).",
                Icono = "fa fa-clipboard",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Autoevaluacion"] = new CardMeta
            {
                Descripcion = "Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).",
                Icono = "fa fa-clipboard",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Evaluacion/Evaluacion"] = new CardMeta
            {
                Descripcion = "Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).",
                Icono = "fa fa-clipboard",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Avance SBR"] = new CardMeta
            {
                Descripcion = "Monitoreo del porcentaje de avance y nivel de respuestas completadas.",
                Icono = "fa fa-tasks",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Evaluacion/Avance"] = new CardMeta
            {
                Descripcion = "Monitoreo del porcentaje de avance y nivel de respuestas completadas.",
                Icono = "fa fa-tasks",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Historial SBR"] = new CardMeta
            {
                Descripcion = "Registro histórico de evaluaciones y calificaciones obtenidas.",
                Icono = "fa fa-history",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Evaluacion/Historial"] = new CardMeta
            {
                Descripcion = "Registro histórico de evaluaciones y calificaciones obtenidas.",
                Icono = "fa fa-history",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Resultados SBR"] = new CardMeta
            {
                Descripcion = "Informe de resultados consolidados y niveles de cumplimiento.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Evaluacion/Resultados"] = new CardMeta
            {
                Descripcion = "Informe de resultados consolidados y niveles de cumplimiento.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Configuración de Preguntas SBR"] = new CardMeta
            {
                Descripcion = "Mantenimiento y configuración del banco de preguntas, pesos y categorías SBR.",
                Icono = "fa fa-list-alt",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Configuracion de Preguntas SBR"] = new CardMeta
            {
                Descripcion = "Mantenimiento y configuración del banco de preguntas, pesos y categorías SBR.",
                Icono = "fa fa-list-alt",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Evaluacion/Index"] = new CardMeta
            {
                Descripcion = "Mantenimiento y configuración del banco de preguntas, pesos y categorías SBR.",
                Icono = "fa fa-list-alt",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Evaluación SBR (Avance y Resultados)"] = new CardMeta
            {
                Descripcion = "Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.",
                Icono = "fa fa-tasks",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Role/Index"] = new CardMeta
            {
                Descripcion = "Gestión de roles y configuración de la matriz de permisos de menú",
                Icono = "fa fa-key",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Roles"] = new CardMeta
            {
                Descripcion = "Gestión de roles y configuración de la matriz de permisos de menú",
                Icono = "fa fa-key",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Menu/Index"] = new CardMeta
            {
                Descripcion = "Administración de opciones de menú, íconos y descripciones para las fichas del sistema",
                Icono = "fa fa-sitemap",
                ColorFondoIcono = "#f1f5f9",
                ColorIcono = "#475569"
            },
            ["Menú del Sistema"] = new CardMeta
            {
                Descripcion = "Administración de opciones de menú, íconos y descripciones para las fichas del sistema",
                Icono = "fa fa-sitemap",
                ColorFondoIcono = "#f1f5f9",
                ColorIcono = "#475569"
            },
            ["Menu del Sistema"] = new CardMeta
            {
                Descripcion = "Administración de opciones de menú, íconos y descripciones para las fichas del sistema",
                Icono = "fa fa-sitemap",
                ColorFondoIcono = "#f1f5f9",
                ColorIcono = "#475569"
            },
            ["Menu Sistema"] = new CardMeta
            {
                Descripcion = "Administración de opciones de menú, íconos y descripciones para las fichas del sistema",
                Icono = "fa fa-sitemap",
                ColorFondoIcono = "#f1f5f9",
                ColorIcono = "#475569"
            },
            // === ESTRUCTURA FINANCIERA (Maqueta original) ===
            ["Estados Financieros"] = new CardMeta
            {
                Descripcion = "Consulte la situaci\u00f3n financiera, los resultados y el movimiento de fondos de la entidad.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#e6f1f8", // Celeste suave
                ColorIcono = "#0071AD"
            },
            ["InformeFinanciero"] = new CardMeta
            {
                Descripcion = "Consulte la situaci\u00f3n financiera, los resultados y el movimiento de fondos de la entidad.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Estructura de Fondeo"] = new CardMeta
            {
                Descripcion = "Analice la composici\u00f3n de las fuentes de fondeo de la entidad.",
                Icono = "fa fa-database",
                ColorFondoIcono = "#fff1eb", // Naranja suave
                ColorIcono = "#FE7235"
            },
            ["Composicion"] = new CardMeta
            {
                Descripcion = "Analice la composici\u00f3n de las fuentes de fondeo de la entidad.",
                Icono = "fa fa-database",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Indicadores Financieros"] = new CardMeta
            {
                Descripcion = "Analice los principales indicadores financieros de la entidad.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#fff1eb", // Coral/rosa suave
                ColorIcono = "#FE7235"
            },
            ["Indicadores"] = new CardMeta
            {
                Descripcion = "Analice los principales indicadores financieros de la entidad.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["M\u00e1rgenes"] = new CardMeta
            {
                Descripcion = "Revise los m\u00e1rgenes financieros y operativos.",
                Icono = "fa fa-percent",
                ColorFondoIcono = "#fff7ec", // Ámbar suave
                ColorIcono = "#FFB347"
            },
            ["Margen"] = new CardMeta
            {
                Descripcion = "Revise los m\u00e1rgenes financieros y operativos.",
                Icono = "fa fa-percent",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Tablas Impl\u00edcitas"] = new CardMeta
            {
                Descripcion = "Consulte las tasas impl\u00edcitas por tipo de activo y pasivo.",
                Icono = "fa fa-bar-chart",
                ColorFondoIcono = "#ccfbf1", // Verde azulado suave
                ColorIcono = "#0d9488"
            },
            ["TasaInteres"] = new CardMeta
            {
                Descripcion = "Consulte las tasas impl\u00edcitas por tipo de activo y pasivo.",
                Icono = "fa fa-bar-chart",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["Proyecciones"] = new CardMeta
            {
                Descripcion = "Realice proyecciones financieras de la entidad.",
                Icono = "fa fa-area-chart",
                ColorFondoIcono = "#dbeafe", // Azul suave
                ColorIcono = "#2563eb"
            },
            ["ProyeccionEEFF"] = new CardMeta
            {
                Descripcion = "Realice proyecciones financieras de la entidad.",
                Icono = "fa fa-area-chart",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#2563eb"
            },
            ["Proyección de Estados Financieros"] = new CardMeta
            {
                Descripcion = "Módulo de proyección de balances, estados de resultados, mora e indicadores.",
                Icono = "fa fa-area-chart",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#2563eb"
            },
            ["Proyecciones IRL"] = new CardMeta
            {
                Descripcion = "Cálculo y evaluación del índice de riesgo y brechas de liquidez proyectadas.",
                Icono = "fa fa-tint",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["IRL"] = new CardMeta
            {
                Descripcion = "Cálculo y evaluación del índice de riesgo y brechas de liquidez proyectadas.",
                Icono = "fa fa-tint",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Simulación ISP"] = new CardMeta
            {
                Descripcion = "Simulación y proyección de adecuación patrimonial y suficiencia de capital.",
                Icono = "fa fa-calculator",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["SimulacionCapital"] = new CardMeta
            {
                Descripcion = "Simulación y proyección de adecuación patrimonial y suficiencia de capital.",
                Icono = "fa fa-calculator",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },

            // === CARTERA DE CRÉDITO ===
            ["Cartera de Cr\u00e9dito"] = new CardMeta
            {
                Descripcion = "Monitoreo de colocaci\u00f3n, morosidad, acuerdos regulatorios y transici\u00f3n de riesgo.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e0e7ff", // Índigo suave
                ColorIcono = "#4338ca"
            },
            ["Cartera de Credito"] = new CardMeta
            {
                Descripcion = "Monitoreo de colocaci\u00f3n, morosidad, acuerdos regulatorios y transici\u00f3n de riesgo.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4338ca"
            },
            ["Riesgo de Crédito"] = new CardMeta
            {
                Descripcion = "Composición de mora, morosidad agrupada y análisis de riesgo según normativa 14-21.",
                Icono = "fa fa-shield",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Riesgo de Credito"] = new CardMeta
            {
                Descripcion = "Composición de mora, morosidad agrupada y análisis de riesgo según normativa 14-21.",
                Icono = "fa fa-shield",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Desempeño"] = new CardMeta
            {
                Descripcion = "Evolución de la cartera total, variaciones mensuales/interanuales, activos y cuentas liquidadas.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Desempeno"] = new CardMeta
            {
                Descripcion = "Evolución de la cartera total, variaciones mensuales/interanuales, activos y cuentas liquidadas.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Cartera 14-21"] = new CardMeta
            {
                Descripcion = "Monitoreo de saldos de cartera seg\u00fan acuerdo SUGEF 14-21.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e6f6fc", // Verde suave
                ColorIcono = "#0099D6"
            },
            ["Cartera14_21"] = new CardMeta
            {
                Descripcion = "Monitoreo de saldos de cartera seg\u00fan acuerdo SUGEF 14-21.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Matrices de Mora"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis de transici\u00f3n de mora y deterioro de cr\u00e9ditos.",
                Icono = "fa fa-th",
                ColorFondoIcono = "#fff1eb", // Coral suave
                ColorIcono = "#FE7235"
            },
            ["Matriz"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis de transici\u00f3n de mora y deterioro de cr\u00e9ditos.",
                Icono = "fa fa-th",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["IRL"] = new CardMeta
            {
                Descripcion = "C\u00e1lculo e insumos del Indicador de Riesgo de Liquidez.",
                Icono = "fa fa-tint",
                ColorFondoIcono = "#e6f1f8", // Celeste suave
                ColorIcono = "#0071AD"
            },

            // === ADMINISTRACIÓN (Captura de pantalla del usuario) ===
            ["Perfiles"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de perfiles de usuario, roles y permisos de acceso a m\u00f3dulos.",
                Icono = "fa fa-id-badge",
                ColorFondoIcono = "#e0e7ff", // Índigo suave
                ColorIcono = "#4338ca"
            },
            ["Roles"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de roles, permisos y niveles de autorizaci\u00f3n en el sistema.",
                Icono = "fa fa-shield",
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4338ca"
            },
            ["Role"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de roles, permisos y niveles de autorizaci\u00f3n en el sistema.",
                Icono = "fa fa-shield",
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4338ca"
            },
            ["Cat\u00e1logo SUGEF"] = new CardMeta
            {
                Descripcion = "Mapeo y homologaci\u00f3n del cat\u00e1logo contable seg\u00fan la normativa SUGEF.",
                Icono = "fa fa-book",
                ColorFondoIcono = "#ccfbf1", // Teal suave
                ColorIcono = "#0d9488"
            },
            ["Catalogo SUGEF"] = new CardMeta
            {
                Descripcion = "Mapeo y homologaci\u00f3n del cat\u00e1logo contable seg\u00fan la normativa SUGEF.",
                Icono = "fa fa-book",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["CatalogoCuenta"] = new CardMeta
            {
                Descripcion = "Mapeo y homologaci\u00f3n del cat\u00e1logo contable seg\u00fan la normativa SUGEF.",
                Icono = "fa fa-book",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["Evaluaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de formularios, par\u00e1metros de supervisi\u00f3n y autoevaluaciones.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#fff7ec", // Ámbar suave
                ColorIcono = "#FFB347"
            },
            ["Evaluacion"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de formularios, par\u00e1metros de supervisi\u00f3n y autoevaluaciones.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Usuarios"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de cuentas de usuario, credenciales y estados de acceso.",
                Icono = "fa fa-users",
                ColorFondoIcono = "#dbeafe", // Azul suave
                ColorIcono = "#2563eb"
            },
            ["Usuario"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de cuentas de usuario, credenciales y estados de acceso.",
                Icono = "fa fa-users",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#2563eb"
            },
            ["Entidades"] = new CardMeta
            {
                Descripcion = "Cat\u00e1logo de entidades participantes y par\u00e1metros generales.",
                Icono = "fa fa-building-o",
                ColorFondoIcono = "#e6f6fc", // Verde suave
                ColorIcono = "#0099D6"
            },
            ["Entidad"] = new CardMeta
            {
                Descripcion = "Cat\u00e1logo de entidades participantes y par\u00e1metros generales.",
                Icono = "fa fa-building-o",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Par\u00e1metros"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de variables operativas y par\u00e1metros globales del sistema.",
                Icono = "fa fa-sliders",
                ColorFondoIcono = "#fff1eb", // Naranja suave
                ColorIcono = "#FE7235"
            },
            ["Parametros"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de variables operativas y par\u00e1metros globales del sistema.",
                Icono = "fa fa-sliders",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Cierres"] = new CardMeta
            {
                Descripcion = "Monitoreo y administraci\u00f3n de fechas de corte y cierres contables.",
                Icono = "fa fa-calendar-check-o",
                ColorFondoIcono = "#e6ecf1", // Púrpura suave
                ColorIcono = "#003F6B"
            },
            ["Cierre"] = new CardMeta
            {
                Descripcion = "Monitoreo y administraci\u00f3n de fechas de corte y cierres contables.",
                Icono = "fa fa-calendar-check-o",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Estatus"] = new CardMeta
            {
                Descripcion = "Monitoreo en tiempo real del estado de cierres contables y procesos de carga.",
                Icono = "fa fa-tachometer",
                ColorFondoIcono = "#e6ecf1", // Violeta suave
                ColorIcono = "#003F6B"
            },
            ["Cierre/Monitor"] = new CardMeta
            {
                Descripcion = "Monitoreo en tiempo real del estado de cierres contables y procesos de carga.",
                Icono = "fa fa-tachometer",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Archivos Cargados"] = new CardMeta
            {
                Descripcion = "Consulta, descarga y auditor\u00eda de archivos y reportes procesados.",
                Icono = "fa fa-folder-open-o",
                ColorFondoIcono = "#e6f6fc", // Verde suave
                ColorIcono = "#0099D6"
            },
            ["Consultar Archivos"] = new CardMeta
            {
                Descripcion = "Consulta, descarga y auditor\u00eda de archivos y reportes procesados.",
                Icono = "fa fa-folder-open-o",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["F\u00f3rmulas"] = new CardMeta
            {
                Descripcion = "Definici\u00f3n y f\u00f3rmulas de c\u00e1lculo de indicadores financieros y normativos.",
                Icono = "fa fa-calculator",
                ColorFondoIcono = "#ebf6f9", // Fucsia suave
                ColorIcono = "#6DB5CB"
            },
            ["Formula"] = new CardMeta
            {
                Descripcion = "Definici\u00f3n y f\u00f3rmulas de c\u00e1lculo de indicadores financieros y normativos.",
                Icono = "fa fa-calculator",
                ColorFondoIcono = "#ebf6f9",
                ColorIcono = "#6DB5CB"
            },
            ["Noticias"] = new CardMeta
            {
                Descripcion = "Publicaci\u00f3n de boletines y comunicados informativos para las entidades.",
                Icono = "fa fa-newspaper-o",
                ColorFondoIcono = "#e6f1f8", // Celeste suave
                ColorIcono = "#0071AD"
            },
            ["Noticia"] = new CardMeta
            {
                Descripcion = "Publicaci\u00f3n de boletines y comunicados informativos para las entidades.",
                Icono = "fa fa-newspaper-o",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Calendario"] = new CardMeta
            {
                Descripcion = "Cronograma de eventos, fechas de entrega y compromisos regulatorios.",
                Icono = "fa fa-calendar",
                ColorFondoIcono = "#fef9c3", // Amarillo suave
                ColorIcono = "#ca8a04"
            },
            ["Facturaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de facturaci\u00f3n y cuotas de mantenimiento de la plataforma.",
                Icono = "fa fa-credit-card",
                ColorFondoIcono = "#e6f6fc", // Verde suave
                ColorIcono = "#15803d"
            },
            ["Facturacion"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de facturaci\u00f3n y cuotas de mantenimiento de la plataforma.",
                Icono = "fa fa-credit-card",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#15803d"
            },
            ["Archivos"] = new CardMeta
            {
                Descripcion = "Carga masiva, validaci\u00f3n y procesamiento de archivos XML.",
                Icono = "fa fa-cloud-upload",
                ColorFondoIcono = "#dbeafe", // Azul suave
                ColorIcono = "#1d4ed8"
            },
            ["Archivo"] = new CardMeta
            {
                Descripcion = "Carga masiva, validaci\u00f3n y procesamiento de archivos XML.",
                Icono = "fa fa-cloud-upload",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#1d4ed8"
            },
            ["Auditor\u00eda de Sesiones"] = new CardMeta
            {
                Descripcion = "Historial de conexiones, sesiones activas y registro de auditor\u00eda.",
                Icono = "fa fa-history",
                ColorFondoIcono = "#fff1eb", // Coral suave
                ColorIcono = "#FE7235"
            },
            ["Seguridad"] = new CardMeta
            {
                Descripcion = "Historial de conexiones, sesiones activas y registro de auditor\u00eda.",
                Icono = "fa fa-lock",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },

            // === MERCADO DE CRÉDITOS ===
            ["Mercado de Cr\u00e9ditos"] = new CardMeta
            {
                Descripcion = "Consulte las ofertas y solicitudes de cr\u00e9ditos vigentes.",
                Icono = "fa fa-university",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Mis Solicitudes"] = new CardMeta
            {
                Descripcion = "Seguimiento al estado de sus solicitudes de financiamiento.",
                Icono = "fa fa-list-alt",
                ColorFondoIcono = "#fef9c3",
                ColorIcono = "#ca8a04"
            },
            ["Publicar Oferta"] = new CardMeta
            {
                Descripcion = "Publique nuevas ofertas de colocaci\u00f3n de cartera en el mercado.",
                Icono = "fa fa-bullhorn",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["Gesti\u00f3n Recibidas"] = new CardMeta
            {
                Descripcion = "Administre las ofertas y solicitudes recibidas de otras entidades.",
                Icono = "fa fa-tasks",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },

            // === GOBIERNO CORPORATIVO Y EVALUACIÓN ===
            ["Autoevaluaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Formularios de autoevaluaci\u00f3n de supervisi\u00f3n basada en riesgos.",
                Icono = "fa fa-pencil-square-o",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Autoevaluacion"] = new CardMeta
            {
                Descripcion = "Formularios de autoevaluaci\u00f3n de supervisi\u00f3n basada en riesgos.",
                Icono = "fa fa-pencil-square-o",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Autoevaluaci\u00f3n SBR"] = new CardMeta
            {
                Descripcion = "Formularios de autoevaluaci\u00f3n de supervisi\u00f3n basada en riesgos.",
                Icono = "fa fa-pencil-square-o",
                ColorFondoIcono = "#e6f1f8",
                ColorIcono = "#0071AD"
            },
            ["Avance"] = new CardMeta
            {
                Descripcion = "Monitoreo del porcentaje de avance y nivel de respuestas completadas.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Avance de Evaluaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Monitoreo del porcentaje de avance y nivel de respuestas completadas.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Historial"] = new CardMeta
            {
                Descripcion = "Consulta de autoevaluaciones concluidas y registros de per\u00edodos anteriores.",
                Icono = "fa fa-history",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Historial de Evaluaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Consulta de autoevaluaciones concluidas y registros de per\u00edodos anteriores.",
                Icono = "fa fa-history",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Resultados"] = new CardMeta
            {
                Descripcion = "Visualizaci\u00f3n de calificaciones globales y reportes consolidados.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Resultados de Evaluaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Visualizaci\u00f3n de calificaciones globales y reportes consolidados.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },

            // === PERFILES Y CONTROL DE ACCESOS ===
            ["Administraci\u00f3n de usuarios"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de cuentas de usuario, asignaci\u00f3n de roles y estados de acceso.",
                Icono = "fa fa-users",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#2563eb"
            },
            ["Administracion de usuarios"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de cuentas de usuario, asignaci\u00f3n de roles y estados de acceso.",
                Icono = "fa fa-users",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#2563eb"
            },
            ["Cambiar contrase\u00f1a"] = new CardMeta
            {
                Descripcion = "Actualizaci\u00f3n de clave de acceso personal y par\u00e1metros de seguridad de la cuenta.",
                Icono = "fa fa-key",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Cambiar contrasena"] = new CardMeta
            {
                Descripcion = "Actualizaci\u00f3n de clave de acceso personal y par\u00e1metros de seguridad de la cuenta.",
                Icono = "fa fa-key",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },

            // === OTROS MÓDULOS DEL SISTEMA ===
            ["Simulaci\u00f3n de Capital"] = new CardMeta
            {
                Descripcion = "Modelado de escenarios de estr\u00e9s y suficiencia patrimonial.",
                Icono = "fa fa-cubes",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["SimulacionCapital"] = new CardMeta
            {
                Descripcion = "Modelado de escenarios de estr\u00e9s y suficiencia patrimonial.",
                Icono = "fa fa-cubes",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Indicadores Econ\u00f3micos"] = new CardMeta
            {
                Descripcion = "Seguimiento de tasas de inter\u00e9s, inflaci\u00f3n y variables macro.",
                Icono = "fa fa-globe",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["IndEconomico"] = new CardMeta
            {
                Descripcion = "Seguimiento de tasas de inter\u00e9s, inflaci\u00f3n y variables macro.",
                Icono = "fa fa-globe",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["Comparativo de Industria"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis comparativo y benchmark frente al sector financiero.",
                Icono = "fa fa-bar-chart",
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4338ca"
            },
            ["Industria"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis comparativo y benchmark frente al sector financiero.",
                Icono = "fa fa-bar-chart",
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4338ca"
            },
            ["Tipo de XML"] = new CardMeta
            {
                Descripcion = "Estructuras, esquemas y definiciones de archivos de intercambio XML.",
                Icono = "fa fa-file-code-o",
                ColorFondoIcono = "#ecfeff",
                ColorIcono = "#0891b2"
            },
            ["TipoXML"] = new CardMeta
            {
                Descripcion = "Estructuras, esquemas y definiciones de archivos de intercambio XML.",
                Icono = "fa fa-file-code-o",
                ColorFondoIcono = "#ecfeff",
                ColorIcono = "#0891b2"
            },
            ["Tipo de Informe"] = new CardMeta
            {
                Descripcion = "Administraci\u00f3n y categorizaci\u00f3n de tipos de informes del sistema.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#ebf6f9",
                ColorIcono = "#6DB5CB"
            },
            ["TipoInforme"] = new CardMeta
            {
                Descripcion = "Administraci\u00f3n y categorizaci\u00f3n de tipos de informes del sistema.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#ebf6f9",
                ColorIcono = "#6DB5CB"
            },
            ["Solicitudes de Cambio"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n y aprobaci\u00f3n de solicitudes de modificaci\u00f3n de informaci\u00f3n.",
                Icono = "fa fa-exchange",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["SolicitudCambio"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n y aprobaci\u00f3n de solicitudes de modificaci\u00f3n de informaci\u00f3n.",
                Icono = "fa fa-exchange",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },

            // === GRÁFICOS E INDICADORES (Opciones Hijas Específicas) ===
            ["SUGEF"] = new CardMeta
            {
                Descripcion = "Consulte y analice los principales indicadores financieros normativos seg\u00fan regulaci\u00f3n SUGEF.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["FFC"] = new CardMeta
            {
                Descripcion = "Analice los indicadores y estad\u00edsticas del Fondo de Financiamiento para la Competitividad.",
                Icono = "fa fa-university",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Indicadores/Sugef"] = new CardMeta
            {
                Descripcion = "Monitoreo de solvencia patrimonial, suficiencia y análisis de indicadores normativos.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#fff1eb",
                ColorIcono = "#FE7235"
            },
            ["Indicadores/FGA"] = new CardMeta
            {
                Descripcion = "Consulte y compare la evolución de los indicadores de cartera, financieros y personalizados.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4338ca"
            },
            ["Econ\u00f3micos"] = new CardMeta
            {
                Descripcion = "Seguimiento de tasas de inter\u00e9s, inflaci\u00f3n y variables macroecon\u00f3micas.",
                Icono = "fa fa-globe",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["Economicos"] = new CardMeta
            {
                Descripcion = "Seguimiento de tasas de inter\u00e9s, inflaci\u00f3n y variables macroecon\u00f3micas.",
                Icono = "fa fa-globe",
                ColorFondoIcono = "#ccfbf1",
                ColorIcono = "#0d9488"
            },
            ["Personalizados"] = new CardMeta
            {
                Descripcion = "Configure y consulte indicadores financieros personalizados seg\u00fan sus criterios de evaluaci\u00f3n.",
                Icono = "fa fa-sliders",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Personalizado"] = new CardMeta
            {
                Descripcion = "Configure y consulte indicadores financieros personalizados seg\u00fan sus criterios de evaluaci\u00f3n.",
                Icono = "fa fa-sliders",
                ColorFondoIcono = "#e6ecf1",
                ColorIcono = "#003F6B"
            },
            ["Notificaciones"] = new CardMeta
            {
                Descripcion = "Monitoreo y consulta de alertas, boletines y notificaciones institucionales.",
                Icono = "fa fa-bell-o",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Notificacion"] = new CardMeta
            {
                Descripcion = "Monitoreo y consulta de alertas, boletines y notificaciones institucionales.",
                Icono = "fa fa-bell-o",
                ColorFondoIcono = "#fff7ec",
                ColorIcono = "#FFB347"
            },
            ["Requisitos"] = new CardMeta
            {
                Descripcion = "Verificaci\u00f3n y estado de cumplimiento de requisitos regulatorios.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Requisites"] = new CardMeta
            {
                Descripcion = "Verificaci\u00f3n y estado de cumplimiento de requisitos regulatorios.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#e6f6fc",
                ColorIcono = "#0099D6"
            },
            ["Informes"] = new CardMeta
            {
                Descripcion = "Generaci\u00f3n y visualizaci\u00f3n de informes gerenciales y financieros.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#2563eb"
            },
            ["Informe"] = new CardMeta
            {
                Descripcion = "Generaci\u00f3n y visualizaci\u00f3n de informes gerenciales y financieros.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#2563eb"
            },
            ["Consultas"] = new CardMeta
            {
                Descripcion = "Consultas avanzadas y an\u00e1lisis din\u00e1mico de informaci\u00f3n consolidada.",
                Icono = "fa fa-search",
                ColorFondoIcono = "#ebf6f9",
                ColorIcono = "#6DB5CB"
            },
            ["Consulta"] = new CardMeta
            {
                Descripcion = "Consultas avanzadas y an\u00e1lisis din\u00e1mico de informaci\u00f3n consolidada.",
                Icono = "fa fa-search",
                ColorFondoIcono = "#ebf6f9",
                ColorIcono = "#6DB5CB"
            }
        };

        public static ModuloMeta GetModuloMeta(string titulo)
        {
            if (string.IsNullOrEmpty(titulo))
                return new ModuloMeta { Subtitulo = "M\u00f3dulo de consultas y reportes", NotaPie = "" };

            if (Modulos.TryGetValue(titulo.Trim(), out var meta))
                return meta;

            // Fallback por defecto
            return new ModuloMeta
            {
                Subtitulo = "Consulta y an\u00e1lisis de informaci\u00f3n correspondiente a " + titulo.Trim(),
                NotaPie = "Nota: Toda la informaci\u00f3n corresponde a los par\u00e1metros seleccionados."
            };
        }

        public static CardMeta GetCardMeta(string menuText, string menuUrl, string dbIcon = null, int index = 0, string dbDesc = null)
        {
            string textTrim = (menuText ?? "").Trim();
            string urlTrim = (menuUrl ?? "").Trim().Trim('/');

            // 1. Asignación estricta de color por índice secuencial en el grid (Garantiza 100% colores distintos en el mismo Hub)
            var colorPair = Palette[Math.Abs(index) % Palette.Length];

            string descFound = null;
            string iconFound = null;

            // 2. Si viene descripción personalizada desde la BD (columna Description), tiene máxima prioridad
            if (!string.IsNullOrWhiteSpace(dbDesc))
            {
                descFound = dbDesc.Trim();
            }

            // 3. Si viene ícono personalizado desde la BD (columna MenuIcon), formatearlo y usarlo con máxima prioridad
            if (!string.IsNullOrWhiteSpace(dbIcon))
            {
                iconFound = NormalizarClaseIcono(dbIcon, null);
            }

            // 4. Buscar si existe metadato de descripción e ícono en Tarjetas por MenuText o URL completa
            if (string.IsNullOrEmpty(descFound) || string.IsNullOrEmpty(iconFound))
            {
                if (!string.IsNullOrEmpty(textTrim) && Tarjetas.TryGetValue(textTrim, out var metaText) && metaText != null)
                {
                    if (string.IsNullOrEmpty(descFound)) descFound = metaText.Descripcion;
                    if (string.IsNullOrEmpty(iconFound)) iconFound = metaText.Icono;
                }
                else if (!string.IsNullOrEmpty(urlTrim) && Tarjetas.TryGetValue(urlTrim, out var metaFull) && metaFull != null)
                {
                    if (string.IsNullOrEmpty(descFound)) descFound = metaFull.Descripcion;
                    if (string.IsNullOrEmpty(iconFound)) iconFound = metaFull.Icono;
                }
                else if (!string.IsNullOrEmpty(urlTrim) && urlTrim.IndexOf('/') <= 0 && Tarjetas.TryGetValue(urlTrim, out var metaCtrl) && metaCtrl != null)
                {
                    if (string.IsNullOrEmpty(descFound)) descFound = metaCtrl.Descripcion;
                    if (string.IsNullOrEmpty(iconFound)) iconFound = metaCtrl.Icono;
                }
            }

            // 5. Inferencia inteligente de ícono por palabras clave en MenuText o URL
            if (string.IsNullOrEmpty(iconFound) || iconFound == "fa fa-file-text-o")
            {
                if (!string.IsNullOrEmpty(textTrim))
                {
                    string lower = textTrim.ToLower();
                    if (lower.Contains("perfil") || lower.Contains("rol")) iconFound = "fa fa-id-badge";
                    else if (lower.Contains("catálogo") || lower.Contains("catalogo") || lower.Contains("cuenta")) iconFound = "fa fa-book";
                    else if (lower.Contains("evalua")) iconFound = "fa fa-check-square-o";
                    else if (lower.Contains("usuario")) iconFound = "fa fa-users";
                    else if (lower.Contains("entidad")) iconFound = "fa fa-building-o";
                    else if (lower.Contains("seguridad") || lower.Contains("auditoria")) iconFound = "fa fa-lock";
                    else if (lower.Contains("parametro")) iconFound = "fa fa-sliders";
                    else if (lower.Contains("cierre")) iconFound = "fa fa-calendar-check-o";
                    else if (lower.Contains("formula")) iconFound = "fa fa-calculator";
                    else if (lower.Contains("noticia") || lower.Contains("notificac")) iconFound = "fa fa-bell-o";
                    else if (lower.Contains("calendario")) iconFound = "fa fa-calendar";
                    else if (lower.Contains("archivo")) iconFound = "fa fa-cloud-upload";
                    else if (lower.Contains("cartera") || lower.Contains("credito")) iconFound = "fa fa-pie-chart";
                    else if (lower.Contains("mora") || lower.Contains("riesgo")) iconFound = "fa fa-th";
                    else if (lower.Contains("sugef")) iconFound = "fa fa-line-chart";
                    else if (lower.Contains("ffc")) iconFound = "fa fa-university";
                    else if (lower.Contains("personaliz")) iconFound = "fa fa-sliders";
                    else if (lower.Contains("econom")) iconFound = "fa fa-globe";
                    else if (lower.Contains("margen") || lower.Contains("márgen")) iconFound = "fa fa-percent";
                    else if (lower.Contains("tasa") || lower.Contains("indicador")) iconFound = "fa fa-line-chart";
                    else if (lower.Contains("informe") || lower.Contains("reporte")) iconFound = "fa fa-file-text-o";
                    else if (lower.Contains("consult")) iconFound = "fa fa-search";
                    else if (lower.Contains("requisito") || lower.Contains("requisit")) iconFound = "fa fa-check-square-o";
                    else iconFound = "fa fa-folder-open-o";
                }
                else
                {
                    iconFound = "fa fa-folder-open-o";
                }
            }

            // 6. Descripción adaptativa personalizada si no existe en el catálogo
            if (string.IsNullOrEmpty(descFound))
            {
                descFound = "Acceda a la informaci\u00f3n y reportes detallados de " + (textTrim != "" ? textTrim : "este m\u00f3dulo") + ".";
                if (!string.IsNullOrEmpty(textTrim))
                {
                    string lowerText = textTrim.ToLower();
                    if (lowerText.Contains("sugef")) descFound = "Consulte y analice los principales indicadores financieros normativos seg\u00fan regulaci\u00f3n SUGEF.";
                    else if (lowerText.Contains("ffc")) descFound = "Analice los indicadores y estad\u00edsticas del Fondo de Financiamiento para la Competitividad.";
                    else if (lowerText.Contains("personaliz")) descFound = "Configure y consulte indicadores financieros personalizados seg\u00fan sus criterios de evaluaci\u00f3n.";
                    else if (lowerText.Contains("econom")) descFound = "Seguimiento de tasas de inter\u00e9s, inflaci\u00f3n y variables macroecon\u00f3micas.";
                    else if (lowerText.Contains("notificac")) descFound = "Monitoreo y consulta de alertas, boletines y notificaciones institucionales.";
                    else if (lowerText.Contains("informe")) descFound = "Generaci\u00f3n y visualizaci\u00f3n de informes gerenciales y financieros.";
                    else if (lowerText.Contains("consult")) descFound = "Consultas avanzadas y an\u00e1lisis din\u00e1mico de informaci\u00f3n consolidada.";
                    else if (lowerText.Contains("requisito")) descFound = "Verificaci\u00f3n y estado de cumplimiento de requisitos regulatorios.";
                }
            }

            return new CardMeta
            {
                Descripcion = descFound,
                Icono = iconFound,
                ColorFondoIcono = colorPair.Fondo,
                ColorIcono = colorPair.Icono
            };
        }

        public static string GetDefaultDescripcion(string menuText, string menuUrl)
        {
            var meta = GetCardMeta(menuText, menuUrl, null, 0, null);
            return meta.Descripcion;
        }

        public static string GetDefaultIcono(string menuText, string menuUrl)
        {
            var meta = GetCardMeta(menuText, menuUrl, null, 0, null);
            return meta.Icono;
        }

        public static string NormalizarClaseIcono(string rawIcon, string defaultIcon = "fa fa-folder-open-o")
        {
            if (string.IsNullOrWhiteSpace(rawIcon))
            {
                return defaultIcon;
            }

            string icon = rawIcon.Trim();

            // Si viene como etiqueta HTML tipo <i class="..."></i> o <span class="...">
            var match = System.Text.RegularExpressions.Regex.Match(icon, @"class\s*=\s*[""']([^""']+)[""']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                icon = match.Groups[1].Value.Trim();
            }
            else
            {
                // Remover cualquier etiqueta HTML si quedaron
                icon = System.Text.RegularExpressions.Regex.Replace(icon, @"<[^>]*>", "").Trim();
            }

            if (string.IsNullOrWhiteSpace(icon))
            {
                return defaultIcon;
            }

            // Si no tiene prefijo 'fa ' ni 'glyphicon ', asegurar prefijo
            if (!icon.StartsWith("fa ") && !icon.StartsWith("glyphicon "))
            {
                if (icon.StartsWith("fa-"))
                {
                    icon = "fa " + icon;
                }
                else
                {
                    icon = "fa fa-" + icon;
                }
            }

            return icon;
        }

        public static string FormatearHtmlIcono(string rawIcon, string defaultIcon = null)
        {
            if (string.IsNullOrWhiteSpace(rawIcon))
            {
                return !string.IsNullOrWhiteSpace(defaultIcon) ? string.Format("<i class=\"{0}\"></i>", NormalizarClaseIcono(defaultIcon, "fa fa-file-text-o")) : null;
            }

            string clase = NormalizarClaseIcono(rawIcon, null);
            if (string.IsNullOrWhiteSpace(clase))
            {
                return null;
            }

            return string.Format("<i class=\"{0}\"></i>", clase);
        }
    }
}

