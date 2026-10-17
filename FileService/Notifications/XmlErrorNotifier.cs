using FGA.Models;
using FGA.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace FileService.Notifications
{
    public static class XmlErrorNotifier
    {
        public static void EnviarCorreoConsolidadoErrores(List<long> idEncabezadosConError)
        {
            if (idEncabezadosConError == null || idEncabezadosConError.Count == 0)
                return;

            try
            {
                using (SIContext db = new SIContext())
                {
                    var encabezados = db.XML_Encabezados
                        .Include("IdArchivo")
                        .Include("IdEntidad")
                        .Where(e => idEncabezadosConError.Contains(e.Id))
                        .ToList();

                    if (encabezados.Count == 0)
                        return;

                    string nombreEntidad = (encabezados.Count > 0 && encabezados[0].IdEntidad != null) ? encabezados[0].IdEntidad.Nombre : "Entidad";
                    string asunto = string.Format("{0}. Notificación de error(es) en carga de archivo(s) XML", nombreEntidad);

                    StringBuilder detalle = new StringBuilder();
                    detalle.Append("El usuario de carga asignado por la cooperativa subió los archivos y se generaron los siguientes errores:<br/><br/>");

                    foreach (var enc in encabezados)
                    {
                        var listaErrores = db.XML_Errores.Where(o => o.IdEncabezado_Id == enc.Id).ToList();
                        string nombreArchivo = enc.IdArchivo != null ? enc.IdArchivo.Nombre : ("Encabezado #" + enc.Id);
                        detalle.AppendFormat("<strong>Archivo: {0}</strong> ({1} errores):<br/><ul>", nombreArchivo, listaErrores.Count);
                        foreach (var err in listaErrores)
                        {
                            detalle.AppendFormat("<li>{0}</li>", err.Detalle);
                        }
                        detalle.Append("</ul><br/>");
                    }

                    detalle.Append("Si requiere de nuestra asesoría, puede comunicarse al número 2257-1111 o contactar a las analistas de riesgo:<br/><br/>");
                    detalle.Append("Cinthya Salazar <a href='mailto:csalazar@ffc.co.cr'>csalazar@ffc.co.cr</a><br/>");
                    detalle.Append("Viviana Zumbado <a href='mailto:vzumbado@ffc.co.cr'>vzumbado@ffc.co.cr</a><br/><br/>");

                    var clavesParam = new[] { Utilitarios.Correo_Error_XML, Utilitarios.Servidor_Correo, Utilitarios.Direccion_Correo, Utilitarios.Contrasena_Correo };
                    var paramsDict = db.Parametros.Where(o => clavesParam.Contains(o.Llave)).ToDictionary(i => i.Llave, i => i.Valor);

                    string correoError = paramsDict.ContainsKey(Utilitarios.Correo_Error_XML) ? paramsDict[Utilitarios.Correo_Error_XML] : "";
                    string servidorCorreo = paramsDict.ContainsKey(Utilitarios.Servidor_Correo) ? paramsDict[Utilitarios.Servidor_Correo] : "";
                    string direccionCorreo = paramsDict.ContainsKey(Utilitarios.Direccion_Correo) ? paramsDict[Utilitarios.Direccion_Correo] : "";
                    string contrasenaCorreo = paramsDict.ContainsKey(Utilitarios.Contrasena_Correo) ? paramsDict[Utilitarios.Contrasena_Correo] : "";

                    string detalleHtml = detalle.ToString();
                    if (!string.IsNullOrEmpty(correoError))
                    {
                        MailSend.Email.EnviarCorreoImagenes(asunto, detalleHtml, correoError, servidorCorreo, direccionCorreo, direccionCorreo, contrasenaCorreo);
                    }

                    string idEntidad = (encabezados.Count > 0 && encabezados[0].IdEntidad != null) ? encabezados[0].IdEntidad.Id : null;
                    if (!string.IsNullOrEmpty(idEntidad))
                    {
                        Entidad entidad = db.Entidades.AsNoTracking().FirstOrDefault(o => o.Contacto != string.Empty && o.Activo == true && o.Id == idEntidad);
                        if (entidad != null && !string.IsNullOrEmpty(entidad.Contacto))
                        {
                            MailSend.Email.EnviarCorreoImagenes(asunto, detalleHtml, entidad.Contacto, servidorCorreo, direccionCorreo, direccionCorreo, contrasenaCorreo);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
