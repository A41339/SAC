using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FGA.Models;

namespace FGA.Controllers
{
    public class AlbumController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                ViewBag.TiposAlbum = new SelectList(tp.GetAll(), "Id", "Nombre").OrderBy(o => o.Text);
            }
            catch
            {
                ViewBag.TiposAlbum = new SelectList(new List<Tipo_Album>(), "Id", "Nombre");
            }
            return View();
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = alb.GetAll();
                var result = from c in tak
                             select new string[] {
                                 Convert.ToString(c.Id),
                                 c.Fecha.ToString("dd/MM/yyyy"),
                                 Convert.ToString(c.Autor),
                                 Convert.ToString(c.Titulo),
                                 c.Tipo != null ? Convert.ToString(c.Tipo.Nombre) : ""
                             };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetAlbum(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Album obj = alb.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Álbum no encontrado." }, JsonRequestBehavior.AllowGet);

                List<string> fileNames = new List<string>();
                try
                {
                    var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(obj);
                    if (Directory.Exists(path))
                    {
                        foreach (string f in Directory.GetFiles(path))
                            fileNames.Add(Path.GetFileName(f));
                    }
                }
                catch { }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Fecha = obj.Fecha.ToString("yyyy-MM-dd"),
                        Autor = obj.Autor,
                        Titulo = obj.Titulo,
                        Detalle = Server.HtmlDecode(obj.Detalle ?? ""),
                        TipoAlbum_Id = obj.TipoAlbum_Id,
                        Files = fileNames
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener el álbum: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalleAlbum(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Identificador no proporcionado." }, JsonRequestBehavior.AllowGet);

                Album obj = alb.Get(id);
                if (obj == null)
                    return Json(new { success = false, message = "Álbum no encontrado." }, JsonRequestBehavior.AllowGet);

                List<string> fileNames = new List<string>();
                try
                {
                    var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(obj);
                    if (Directory.Exists(path))
                    {
                        foreach (string f in Directory.GetFiles(path))
                            fileNames.Add(Path.GetFileName(f));
                    }
                }
                catch { }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = obj.Id,
                        Fecha = obj.Fecha.ToString("dd/MM/yyyy"),
                        Autor = obj.Autor ?? "-",
                        Titulo = obj.Titulo ?? "-",
                        Tipo = obj.Tipo != null ? obj.Tipo.Nombre : "-",
                        Detalle = Server.HtmlDecode(obj.Detalle ?? "-"),
                        Files = fileNames
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener detalle: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult GuardarAlbum(bool esNuevo, int? id, DateTime fecha, string autor, string titulo, int? tipoAlbumId, string detalle)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(autor))
                    return Json(new { success = false, message = "El campo Autor es obligatorio." });

                if (string.IsNullOrWhiteSpace(titulo))
                    return Json(new { success = false, message = "El campo Título es obligatorio." });

                if (esNuevo)
                {
                    Album newAlbum = new Album
                    {
                        Fecha = fecha,
                        Autor = autor.Trim(),
                        Titulo = titulo.Trim(),
                        TipoAlbum_Id = tipoAlbumId,
                        Detalle = detalle
                    };

                    if (newAlbum.TipoAlbum_Id.HasValue && newAlbum.TipoAlbum_Id.Value != (int)Utility.Utilitarios.enum_tipoAlbum.blog)
                        newAlbum.Detalle = Utility.Utilitarios.StripHTML(newAlbum.Detalle);

                    alb.Add(ref newAlbum);

                    if (Request.Files.Count > 0)
                    {
                        var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(newAlbum);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            var file = Request.Files[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                var fileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                                file.SaveAs(Path.Combine(path, fileName));
                            }
                        }
                    }

                    return Json(new { success = true, message = "Álbum registrado exitosamente." });
                }
                else
                {
                    if (!id.HasValue)
                        return Json(new { success = false, message = "ID de álbum no válido." });

                    Album editAlbum = alb.Get(id.Value.ToString());
                    if (editAlbum == null)
                        return Json(new { success = false, message = "El álbum no existe." });

                    editAlbum.Fecha = fecha;
                    editAlbum.Autor = autor.Trim();
                    editAlbum.Titulo = titulo.Trim();
                    editAlbum.TipoAlbum_Id = tipoAlbumId;
                    editAlbum.Detalle = detalle;

                    if (editAlbum.TipoAlbum_Id.HasValue && editAlbum.TipoAlbum_Id.Value != (int)Utility.Utilitarios.enum_tipoAlbum.blog)
                        editAlbum.Detalle = Utility.Utilitarios.StripHTML(editAlbum.Detalle);

                    alb.Update(editAlbum);

                    if (Request.Files.Count > 0)
                    {
                        var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(editAlbum);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            var file = Request.Files[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                var fileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                                file.SaveAs(Path.Combine(path, fileName));
                            }
                        }
                    }

                    return Json(new { success = true, message = "Álbum actualizado exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar el álbum: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarFotoAlbum(string id, string fileName)
        {
            try
            {
                Album objAlbum = alb.Get(id);
                if (objAlbum != null)
                {
                    var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(objAlbum);
                    var fullPath = Path.Combine(path, fileName);
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                return Json(new { success = true, message = "Foto eliminada." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarAlbum(string id)
        {
            try
            {
                alb.Delete(id);
                return Json(new { success = true, message = "Álbum eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el álbum: " + ex.Message });
            }
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
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                if (ModelState.IsValid)
                {
                    if (ObjAlbum.TipoAlbum_Id.Value != (int)Utility.Utilitarios.enum_tipoAlbum.blog)
                        ObjAlbum.Detalle = Utility.Utilitarios.StripHTML(ObjAlbum.Detalle);

                    alb.Add(ref ObjAlbum);

                    var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(ObjAlbum);
                    bool exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    if (files != null)
                    {
                        foreach (HttpPostedFileBase file in files)
                        {
                            if (file != null)
                            {
                                var InputFileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                                var ServerSavePath = path + "\\" + InputFileName;
                                file.SaveAs(ServerSavePath);
                            }
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
            if (ObjAlbum == null)
                return HttpNotFound();

            Album_Update albUpd = new Album_Update();
            albUpd.Files = new Dictionary<string, string>();
            albUpd.Autor = ObjAlbum.Autor;
            albUpd.Detalle = Server.HtmlDecode(ObjAlbum.Detalle);
            albUpd.Fecha = ObjAlbum.Fecha;
            albUpd.Id = ObjAlbum.Id;
            albUpd.TipoAlbum_Id = ObjAlbum.TipoAlbum_Id;
            albUpd.Tipo = ObjAlbum.Tipo;
            albUpd.Titulo = ObjAlbum.Titulo;

            try
            {
                var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(ObjAlbum);
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        string fileName = file.Substring(file.LastIndexOf("\\") + 1);
                        albUpd.Files.Add(fileName, file);
                    }
                }
            }
            catch { }

            ViewBag.TipoAlbum_Id = new SelectList(tp.GetAll(), "Id", "Nombre", ObjAlbum.TipoAlbum_Id).OrderBy(o => o.Text);
            return View(albUpd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Album ObjAlbum, System.Web.HttpPostedFileBase[] files)
        {
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

                    if (files != null)
                    {
                        foreach (HttpPostedFileBase file in files)
                        {
                            if (file != null)
                            {
                                var InputFileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                                var ServerSavePath = path + "\\" + InputFileName;
                                file.SaveAs(ServerSavePath);
                            }
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
                Album ObjAlbum = alb.Get(id);
                var path = System.Configuration.ConfigurationManager.AppSettings["RutaImg"] + "galeria\\" + Utility.Utilitarios.GetDirectoryName(ObjAlbum);
                System.IO.File.Delete(path + "\\" + file);
            }
            catch
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