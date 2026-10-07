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

            var allPermitted = Env.GetRoleMenuPermissions(roleId).ToList();

                MenuPermission rootPerm = null;
                if (id.HasValue)
                {
                    rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == id.Value);
                }
                
                if (rootPerm == null && !string.IsNullOrWhiteSpace(modulo))
                {
                    string modClean = modulo.Trim();

                    // 0. Coincidencia por ID numérico (ej. ?modulo=9000 o ?modulo=3000)
                    if (int.TryParse(modClean, out int parsedId))
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == parsedId);
                    }

                    // 1. Coincidencia por alias históricos de módulos
                    if (modClean.IndexOf("Accesos", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == 9000);
                    }
                    else if (modClean.IndexOf("Mantenimiento", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == 9200)
                                ?? allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == 9000);
                    }

                    // 2. Coincidencia exacta por nombre de menú (MenuText)
                    if (rootPerm == null)
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuText != null &&
                            string.Equals(p.Menu_MenuId.MenuText.Trim(), modClean, StringComparison.OrdinalIgnoreCase));
                    }

                    // 3. Coincidencia normalizada sin tildes ni mayúsculas por nombre de menú
                    if (rootPerm == null)
                    {
                        string modNorm = NormalizarTexto(modClean);
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuText != null &&
                            (NormalizarTexto(p.Menu_MenuId.MenuText) == modNorm ||
                             NormalizarTexto(p.Menu_MenuId.MenuText).Contains(modNorm) ||
                             modNorm.Contains(NormalizarTexto(p.Menu_MenuId.MenuText))));
                    }

                    // 4. Coincidencia por MenuURL (ej. "InformeFinanciero/Index", "Calendario/Index", etc.)
                    // Si una vista hija pasa su URL de acción como parámetro modulo, resolvemos a su módulo padre contenedor
                    if (rootPerm == null)
                    {
                        string cleanPath = modClean.Trim().Trim('/');
                        var leafPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuURL != null &&
                            (string.Equals(p.Menu_MenuId.MenuURL.Trim().Trim('/'), cleanPath, StringComparison.OrdinalIgnoreCase) ||
                             p.Menu_MenuId.MenuURL.Trim().Trim('/').StartsWith(cleanPath + "/", StringComparison.OrdinalIgnoreCase) ||
                             cleanPath.StartsWith(p.Menu_MenuId.MenuURL.Trim().Trim('/') + "/", StringComparison.OrdinalIgnoreCase)));

                        if (leafPerm != null && leafPerm.Menu_MenuId != null)
                        {
                            string leafUrl = (leafPerm.Menu_MenuId.MenuURL ?? "").Trim();
                            bool isHub = leafUrl.Equals("root", StringComparison.OrdinalIgnoreCase) ||
                                         leafUrl.Equals("filter", StringComparison.OrdinalIgnoreCase) ||
                                         leafUrl == "#";

                            if (!isHub && leafPerm.Menu_MenuId.ParentId.HasValue)
                            {
                                rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == leafPerm.Menu_MenuId.ParentId.Value) ?? leafPerm;
                            }
                            else
                            {
                                rootPerm = leafPerm;
                            }
                        }
                    }
                }

                // Soporte explícito para sub-módulos hijos de Administración (Gestión Operativa, Evaluación SBR, Carga de Datos, Informes)
                // Si rootPerm es una opción hoja pero tiene un ParentId,
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
                string rootUrl = (rootPerm.Menu_MenuId.MenuURL ?? "").Trim();
                bool tieneHijos = allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == rootId);

                if (!tieneHijos && !string.IsNullOrEmpty(rootUrl) && !rootUrl.Equals("root", StringComparison.OrdinalIgnoreCase) && !rootUrl.Equals("filter", StringComparison.OrdinalIgnoreCase) && rootUrl != "#")
                {
                    return Redirect("~/" + rootUrl.TrimStart('~', '/'));
                }

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
                                     || string.Equals(m.MenuURL?.Trim(), "filter", StringComparison.OrdinalIgnoreCase)
                                     || string.IsNullOrWhiteSpace(m.MenuURL)
                                     || m.MenuURL.Trim() == "#"
                                     || allPermitted.Any(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);

                    string targetUrl = "";
                    int subOptionsCount = 0;

                    if (isSubMenu)
                    {
                        targetUrl = Url.Action("Index", "Modulo", new { id = m.Id });
                        subOptionsCount = allPermitted.Count(c => c.Menu_MenuId != null && c.Menu_MenuId.ParentId == m.Id);

                        // Si es un contenedor de tipo raíz/filtro sin URL ejecutable y no tiene hijas permitidas, no mostrar tarjeta vacía
                        bool sinUrlEjecutable = string.IsNullOrWhiteSpace(m.MenuURL) ||
                                                string.Equals(m.MenuURL.Trim(), "root", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(m.MenuURL.Trim(), "filter", StringComparison.OrdinalIgnoreCase) ||
                                                m.MenuURL.Trim() == "#";

                        if (subOptionsCount == 0 && sinUrlEjecutable)
                        {
                            continue;
                        }
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

                string idEntidadSesion = Session["IdEntidad"]?.ToString() ?? "";
                string nomEntidadSesion = Session["NomEntidad"]?.ToString() ?? "";
                var entidadesSelectList = ViewBag.Entidades as SelectList;

                if (string.IsNullOrEmpty(nomEntidadSesion) && entidadesSelectList != null)
                {
                    var matching = entidadesSelectList.FirstOrDefault(x => x.Value != null && (x.Value.Trim() == idEntidadSesion.Trim() || (int.TryParse(x.Value, out int v1) && int.TryParse(idEntidadSesion, out int v2) && v1 == v2)))
                                ?? entidadesSelectList.FirstOrDefault(x => x.Selected);
                    if (matching != null)
                    {
                        nomEntidadSesion = matching.Text;
                    }
                }

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
                    IdEntidad = idEntidadSesion,
                    NomEntidad = nomEntidadSesion,
                    PeriodoReferencia = formattedPeriodo,
                    TipoComparacion = Session["TipoComparacion"]?.ToString() ?? "Interanual",
                    Entidades = entidadesSelectList,
                    Tarjetas = tarjetas
                };

                return View(model);
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

                var allPermitted = Env.GetRoleMenuPermissions(roleId);

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
