using System.Collections.Generic;
using System.Linq;

namespace FGA.Models
{

    public class ItemEstado {

        public ItemEstado(int vId, string vNombre) {
            Id = vId;
            Nombre = vNombre;
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public static class Estado
    {
        public static List<ItemEstado> Init() {
            List<ItemEstado> list = new List<ItemEstado>();
            list.Add(new ItemEstado(0, "Rechazado"));
            list.Add(new ItemEstado(1, "Registrado"));
            list.Add(new ItemEstado(2, "Desarrollo"));
            list.Add(new ItemEstado(3, "Pruebas"));
            list.Add(new ItemEstado(4, "Pase producción"));
            list.Add(new ItemEstado(5, "Finalizado"));
            list.Add(new ItemEstado(6, "Eliminado"));

            return list;
        }

        public static string GetNombre(int estado)
        {
            string nombre = Init().FirstOrDefault(o => o.Id == estado).Nombre;
            string etiqueta = string.Empty;

            switch (estado)
            {
                case 0:
                    etiqueta = "<span class=\"btn btn-danger btn-xs\">Rechazado</span>";
                    break;
                case 1:
                    etiqueta = "<span class=\"btn btn-info btn-xs\">Registrado</span>";
                    break;
                case 2:
                    etiqueta = "<span class=\"btn btn-primary btn-xs\">Desarrollo</span>";
                    break;
                case 3:
                    etiqueta = "<span class=\"btn btn-warning btn-xs\">Pruebas</span>";
                    break;
                case 4:
                    etiqueta = "<span class=\"btn btn-default btn-xs\">Pase a producción</span>";
                    break;
                default:
                    etiqueta = "<span class=\"btn btn-facebook btn-xs\">Finalizado</span>";
                    break;

            }

            return etiqueta; // "<span class=\"label label-danger\">Highest</span>";
        }

        public static string GetTooltip(int estado) {
            string nombre = Init().FirstOrDefault(o => o.Id == estado).Nombre;
            string etiqueta = string.Empty;

            switch (estado) {
                case 0:
                    etiqueta = "Rechazado";//"<span class=\"btn btn-danger btn-xs\">Rechazado</span>";
                    break;
                case 1:
                    etiqueta = "Registrado"; //" < span class=\"btn btn-info btn-xs\">Registrado</span>";
                    break;
                case 2:
                    etiqueta = "Desarrollo";//"<span class=\"btn btn-link btn-xs\">Desarrollo</span>";
                    break;
                case 3:
                    etiqueta = "Pruebas";//"<span class=\"btn btn-warning btn-xs\">Pruebas</span>";
                    break;
                case 4:
                    etiqueta = "Pase a producción";//"<span class=\"btn btn-default btn-xs\">Pase a producción</span>";
                    break;
                default:
                    etiqueta = "Finalizado";//"<span class=\"btn btn-facebook btn-xs\">Finalizado</span>";
                    break;

            }

            return etiqueta; // "<span class=\"label label-danger\">Highest</span>";
        }

        public static bool EnRechazo(int estado) {
            return estado == 0 ? true : false;
        }
        public static bool EnRegistrado(int estado) {
            return estado == 1 ? true : false;
        }
        public static bool EnDesarrollo(int estado){
            return estado == 2 ? true : false;
        }
        public static bool EnPruebas(int estado){
            return estado == 3 ? true : false;
        }
        public static bool EnPaseProduccion(int estado){
            return estado == 4 ? true : false;
        }
        public static bool EnFinalizado(int estado){
            return estado == 5 ? true : false;
        }        
        public static List<ItemEstado> GetAll() {            
            return Init();
        }
    }
}
