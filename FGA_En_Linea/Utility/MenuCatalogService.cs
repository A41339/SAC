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

    public static class MenuCatalogService
    {
        // Metadatos de modulos raiz (por texto o ID)
        private static readonly Dictionary<string, ModuloMeta> Modulos = new Dictionary<string, ModuloMeta>(StringComparer.OrdinalIgnoreCase)
        {
            ["Estructura Financiera"] = new ModuloMeta
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
            // === ESTRUCTURA FINANCIERA (Maqueta del usuario) ===
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
                ColorFondoIcono = "#ffedd5", // Naranja suave
                ColorIcono = "#f97316"
            },
            ["Margen"] = new CardMeta
            {
                Descripcion = "Revise los m\u00e1rgenes financieros y operativos.",
                Icono = "fa fa-percent",
                ColorFondoIcono = "#ffedd5",
                ColorIcono = "#f97316"
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

            // === CARTERA ===
            ["Cartera de Cr\u00e9dito"] = new CardMeta
            {
                Descripcion = "Monitoreo de colocaci\u00f3n, morosidad, acuerdos regulatorios y transici\u00f3n de riesgo.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e0e7ff",
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
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4f46e5"
            },
            ["Cartera14_21"] = new CardMeta
            {
                Descripcion = "Monitoreo de saldos de cartera seg\u00fan acuerdo SUGEF 14-21.",
                Icono = "fa fa-pie-chart",
                ColorFondoIcono = "#e0e7ff",
                ColorIcono = "#4f46e5"
            },
            ["Matrices de Mora"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis de transici\u00f3n de mora y deterioro de cr\u00e9ditos.",
                Icono = "fa fa-th",
                ColorFondoIcono = "#fee2e2",
                ColorIcono = "#dc2626"
            },
            ["Matriz"] = new CardMeta
            {
                Descripcion = "An\u00e1lisis de transici\u00f3n de mora y deterioro de cr\u00e9ditos.",
                Icono = "fa fa-th",
                ColorFondoIcono = "#fee2e2",
                ColorIcono = "#dc2626"
            },
            ["IRL"] = new CardMeta
            {
                Descripcion = "C\u00e1lculo e insumos del Indicador de Riesgo de Liquidez.",
                Icono = "fa fa-tint",
                ColorFondoIcono = "#e0f2fe",
                ColorIcono = "#0284c7"
            },

            // === MERCADO DE CRÉDITOS ===
            ["Mercado de Cr\u00e9ditos"] = new CardMeta
            {
                Descripcion = "Consulte las ofertas y solicitudes de cr\u00e9ditos vigentes.",
                Icono = "fa fa-university",
                ColorFondoIcono = "#f0fdf4",
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
                ColorFondoIcono = "#ecfdf5",
                ColorIcono = "#059669"
            },
            ["Gesti\u00f3n Recibidas"] = new CardMeta
            {
                Descripcion = "Administre las ofertas y solicitudes recibidas de otras entidades.",
                Icono = "fa fa-tasks",
                ColorFondoIcono = "#f1f5f9",
                ColorIcono = "#475569"
            },

            // === GOBIERNO CORPORATIVO ===
            ["Autoevaluaci\u00f3n SBR"] = new CardMeta
            {
                Descripcion = "Formularios de autoevaluaci\u00f3n de supervisi\u00f3n basada en riesgos.",
                Icono = "fa fa-check-square-o",
                ColorFondoIcono = "#e0f2fe",
                ColorIcono = "#0369a1"
            },
            ["Avance de Evaluaci\u00f3n"] = new CardMeta
            {
                Descripcion = "Monitoreo del porcentaje de avance y respuestas completadas.",
                Icono = "fa fa-tasks",
                ColorFondoIcono = "#fef3c7",
                ColorIcono = "#d97706"
            },

            // === ADMINISTRACIÓN / SEGURIDAD ===
            ["Usuarios"] = new CardMeta
            {
                Descripcion = "Gesti\u00f3n de usuarios, credenciales y estados de acceso.",
                Icono = "fa fa-users",
                ColorFondoIcono = "#f1f5f9",
                ColorIcono = "#334155"
            },
            ["Roles"] = new CardMeta
            {
                Descripcion = "Configuraci\u00f3n de roles y permisos de acceso a men\u00fas.",
                Icono = "fa fa-shield",
                ColorFondoIcono = "#e2e8f0",
                ColorIcono = "#1e293b"
            },
            ["Entidades"] = new CardMeta
            {
                Descripcion = "Cat\u00e1logo de entidades participantes y par\u00e1metros generales.",
                Icono = "fa fa-building-o",
                ColorFondoIcono = "#dbeafe",
                ColorIcono = "#1d4ed8"
            },
            ["Auditor\u00eda de Sesiones"] = new CardMeta
            {
                Descripcion = "Historial de conexiones y control de sesiones activas.",
                Icono = "fa fa-history",
                ColorFondoIcono = "#fef2f2",
                ColorIcono = "#b91c1c"
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

        public static CardMeta GetCardMeta(string menuText, string menuUrl, string dbIcon = null)
        {
            // 1. Buscar por MenuText
            if (!string.IsNullOrEmpty(menuText) && Tarjetas.TryGetValue(menuText.Trim(), out var metaText))
                return metaText;

            // 2. Buscar por Controller en la URL (ej: "InformeFinanciero/ER" -> "InformeFinanciero")
            if (!string.IsNullOrEmpty(menuUrl))
            {
                string cleanUrl = menuUrl.Trim().Trim('/');
                int slashIdx = cleanUrl.IndexOf('/');
                string controller = slashIdx > 0 ? cleanUrl.Substring(0, slashIdx) : cleanUrl;

                if (Tarjetas.TryGetValue(controller, out var metaCtrl))
                    return metaCtrl;
            }

            // 3. Fallback inteligente: extraer icono existente de la base de datos si viene en formato HTML
            string fallbackIcon = "fa fa-file-text-o";
            if (!string.IsNullOrEmpty(dbIcon))
            {
                var match = System.Text.RegularExpressions.Regex.Match(dbIcon, @"fa-[a-zA-Z0-9_-]+");
                if (match.Success)
                {
                    fallbackIcon = "fa " + match.Value;
                }
            }

            return new CardMeta
            {
                Descripcion = "Acceda a la informaci\u00f3n y reportes detallados de " + (menuText ?? "este m\u00f3dulo") + ".",
                Icono = fallbackIcon,
                ColorFondoIcono = "#f1f5f9",
                ColorIcono = "#475569"
            };
        }
    }
}
