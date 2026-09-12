using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Rpt_Graph
    {
        [Key()]
        public int Id { get; set; }

        public string Nombre { get; set; }

        public bool Ind_FGA { get; set; }

        public bool Ind_Sector { get; set; }

        public bool Ind_SF { get; set; }
    }
}
