using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;
using FGA.Utility;

namespace FGA.Controllers
{
    public class RoleController : BaseController
    {
        private readonly FGA_En_Linea.RoleService.ServiceOf_RoleClient db = new FGA_En_Linea.RoleService.ServiceOf_RoleClient();

        public ActionResult Index()
        {
            Load();
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = db.GetAll().OrderBy(r => r.Nombre);
                var result = from c in tak
                             select new string[] {
                                 Convert.ToString(c.Id),
                                 Convert.ToString(c.Nombre),
                                 c.EsEntidad ? "1" : "0",
                                 c.Activo ? "1" : "0",
                                 Convert.ToString(c.Id)
                             };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { aaData = new string[0][], error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetRole(int id)
        {
            try
            {
                var r = db.Get(id.ToString());
                if (r == null)
                {
                    return Json(new { success = false, message = "El rol solicitado no existe." }, JsonRequestBehavior.AllowGet);
                }
                return Json(new
                {
                    success = true,
                    role = new
                    {
                        id = r.Id,
                        nombre = r.Nombre,
                        activo = r.Activo,
                        esEntidad = r.EsEntidad
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener rol: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GuardarRol(int? id, string nombre, bool activo, bool esEntidad)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return Json(new { success = false, message = "El nombre del rol es obligatorio." });
                }

                nombre = nombre.Trim();
                if (nombre.Length < 3 || nombre.Length > 100)
                {
                    return Json(new { success = false, message = "El nombre del rol debe tener entre 3 y 100 caracteres." });
                }

                var allRoles = db.GetAll();
                bool duplicate = allRoles.Any(r => r.Nombre.Trim().Equals(nombre, StringComparison.OrdinalIgnoreCase) && (!id.HasValue || r.Id != id.Value));
                if (duplicate)
                {
                    return Json(new { success = false, message = "Ya existe un rol con el nombre '" + nombre + "'." });
                }

                if (!id.HasValue || id.Value == 0)
                {
                    var nuevo = new Role
                    {
                        Nombre = nombre,
                        Activo = activo,
                        EsEntidad = esEntidad,
                        Url = ""
                    };
                    db.Add(ref nuevo);
                    return Json(new { success = true, message = "Rol creado exitosamente.", rolId = nuevo.Id });
                }
                else
                {
                    var existing = db.Get(id.Value.ToString());
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "El rol que intenta modificar no existe." });
                    }

                    existing.Nombre = nombre;
                    existing.Activo = activo;
                    existing.EsEntidad = esEntidad;
                    db.Update(existing);
                    return Json(new { success = true, message = "Rol actualizado exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar el rol: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarRol(int id)
        {
            try
            {
                // 1. Proteger roles críticos del sistema
                int directorFga = int.Parse(Utilitarios.roleDirectorFGA);
                int adminEntidad = int.Parse(Utilitarios.roleAdminEntidad);
                int maestroEntidad = int.Parse(Utilitarios.roleMaestroEntidad);

                if (id == directorFga || id == adminEntidad || id == maestroEntidad)
                {
                    return Json(new { success = false, message = "No es posible eliminar este rol porque es un rol crítico protegido del sistema." });
                }

                // 2. Validar si existen usuarios activos asignados a este rol
                using (var usrClient = new FGA_En_Linea.UsuarioService.UsuarioServiceClient())
                {
                    var users = usrClient.GetAll();
                    if (users != null && users.Any(u => u.Role_Usuario_Id == id && u.Estado_Usuario_Id != Utilitarios.estadoBorrado))
                    {
                        return Json(new { success = false, message = "No se puede eliminar el rol porque tiene usuarios activos asignados. Reasigne los usuarios antes de eliminarlo." });
                    }
                }

                // 3. Limpiar permisos asociados a este rol
                using (var menPermClient = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient())
                {
                    var perms = menPermClient.GetMenu(id);
                    if (perms != null)
                    {
                        foreach (var p in perms)
                        {
                            try { menPermClient.Delete(p.Id.ToString()); } catch { }
                        }
                    }
                }

                // 4. Eliminar el rol
                db.Delete(id.ToString());

                // 5. Invalidar caché global de menús
                HttpRuntime.Cache.Remove("AllMenuBar");

                return Json(new { success = true, message = "Rol eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el rol: " + ex.Message });
            }
        }

        public ActionResult Permisos(int? id)
        {
            Load();

            var allRoles = db.GetAll().OrderBy(r => r.Nombre).ToList();
            if (allRoles.Count == 0)
            {
                return RedirectToAction("Index");
            }

            if (!id.HasValue || id.Value <= 0)
            {
                id = allRoles.First().Id;
            }

            var currentRole = allRoles.FirstOrDefault(r => r.Id == id.Value);
            if (currentRole == null)
            {
                return HttpNotFound();
            }

            var viewModel = new RolPermisosViewModel
            {
                RolId = currentRole.Id,
                RolNombre = currentRole.Nombre,
                Activo = currentRole.Activo,
                EsEntidad = currentRole.EsEntidad,
                ListaRoles = allRoles.Select(r => new RoleItemDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Activo = r.Activo,
                    EsEntidad = r.EsEntidad
                }).ToList()
            };

            // Obtener catálogo completo de menús del sistema
            FGA.Models.Menu[] allMenus;
            using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
            {
                allMenus = menuClient.GetAll();
            }

            // Excluir opción de autoevaluación si aplica
            allMenus = allMenus.Where(m => m.Id != Utilitarios.opcionEvaluacion).ToArray();

            // Obtener permisos configurados para este rol
            FGA.Models.MenuPermission[] rolePerms;
            using (var menPermClient = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient())
            {
                rolePerms = menPermClient.GetMenu(currentRole.Id) ?? new FGA.Models.MenuPermission[0];
            }

            var permDict = rolePerms.Where(p => p.MenuId.HasValue).ToDictionary(p => p.MenuId.Value, p => p);

            // Obtener raíces
            var rootMenus = allMenus.Where(m => m.ParentId == null).OrderBy(m => m.SortOrder ?? 0).ThenBy(m => m.MenuText).ToList();

            int rootIndex = 0;
            foreach (var root in rootMenus)
            {
                var meta = MenuCatalogService.GetCardMeta(root.MenuText, root.MenuURL, root.MenuIcon, rootIndex++);
                permDict.TryGetValue(root.Id, out var rootPerm);
                bool rootAsignado = rootPerm != null;

                var grupo = new ModuloPermisoGrupo
                {
                    MenuId = root.Id,
                    MenuText = root.MenuText,
                    MenuIcon = !string.IsNullOrEmpty(meta.Icono) ? meta.Icono : "fa fa-folder",
                    ColorFondo = meta.ColorFondoIcono,
                    ColorIcono = meta.ColorIcono,
                    PermissionId = rootPerm?.Id,
                    Asignado = rootAsignado,
                    IsRead = rootAsignado,
                    IsCreate = rootPerm?.IsCreate ?? false,
                    IsUpdate = rootPerm?.IsUpdate ?? false,
                    IsDelete = rootPerm?.IsDelete ?? false
                };

                // Hijos directos de la raíz
                var directChildren = allMenus.Where(m => m.ParentId == root.Id).OrderBy(m => m.SortOrder ?? 0).ThenBy(m => m.MenuText).ToList();

                int childIndex = 0;
                foreach (var child in directChildren)
                {
                    permDict.TryGetValue(child.Id, out var childPerm);
                    var childMeta = MenuCatalogService.GetCardMeta(child.MenuText, child.MenuURL, child.MenuIcon, childIndex++);
                    bool childAsignado = childPerm != null;

                    bool esSubModulo = string.Equals(child.MenuURL, "root", StringComparison.OrdinalIgnoreCase);

                    var childItem = new ModuloPermisoItem
                    {
                        MenuId = child.Id,
                        MenuText = child.MenuText,
                        MenuIcon = !string.IsNullOrEmpty(childMeta.Icono) ? childMeta.Icono : (esSubModulo ? "fa fa-folder-open-o" : "fa fa-file-text-o"),
                        MenuURL = child.MenuURL,
                        ParentId = child.ParentId,
                        EsSubModulo = esSubModulo,
                        PermissionId = childPerm?.Id,
                        Asignado = childAsignado,
                        IsRead = childAsignado,
                        IsCreate = childPerm?.IsCreate ?? false,
                        IsUpdate = childPerm?.IsUpdate ?? false,
                        IsDelete = childPerm?.IsDelete ?? false
                    };

                    // Si es un sub-módulo, buscar sus hijos
                    if (esSubModulo)
                    {
                        var grandChildren = allMenus.Where(m => m.ParentId == child.Id).OrderBy(m => m.SortOrder ?? 0).ThenBy(m => m.MenuText).ToList();
                        int gcIndex = 0;
                        foreach (var gc in grandChildren)
                        {
                            permDict.TryGetValue(gc.Id, out var gcPerm);
                            var gcMeta = MenuCatalogService.GetCardMeta(gc.MenuText, gc.MenuURL, gc.MenuIcon, gcIndex++);
                            bool gcAsignado = gcPerm != null;

                            childItem.Hijos.Add(new ModuloPermisoItem
                            {
                                MenuId = gc.Id,
                                MenuText = gc.MenuText,
                                MenuIcon = !string.IsNullOrEmpty(gcMeta.Icono) ? gcMeta.Icono : "fa fa-file-text-o",
                                MenuURL = gc.MenuURL,
                                ParentId = gc.ParentId,
                                EsSubModulo = false,
                                PermissionId = gcPerm?.Id,
                                Asignado = gcAsignado,
                                IsRead = gcAsignado,
                                IsCreate = gcPerm?.IsCreate ?? false,
                                IsUpdate = gcPerm?.IsUpdate ?? false,
                                IsDelete = gcPerm?.IsDelete ?? false
                            });
                        }
                    }

                    grupo.SubOpciones.Add(childItem);
                }

                viewModel.Modulos.Add(grupo);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult GuardarPermisos(int rolId, List<PermisoAsignadoDto> permisos)
        {
            try
            {
                if (rolId <= 0)
                {
                    return Json(new { success = false, message = "Identificador de rol inválido." });
                }

                var role = db.Get(rolId.ToString());
                if (role == null)
                {
                    return Json(new { success = false, message = "El rol especificado no existe." });
                }

                if (permisos == null)
                {
                    permisos = new List<PermisoAsignadoDto>();
                }

                // Obtener todos los menús para resolver jerarquías padre-hijo
                FGA.Models.Menu[] allMenus;
                using (var menuClient = new FGA_En_Linea.MenuService.ServiceOf_MenuClient())
                {
                    allMenus = menuClient.GetAll();
                }
                var menuMap = allMenus.ToDictionary(m => m.Id);

                // Mapear permisos recibidos
                var dictPermisos = new Dictionary<int, PermisoAsignadoDto>();
                foreach (var p in permisos)
                {
                    dictPermisos[p.MenuId] = p;
                }

                // AUTO-PROPAGACIÓN: Si cualquier hijo está asignado, su contenedor padre DEBE estar incluido en MenuPermission
                var itemsConPermiso = dictPermisos.Values.Where(p => p.Asignado || p.IsRead).ToList();
                foreach (var item in itemsConPermiso)
                {
                    if (menuMap.TryGetValue(item.MenuId, out var currentMenu))
                    {
                        var parentId = currentMenu.ParentId;
                        while (parentId.HasValue && menuMap.ContainsKey(parentId.Value))
                        {
                            if (!dictPermisos.TryGetValue(parentId.Value, out var parentPerm))
                            {
                                parentPerm = new PermisoAsignadoDto
                                {
                                    MenuId = parentId.Value,
                                    Asignado = true,
                                    IsRead = true
                                };
                                dictPermisos[parentId.Value] = parentPerm;
                            }
                            else
                            {
                                parentPerm.Asignado = true;
                                parentPerm.IsRead = true;
                            }
                            parentId = menuMap[parentId.Value].ParentId;
                        }
                    }
                }

                // Obtener permisos existentes en BD para este rol
                using (var menPermClient = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient())
                {
                    var existingPerms = (menPermClient.GetMenu(rolId) ?? new FGA.Models.MenuPermission[0]).ToList();
                    var existingDict = existingPerms.Where(e => e.MenuId.HasValue).ToDictionary(e => e.MenuId.Value);

                    // Sincronizar cada menú del sistema: lo que da acceso es la existencia del registro en MenuPermission
                    foreach (var m in allMenus)
                    {
                        bool hasDto = dictPermisos.TryGetValue(m.Id, out var dto);
                        bool isIncluded = hasDto && (dto.Asignado || dto.IsRead);
                        existingDict.TryGetValue(m.Id, out var existing);

                        if (isIncluded)
                        {
                            if (existing != null)
                            {
                                var updateEntity = new FGA.Models.MenuPermission
                                {
                                    Id = existing.Id,
                                    RoleId = rolId,
                                    MenuId = m.Id,
                                    IsRead = true,
                                    IsCreate = true,
                                    IsUpdate = true,
                                    IsDelete = true,
                                    SortOrder = m.SortOrder ?? 0
                                };
                                menPermClient.Update(updateEntity);
                            }
                            else
                            {
                                var newEntity = new FGA.Models.MenuPermission
                                {
                                    RoleId = rolId,
                                    MenuId = m.Id,
                                    IsRead = true,
                                    IsCreate = true,
                                    IsUpdate = true,
                                    IsDelete = true,
                                    SortOrder = m.SortOrder ?? 0
                                };
                                menPermClient.Add(ref newEntity);
                            }
                        }
                        else
                        {
                            // Si no está incluido y existía en BD, eliminarlo
                            if (existing != null)
                            {
                                menPermClient.Delete(existing.Id.ToString());
                            }
                        }
                    }
                }

                // Invalidar caché global de menús de BaseController en tiempo real
                HttpRuntime.Cache.Remove("AllMenuBar");

                return Json(new { success = true, message = "Permisos guardados y sincronizados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar permisos: " + ex.Message });
            }
        }

        public ActionResult ModelBindIndex()
        {
            return View();
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role ObjRole = db.Get(id.ToString());
            if (ObjRole == null)
            {
                return HttpNotFound();
            }
            return View(ObjRole);
        }

        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Role ObjRole)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Add(ref ObjRole);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                    {
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                        {
                            sb.Append(err.ErrorMessage + "<br/>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());

        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role ObjRole = db.Get(id.ToString());
            if (ObjRole == null)
            {
                return HttpNotFound();
            }

            return View(ObjRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Role ObjRole)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Update(ObjRole);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                    {
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                        {
                            sb.Append(err.ErrorMessage + "<br/>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());

        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role ObjRole = db.Get(id.ToString());
            if (ObjRole == null)
            {
                return HttpNotFound();
            }
            return View(ObjRole);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                db.Delete(id.ToString());
                sb.Append("Sumitted");
                return Content(sb.ToString());

            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());

        }

        public ActionResult MultiViewIndex(int? id)
        {
            Role ObjRole = db.Get(id.ToString());
            ViewBag.IsWorking = 0;
            if (id > 0)
            {
                ViewBag.IsWorking = id;

            }

            return View(ObjRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult MultiViewIndex(Role ObjRole)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Update(ObjRole);
                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                    {
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                        {
                            sb.Append(err.ErrorMessage + "<br/>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Close();
            }
            base.Dispose(disposing);
        }
    }
}

