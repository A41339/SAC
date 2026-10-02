using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FGA.Models;
using FGA.Utility;

namespace FGA.Controllers
{
    public class ModuloController : BaseController
    {
        public ActionResult Index(int? id, string modulo = null, string IdEntidad = null, string Periodo = null, string TipoComparacion = null)
        {
            if (!string.IsNullOrWhiteSpace(IdEntidad))
            {
                if (Session["IdEntidad"] != null && Session["IdEntidad"].ToString() != IdEntidad)
                {
                    // Si cambió la entidad explícitamente en la URL, resetear Periodo2 para recalcularlo al último cargado de la nueva entidad
                    Session["Periodo2"] = null;
                }
                Session["IdEntidad"] = IdEntidad;
            }

            Load();

            // Determinar la fecha de cierre y el último período cargado de la entidad activa
            string currentEntidad = Session["IdEntidad"]?.ToString() ?? "2";
            DateTime? ultimoPeriodoCargado = null;
            DateTime? fechaCierreCalculada = null;

            try
            {
                if (Session["Periodo"] != null)
                {
                    fechaCierreCalculada = Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
                    ultimoPeriodoCargado = fechaCierreCalculada.Value.AddMonths(-1);
                }
                else
                {
                    using (var sp = new FGA_En_Linea.SPService.SPClient())
                    {
                        var fc = sp.FGA_Consultar_FechaCierre(currentEntidad);
                        Session["Periodo"] = fc.ToShortDateString();
                        fechaCierreCalculada = fc;
                        ultimoPeriodoCargado = fc.AddMonths(-1);
                    }
                }
            }
            catch { }

            // Si se envió un Período explícito por parámetro en la URL
            if (!string.IsNullOrWhiteSpace(Periodo))
            {
                try
                {
                    DateTime refDate = Utilitarios.ConvertirAFecha(Periodo);
                    Session["Periodo2"] = refDate.ToShortDateString();
                }
                catch { }
            }
            else
            {
                // Si no se especificó un período en la URL:
                // Si Session["Periodo2"] es null, o si erróneamente coincidía con fecha de cierre (mes siguiente/actual),
                // inicializar por default con el último período cargado de la entidad activa (AddMonths(-1))
                if (Session["Periodo2"] == null ||
                    (fechaCierreCalculada.HasValue && Session["Periodo2"].ToString() == fechaCierreCalculada.Value.ToShortDateString()))
                {
                    if (ultimoPeriodoCargado.HasValue)
                    {
                        Session["Periodo2"] = ultimoPeriodoCargado.Value.ToShortDateString();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(TipoComparacion))
            {
                Session["TipoComparacion"] = TipoComparacion;
            }
            else if (Session["TipoComparacion"] == null)
            {
                Session["TipoComparacion"] = "Interanual";
            }

            // Sincronizar Periodo1 y Periodo2 según TipoComparacion
            try
            {
                // Si venimos de un informe de 3 períodos (donde Periodo3 es el corte final y Periodo2 es intermedio)
                if (modulo != "InformeFinanciero/Consolidado" && Session["Periodo3"] != null && Session["Periodo2"] != null)
                {
                    DateTime f3 = Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString());
                    DateTime f2 = Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString());
                    if (f3 > f2)
                    {
                        Session["Periodo2"] = f3.ToShortDateString();
                    }
                }

                string periodoReferenciaStr = Session["Periodo2"]?.ToString();
                if (string.IsNullOrEmpty(periodoReferenciaStr) && ultimoPeriodoCargado.HasValue)
                {
                    periodoReferenciaStr = ultimoPeriodoCargado.Value.ToShortDateString();
                    Session["Periodo2"] = periodoReferenciaStr;
                }

                if (!string.IsNullOrEmpty(periodoReferenciaStr))
                {
                    DateTime refDate = Utilitarios.ConvertirAFecha(periodoReferenciaStr);
                    Session["Periodo2"] = refDate.ToShortDateString();

                    string tipo = Session["TipoComparacion"]?.ToString() ?? "Interanual";
                    if (tipo.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["Periodo1"] = refDate.AddMonths(-3).ToShortDateString();
                    }
                    else if (tipo.Equals("Mensual", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["Periodo1"] = refDate.AddMonths(-1).ToShortDateString();
                    }
                    else
                    {
                        Session["Periodo1"] = refDate.AddYears(-1).ToShortDateString();
                    }
                }
            }
            catch { }

            int roleId = 0;
            int.TryParse(Env.GetUserInfo("roleid"), out roleId);

            using (var men = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient())
            {
                var allPermitted = (men.GetMenu(roleId) ?? new MenuPermission[0]).ToList();

                MenuPermission rootPerm = null;
                if (id.HasValue)
                {
                    rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == id.Value);
                }
                
                if (rootPerm == null && !string.IsNullOrWhiteSpace(modulo))
                {
                    string modClean = modulo.Trim();

                    // 0. Coincidencia por ID numérico (ej. ?modulo=9000)
                    if (int.TryParse(modClean, out int parsedId))
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == parsedId);
                    }

                    // 1. Coincidencia exacta o parcial por MenuURL (ej. "Facturacion/Index", "Facturacion", "Role/Index", "Explorer/Notificacion", "Evaluacion/Historial")
                    if (rootPerm == null)
                    {
                        string cleanPath = modClean.Trim().Trim('/');
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuURL != null &&
                            (string.Equals(p.Menu_MenuId.MenuURL.Trim().Trim('/'), cleanPath, StringComparison.OrdinalIgnoreCase) ||
                             p.Menu_MenuId.MenuURL.Trim().Trim('/').StartsWith(cleanPath + "/", StringComparison.OrdinalIgnoreCase) ||
                             cleanPath.StartsWith(p.Menu_MenuId.MenuURL.Trim().Trim('/') + "/", StringComparison.OrdinalIgnoreCase)));
                    }

