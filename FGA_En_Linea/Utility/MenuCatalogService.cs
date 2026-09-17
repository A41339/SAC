using System;
using System.Collections.Generic;

namespace FGA.Utility
{
    public class CardMeta
    {
        public string Descripcion { get; set; } = "";
        public string Icono { get; set; } = "fa fa-file-text-o";
        public string ColorFondoIcono { get; set; } = "#e0f2fe";
        public string ColorIcono { get; set; } = "#0284c7";
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
        // Paleta dinámica de 12 colores modernos (fondo pastel suave + ícono saturado)
        public static readonly ColorPair[] Palette = new[]
        {
            new ColorPair { Fondo = "#e0f2fe", Icono = "#0284c7" }, // Celeste
            new ColorPair { Fondo = "#ffedd5", Icono = "#ea580c" }, // Naranja
            new ColorPair { Fondo = "#dcfce7", Icono = "#16a34a" }, // Esmeralda / Verde
            new ColorPair { Fondo = "#f3e8ff", Icono = "#7c3aed" }, // Violeta / Púrpura
            new ColorPair { Fondo = "#ffe4e6", Icono = "#e11d48" }, // Rosa / Coral
            new ColorPair { Fondo = "#ccfbf1", Icono = "#0d9488" }, // Teal / Verde azulado
            new ColorPair { Fondo = "#e0e7ff", Icono = "#4338ca" }, // Índigo
            new ColorPair { Fondo = "#fef3c7", Icono = "#d97706" }, // Ámbar
            new ColorPair { Fondo = "#dbeafe", Icono = "#2563eb" }, // Azul
            new ColorPair { Fondo = "#fae8ff", Icono = "#a21caf" }, // Fucsia
            new ColorPair { Fondo = "#ecfeff", Icono = "#0891b2" }, // Cyan
            new ColorPair { Fondo = "#fef9c3", Icono = "#ca8a04" }  // Dorado
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
                Subtitulo = "Evaluaci\u00f3n y seguimiento de la Supervisi\u00f3n Basada en Riesgos (SBR)",
                NotaPie = "Nota: Los resultados de autoevaluaci\u00f3n se procesan seg\u00fan la metodolog\u00eda SUGEF."
            },
            ["Evaluación"] = new ModuloMeta
            {
                Subtitulo = "Evaluaci\u00f3n y seguimiento de la Supervisi\u00f3n Basada en Riesgos (SBR)",
                NotaPie = "Nota: Los resultados de autoevaluaci\u00f3n se procesan seg\u00fan la metodolog\u00eda SUGEF."
            },
            ["Administracion"] = new ModuloMeta
            {
                Subtitulo = "Gesti\u00f3n de usuarios, roles, par\u00e1metros y configuraciones del sistema",
                NotaPie = "Nota: Los cambios en configuraciones quedan auditados en la bit\u00e1cora de seguridad."
            },
            ["Administraci\u00f3n"] = new ModuloMeta
            {
                Subtitulo = "Gesti\u00f3n de usuarios, roles, par\u00e1metros y configuraciones del sistema",
                NotaPie = "Nota: Los cambios en configuraciones quedan auditados en la bit\u00e1cora de seguridad."
            },
            ["Seguridad"] = new ModuloMeta
            {
                Subtitulo = "Monitoreo de sesiones activas y auditor\u00eda de accesos a la plataforma",
                NotaPie = "Nota: Los registros de sesi\u00f3n se actualizan autom\u00e1ticamente en tiempo real."
            }
        };

