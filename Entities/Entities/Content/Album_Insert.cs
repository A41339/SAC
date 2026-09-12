using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

namespace FGA.Models
{
    [NotMapped]
    public class Album_Insert : Album
    {
        [NotMapped]
        public string FilesToBeUploaded { get; set; }
    }
}