                    // 2. Coincidencia exacta por nombre de menú (MenuText)
                    if (rootPerm == null)
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuText != null &&
                            string.Equals(p.Menu_MenuId.MenuText.Trim(), modClean, StringComparison.OrdinalIgnoreCase));
                    }

                    // 3. Coincidencia normalizada sin tildes ni mayúsculas
                    if (rootPerm == null)
                    {
                        string modNorm = NormalizarTexto(modClean);
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuText != null &&
                            (NormalizarTexto(p.Menu_MenuId.MenuText) == modNorm ||
                             NormalizarTexto(p.Menu_MenuId.MenuText).Contains(modNorm) ||
                             modNorm.Contains(NormalizarTexto(p.Menu_MenuId.MenuText))));
                    }
                }

                // Soporte explícito para sub-módulos hijos de Administración (Gestión Operativa, Evaluación SBR, Carga de Datos, Informes)
                if (rootPerm == null)
                {
                    if ((id.HasValue && id.Value == 9200) || (!string.IsNullOrWhiteSpace(modulo) && modulo.IndexOf("Operativa", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        rootPerm = new MenuPermission
                        {
                            MenuId = 9200,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 9200,
                                MenuText = "Gestión Operativa",
                                MenuURL = "root",
                                ParentId = 9000,
                                MenuIcon = "<i class=\"fa fa-cogs\"></i>",
                                Description = "Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades afiliadas, parámetros globales, roles y menú del sistema."
                            }
                        };
                    }
                    else if ((id.HasValue && id.Value == 9300) || (!string.IsNullOrWhiteSpace(modulo) && modulo.IndexOf("Avance", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        rootPerm = new MenuPermission
                        {
                            MenuId = 9300,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 9300,
                                MenuText = "Evaluación SBR",
                                MenuURL = "filter",
                                ParentId = 9000,
                                MenuIcon = "<i class=\"fa fa-tasks\"></i>",
                                Description = "Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22."
                            }
                        };
                    }
                    else if ((id.HasValue && id.Value == 6000) || (!string.IsNullOrWhiteSpace(modulo) && modulo.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0 && id != 9300 && id != 9200))
                    {
                        rootPerm = new MenuPermission
                        {
                            MenuId = 6000,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 6000,
                                MenuText = "Evaluación SBR",
                                MenuURL = "root",
                                ParentId = null,
                                MenuIcon = "<i class=\"fa fa-pie-chart\"></i>",
                                Description = "Autoevaluación de los aspectos del Reglamento SUGEF 24-22"
                            }
                        };
                    }
                    else if (id.HasValue && id.Value == 7000)
                    {
                        rootPerm = new MenuPermission
                        {
                            MenuId = 7000,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 7000,
                                MenuText = "Carga de Datos",
                                MenuURL = "root",
                                ParentId = 9000,
                                MenuIcon = "<i class=\"fa fa-upload\"></i>",
                                Description = "Carga de archivos XML, estatus de la carga y consulta de archivos cargados."
                            }
                        };
                    }
                    else if (id.HasValue && id.Value == 11000)
                    {
                        rootPerm = new MenuPermission
                        {
                            MenuId = 11000,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 11000,
                                MenuText = "Gestión de Informes",
                                MenuURL = "root",
                                ParentId = 9000,
                                MenuIcon = "<i class=\"fa fa-file-pdf-o\"></i>",
                                Description = "Carga de informes, consulta de documentos, notificaciones institucionales y tipos de informe."
                            }
                        };
                    }
                }

                // Si rootPerm es una opción hoja (no tiene opciones hijas que mostrar en el Hub) pero tiene un ParentId,
                // ascender recursivamente en el árbol de menús hasta encontrar su módulo papá contenedor definido en la base de datos
                while (rootPerm != null && rootPerm.Menu_MenuId != null && rootPerm.Menu_MenuId.ParentId.HasValue
                       && rootPerm.Menu_MenuId.Id != 9200 && rootPerm.Menu_MenuId.Id != 9300 && rootPerm.Menu_MenuId.Id != 7000 && rootPerm.Menu_MenuId.Id != 11000
                       && !allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == rootPerm.Menu_MenuId.Id))
                {
                    var parentPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == rootPerm.Menu_MenuId.ParentId.Value);
                    if (parentPerm != null)
                    {
                        rootPerm = parentPerm;
                    }
                    else
                    {
                        break;
                    }
                }

                if (rootPerm == null)
                {
                    // Buscar primer menú raíz que tenga hijos
                    rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == null && allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == p.Menu_MenuId.Id));
                }

                if (rootPerm == null || rootPerm.Menu_MenuId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (rootPerm.Menu_MenuId.Id == 6000)
                {
                    rootPerm.Menu_MenuId.ParentId = null;
                    rootPerm.Menu_MenuId.MenuText = "Evaluación SBR";
                    rootPerm.Menu_MenuId.MenuIcon = "<i class=\"fa fa-pie-chart\"></i>";
                }
                else if (rootPerm.Menu_MenuId.Id == 9300)
                {
                    rootPerm.Menu_MenuId.ParentId = 9000;
                    rootPerm.Menu_MenuId.MenuText = "Evaluación SBR";
                    rootPerm.Menu_MenuId.MenuIcon = "<i class=\"fa fa-tasks\"></i>";
                }

                int rootId = rootPerm.Menu_MenuId.Id;
                string rootUrl = (rootPerm.Menu_MenuId.MenuURL ?? "").Trim();
                if (!string.IsNullOrEmpty(rootUrl) && !rootUrl.Equals("root", StringComparison.OrdinalIgnoreCase) && !rootUrl.Equals("filter", StringComparison.OrdinalIgnoreCase) && rootUrl != "#")
                {
                    return Redirect("~/" + rootUrl.TrimStart('~', '/'));
                }
                string titulo = rootPerm.Menu_MenuId.MenuText ?? "";
                var moduloMeta = MenuCatalogService.GetModuloMeta(titulo);

                // Si el módulo actual es Gestión Operativa (Id 9200), asegurar que Roles, Menú del Sistema y Configuración de Preguntas SBR se mapeen como hijas
                if (rootId == 9200 || titulo.IndexOf("Operativa", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    foreach (var p in allPermitted.Where(p => p.Menu_MenuId != null))
                    {
                        string mUrl = (p.Menu_MenuId.MenuURL ?? "").Trim();
                        string mText = (p.Menu_MenuId.MenuText ?? "").Trim();
                        if (p.Menu_MenuId.Id == 9145 || p.Menu_MenuId.Id == 13001 || p.Menu_MenuId.Id == 9135 ||
                            mUrl.Equals("Role/Index", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Role", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Menu/Index", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.Equals("Menu", StringComparison.OrdinalIgnoreCase) ||
                            mUrl.IndexOf("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.Equals("Roles", StringComparison.OrdinalIgnoreCase) ||
                            mText.IndexOf("Menú del Sistema", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Menu del Sistema", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Menu Sistema", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Preguntas SBR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Configuración de Preguntas", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Configuracion de Preguntas", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            p.Menu_MenuId.ParentId = 9200;
                            if (p.Menu_MenuId.Id == 9145 || mUrl.IndexOf("Role", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                p.Menu_MenuId.SortOrder = p.Menu_MenuId.SortOrder ?? 7;
                            }
                            else if (p.Menu_MenuId.Id == 9135 || mText.IndexOf("Preguntas", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                p.Menu_MenuId.SortOrder = p.Menu_MenuId.SortOrder ?? 9;
                            }
                            else
                            {
                                p.Menu_MenuId.SortOrder = p.Menu_MenuId.SortOrder ?? 8;
                            }
                        }
                    }

                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9200 && (p.Menu_MenuId.Id == 9135 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) >= 0 || (p.Menu_MenuId.MenuText ?? "").IndexOf("Preguntas SBR", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 9135, RoleId = roleId, Menu_MenuId = new Menu { Id = 9135, MenuText = "Configuración de Preguntas SBR", MenuURL = "Evaluacion/Index", ParentId = 9200, SortOrder = 9, MenuIcon = "<i class=\"fa fa-question-circle\"></i>", Description = "Mantenimiento y configuración del catálogo de preguntas de la evaluación SBR." } });
                    }
                }
                else if (rootId == 9300 || (titulo.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0 && rootPerm.Menu_MenuId.ParentId == 9000))
                {
                    foreach (var p in allPermitted.Where(p => p.Menu_MenuId != null))
                    {
                        string mUrl = (p.Menu_MenuId.MenuURL ?? "").Trim();
                        string mText = (p.Menu_MenuId.MenuText ?? "").Trim();
                        if (p.Menu_MenuId.Id == 6002 || p.Menu_MenuId.Id == 6003 || p.Menu_MenuId.Id == 6005 || p.Menu_MenuId.Id == 9135 ||
                            mUrl.IndexOf("Evaluacion/Avance", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mUrl.IndexOf("Evaluacion/Historial", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mUrl.IndexOf("Evaluacion/Resultados", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mUrl.IndexOf("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Avance SBR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Historial SBR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Resultados SBR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Configuración de Preguntas", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Configuracion de Preguntas", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Preguntas SBR", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            p.Menu_MenuId.ParentId = 9300;
                        }
                    }

                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 6002 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Avance", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 6002, RoleId = roleId, Menu_MenuId = new Menu { Id = 6002, MenuText = "Avance SBR", MenuURL = "Evaluacion/Avance", ParentId = 9300, SortOrder = 1, MenuIcon = "<i class=\"fa fa-line-chart\"></i>", Description = "Monitoreo del porcentaje de avance en el diligenciamiento de la evaluación SBR." } });
                    }
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 6003 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Historial", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 6003, RoleId = roleId, Menu_MenuId = new Menu { Id = 6003, MenuText = "Historial SBR", MenuURL = "Evaluacion/Historial", ParentId = 9300, SortOrder = 2, MenuIcon = "<i class=\"fa fa-history\"></i>", Description = "Bitácora histórica de evaluaciones cerradas y enviadas." } });
                    }
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 6005 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Resultados", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 6005, RoleId = roleId, Menu_MenuId = new Menu { Id = 6005, MenuText = "Resultados SBR", MenuURL = "Evaluacion/Resultados", ParentId = 9300, SortOrder = 3, MenuIcon = "<i class=\"fa fa-bar-chart\"></i>", Description = "Resumen de calificaciones y nivel de cumplimiento por aspecto." } });
                    }
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 9135 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) >= 0 || (p.Menu_MenuId.MenuText ?? "").IndexOf("Preguntas", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 9135, RoleId = roleId, Menu_MenuId = new Menu { Id = 9135, MenuText = "Configuración de Preguntas SBR", MenuURL = "Evaluacion/Index", ParentId = 9300, SortOrder = 4, MenuIcon = "<i class=\"fa fa-list-alt\"></i>", Description = "Mantenimiento y configuración del catálogo de preguntas de la evaluación SBR." } });
                    }
                }
                else if (rootId == 9000 || titulo.IndexOf("Administra", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 1. Asegurar que Evaluación SBR (9300) esté como hija directa de Administración (9000)
                    var sbrAdminPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && (p.Menu_MenuId.Id == 9300 || ((p.Menu_MenuId.MenuText ?? "").IndexOf("Evaluación SBR", StringComparison.OrdinalIgnoreCase) >= 0 && p.Menu_MenuId.Id != 6000)));
                    if (sbrAdminPerm == null)
                    {
                        sbrAdminPerm = new MenuPermission
                        {
                            MenuId = 9300,
                            RoleId = roleId,
                            Menu_MenuId = new Menu
                            {
                                Id = 9300,
                                MenuText = "Evaluación SBR",
                                MenuURL = "filter",
                                ParentId = 9000,
                                SortOrder = 4,
                                MenuIcon = "<i class=\"fa fa-tasks\"></i>",
                                Description = "Monitoreo de avance, historial, resultados y configuración de la evaluación SBR."
                            }
                        };
                        allPermitted.Add(sbrAdminPerm);
                    }
                    else
                    {
                        sbrAdminPerm.Menu_MenuId.Id = 9300;
                        sbrAdminPerm.Menu_MenuId.ParentId = 9000;
                        sbrAdminPerm.Menu_MenuId.MenuText = "Evaluación SBR";
                        sbrAdminPerm.Menu_MenuId.MenuURL = "filter";
                        sbrAdminPerm.Menu_MenuId.MenuIcon = "<i class=\"fa fa-tasks\"></i>";
                        sbrAdminPerm.Menu_MenuId.SortOrder = 4;
                    }

                    // 2. Asegurar que las opciones hijas de SBR (9300) existan en allPermitted
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 6002 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Avance", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 6002, RoleId = roleId, Menu_MenuId = new Menu { Id = 6002, MenuText = "Avance SBR", MenuURL = "Evaluacion/Avance", ParentId = 9300, SortOrder = 1, MenuIcon = "<i class=\"fa fa-line-chart\"></i>", Description = "Monitoreo del porcentaje de avance en el diligenciamiento de la evaluación SBR." } });
                    }
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 6003 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Historial", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 6003, RoleId = roleId, Menu_MenuId = new Menu { Id = 6003, MenuText = "Historial SBR", MenuURL = "Evaluacion/Historial", ParentId = 9300, SortOrder = 2, MenuIcon = "<i class=\"fa fa-history\"></i>", Description = "Bitácora histórica de evaluaciones cerradas y enviadas." } });
                    }
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 6005 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Resultados", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 6005, RoleId = roleId, Menu_MenuId = new Menu { Id = 6005, MenuText = "Resultados SBR", MenuURL = "Evaluacion/Resultados", ParentId = 9300, SortOrder = 3, MenuIcon = "<i class=\"fa fa-bar-chart\"></i>", Description = "Resumen de calificaciones y nivel de cumplimiento por aspecto." } });
                    }
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9300 && (p.Menu_MenuId.Id == 9135 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) >= 0 || (p.Menu_MenuId.MenuText ?? "").IndexOf("Preguntas", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 9135, RoleId = roleId, Menu_MenuId = new Menu { Id = 9135, MenuText = "Configuración de Preguntas SBR", MenuURL = "Evaluacion/Index", ParentId = 9300, SortOrder = 4, MenuIcon = "<i class=\"fa fa-list-alt\"></i>", Description = "Mantenimiento y configuración del catálogo de preguntas de la evaluación SBR." } });
                    }

                    // 3. Asegurar que Configuración de Preguntas SBR también esté como hija directa de Administración si se requiere
                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 9000 && (p.Menu_MenuId.Id == 9135 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion/Index", StringComparison.OrdinalIgnoreCase) >= 0 || (p.Menu_MenuId.MenuText ?? "").IndexOf("Preguntas", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 9135, RoleId = roleId, Menu_MenuId = new Menu { Id = 9135, MenuText = "Configuración de Preguntas SBR", MenuURL = "Evaluacion/Index", ParentId = 9000, SortOrder = 5, MenuIcon = "<i class=\"fa fa-list-alt\"></i>", Description = "Mantenimiento y configuración del catálogo de preguntas de la evaluación SBR." } });
                    }
                }
                else if (rootId == 6000)
                {
                    foreach (var p in allPermitted.Where(p => p.Menu_MenuId != null))
                    {
                        string mUrl = (p.Menu_MenuId.MenuURL ?? "").Trim();
                        string mText = (p.Menu_MenuId.MenuText ?? "").Trim();
                        if (p.Menu_MenuId.Id == 6001 ||
                            mUrl.IndexOf("Evaluacion/Evaluacion", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mUrl.IndexOf("Evaluacion/Autoevaluacion", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            mText.IndexOf("Autoevaluaci", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            p.Menu_MenuId.ParentId = 6000;
                        }
                    }

                    if (!allPermitted.Any(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == 6000 && (p.Menu_MenuId.Id == 6001 || (p.Menu_MenuId.MenuURL ?? "").IndexOf("Evaluacion", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        allPermitted.Add(new MenuPermission { MenuId = 6001, RoleId = roleId, Menu_MenuId = new Menu { Id = 6001, MenuText = "Autoevaluación", MenuURL = "Evaluacion/Evaluacion", ParentId = 6000, SortOrder = 1, MenuIcon = "<i class=\"fa fa-pencil-square-o\"></i>", Description = "Diligenciamiento de la autoevaluación SBR 24-22." } });
                    }
                }

                var childPerms = allPermitted
                    .Where(p => p.Menu_MenuId != null && p.Menu_MenuId.ParentId == rootId)
                    .OrderBy(p => p.Menu_MenuId.SortOrder ?? 0)
                    .ThenBy(p => p.Menu_MenuId.MenuText)
                    .ToList();

                var tarjetas = new List<ModuloTarjetaItem>();
                int cardIndex = 0;
                foreach (var child in childPerms)
                {
                    var m = child.Menu_MenuId;
                    var cardMeta = MenuCatalogService.GetCardMeta(m.MenuText, m.MenuURL, m.MenuIcon, cardIndex++, m.Description);

                    bool isSubMenu = m.Id == 9300
                                     || string.Equals(m.MenuURL?.Trim(), "root", StringComparison.OrdinalIgnoreCase)
                                     || string.IsNullOrWhiteSpace(m.MenuURL)
                                     || m.MenuURL.Trim() == "#"
                                     || allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);

                    string targetUrl = "";
                    int subOptionsCount = 0;

                    if (isSubMenu)
                    {
                        subOptionsCount = allPermitted.Count(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);
                        if (m.Id == 9300 && subOptionsCount == 0)
                        {
                            subOptionsCount = 4;
                        }

                        // Si es un contenedor de tipo raíz/filtro sin URL ejecutable y no tiene hijas permitidas, no mostrar tarjeta vacía (excepto 9300)
                        bool sinUrlEjecutable = string.IsNullOrWhiteSpace(m.MenuURL) ||
                                                string.Equals(m.MenuURL.Trim(), "root", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(m.MenuURL.Trim(), "filter", StringComparison.OrdinalIgnoreCase) ||
                                                m.MenuURL.Trim() == "#";

                        if (m.Id != 9300 && subOptionsCount == 0 && sinUrlEjecutable)
                        {
                            continue;
                        }

                        targetUrl = Url.Action("Index", "Modulo", new { id = m.Id });
                    }
                    else
                    {
                        string cleanUrl = (m.MenuURL ?? "").Trim();
                        if (!cleanUrl.StartsWith("~") && !cleanUrl.StartsWith("/"))
                        {
                            cleanUrl = "~/" + cleanUrl;
                        }
                        targetUrl = Url.Content(cleanUrl);
                    }

                    tarjetas.Add(new ModuloTarjetaItem
                    {
                        Id = m.Id,
                        Titulo = m.MenuText ?? "",
                        Descripcion = cardMeta.Descripcion,
                        Url = targetUrl,
                        Icono = cardMeta.Icono,
                        ColorFondoIcono = cardMeta.ColorFondoIcono,
                        ColorIcono = cardMeta.ColorIcono,
                        SortOrder = m.SortOrder ?? 0,
                        EsSubModulo = isSubMenu,
                        CantidadOpciones = subOptionsCount
                    });
                }

                // Asegurar que la ficha de Estructura de Fondeo esté visible en el módulo Estructura Financiera
                if ((titulo.IndexOf("Estructura Financiera", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     titulo.IndexOf("Información Financiera", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     titulo.IndexOf("Informacion Financiera", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    tarjetas.All(t => t.Titulo.IndexOf("Fondeo", StringComparison.OrdinalIgnoreCase) < 0 &&
                                      t.Url.IndexOf("EstructuraFondeo", StringComparison.OrdinalIgnoreCase) < 0))
                {
                    var fondeoMeta = MenuCatalogService.GetCardMeta("Estructura de Fondeo", "EstructuraFondeo/Index", "fa fa-database", 1, "Analice la composición de las fuentes de fondeo de la entidad.");
                    var fondeoCard = new ModuloTarjetaItem
                    {
                        Id = 99210,
                        Titulo = "Estructura de Fondeo",
                        Descripcion = fondeoMeta.Descripcion,
                        Url = Url.Content("~/EstructuraFondeo/Index"),
                        Icono = fondeoMeta.Icono,
                        ColorFondoIcono = fondeoMeta.ColorFondoIcono,
                        ColorIcono = fondeoMeta.ColorIcono,
                        SortOrder = 2,
                        EsSubModulo = false,
                        CantidadOpciones = 0
                    };

                    int insertPos = Math.Min(1, tarjetas.Count);
                    tarjetas.Insert(insertPos, fondeoCard);
                }

                // Asegurar que Roles y Menú del Sistema estén visibles en Gestión Operativa para roles administrativos
                if ((rootId == 9200 || titulo.IndexOf("Operativa", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (roleId == 1 || roleId == 2 || roleId == 7 || Session["IsFGA"]?.ToString() == "1" || Env.GetUserInfo("roleid") == "1"))
                {
                    if (tarjetas.All(t => t.Url.IndexOf("Role", StringComparison.OrdinalIgnoreCase) < 0 && t.Titulo.IndexOf("Role", StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        var roleMeta = MenuCatalogService.GetCardMeta("Roles", "Role/Index", "fa fa-key", tarjetas.Count + 1, "Gestión de roles y configuración de la matriz de permisos de menú.");
                        tarjetas.Add(new ModuloTarjetaItem
                        {
                            Id = 9145,
                            Titulo = "Roles",
                            Descripcion = roleMeta.Descripcion,
                            Url = Url.Content("~/Role/Index"),
                            Icono = roleMeta.Icono,
                            ColorFondoIcono = roleMeta.ColorFondoIcono,
                            ColorIcono = roleMeta.ColorIcono,
                            SortOrder = 7,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        });
                    }

                    if (tarjetas.All(t => t.Url.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) < 0 && t.Titulo.IndexOf("Menú", StringComparison.OrdinalIgnoreCase) < 0 && t.Titulo.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        var menuMeta = MenuCatalogService.GetCardMeta("Menú del Sistema", "Menu/Index", "fa fa-sitemap", tarjetas.Count + 1, "Administración de opciones de menú, íconos y descripciones para las fichas del sistema.");
                        tarjetas.Add(new ModuloTarjetaItem
                        {
                            Id = 13001,
                            Titulo = "Menú del Sistema",
                            Descripcion = menuMeta.Descripcion,
                            Url = Url.Content("~/Menu/Index"),
                            Icono = menuMeta.Icono,
                            ColorFondoIcono = menuMeta.ColorFondoIcono,
                            ColorIcono = menuMeta.ColorIcono,
                            SortOrder = 8,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        });
                    }
                }

                // Requerimiento Fase II - Cartera de Crédito: 3 cajas (Riesgo de Crédito, Indicadores, Desempeño)
                if (titulo.IndexOf("Cartera", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var riesgoMeta = MenuCatalogService.GetCardMeta("Riesgo de Crédito", "Cartera14_21/Index", "fa fa-shield", 0, "Composición de mora, morosidad agrupada y análisis de riesgo según normativa 14-21.");
                    var indicMeta = MenuCatalogService.GetCardMeta("Indicadores", "Indicadores/FGA", "fa fa-line-chart", 1, "Indicadores normativos SUGEF y FFC de la cartera crediticia.");
                    var desempMeta = MenuCatalogService.GetCardMeta("Desempeño", "Composicion/Index", "fa fa-pie-chart", 2, "Evolución de la cartera total, variaciones mensuales/interanuales, activos y cuentas liquidadas.");

                    tarjetas = new List<ModuloTarjetaItem>
                    {
                        new ModuloTarjetaItem
                        {
                            Id = 4130,
                            Titulo = "Riesgo de Crédito",
                            Descripcion = riesgoMeta.Descripcion,
                            Url = Url.Content("~/Cartera14_21/Index"),
                            Icono = "fa fa-shield",
                            ColorFondoIcono = "#ffe4e6",
                            ColorIcono = "#e11d48",
                            SortOrder = 1,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 4120,
                            Titulo = "Indicadores",
                            Descripcion = indicMeta.Descripcion,
                            Url = Url.Content("~/Indicadores/FGA"),
                            Icono = "fa fa-line-chart",
                            ColorFondoIcono = "#e0e7ff",
                            ColorIcono = "#4338ca",
                            SortOrder = 2,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 4100,
                            Titulo = "Desempeño",
                            Descripcion = desempMeta.Descripcion,
                            Url = Url.Content("~/Composicion/Index"),
                            Icono = "fa fa-area-chart",
                            ColorFondoIcono = "#dcfce7",
                            ColorIcono = "#16a34a",
                            SortOrder = 3,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        }
                    };
                }

                // Requerimiento Fase II - Evaluación SBR: Únicamente Autoevaluación para todos los usuarios en el módulo raíz 6000
                if ((titulo.IndexOf("Evaluaci", StringComparison.OrdinalIgnoreCase) >= 0 || titulo.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0)
                    && (rootId == 6000 || rootId == 6001))
                {
                    tarjetas = new List<ModuloTarjetaItem>
                    {
                        new ModuloTarjetaItem
                        {
                            Id = 6001,
                            Titulo = "Autoevaluación",
                            Descripcion = "Diligencie la autoevaluación de Supervisión Basada en Riesgos acorde con el acuerdo SUGEF 24-22.",
                            Url = Url.Content("~/Evaluacion/Evaluacion"),
                            Icono = "fa fa-check-square-o",
                            ColorFondoIcono = "#e0f2fe",
                            ColorIcono = "#0284c7",
                            SortOrder = 1,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        }
                    };
                }

                // Requerimiento Fase II - Proyecciones: 3 cajas (Proyección de Estados Financieros, Proyecciones IRL, Simulación ISP)
                if (titulo.IndexOf("Proyecci", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tarjetas = new List<ModuloTarjetaItem>
                    {
                        new ModuloTarjetaItem
                        {
                            Id = 2260,
                            Titulo = "Proyección de Estados Financieros",
                            Descripcion = "Módulo de proyección de balances, estados de resultados e indicadores futuros.",
                            Url = Url.Content("~/ProyeccionEEFF/Index"),
                            Icono = "fa fa-line-chart",
                            ColorFondoIcono = "#e0e7ff",
                            ColorIcono = "#4338ca",
                            SortOrder = 1,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 5000,
                            Titulo = "Proyecciones IRL",
                            Descripcion = "Seguimiento y estimación del Indicador de Riesgo de Liquidez regulatorio.",
                            Url = Url.Content("~/IRL/IRL"),
                            Icono = "fa fa-tint",
                            ColorFondoIcono = "#e0f2fe",
                            ColorIcono = "#0284c7",
                            SortOrder = 2,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 6501,
                            Titulo = "Simulación ISP",
                            Descripcion = "Cálculo y simulaciones del Indicador de Suficiencia Patrimonial (ISP).",
                            Url = Url.Content("~/SimulacionCapital/Index"),
                            Icono = "fa fa-calculator",
                            ColorFondoIcono = "#fef3c7",
                            ColorIcono = "#d97706",
                            SortOrder = 3,
                            EsSubModulo = false,
                            CantidadOpciones = 0
                        }
                    };
                }

                // Requerimiento Fase II - Administración: Cajas diferenciadas para Cooperativas vs FFC
                if (titulo.IndexOf("Administra", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    titulo.IndexOf("Mantenimiento", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bool esFFC = (Session["IsFGA"]?.ToString() == "1") ||
                                 (Env.GetUserInfo("entidad") == Utility.Utilitarios.nombreEntidadAdmin) ||
                                 (roleId == 1 || roleId == 7);

                    if (!esFFC)
                    {
                        // Para usuarios de Cooperativas (Requerimiento FFC-112):
                        // Usuarios SAC (solo maestro), Contraseña, Carga de datos, Contribuciones, Informes
                        tarjetas = new List<ModuloTarjetaItem>
                        {
                            new ModuloTarjetaItem
                            {
                                Id = 8100,
                                Titulo = "Usuarios SAC",
                                Descripcion = "Gestión de usuarios y accesos autorizados de la cooperativa.",
                                Url = Url.Content("~/Usuario/Index"),
                                Icono = "fa fa-users",
                                ColorFondoIcono = "#e0e7ff",
                                ColorIcono = "#4338ca",
                                SortOrder = 1
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 8110,
                                Titulo = "Contraseña",
                                Descripcion = "Cambio y actualización de contraseña de acceso.",
                                Url = Url.Content("~/Usuario/ChangePassword"),
                                Icono = "fa fa-key",
                                ColorFondoIcono = "#fef3c7",
                                ColorIcono = "#d97706",
                                SortOrder = 2
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 7110,
                                Titulo = "Carga de Datos",
                                Descripcion = "Carga y transmisión de archivos XML hacia la plataforma SAC.",
                                Url = Url.Content("~/Archivo/Index"),
                                Icono = "fa fa-upload",
                                ColorFondoIcono = "#e0f2fe",
                                ColorIcono = "#0284c7",
                                SortOrder = 3
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 6502,
                                Titulo = "Contribuciones",
                                Descripcion = "Consulta de facturas y aportes de contribución de la entidad.",
                                Url = Url.Content("~/Facturacion/Index"),
                                Icono = "fa fa-dollar",
                                ColorFondoIcono = "#dcfce7",
                                ColorIcono = "#16a34a",
                                SortOrder = 4
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 11110,
                                Titulo = "Informes",
                                Descripcion = "Consulta y descarga de dictámenes e informes emitidos.",
                                Url = Url.Content("~/Explorer/Consulta"),
                                Icono = "fa fa-file-pdf-o",
                                ColorFondoIcono = "#ffe4e6",
                                ColorIcono = "#e11d48",
                                SortOrder = 5
                            }
                        };
                    }
                    else
                    {
                        // Para usuarios FFC: Opciones completas requeridas
                        // Perfiles, Evaluación SBR, Administración de Usuarios, Cambiar contraseña, Carga de Datos, Contribuciones, Informes, Notificaciones, Gestión operativa
                        tarjetas = new List<ModuloTarjetaItem>
                        {
                            new ModuloTarjetaItem
                            {
                                Id = 1,
                                Titulo = "Perfiles",
                                Descripcion = "Administración de perfiles y asignación de accesos al sistema.",
                                Url = Url.Content("~/Cierre/Perfiles"),
                                Icono = "fa fa-shield",
                                ColorFondoIcono = "#e0e7ff",
                                ColorIcono = "#4338ca",
                                SortOrder = 1
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 9300,
                                Titulo = "Evaluación SBR",
                                Descripcion = "Monitoreo del avance, bitácora histórica, resultados consolidados y configuración SBR 24-22.",
                                Url = Url.Action("Index", "Modulo", new { id = 9300 }),
                                Icono = "fa fa-tasks",
                                ColorFondoIcono = "#e0f2fe",
                                ColorIcono = "#0284c7",
                                SortOrder = 2,
                                EsSubModulo = true,
                                CantidadOpciones = 4
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 8100,
                                Titulo = "Administración de Usuarios",
                                Descripcion = "Gestión centralizada de cuentas de usuario, roles y estados.",
                                Url = Url.Content("~/Usuario/Index"),
                                Icono = "fa fa-users",
                                ColorFondoIcono = "#e0f2fe",
                                ColorIcono = "#0284c7",
                                SortOrder = 3
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 8110,
                                Titulo = "Cambiar Contraseña",
                                Descripcion = "Actualización de credenciales y políticas de acceso.",
                                Url = Url.Content("~/Usuario/ChangePassword"),
                                Icono = "fa fa-key",
                                ColorFondoIcono = "#fef3c7",
                                ColorIcono = "#d97706",
                                SortOrder = 4
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 7000,
                                Titulo = "Carga de Datos",
                                Descripcion = "Procesamiento de archivos XML, estatus de la carga y archivos cargados.",
                                Url = Url.Action("Index", "Modulo", new { id = 7000 }),
                                Icono = "fa fa-upload",
                                ColorFondoIcono = "#dcfce7",
                                ColorIcono = "#16a34a",
                                SortOrder = 5,
                                EsSubModulo = true,
                                CantidadOpciones = 4
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 6502,
                                Titulo = "Contribuciones",
                                Descripcion = "Cálculo, facturación y control de aportes de entidades afiliadas.",
                                Url = Url.Content("~/Facturacion/Index"),
                                Icono = "fa fa-dollar",
                                ColorFondoIcono = "#fae8ff",
                                ColorIcono = "#a855f7",
                                SortOrder = 6
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 11000,
                                Titulo = "Gestión de Informes",
                                Descripcion = "Carga de informes técnicos, notificaciones institucionales, tipos de informe y consulta de documentos.",
                                Url = Url.Action("Index", "Modulo", new { id = 11000 }),
                                Icono = "fa fa-file-pdf-o",
                                ColorFondoIcono = "#fee2e2",
                                ColorIcono = "#ef4444",
                                SortOrder = 7,
                                EsSubModulo = true,
                                CantidadOpciones = 2
                            },
                            new ModuloTarjetaItem
                            {
                                Id = 9200,
                                Titulo = "Gestión Operativa",
                                Descripcion = "Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.",
                                Url = Url.Action("Index", "Modulo", new { id = 9200 }),
                                Icono = "fa fa-cogs",
                                ColorFondoIcono = "#f1f5f9",
                                ColorIcono = "#475569",
                                SortOrder = 8,
                                EsSubModulo = true,
                                CantidadOpciones = 9
                            }
                        };
                    }
                }

                // Sub-módulo: Gestión Operativa (Id: 9200)
                if (rootId == 9200 || titulo.IndexOf("Operativa", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tarjetas = new List<ModuloTarjetaItem>
                    {
                        new ModuloTarjetaItem
                        {
                            Id = 9050,
                            Titulo = "Calendario",
                            Descripcion = "Cronograma operativo institucional y fechas de corte de información.",
                            Url = Url.Content("~/Calendario/Index"),
                            Icono = "fa fa-calendar",
                            ColorFondoIcono = "#e0f2fe",
                            ColorIcono = "#0284c7",
                            SortOrder = 1
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9110,
                            Titulo = "Catálogo de Fórmulas",
                            Descripcion = "Parametrización de fórmulas matemáticas para razones e indicadores financieros.",
                            Url = Url.Content("~/Formula/Index"),
                            Icono = "fa fa-calculator",
                            ColorFondoIcono = "#fef3c7",
                            ColorIcono = "#d97706",
                            SortOrder = 2
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9100,
                            Titulo = "Catálogo SUGEF",
                            Descripcion = "Catálogo oficial de cuentas y agrupaciones contables del regulador.",
                            Url = Url.Content("~/CatalogoCuenta/Index"),
                            Icono = "fa fa-book",
                            ColorFondoIcono = "#dcfce7",
                            ColorIcono = "#16a34a",
                            SortOrder = 3
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9120,
                            Titulo = "Tipos de XML",
                            Descripcion = "Esquemas y estructuras de archivos XML admitidos por la plataforma.",
                            Url = Url.Content("~/TipoXML/Index"),
                            Icono = "fa fa-code",
                            ColorFondoIcono = "#fae8ff",
                            ColorIcono = "#a855f7",
                            SortOrder = 4
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9130,
                            Titulo = "Entidades Afiliadas",
                            Descripcion = "Registro y mantenimiento del padrón de cooperativas y entidades afiliadas al FFC.",
                            Url = Url.Content("~/Entidad/Index"),
                            Icono = "fa fa-institution",
                            ColorFondoIcono = "#e0e7ff",
                            ColorIcono = "#4338ca",
                            SortOrder = 5
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9150,
                            Titulo = "Parámetros Globales",
                            Descripcion = "Variables del entorno, configuración de servidores de correo y parámetros SAC.",
                            Url = Url.Content("~/Parametros/Index"),
                            Icono = "fa fa-cogs",
                            ColorFondoIcono = "#f1f5f9",
                            ColorIcono = "#475569",
                            SortOrder = 6
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9145,
                            Titulo = "Roles",
                            Descripcion = "Gestión de roles y configuración de la matriz de permisos de menú.",
                            Url = Url.Content("~/Role/Index"),
                            Icono = "fa fa-lock",
                            ColorFondoIcono = "#fee2e2",
                            ColorIcono = "#ef4444",
                            SortOrder = 7
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 13001,
                            Titulo = "Menú del Sistema",
                            Descripcion = "Administración de opciones de menú, íconos y descripciones para las fichas del sistema.",
                            Url = Url.Content("~/Menu/Index"),
                            Icono = "fa fa-sitemap",
                            ColorFondoIcono = "#e0e7ff",
                            ColorIcono = "#4338ca",
                            SortOrder = 8
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9135,
                            Titulo = "Configuración de Preguntas SBR",
                            Descripcion = "Mantenimiento y configuración del catálogo de preguntas de la evaluación SBR.",
                            Url = Url.Content("~/Evaluacion/Index"),
                            Icono = "fa fa-question-circle",
                            ColorFondoIcono = "#e0f2fe",
                            ColorIcono = "#0284c7",
                            SortOrder = 9
                        }
                    };
                }

                // Sub-módulo: Evaluación SBR (Id: 9300) dentro de Administración
                if (rootId == 9300 || ((titulo.IndexOf("Evaluaci", StringComparison.OrdinalIgnoreCase) >= 0 || titulo.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0) && rootId != 6000 && rootId != 6001))
                {
                    tarjetas = new List<ModuloTarjetaItem>
                    {
                        new ModuloTarjetaItem
                        {
                            Id = 6002,
                            Titulo = "Avance SBR",
                            Descripcion = "Monitoreo del porcentaje de avance en el diligenciamiento de la evaluación SBR.",
                            Url = Url.Content("~/Evaluacion/Avance"),
                            Icono = "fa fa-line-chart",
                            ColorFondoIcono = "#e0f2fe",
                            ColorIcono = "#0284c7",
                            SortOrder = 1
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 6003,
                            Titulo = "Historial SBR",
                            Descripcion = "Bitácora histórica de evaluaciones cerradas y enviadas.",
                            Url = Url.Content("~/Evaluacion/Historial"),
                            Icono = "fa fa-history",
                            ColorFondoIcono = "#fef3c7",
                            ColorIcono = "#d97706",
                            SortOrder = 2
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 6005,
                            Titulo = "Resultados SBR",
                            Descripcion = "Resumen de calificaciones y nivel de cumplimiento por aspecto.",
                            Url = Url.Content("~/Evaluacion/Resultados"),
                            Icono = "fa fa-bar-chart",
                            ColorFondoIcono = "#dcfce7",
                            ColorIcono = "#16a34a",
                            SortOrder = 3
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 9135,
                            Titulo = "Configuración de Preguntas SBR",
                            Descripcion = "Mantenimiento y configuración del catálogo de preguntas de la evaluación SBR.",
                            Url = Url.Content("~/Evaluacion/Index"),
                            Icono = "fa fa-question-circle",
                            ColorFondoIcono = "#fae8ff",
                            ColorIcono = "#a855f7",
                            SortOrder = 4
                        }
                    };
                }

                // Sub-módulo: Carga de Datos (Id: 7000)
                if (rootId == 7000 || (titulo.IndexOf("Carga de Datos", StringComparison.OrdinalIgnoreCase) >= 0 && rootId != 9000))
                {
                    tarjetas = new List<ModuloTarjetaItem>
                    {
                        new ModuloTarjetaItem
                        {
                            Id = 7110,
                            Titulo = "Carga de Datos (XML)",
                            Descripcion = "Subida y procesamiento de archivos XML contables y financieros.",
                            Url = Url.Content("~/Archivo/Index"),
                            Icono = "fa fa-upload",
                            ColorFondoIcono = "#e0f2fe",
                            ColorIcono = "#0284c7",
                            SortOrder = 1
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 7100,
                            Titulo = "Estatus y Archivos Cargados",
                            Descripcion = "Monitoreo del avance de carga y detalle de archivos XML procesados.",
                            Url = Url.Content("~/Cierre/Index"),
                            Icono = "fa fa-check-circle",
                            ColorFondoIcono = "#dcfce7",
                            ColorIcono = "#16a34a",
                            SortOrder = 2
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 7150,
                            Titulo = "Monitor de Cierres",
                            Descripcion = "Panel de control y seguimiento de cierres de cooperativas.",
                            Url = Url.Content("~/Cierre/Monitor"),
                            Icono = "fa fa-desktop",
                            ColorFondoIcono = "#e0e7ff",
                            ColorIcono = "#4338ca",
                            SortOrder = 3
                        }
                    };
                }

                // Sub-módulo: Informes (Id: 11000)
                if (rootId == 11000 || (titulo.IndexOf("Informes", StringComparison.OrdinalIgnoreCase) >= 0 && rootId != 9000))
                {
                    tarjetas = new List<ModuloTarjetaItem>
                    {
                        new ModuloTarjetaItem
                        {
                            Id = 11100,
                            Titulo = "Carga y Emisión de Informes",
                            Descripcion = "Carga de informes, emisión de notificaciones a entidades y catálogo de tipos de informe.",
                            Url = Url.Content("~/Explorer/Informe"),
                            Icono = "fa fa-folder-open",
                            ColorFondoIcono = "#e0f2fe",
                            ColorIcono = "#0284c7",
                            SortOrder = 1
                        },
                        new ModuloTarjetaItem
                        {
                            Id = 11110,
                            Titulo = "Consulta de Informes",
                            Descripcion = "Consulta y descarga de reportes oficiales emitidos por la entidad.",
                            Url = Url.Content("~/Explorer/Consulta"),
                            Icono = "fa fa-file-pdf-o",
                            ColorFondoIcono = "#fee2e2",
                            ColorIcono = "#ef4444",
                            SortOrder = 2
                        }
                    };
                }

                // Asegurar que cualquier opción hija permitida en base de datos para este módulo (childPerms) que no esté en la lista predeterminada se incluya
                foreach (var child in childPerms)
                {
                    if (child.Menu_MenuId == null) continue;
                    var m = child.Menu_MenuId;
                    if (!tarjetas.Any(t => t.Id == m.Id || (!string.IsNullOrEmpty(t.Url) && !string.IsNullOrEmpty(m.MenuURL) && t.Url.IndexOf(m.MenuURL.Trim(), StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        var cardMeta = MenuCatalogService.GetCardMeta(m.MenuText, m.MenuURL, m.MenuIcon, tarjetas.Count, m.Description);
                        bool isSub = m.Id == 9300 || m.Id == 9200 || m.Id == 7000 || m.Id == 11000
                                     || string.Equals(m.MenuURL?.Trim(), "root", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(m.MenuURL?.Trim(), "filter", StringComparison.OrdinalIgnoreCase)
                                     || string.IsNullOrWhiteSpace(m.MenuURL)
                                     || m.MenuURL.Trim() == "#"
                                     || allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);

                        string target = isSub ? Url.Action("Index", "Modulo", new { id = m.Id }) : Url.Content("~/" + (m.MenuURL ?? "").Trim().TrimStart('~', '/'));
                        int subCount = allPermitted.Count(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);
                        tarjetas.Add(new ModuloTarjetaItem
                        {
                            Id = m.Id,
                            Titulo = m.MenuText ?? "",
                            Descripcion = cardMeta.Descripcion,
                            Url = target,
                            Icono = cardMeta.Icono,
                            ColorFondoIcono = cardMeta.ColorFondoIcono,
                            ColorIcono = cardMeta.ColorIcono,
                            SortOrder = m.SortOrder ?? 99,
                            EsSubModulo = isSub,
                            CantidadOpciones = subCount
                        });
                    }
                }

                // Sincronizar SortOrder con la base de datos (allPermitted) para que respete fielmente el orden configurado
                foreach (var t in tarjetas)
                {
                    var matchingPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && 
                        (p.Menu_MenuId.Id == t.Id || 
                         (!string.IsNullOrEmpty(t.Url) && !string.IsNullOrEmpty(p.Menu_MenuId.MenuURL) && t.Url.IndexOf(p.Menu_MenuId.MenuURL.Trim(), StringComparison.OrdinalIgnoreCase) >= 0) ||
                         string.Equals(p.Menu_MenuId.MenuText?.Trim(), t.Titulo?.Trim(), StringComparison.OrdinalIgnoreCase)));

                    if (matchingPerm != null && matchingPerm.Menu_MenuId != null && matchingPerm.Menu_MenuId.SortOrder.HasValue)
                    {
                        t.SortOrder = matchingPerm.Menu_MenuId.SortOrder.Value;
                    }
                }
                tarjetas = tarjetas.OrderBy(t => t.SortOrder).ThenBy(t => t.Titulo).ToList();

                string formattedPeriodo = "";
                try
                {
                    if (Session["Periodo2"] != null)
                    {
                        formattedPeriodo = Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()).ToString("MM/yyyy");
                    }
                    else if (ultimoPeriodoCargado.HasValue)
                    {
                        formattedPeriodo = ultimoPeriodoCargado.Value.ToString("MM/yyyy");
                    }
                }
                catch
                {
                    if (ultimoPeriodoCargado.HasValue)
                    {
                        formattedPeriodo = ultimoPeriodoCargado.Value.ToString("MM/yyyy");
                    }
                }

                bool esSbr = rootId == 9300 || titulo.IndexOf("SBR", StringComparison.OrdinalIgnoreCase) >= 0;

                // Si el módulo actual tiene un padre (es un sub-módulo), obtener info del padre
                int? parentMenuId = null;
                string parentMenuTitulo = "";
                if (rootPerm.Menu_MenuId.ParentId.HasValue && !esSbr)
                {
                    var parentPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == rootPerm.Menu_MenuId.ParentId.Value);
                    if (parentPerm != null && parentPerm.Menu_MenuId != null)
                    {
                        parentMenuId = parentPerm.Menu_MenuId.Id;
                        parentMenuTitulo = parentPerm.Menu_MenuId.MenuText ?? "";
                    }
                }

                string rootMenuUrl = rootPerm.Menu_MenuId.MenuURL?.Trim() ?? "";
                bool esAdministracion = rootId == 9000 ||
                                        titulo.IndexOf("Administra", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        titulo.IndexOf("Mantenimiento", StringComparison.OrdinalIgnoreCase) >= 0;

                bool mostrarFiltros = (!esAdministracion && string.Equals(rootMenuUrl, "filter", StringComparison.OrdinalIgnoreCase)) || esSbr;

                var model = new ModuloHubViewModel
                {
                    MenuId = rootId,
                    Titulo = titulo,
                    Subtitulo = moduloMeta.Subtitulo,
                    NotaPie = moduloMeta.NotaPie,
                    ParentMenuId = parentMenuId,
                    ParentMenuTitulo = parentMenuTitulo,
                    MenuURL = rootMenuUrl,
                    MostrarFiltros = mostrarFiltros,
                    IdEntidad = Session["IdEntidad"]?.ToString() ?? "",
                    PeriodoReferencia = formattedPeriodo,
                    TipoComparacion = Session["TipoComparacion"]?.ToString() ?? "Interanual",
                    Entidades = ViewBag.Entidades as SelectList,
                    Tarjetas = tarjetas
                };

                return View(model);
            }
        }

        [HttpPost]
        public JsonResult ActualizarFiltros(string idEntidad, string periodo, string tipoComparacion)
        {
            try
            {
                string nuevaFechaPeriodo = null;
                if (!string.IsNullOrEmpty(idEntidad))
                {
                    Session["IdEntidad"] = idEntidad;
                    using (var ent = new FGA_En_Linea.EntidadService.EntidadServiceClient())
                    {
                        var e = ent.Get(idEntidad);
                        if (e != null)
                        {
                            Session["NomEntidad"] = e.Nombre;
                        }
                    }

                    if (string.IsNullOrEmpty(periodo))
                    {
                        using (var sp = new FGA_En_Linea.SPService.SPClient())
                        {
                            var fechaCierre = sp.FGA_Consultar_FechaCierre(idEntidad);
                            var ultimoPeriodoCargado = fechaCierre.AddMonths(-1);
                            Session["Periodo"] = fechaCierre.ToShortDateString();
                            Session["Periodo2"] = ultimoPeriodoCargado.ToShortDateString();
                            nuevaFechaPeriodo = ultimoPeriodoCargado.ToString("MM/yyyy");
                            periodo = ultimoPeriodoCargado.ToShortDateString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(periodo))
                {
                    DateTime refDate = Utilitarios.ConvertirAFecha(periodo);
                    Session["Periodo2"] = refDate.ToShortDateString();

                    string tipo = tipoComparacion ?? Session["TipoComparacion"]?.ToString() ?? "Interanual";
                    Session["TipoComparacion"] = tipo;

                    if (tipo.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["Periodo1"] = refDate.AddMonths(-3).ToShortDateString();
                    }
                    else if (tipo.Equals("Interanual", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["Periodo1"] = refDate.AddYears(-1).ToShortDateString();
                    }
                    else if (tipo.Equals("Mensual", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["Periodo1"] = refDate.AddMonths(-1).ToShortDateString();
                    }
                    else
                    {
                        Session["Periodo1"] = refDate.AddYears(-1).ToShortDateString();
                    }
                }
                else if (!string.IsNullOrEmpty(tipoComparacion))
                {
                    Session["TipoComparacion"] = tipoComparacion;
                    if (Session["Periodo2"] != null)
                    {
                        DateTime refDate = Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString());
                        if (tipoComparacion.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
                        {
                            Session["Periodo1"] = refDate.AddMonths(-3).ToShortDateString();
                        }
                        else if (tipoComparacion.Equals("Mensual", StringComparison.OrdinalIgnoreCase))
                        {
                            Session["Periodo1"] = refDate.AddMonths(-1).ToShortDateString();
                        }
                        else
                        {
                            Session["Periodo1"] = refDate.AddYears(-1).ToShortDateString();
                        }
                    }
                }

                return Json(new { success = true, periodo = nuevaFechaPeriodo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetSearchMenuCatalog()
        {
            try
            {
                int roleId = 0;
                int.TryParse(Env.GetUserInfo("roleid"), out roleId);

                using (var men = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient())
                {
                    var allPermitted = (men.GetMenu(roleId) ?? new MenuPermission[0]).ToArray();

                    var list = new List<object>();

                    foreach (var p in allPermitted)
                    {
                        var m = p.Menu_MenuId;
                        if (m == null || string.IsNullOrWhiteSpace(m.MenuText)) continue;

                        bool hasChildren = allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);
                        bool isSubMenu = string.Equals(m.MenuURL?.Trim(), "root", StringComparison.OrdinalIgnoreCase)
                                         || string.Equals(m.MenuURL?.Trim(), "filter", StringComparison.OrdinalIgnoreCase)
                                         || string.IsNullOrWhiteSpace(m.MenuURL)
                                         || m.MenuURL.Trim() == "#"
                                         || hasChildren;

                        string targetUrl = "";
                        if (isSubMenu)
                        {
                            targetUrl = Url.Action("Index", "Modulo", new { id = m.Id });
                        }
                        else
                        {
                            string cleanUrl = m.MenuURL.Trim();
                            if (!cleanUrl.StartsWith("~") && !cleanUrl.StartsWith("/"))
                            {
                                cleanUrl = "~/" + cleanUrl;
                            }
                            targetUrl = Url.Content(cleanUrl);
                        }

                        string parentName = "";
                        if (m.ParentId.HasValue)
                        {
                            var parentPerm = allPermitted.FirstOrDefault(x => x.Menu_MenuId != null && x.Menu_MenuId.Id == m.ParentId.Value);
                            if (parentPerm != null && parentPerm.Menu_MenuId != null)
                            {
                                parentName = parentPerm.Menu_MenuId.MenuText ?? "";
                            }
                        }

                        var cardMeta = MenuCatalogService.GetCardMeta(m.MenuText, m.MenuURL, m.MenuIcon, m.Id, m.Description);

                        list.Add(new
                        {
                            id = m.Id,
                            title = m.MenuText,
                            url = targetUrl,
                            parent = parentName,
                            icon = cardMeta.Icono
                        });
                    }

                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        private static string NormalizarTexto(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Trim().ToLower()
                .Replace("ã³", "o").Replace("ã¡", "a").Replace("ã©", "e").Replace("ã­", "i").Replace("ãº", "u").Replace("ã±", "n")
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n");
        }
    }
}