        // Metadatos de tarjetas hijas (por coincidencia de URL o texto)
        private static readonly Dictionary<string, CardMeta> Tarjetas = new Dictionary<string, CardMeta>(StringComparer.OrdinalIgnoreCase)
        {
            // === ESTRUCTURA FINANCIERA (Maqueta original) ===
            ["Estados Financieros"] = new CardMeta
            {
                Descripcion = "Consulte la situaci\u00f3n financiera, los resultados y el movimiento de fondos de la entidad.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#e0f2fe", // Celeste suave
                ColorIcono = "#0284c7"
            },
            ["InformeFinanciero"] = new CardMeta
            {
                Descripcion = "Consulte la situaci\u00f3n financiera, los resultados y el movimiento de fondos de la entidad.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#e0f2fe",
                ColorIcono = "#0284c7"
            },
            ["Estructura de Fondeo"] = new CardMeta
            {
                Descripcion = "Analice la composici\u00f3n de las fuentes de fondeo de la entidad.",
                Icono = "fa fa-database",
                ColorFondoIcono = "#ffedd5", // Naranja suave
                ColorIcono = "#ea580c"
            },
            ["Composicion"] = new CardMeta
            {
                Descripcion = "Analice la composici\u00f3n de las fuentes de fondeo de la entidad.",
                Icono = "fa fa-database",
                ColorFondoIcono = "#ffedd5",
                ColorIcono = "#ea580c"
            },
            ["Indicadores Financieros"] = new CardMeta
            {
                Descripcion = "Analice los principales indicadores financieros de la entidad.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#ffe4e6", // Coral/rosa suave
                ColorIcono = "#e11d48"
            },
            ["Indicadores"] = new CardMeta
            {
                Descripcion = "Analice los principales indicadores financieros de la entidad.",
                Icono = "fa fa-line-chart",
                ColorFondoIcono = "#ffe4e6",
                ColorIcono = "#e11d48"
            },
            ["M\u00e1rgenes"] = new CardMeta
            {
                Descripcion = "Revise los m\u00e1rgenes financieros y operativos.",
                Icono = "fa fa-percent",
                ColorFondoIcono = "#fef3c7", // Ámbar suave
                ColorIcono = "#d97706"
            },
            ["Margen"] = new CardMeta
            {
                Descripcion = "Revise los m\u00e1rgenes financieros y operativos.",
                Icono = "fa fa-percent",
                ColorFondoIcono = "#fef3c7",
                ColorIcono = "#d97706"
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
            ["Cartera 14-21"] = new CardMeta
            {
                Descripcion = "Monitoreo de saldos de cartera seg\u00fan acuerdo SUGEF 14-21.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#dcfce7", // Verde suave
                ColorIcono = "#16a34a"
            },
            ["Cartera14_21"] = new CardMeta
            {
                Descripcion = "Monitoreo de saldos de cartera seg\u00fan acuerdo SUGEF 14-21.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#dcfce7",
                ColorIcono = "#16a34a"
            },
            ["Matrices de Mora"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis de transici\u00f3n de mora y deterioro de cr\u00e9ditos.",
                Icono = "fa fa-th",
                ColorFondoIcono = "#ffe4e6", // Coral suave
                ColorIcono = "#e11d48"
            },
            ["Matriz"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis de transici\u00f3n de mora y deterioro de cr\u00e9ditos.",
                Icono = "fa fa-th",
                ColorFondoIcono = "#ffe4e6",
                ColorIcono = "#e11d48"
            },
            ["IRL"] = new CardMeta
            {
                Descripcion = "C\u00e1lculo e insumos del Indicador de Riesgo de Liquidez.",
                Icono = "fa fa-tint",
                ColorFondoIcono = "#e0f2fe", // Celeste suave
                ColorIcono = "#0284c7"
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
                ColorFondoIcono = "#fef3c7", // Ámbar suave
                ColorIcono = "#d97706"
            },
            ["Evaluacion"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de formularios, par\u00e1metros de supervisi\u00f3n y autoevaluaciones.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#fef3c7",
                ColorIcono = "#d97706"
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
                ColorFondoIcono = "#dcfce7", // Verde suave
                ColorIcono = "#16a34a"
            },
            ["Entidad"] = new CardMeta
            {
                Descripcion = "Cat\u00e1logo de entidades participantes y par\u00e1metros generales.",
                Icono = "fa fa-building-o",
                ColorFondoIcono = "#dcfce7",
                ColorIcono = "#16a34a"
            },
            ["Par\u00e1metros"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de variables operativas y par\u00e1metros globales del sistema.",
                Icono = "fa fa-sliders",
                ColorFondoIcono = "#ffedd5", // Naranja suave
                ColorIcono = "#ea580c"
            },
            ["Parametros"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de variables operativas y par\u00e1metros globales del sistema.",
                Icono = "fa fa-sliders",
                ColorFondoIcono = "#ffedd5",
                ColorIcono = "#ea580c"
            },
            ["Cierres"] = new CardMeta
            {
                Descripcion = "Monitoreo y administraci\u00f3n de fechas de corte y cierres contables.",
                Icono = "fa fa-calendar-check-o",
                ColorFondoIcono = "#f3e8ff", // Púrpura suave
                ColorIcono = "#7c3aed"
            },
            ["Cierre"] = new CardMeta
            {
                Descripcion = "Monitoreo y administraci\u00f3n de fechas de corte y cierres contables.",
                Icono = "fa fa-calendar-check-o",
                ColorFondoIcono = "#f3e8ff",
                ColorIcono = "#7c3aed"
            },
            ["F\u00f3rmulas"] = new CardMeta
            {
                Descripcion = "Definici\u00f3n y f\u00f3rmulas de c\u00e1lculo de indicadores financieros y normativos.",
                Icono = "fa fa-calculator",
                ColorFondoIcono = "#fae8ff", // Fucsia suave
                ColorIcono = "#a21caf"
            },
            ["Formula"] = new CardMeta
            {
                Descripcion = "Definici\u00f3n y f\u00f3rmulas de c\u00e1lculo de indicadores financieros y normativos.",
                Icono = "fa fa-calculator",
                ColorFondoIcono = "#fae8ff",
                ColorIcono = "#a21caf"
            },
            ["Noticias"] = new CardMeta
            {
                Descripcion = "Publicaci\u00f3n de boletines y comunicados informativos para las entidades.",
                Icono = "fa fa-newspaper-o",
                ColorFondoIcono = "#e0f2fe", // Celeste suave
                ColorIcono = "#0284c7"
            },
            ["Noticia"] = new CardMeta
            {
                Descripcion = "Publicaci\u00f3n de boletines y comunicados informativos para las entidades.",
                Icono = "fa fa-newspaper-o",
                ColorFondoIcono = "#e0f2fe",
                ColorIcono = "#0284c7"
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
                ColorFondoIcono = "#dcfce7", // Verde suave
                ColorIcono = "#15803d"
            },
            ["Facturacion"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de facturaci\u00f3n y cuotas de mantenimiento de la plataforma.",
                Icono = "fa fa-credit-card",
                ColorFondoIcono = "#dcfce7",
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
                ColorFondoIcono = "#ffe4e6", // Coral suave
                ColorIcono = "#e11d48"
            },
            ["Seguridad"] = new CardMeta
            {
                Descripcion = "Historial de conexiones, sesiones activas y registro de auditor\u00eda.",
                Icono = "fa fa-lock",
                ColorFondoIcono = "#ffe4e6",
                ColorIcono = "#e11d48"
            },

            // === MERCADO DE CRÉDITOS ===
            ["Mercado de Cr\u00e9ditos"] = new CardMeta
            {
                Descripcion = "Consulte las ofertas y solicitudes de cr\u00e9ditos vigentes.",
                Icono = "fa fa-university",
                ColorFondoIcono = "#dcfce7",
                ColorIcono = "#16a34a"
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
                ColorFondoIcono = "#f3e8ff",
                ColorIcono = "#7c3aed"
            },

            // === GOBIERNO CORPORATIVO ===
            ["Autoevaluaci\u00f3n SBR"] = new CardMeta
            {
                Descripcion = "Formularios de autoevaluaci\u00f3n de supervisi\u00f3n basada en riesgos.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#e0f2fe",
                ColorIcono = "#0284c7"
            },
            ["Avance de Evaluaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Monitoreo del porcentaje de avance y respuestas completadas.",
                Icono = "fa fa-tasks",
                ColorFondoIcono = "#fef3c7",
                ColorIcono = "#d97706"
            },

            // === OTROS MÓDULOS DEL SISTEMA ===
            ["Simulaci\u00f3n de Capital"] = new CardMeta
            {
                Descripcion = "Modelado de escenarios de estr\u00e9s y suficiencia patrimonial.",
                Icono = "fa fa-cubes",
                ColorFondoIcono = "#ffedd5",
                ColorIcono = "#ea580c"
            },
            ["SimulacionCapital"] = new CardMeta
            {
                Descripcion = "Modelado de escenarios de estr\u00e9s y suficiencia patrimonial.",
                Icono = "fa fa-cubes",
                ColorFondoIcono = "#ffedd5",
                ColorIcono = "#ea580c"
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
                ColorFondoIcono = "#fae8ff",
                ColorIcono = "#a21caf"
            },
            ["TipoInforme"] = new CardMeta
            {
                Descripcion = "Administraci\u00f3n y categorizaci\u00f3n de tipos de informes del sistema.",
                Icono = "fa fa-file-text-o",
                ColorFondoIcono = "#fae8ff",
                ColorIcono = "#a21caf"
            },
            ["Solicitudes de Cambio"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n y aprobaci\u00f3n de solicitudes de modificaci\u00f3n de informaci\u00f3n.",
                Icono = "fa fa-exchange",
                ColorFondoIcono = "#fef3c7",
                ColorIcono = "#d97706"
            },
            ["SolicitudCambio"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n y aprobaci\u00f3n de solicitudes de modificaci\u00f3n de informaci\u00f3n.",
                Icono = "fa fa-exchange",
                ColorFondoIcono = "#fef3c7",
                ColorIcono = "#d97706"
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

        public static CardMeta GetCardMeta(string menuText, string menuUrl, string dbIcon = null, int index = 0)
        {
            // 1. Buscar por MenuText
            if (!string.IsNullOrEmpty(menuText) && Tarjetas.TryGetValue(menuText.Trim(), out var metaText) && metaText != null)
                return metaText;

            // 2. Buscar por Controller en la URL (ej: "InformeFinanciero/ER" -> "InformeFinanciero")
            if (!string.IsNullOrEmpty(menuUrl))
            {
                string cleanUrl = menuUrl.Trim().Trim('/');
                int slashIdx = cleanUrl.IndexOf('/');
                string controller = slashIdx > 0 ? cleanUrl.Substring(0, slashIdx) : cleanUrl;

                if (Tarjetas.TryGetValue(controller, out var metaCtrl) && metaCtrl != null)
                    return metaCtrl;
            }

            // 3. Extracción de ícono si viene en formato HTML desde la BD
            string fallbackIcon = "fa fa-file-text-o";
            if (!string.IsNullOrEmpty(dbIcon))
            {
                var match = System.Text.RegularExpressions.Regex.Match(dbIcon, @"fa-[a-zA-Z0-9_-]+");
                if (match.Success)
                {
                    fallbackIcon = "fa " + match.Value;
                }
            }

            // Si el ícono es genérico, intentar inferir por palabras clave
            if (fallbackIcon == "fa fa-file-text-o" && !string.IsNullOrEmpty(menuText))
            {
                string lower = menuText.ToLower();
                if (lower.Contains("perfil") || lower.Contains("rol")) fallbackIcon = "fa fa-id-badge";
                else if (lower.Contains("catálogo") || lower.Contains("catalogo") || lower.Contains("cuenta")) fallbackIcon = "fa fa-book";
                else if (lower.Contains("evalua")) fallbackIcon = "fa fa-check-square-o";
                else if (lower.Contains("usuario")) fallbackIcon = "fa fa-users";
                else if (lower.Contains("entidad")) fallbackIcon = "fa fa-building-o";
                else if (lower.Contains("seguridad") || lower.Contains("auditoria")) fallbackIcon = "fa fa-lock";
                else if (lower.Contains("parametro")) fallbackIcon = "fa fa-sliders";
                else if (lower.Contains("cierre")) fallbackIcon = "fa fa-calendar-check-o";
                else if (lower.Contains("formula")) fallbackIcon = "fa fa-calculator";
                else if (lower.Contains("noticia")) fallbackIcon = "fa fa-newspaper-o";
                else if (lower.Contains("calendario")) fallbackIcon = "fa fa-calendar";
                else if (lower.Contains("archivo")) fallbackIcon = "fa fa-cloud-upload";
                else if (lower.Contains("cartera") || lower.Contains("credito")) fallbackIcon = "fa fa-pie-chart";
                else if (lower.Contains("mora") || lower.Contains("riesgo")) fallbackIcon = "fa fa-th";
                else if (lower.Contains("tasa") || lower.Contains("indicador")) fallbackIcon = "fa fa-line-chart";
            }

            // 4. Color dinámico vibrante asignado por paleta (NUNCA gris aburrido)
            var colorPair = GetPaletteColor(menuText ?? menuUrl, index);

            return new CardMeta
            {
                Descripcion = "Acceda a la informaci\u00f3n y reportes detallados de " + (menuText ?? "este m\u00f3dulo") + ".",
                Icono = fallbackIcon,
                ColorFondoIcono = colorPair.Fondo,
                ColorIcono = colorPair.Icono
            };
        }
    }
}
