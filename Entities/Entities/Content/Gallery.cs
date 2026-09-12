using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FGA.Models;

namespace FGA.Models
{
    public class Gallery
    {
        public Seccion galeria;
        public Album[] listaAlbum;
        public int currentPage;
        public int pageSize;
        public int totalPage;

    }
}
