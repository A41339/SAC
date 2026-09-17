using System.Collections.Generic;

namespace FGA.Models
{
    public class RolPermisosViewModel
    {
        public int RolId { get; set; }
        public string RolNombre { get; set; } = "";
        public bool Activo { get; set; }
        public bool EsEntidad { get; set; }

        public List<RoleItemDto> ListaRoles { get; set; } = new List<RoleItemDto>();
        public List<ModuloPermisoGrupo> Modulos { get; set; } = new List<ModuloPermisoGrupo>();
    }

    public class RoleItemDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Activo { get; set; }
        public bool EsEntidad { get; set; }
    }

    public class ModuloPermisoGrupo
    {
        public int MenuId { get; set; }
        public string MenuText { get; set; } = "";
        public string MenuIcon { get; set; } = "";
        public string ColorFondo { get; set; } = "#e0f2fe";
        public string ColorIcono { get; set; } = "#0284c7";

        // Permiso del módulo raíz
        public int? PermissionId { get; set; }
        public bool IsRead { get; set; }
        public bool IsCreate { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }

        // Opciones hijas o sub-módulos pertenecientes a este módulo
        public List<ModuloPermisoItem> SubOpciones { get; set; } = new List<ModuloPermisoItem>();
    }

    public class ModuloPermisoItem
    {
        public int MenuId { get; set; }
        public string MenuText { get; set; } = "";
        public string MenuIcon { get; set; } = "";
        public string MenuURL { get; set; } = "";
        public int? ParentId { get; set; }
        public bool EsSubModulo { get; set; }

        public int? PermissionId { get; set; }
        public bool IsRead { get; set; }
        public bool IsCreate { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }

        public List<ModuloPermisoItem> Hijos { get; set; } = new List<ModuloPermisoItem>();
    }

    public class PermisoAsignadoDto
    {
        public int MenuId { get; set; }
        public bool IsRead { get; set; }
        public bool IsCreate { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
    }
}
