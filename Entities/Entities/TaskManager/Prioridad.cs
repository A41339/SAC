using System.Collections.Generic;
using System.Linq;

namespace FGA.Models
{

    public class ItemPrioridad
    {

        public ItemPrioridad(int vId, string vNombre)
        {
            Id = vId;
            Nombre = vNombre;
        }

        public int Id { get; set; }
        public string Nombre { get; set; }

    }

    public static class Prioridad
    {
        public static List<ItemPrioridad> Init()
        {
            List<ItemPrioridad> list = new List<ItemPrioridad>();
            list.Add(new ItemPrioridad(1, "Baja"));
            list.Add(new ItemPrioridad(2, "Media"));
            list.Add(new ItemPrioridad(3, "Alta"));

            return list;
        }
        public static string GetNombre(int estado)
        {
            return Init().FirstOrDefault(o => o.Id == estado).Nombre;
        }
      
        public static List<ItemPrioridad> GetAll()
        {
            return Init();
        }
    }
}
