using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FGA.Models;
using FGA.Utility;

namespace FGA.Controllers
{
    public class MenuController : BaseController
    {
        // GET: Menu
        public ActionResult Index()
        {
            Load();
            return View();
        }

        // GET: Menu/GetGrid
        [HttpPost]
        public ActionResult GetGrid()
        {
            try
            {
                using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
                {
                    var allMenus = menuClient.GetAll() ?? new FGA.Models.Menu[0];
                    var menuDict = allMenus.ToDictionary(m => m.Id, m => m);

                    var result = allMenus
                        .OrderBy(m => m.ParentId.HasValue ? 1 : 0)
                        .ThenBy(m => m.ParentId)
                        .ThenBy(m => m.SortOrder ?? 0)
                        .ThenBy(m => m.MenuText)
                        .Select(m =>
                        {
                            string parentName = "Módulo Principal";
                            if (m.ParentId.HasValue && menuDict.TryGetValue(m.ParentId.Value, out var parentMenu))
                            {
                                parentName = parentMenu.MenuText ?? ("ID " + m.ParentId.Value);
                            }

                            bool isCustomIcon = !string.IsNullOrWhiteSpace(m.MenuIcon);
                            bool isCustomDesc = !string.IsNullOrWhiteSpace(m.Description);

                            string defaultIcon = MenuCatalogService.GetDefaultIcono(m.MenuText, m.MenuURL);
                            string defaultDesc = MenuCatalogService.GetDefaultDescripcion(m.MenuText, m.MenuURL);

                            string displayIcon = isCustomIcon ? MenuCatalogService.NormalizarClaseIcono(m.MenuIcon, defaultIcon) : defaultIcon;
                            string displayDesc = isCustomDesc ? m.Description.Trim() : defaultDesc;

                            return new
                            {
                                id = m.Id,
                                parentId = m.ParentId,
                                parentName = parentName,
                                menuText = m.MenuText ?? "",
                                menuUrl = m.MenuURL ?? "",
                                sortOrder = m.SortOrder ?? 0,
                                menuIcon = displayIcon,
                                rawIcon = m.MenuIcon ?? "",
                                isCustomIcon = isCustomIcon,
                                description = displayDesc,
                                rawDescription = m.Description ?? "",
                                isCustomDesc = isCustomDesc
                            };
                        }).ToList();

                    return Json(new { aaData = result, success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { aaData = new object[0], success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Menu/Get/5
        [HttpGet]
        public ActionResult Get(int id)
        {
            try
            {
                using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
                {
                    var m = menuClient.Get(id.ToString());
                    if (m == null)
                    {
                        return Json(new { success = false, message = "La opción no fue encontrada." }, JsonRequestBehavior.AllowGet);
                    }

                    string defaultIcon = MenuCatalogService.GetDefaultIcono(m.MenuText, m.MenuURL);
                    string defaultDesc = MenuCatalogService.GetDefaultDescripcion(m.MenuText, m.MenuURL);

                    return Json(new
                    {
                        success = true,
                        menu = new
                        {
                            id = m.Id,
                            menuText = m.MenuText,
                            menuUrl = m.MenuURL,
                            parentId = m.ParentId,
                            sortOrder = m.SortOrder ?? 0,
                            menuIcon = MenuCatalogService.NormalizarClaseIcono(m.MenuIcon, ""),
                            rawIcon = m.MenuIcon ?? "",
                            description = m.Description ?? "",
                            defaultIcon = defaultIcon,
                            defaultDescription = defaultDesc
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al consultar la opción: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Menu/GetParents
        [HttpGet]
        public ActionResult GetParents(int? excludeId = null)
        {
            try
            {
                using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
                {
                    var allMenus = menuClient.GetAll() ?? new FGA.Models.Menu[0];

                    // Si viene excludeId, calculamos todos sus descendientes para evitar ciclos o que sea su propio padre
                    var excludedIds = new HashSet<int>();
                    if (excludeId.HasValue && excludeId.Value > 0)
                    {
                        excludedIds.Add(excludeId.Value);
                        Action<int> collectDescendants = null;
                        collectDescendants = (parent) =>
                        {
                            foreach (var child in allMenus.Where(c => c.ParentId == parent))
                            {
                                if (excludedIds.Add(child.Id))
                                {
                                    collectDescendants(child.Id);
                                }
                            }
                        };
                        collectDescendants(excludeId.Value);
                    }

                    // Candidatos a padre: raíces o módulos/submódulos que sean contenedores
                    var parentCandidates = allMenus
                        .Where(m => !excludedIds.Contains(m.Id))
                        .Where(m => m.ParentId == null
                                 || allMenus.Any(c => c.ParentId == m.Id)
                                 || string.Equals(m.MenuURL, "root", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(m.MenuURL, "filter", StringComparison.OrdinalIgnoreCase)
                                 || m.MenuURL == "#")
                        .ToList();

                    var lookupByParent = parentCandidates.ToLookup(m => m.ParentId);
                    var result = new List<object>();

                    Action<int?, int> agregarConNivel = null;
                    agregarConNivel = (parentId, nivel) =>
                    {
                        var items = lookupByParent[parentId]
                            .OrderBy(m => m.SortOrder ?? 0)
                            .ThenBy(m => m.MenuText);

                        foreach (var item in items)
                        {
                            string prefijo = nivel == 0 ? "" : (new string(' ', (nivel - 1) * 4) + "└── ");
                            string sufijo = nivel == 0 ? " (Raíz)" : "";
                            result.Add(new
                            {
                                id = item.Id,
                                nombre = prefijo + (item.MenuText ?? "") + sufijo,
                                nivel = nivel
                            });
                            agregarConNivel(item.Id, nivel + 1);
                        }
                    };

                    agregarConNivel(null, 0);

                    // Candidatos restantes (huérfanos contenedores)
                    var agregadosIds = new HashSet<int>(result.Select(r => (int)((dynamic)r).id));
                    foreach (var rest in parentCandidates.Where(p => !agregadosIds.Contains(p.Id)))
                    {
                        result.Add(new
                        {
                            id = rest.Id,
                            nombre = rest.MenuText ?? "",
                            nivel = 1
                        });
                    }

                    return Json(new { success = true, parents = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Menu/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(Menu model)
        {
            try
            {
                if (model == null || model.Id <= 0)
                {
                    return Json(new { success = false, message = "Datos de opción de menú inválidos." });
                }

                if (string.IsNullOrWhiteSpace(model.MenuText))
                {
                    return Json(new { success = false, message = "El nombre de la opción es requerido." });
                }

                // Normalizar ParentId: si viene 0 o negativo, es null (módulo raíz)
                if (model.ParentId.HasValue && model.ParentId.Value <= 0)
                {
                    model.ParentId = null;
                }

                using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
                {
                    var existing = menuClient.Get(model.Id.ToString());
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "La opción seleccionada no existe en la base de datos." });
                    }

                    // Validación anti-ciclos: una opción no puede ser su propio padre ni tener como padre a un descendiente suyo
                    if (model.ParentId.HasValue)
                    {
                        if (model.ParentId.Value == model.Id)
                        {
                            return Json(new { success = false, message = "Una opción no puede ser su propio módulo padre." });
                        }

                        var allMenus = menuClient.GetAll() ?? new FGA.Models.Menu[0];
                        var currentCheck = allMenus.FirstOrDefault(m => m.Id == model.ParentId.Value);
                        while (currentCheck != null && currentCheck.ParentId.HasValue)
                        {
                            if (currentCheck.ParentId.Value == model.Id)
                            {
                                return Json(new { success = false, message = "No es posible asignar como padre a una opción que depende de este módulo (referencia circular)." });
                            }
                            currentCheck = allMenus.FirstOrDefault(m => m.Id == currentCheck.ParentId.Value);
                        }
                    }

                    existing.MenuText = model.MenuText.Trim();
                    
                    // Si el ícono viene vacío, guardamos null para que tome el valor por defecto.
                    // Si viene con valor, guardamos la etiqueta HTML completa (ej: "<i class=\"fa fa-bar-chart\"></i>") requerida por el menú.
                    if (string.IsNullOrWhiteSpace(model.MenuIcon))
                    {
                        existing.MenuIcon = null;
                    }
                    else
                    {
                        existing.MenuIcon = MenuCatalogService.FormatearHtmlIcono(model.MenuIcon, null);
                    }

                    // Si la descripción viene vacía, guardamos null para que tome el valor por defecto
                    existing.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

                    // Si cambió de módulo padre, asignar al final del nuevo contenedor
                    if (existing.ParentId != model.ParentId)
                    {
                        var allMenus = menuClient.GetAll() ?? new FGA.Models.Menu[0];
                        var newSiblings = allMenus.Where(m => m.ParentId == model.ParentId && m.Id != existing.Id).ToList();
                        int maxSort = newSiblings.Count > 0 ? (newSiblings.Max(s => s.SortOrder ?? 0)) : 0;
                        existing.SortOrder = maxSort + 1;
                        existing.ParentId = model.ParentId;
                    }

                    // Desvincular propiedades de navegación para evitar ciclos en la serialización WCF
                    existing.Menu2 = null;
                    existing.Menu_ParentIds = null;
                    existing.MenuPermission_MenuIds = null;
                    existing.Solicitudes = null;

                    menuClient.Update(existing);

                    // Limpiar caché de barras de menú de la aplicación
                    try
                    {
                        HttpRuntime.Cache.Remove("AllMenuBar");
                    }
                    catch { }

                    return Json(new { success = true, message = "Opción de menú actualizada con éxito." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar la opción: " + ex.Message });
            }
        }

        // GET: Menu/GetHierarchy
        [HttpGet]
        public ActionResult GetHierarchy()
        {
            try
            {
                using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
                {
                    var allMenus = menuClient.GetAll() ?? new FGA.Models.Menu[0];
                    var menuDict = allMenus.ToDictionary(m => m.Id, m => m);
                    var lookupByParent = allMenus.ToLookup(m => m.ParentId);

                    // Función local recursiva para serializar un nodo y sus hijos
                    Func<FGA.Models.Menu, object> serializarNodo = null;
                    serializarNodo = (m) =>
                    {
                        var hijosMenus = lookupByParent[m.Id]
                            .OrderBy(c => c.SortOrder ?? 0)
                            .ThenBy(c => c.MenuText)
                            .ToList();

                        bool isContainer = hijosMenus.Count > 0
                            || m.ParentId == null
                            || string.Equals(m.MenuURL, "root", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(m.MenuURL, "filter", StringComparison.OrdinalIgnoreCase)
                            || m.MenuURL == "#";

                        string defaultIcon = MenuCatalogService.GetDefaultIcono(m.MenuText, m.MenuURL);
                        string defaultDesc = MenuCatalogService.GetDefaultDescripcion(m.MenuText, m.MenuURL);
                        string displayIcon = !string.IsNullOrWhiteSpace(m.MenuIcon)
                            ? MenuCatalogService.NormalizarClaseIcono(m.MenuIcon, defaultIcon)
                            : defaultIcon;
                        string displayDesc = !string.IsNullOrWhiteSpace(m.Description)
                            ? m.Description.Trim()
                            : defaultDesc;

                        return new
                        {
                            id = m.Id,
                            parentId = m.ParentId,
                            menuText = m.MenuText ?? "",
                            menuUrl = m.MenuURL ?? "",
                            sortOrder = m.SortOrder ?? 0,
                            menuIcon = displayIcon,
                            description = displayDesc,
                            isContainer = isContainer,
                            isCustomIcon = !string.IsNullOrWhiteSpace(m.MenuIcon),
                            isCustomDesc = !string.IsNullOrWhiteSpace(m.Description),
                            children = hijosMenus.Select(serializarNodo).ToList()
                        };
                    };

                    // Módulos raíz (ParentId == null)
                    var roots = lookupByParent[null]
                        .OrderBy(m => m.SortOrder ?? 0)
                        .ThenBy(m => m.MenuText)
                        .Select(serializarNodo)
                        .ToList();

                    // Opciones huérfanas: únicamente aquellas que tienen un ParentId pero su padre NO existe en la base de datos
                    var unassigned = allMenus
                        .Where(m => m.ParentId.HasValue && !menuDict.ContainsKey(m.ParentId.Value))
                        .OrderBy(m => m.SortOrder ?? 0)
                        .ThenBy(m => m.MenuText)
                        .Select(serializarNodo)
                        .ToList();

                    return Json(new
                    {
                        success = true,
                        modules = roots,
                        unassigned = unassigned,
                        totalItems = allMenus.Length
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener la jerarquía del menú: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Menu/UpdateHierarchy
        [HttpPost]
        public ActionResult UpdateHierarchy(List<MenuHierarchyItemDto> items)
        {
            try
            {
                if (items == null || items.Count == 0)
                {
                    return Json(new { success = false, message = "No se recibieron elementos para actualizar." });
                }

                // Validación anti-ciclos directa
                foreach (var it in items)
                {
                    if (it.ParentId.HasValue && it.ParentId.Value == it.Id)
                    {
                        return Json(new { success = false, message = $"El elemento #{it.Id} no puede ser su propio padre." });
                    }
                }

                using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
                {
                    var allMenus = menuClient.GetAll() ?? new FGA.Models.Menu[0];
                    var menuDict = allMenus.ToDictionary(m => m.Id, m => m);

                    int updatedCount = 0;

                    foreach (var it in items)
                    {
                        if (menuDict.TryGetValue(it.Id, out var existing))
                        {
                            bool parentChanged = existing.ParentId != it.ParentId;
                            bool sortChanged = (existing.SortOrder ?? 0) != it.SortOrder;

                            if (parentChanged || sortChanged)
                            {
                                var updateEntity = new FGA.Models.Menu
                                {
                                    Id = existing.Id,
                                    MenuText = existing.MenuText,
                                    MenuURL = existing.MenuURL,
                                    ParentId = it.ParentId,
                                    SortOrder = it.SortOrder,
                                    MenuIcon = existing.MenuIcon,
                                    Description = existing.Description,
                                    Menu2 = null,
                                    Menu_ParentIds = null,
                                    MenuPermission_MenuIds = null,
                                    Solicitudes = null
                                };

                                menuClient.Update(updateEntity);
                                updatedCount++;
                            }
                        }
                    }

                    // Limpiar caché de barras de menú de la aplicación
                    try
                    {
                        HttpRuntime.Cache.Remove("AllMenuBar");
                    }
                    catch { }

                    return Json(new
                    {
                        success = true,
                        message = updatedCount > 0
                            ? $"Se actualizaron {updatedCount} opciones en la jerarquía del menú correctamente."
                            : "No se detectaron cambios pendientes por guardar.",
                        updatedCount = updatedCount
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar la jerarquía del menú: " + ex.Message });
            }
        }
    }

    public class MenuHierarchyItemDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
    }
}

