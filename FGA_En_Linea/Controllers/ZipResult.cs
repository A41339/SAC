using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;


namespace FGA.Controllers
{
    public class ZipResult : ActionResult
    {
        List<string> archives = new List<string>();

        public override void ExecuteResult(ControllerContext context)
        {
            if (archives.Count == 0)
                return;
            string zipName;
            bool zipped = false;
            if (archives.Count == 1)
            {
                if (Path.GetExtension(archives[0]) == ".zip")
                    zipped = true;
                zipName = Path.GetFileNameWithoutExtension(archives[0]) + ".zip";
            }
            else
                zipName = String.Format("archive-{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));

            context.HttpContext.Response.Buffer = true;
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
            context.HttpContext.Response.ContentType = "application/zip";

            if (zipped)
                context.HttpContext.Response.WriteFile(archives[0]);
            else
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    foreach (string path in archives)
                    {
                        try
                        {
                            FileAttributes attr = System.IO.File.GetAttributes(path);

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                zip.AddDirectory(path, Path.GetFileNameWithoutExtension(path));
                            else
                                zip.AddFile(path, "");
                        }
                        catch (Exception)
                        {
                        }
                    }
                    zip.Save(context.HttpContext.Response.OutputStream);
                }
            }
        }

        public void AddFile(string filepath)
        {
            archives.Add(filepath);
        }

    }
}
