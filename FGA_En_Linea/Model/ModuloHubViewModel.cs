using System.Collections.Generic;
using System.Web.Mvc;

namespace FGA.Models
{
    public class ModuloHubViewModel
    {
        public int MenuId { get; set; }
        public string Titulo { get; set; } = "";
        public string Subtitulo { get; set; } = "";
        public string NotaPie { get; set; } = "";
        public int? ParentMenuId { get; set; }
        public string ParentMenuTitulo { get; set; } = "";

        // Filtros globales
        public bool MostrarFiltros { get; set; }
        public string MenuURL { get; set; } = "";
        public string IdEntidad { get; set; } = "";
        public string PeriodoReferencia { get; set; } = "";
        public string TipoComparacion { get; set; } = "Interanual"; // Mensual, Trimestral, Interanual
        public SelectList Entidades { get; set; }

        // Lista de tarjetas del modulo
        public List<ModuloTarjetaItem> Tarjetas { get; set; } = new List<ModuloTarjetaItem>();
    }

    public class ModuloTarjetaItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Url { get; set; } = "";
        public string Icono { get; set; } = "fa fa-file-text-o";
        public string ColorFondoIcono { get; set; } = "#e0f2fe";
        public string ColorIcono { get; set; } = "#0284c7";
        public int SortOrder { get; set; }
        public bool EsSubModulo { get; set; }
        public int CantidadOpciones { get; set; }
    }
}
