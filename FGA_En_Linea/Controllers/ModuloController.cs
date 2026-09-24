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
        public ActionResult Index(int? id, string modulo = null, string IdEntidad = null, string Periodo = null, string TipoComparacion = "Mensual")
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
                var evaluacion = Env.GetUserInfo("evaluacion") == "S" ? -1 : Utilitarios.opcionEvaluacion;
                var allPermitted = men.GetMenu(roleId).Where(o => o.MenuId != evaluacion).ToArray();

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

                // Si rootPerm es una opción hoja (no tiene opciones hijas que mostrar en el Hub) pero tiene un ParentId,
                // ascender recursivamente en el árbol de menús hasta encontrar su módulo papá contenedor definido en la base de datos
                while (rootPerm != null && rootPerm.Menu_MenuId != null && rootPerm.Menu_MenuId.ParentId.HasValue
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

                int rootId = rootPerm.Menu_MenuId.Id;
                string titulo = rootPerm.Menu_MenuId.MenuText ?? "";
                var moduloMeta = MenuCatalogService.GetModuloMeta(titulo);

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

                    bool isSubMenu = string.Equals(m.MenuURL?.Trim(), "root", StringComparison.OrdinalIgnoreCase)
                                     || string.IsNullOrWhiteSpace(m.MenuURL)
                                     || m.MenuURL.Trim() == "#"
                                     || allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);

                    string targetUrl = "";
                    int subOptionsCount = 0;

                    if (isSubMenu)
                    {
                        targetUrl = Url.Action("Index", "Modulo", new { id = m.Id });
                        subOptionsCount = allPermitted.Count(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);
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

                // Si el módulo actual tiene un padre (es un sub-módulo), obtener info del padre
                int? parentMenuId = null;
                string parentMenuTitulo = "";
                if (rootPerm.Menu_MenuId.ParentId.HasValue)
                {
                    var parentPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == rootPerm.Menu_MenuId.ParentId.Value);
                    if (parentPerm != null && parentPerm.Menu_MenuId != null)
                    {
                        parentMenuId = parentPerm.Menu_MenuId.Id;
                        parentMenuTitulo = parentPerm.Menu_MenuId.MenuText ?? "";
                    }
                }

                string rootMenuUrl = rootPerm.Menu_MenuId.MenuURL?.Trim() ?? "";
                bool mostrarFiltros = string.Equals(rootMenuUrl, "filter", StringComparison.OrdinalIgnoreCase);

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
                    TipoComparacion = Session["TipoComparacion"]?.ToString() ?? "Mensual",
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
                    else
                    {
                        Session["Periodo1"] = refDate.AddMonths(-1).ToShortDateString();
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
                        else if (tipoComparacion.Equals("Interanual", StringComparison.OrdinalIgnoreCase))
                        {
                            Session["Periodo1"] = refDate.AddYears(-1).ToShortDateString();
                        }
                        else
                        {
                            Session["Periodo1"] = refDate.AddMonths(-1).ToShortDateString();
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
                    var evaluacion = Env.GetUserInfo("evaluacion") == "S" ? -1 : Utilitarios.opcionEvaluacion;
                    var allPermitted = men.GetMenu(roleId).Where(o => o.MenuId != evaluacion).ToArray();

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
