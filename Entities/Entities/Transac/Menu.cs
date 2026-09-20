using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Menu
    {
        [DisplayName("S.No")] 
        public int Id { get; set; }
        [Required]
        [StringLength(100)] 
        [DisplayName("Menu Text")] 
        public string MenuText { get; set; }
        [Required]
        [StringLength(400)] 
        [DisplayName("Menu U R L")] 
        public string MenuURL { get; set; }
        [DisplayName("Parent")] 
        public Nullable<int> ParentId { get; set; }
        public virtual Menu Menu2 { get; set; }
        [DisplayName("Sort Order")] 
        public Nullable<int> SortOrder { get; set; }
        [StringLength(100)] 
        [DisplayName("Menu Icon")] 
        public string MenuIcon { get; set; }
        [StringLength(2000)]
        [DisplayName("Descripción")]
        public string Description { get; set; }
        public virtual ICollection<MenuPermission> MenuPermission_MenuIds { get; set; }
        public virtual ICollection<Menu> Menu_ParentIds { get; set; }

        public virtual ICollection<SolicitudCambio> Solicitudes { get; set; }
    }
}
