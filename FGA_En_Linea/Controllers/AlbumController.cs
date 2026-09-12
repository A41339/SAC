using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using FGA.Models;
using System.Data.Entity.Validation;

namespace FGA.Controllers
{
    public class AlbumController : BaseController
    {
        public ActionResult Index()
        {
            return View();

        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = alb.GetAll();
                var result = from c in tak
                             select new string[] { Convert.ToString(c.Id),
                                                    Convert.ToString(c.Fecha.ToShortDateString()),
                                                    Convert.ToString(c.Autor),
                                                    Convert.ToString(c.Titulo),
                                                    Convert.ToString(c.Tipo.Nombre)};
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Album ObjAlbum = alb.Get(id);
            if (ObjAlbum == null)
                return HttpNotFound();

            return View(ObjAlbum);
        }

        public ActionResult Create()
        {
            Album_Insert albU = new Album_Insert();
            ViewBag.TipoAlbum_Id = new SelectList(tp.GetAll(), "Id", "Nombre").OrderBy(o => o.Text);
            return View(albU);
        }

        [HttpPost]
        public ActionResult Create(Album ObjAlbum, List<HttpPostedFileBase> files)
        {
            ViewBag.TipoAlbum_Id = new SelectList(tp.GetAll(), "Id", "Nombre").OrderBy(o => o.Text);
            string mensaje = string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                    if (ModelState.IsValid)
                {
                    if (ObjAlbum.TipoAlbum_Id.Value != (int)Utility.Utilitarios.enum_tipoAlbum.blog)
                        ObjAlbum.Detalle = Utility.Utilitarios.StripHTML(ObjAlbum.Detalle);

                    //ObjAlbum.Detalle = Server.HtmlEncode(ObjAlbum.Detalle);
                    alb.Add(ref ObjAlbum);

                    var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(ObjAlbum);
                    bool exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null)
                        {
                            var InputFileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                            var ServerSavePath = path + "\\" + InputFileName;
                            file.SaveAs(ServerSavePath);
                        }
                    }

                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                            sb.Append(err.ErrorMessage + "<br/>");
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Album ObjAlbum = alb.Get(id);
            Album_Update albUpd = new Album_Update();
            albUpd.Files = new Dictionary<string, string>();
            albUpd.Autor = ObjAlbum.Autor;
            albUpd.Detalle = Server.HtmlDecode(ObjAlbum.Detalle);
            albUpd.Fecha = ObjAlbum.Fecha;
            albUpd.Id = ObjAlbum.Id;
            albUpd.TipoAlbum_Id = ObjAlbum.TipoAlbum_Id;
            albUpd.Tipo = ObjAlbum.Tipo;
            albUpd.Titulo = ObjAlbum.Titulo;
            
            var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(ObjAlbum);

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string fileName = file.Substring(file.LastIndexOf("\\") + 1);
                albUpd.Files.Add(fileName, file);
            }

            if (ObjAlbum == null)
                return HttpNotFound();

            ViewBag.TipoAlbum_Id = new SelectList(tp.GetAll(), "Id", "Nombre", ObjAlbum.TipoAlbum_Id).OrderBy(o => o.Text);
            return View(albUpd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Album ObjAlbum, System.Web.HttpPostedFileBase[] files)
        {
            string mensaje = string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                if (ModelState.IsValid)
                {
                    if (ObjAlbum.TipoAlbum_Id.Value != (int)Utility.Utilitarios.enum_tipoAlbum.blog)
                        ObjAlbum.Detalle = Utility.Utilitarios.StripHTML(ObjAlbum.Detalle);

                    ObjAlbum.Detalle = Server.HtmlEncode(ObjAlbum.Detalle);
                    alb.Update(ObjAlbum);
                    ViewBag.TipoAlbum_Id = new SelectList(tp.GetAll(), "Id", "Nombre", ObjAlbum.TipoAlbum_Id).OrderBy(o => o.Text);

                    var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(ObjAlbum);
                    bool exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null)
                        {
                            var InputFileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                            var ServerSavePath = path + "\\" + InputFileName;
                            file.SaveAs(ServerSavePath);
                        }
                    }

                    sb.Append("Sumitted");
                    return Content(sb.ToString());
                }
                else
                {
                    foreach (var key in this.ViewData.ModelState.Keys)
                        foreach (var err in this.ViewData.ModelState[key].Errors)
                            sb.Append(err.ErrorMessage + "<br/>");
                }
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        public ActionResult Delete(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Album ObjAlbum = alb.Get(id);
            if (ObjAlbum == null)
                return HttpNotFound();

            return View(ObjAlbum);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                alb.Delete(id);
                sb.Append("Sumitted");
                return Content(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.Append("Error :" + ex.Message);
            }

            return Content(sb.ToString());
        }

        public void DeleteFile(string id, string file)
        {
            file = file.Replace("$", ".");

            try
            {
                if (ModelState.IsValid)
                {
                    Album ObjAlbum = alb.Get(id);
                    ViewBag.TipoAlbum_Id = new SelectList(tp.GetAll(), "Id", "Nombre", ObjAlbum.TipoAlbum_Id).OrderBy(o => o.Text);
                    var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(ObjAlbum);
                    System.IO.File.Delete(path + "\\" + file);

                }
            }
            catch (Exception)
            {
            }
        }

        private FGA_En_Linea.AlbumService.ServiceOf_AlbumClient alb = new FGA_En_Linea.AlbumService.ServiceOf_AlbumClient();
        private FGA_En_Linea.Tipo_AlbumService.ServiceOf_Tipo_AlbumClient tp = new FGA_En_Linea.Tipo_AlbumService.ServiceOf_Tipo_AlbumClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                alb.Close();
            }
            base.Dispose(disposing);
        }
    }
}