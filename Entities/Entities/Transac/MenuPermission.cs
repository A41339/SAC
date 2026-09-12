using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class MenuPermission
    {
        [DisplayName("S.No")]
        public int Id { get; set; }
        [DisplayName("Menu")]
        public int? MenuId { get; set; }
        public virtual Menu Menu_MenuId { get; set; }
        [Required]
        [DisplayName("Role")]
        public int? RoleId { get; set; }
        public virtual Role Role_RoleId { get; set; }
        [DisplayName("Sort Order")]
        public Nullable<int> SortOrder { get; set; }
        [Required]
        [DisplayName("Is Create")]
        public bool IsCreate { get; set; }
        [Required]
        [DisplayName("Is Read")]
        public bool IsRead { get; set; }
        [Required]
        [DisplayName("Is Update")]
        public bool IsUpdate { get; set; }
        [Required]
        [DisplayName("Is Delete")]
        public bool IsDelete { get; set; }
    }
}
