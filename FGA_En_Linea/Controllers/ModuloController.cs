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
                Session["IdEntidad"] = IdEntidad;
            }

            Load();

            if (!string.IsNullOrWhiteSpace(Periodo))
            {
                try
                {
                    DateTime refDate = Utilitarios.ConvertirAFecha(Periodo);
                    Session["Periodo"] = refDate.ToShortDateString();
                    Session["Periodo2"] = refDate.ToShortDateString();
                }
                catch { }
            }

            if (!string.IsNullOrWhiteSpace(TipoComparacion))
            {
                Session["TipoComparacion"] = TipoComparacion;
            }
            else if (Session["TipoComparacion"] == null)
            {
                Session["TipoComparacion"] = "Mensual";
            }

            // Sincronizar Periodo1 y Periodo2 según TipoComparacion
            try
            {
                string periodoActualStr = Session["Periodo"]?.ToString() ?? Session["Periodo2"]?.ToString();
                if (!string.IsNullOrEmpty(periodoActualStr))
                {
                    DateTime refDate = Utilitarios.ConvertirAFecha(periodoActualStr);
                    Session["Periodo2"] = refDate.ToShortDateString();

                    string tipo = Session["TipoComparacion"]?.ToString() ?? "Mensual";
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
                else if (!string.IsNullOrWhiteSpace(modulo))
                {
                    string modClean = modulo.Trim();

                    // 0. Si se envió un ID numérico en el parámetro modulo (ej. ?modulo=9000)
                    if (int.TryParse(modClean, out int parsedId))
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == parsedId);
                    }

                    // 1. Coincidencia exacta por nombre
                    if (rootPerm == null)
                    {
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuText != null &&
                            string.Equals(p.Menu_MenuId.MenuText.Trim(), modClean, StringComparison.OrdinalIgnoreCase));
                    }

                    // 2. Coincidencia normalizada sin tildes ni mayúsculas
                    if (rootPerm == null)
                    {
                        string modNorm = NormalizarTexto(modClean);
                        rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuText != null &&
                            (NormalizarTexto(p.Menu_MenuId.MenuText) == modNorm ||
                             NormalizarTexto(p.Menu_MenuId.MenuText).Contains(modNorm) ||
                             modNorm.Contains(NormalizarTexto(p.Menu_MenuId.MenuText))));

                        // 3. Mapeo de alias comunes hacia su módulo contenedor (Mantenimientos 9000 / Administración)
                        if (rootPerm == null)
                        {
                            if (modNorm == "seguridad" || modNorm == "roles" || modNorm == "usuarios" ||
                                modNorm == "evaluacion" || modNorm == "evaluaciones" ||
                                modNorm.Contains("administra") || modNorm.Contains("mantenimiento"))
                            {
                                // Prioridad 1: Módulo 9000 (Mantenimientos)
                                rootPerm = allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.Id == 9000)
                                           ?? allPermitted.FirstOrDefault(p => p.Menu_MenuId != null && p.Menu_MenuId.MenuText != null &&
                                               (NormalizarTexto(p.Menu_MenuId.MenuText).Contains("mantenimiento") || NormalizarTexto(p.Menu_MenuId.MenuText).Contains("administra")));
                            }
                        }
                    }
                }

                // Si rootPerm es una opción hoja (no tiene opciones hijas que mostrar en el Hub) pero tiene un ParentId,
                // ascender recursivamente en el árbol de menús hasta encontrar su módulo papá contenedor (ej. Roles o Evaluación -> Administración)
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
                    .OrderBy(p => p.SortOrder ?? p.Menu_MenuId.SortOrder ?? 0)
                    .ToList();

                var tarjetas = new List<ModuloTarjetaItem>();
                int cardIndex = 0;
                foreach (var child in childPerms)
                {
                    var m = child.Menu_MenuId;
                    var cardMeta = MenuCatalogService.GetCardMeta(m.MenuText, m.MenuURL, m.MenuIcon, cardIndex++);

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
                        SortOrder = child.SortOrder ?? m.SortOrder ?? 0,
                        EsSubModulo = isSubMenu,
                        CantidadOpciones = subOptionsCount
                    });
                }

                string formattedPeriodo = "";
                try
                {
                    if (Session["Periodo"] != null)
                    {
                        formattedPeriodo = Utilitarios.ConvertirAFecha(Session["Periodo"].ToString()).ToString("MM/yyyy");
                    }
                }
                catch
                {
                    formattedPeriodo = DateTime.Now.ToString("MM/yyyy");
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

                var model = new ModuloHubViewModel
                {
                    MenuId = rootId,
                    Titulo = titulo,
                    Subtitulo = moduloMeta.Subtitulo,
                    NotaPie = moduloMeta.NotaPie,
                    ParentMenuId = parentMenuId,
                    ParentMenuTitulo = parentMenuTitulo,
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
                            Session["Periodo"] = fechaCierre.ToShortDateString();
                            nuevaFechaPeriodo = fechaCierre.ToString("MM/yyyy");
                            periodo = fechaCierre.ToShortDateString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(periodo))
                {
                    DateTime refDate = Utilitarios.ConvertirAFecha(periodo);
                    Session["Periodo"] = refDate.ToShortDateString();
                    Session["Periodo2"] = refDate.ToShortDateString();

                    string tipo = tipoComparacion ?? Session["TipoComparacion"]?.ToString() ?? "Mensual";
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
