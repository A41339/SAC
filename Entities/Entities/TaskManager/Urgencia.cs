using System.Collections.Generic;
using System.Linq;

namespace FGA.Models
{

    public class ItemUrgencia
    {

        public ItemUrgencia(int vId, string vNombre)
        {
            Id = vId;
            Nombre = vNombre;
        }

        public int Id { get; set; }
        public string Nombre { get; set; }

    }

    public static class Urgencia
    {
        public static List<ItemUrgencia> Init()
        {
            List<ItemUrgencia> list = new List<ItemUrgencia>();
            list.Add(new ItemUrgencia(1, "Bajo"));
            list.Add(new ItemUrgencia(2, "Medio"));
            list.Add(new ItemUrgencia(3, "Alto"));

            return list;
        }
        public static string GetNombre(int estado)
        {
            return Init().FirstOrDefault(o => o.Id == estado).Nombre;
        }
      
        public static List<ItemUrgencia> GetAll()
        {
            return Init();
        }
    }
}
