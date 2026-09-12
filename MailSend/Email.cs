using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace MailSend
{
    public static class Email
    {
        static string htmlTemplate = @"
        <!DOCTYPE html>
        <html lang='en' xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Notificación</title>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    font-size: 0px;
                    margin: 0;
                    padding: 0;
                    background-color: white;
                }
                table {
                    max-width: 650px;
                    width: 100%;
                    border-collapse: collapse;
                }
                img {
                    width: 100%;
                    min-height: 150px;
                }
                .content {
                    padding: 10px;
                    background-color: white;
                    text-align: left;
                }
                .header {
                    font-family: Verdana, sans-serif;
                    font-size: 14px;
                    color: #484848;
                    line-height: 14px;
                    text-align: center;
                    color: #336699;
                    font-size: 20px;
                }
                .message {
                    font-family: Arial, sans-serif;
                    font-size: 18px;
                    color: #131313;
                    line-height: 25px;
                    min-height: 150px;
                    text-align: justify;
                }
                .footer {
                    padding: 10px;
                    background-color: #525252;
                    border-top: solid 1px black;
                    text-align: center;
                    color: white;
                    font-size: 22px;
                }
                .button-container {
                    text-align: center;
                    padding: 20px;
                }
                .button {
                    display: inline-block;
                    padding: 10px 20px;
                    font-family: Arial, sans-serif;
                    font-size: 16px;
                    color: white !important;
                    background-color: #28a745;
                    text-decoration: none;
                    border-radius: 5px;
                }
            </style>
        </head>
        <body>
            <div align='center'>
                <table cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td><img src='cid:Pic1' alt='FFC' /></td>
                    </tr>
                    <tr>
                        <td class='content'>
                            <div class='header'>
                                 <p> Tasunto </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td class='content' style='border-top: solid 1px black;'>
                            <div class='message'>
                                <p> Tmensaje </p>
                            </div>
                            <div style='color:white' class='button-container'>
                                <a style='color:white' href='https://www.ffc.co.cr/FFC/Account/Login' class='button'>INGRESE AQUÍ</a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer'>
                            Mensaje automático, por favor no responda este correo
                        </td>
                    </tr>
                </table>
            </div>
        </body>
        </html>";

        public static void EnviarCorreoImagenes(string asunto, string mensaje, string correoDestino, string nombreServerCorreo,
            string correoServer, string usuarioCorreo, string password)
        {
            //Task.Run(() =>
            // {
            try
            {
            string mensajeTexto = string.Empty;
            string mensajeCorreo = string.Empty;
            string htmlBody = htmlTemplate.Replace("Tmensaje", mensaje).Replace("Tasunto", asunto);
            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            string absolutePath = "C:\\\\Logo.jpg";
            LinkedResource pic1 = new LinkedResource(absolutePath, MediaTypeNames.Image.Jpeg);
            pic1.ContentId = "Pic1";
            avHtml.LinkedResources.Add(pic1);
            MailMessage correo = new MailMessage();

            correo.AlternateViews.Add(avHtml);
            correo.From = new MailAddress(correoServer);
            correo.Subject = asunto;
            correo.Priority = MailPriority.Normal;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = nombreServerCorreo;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential(usuarioCorreo, password);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            smtp.Port = 587;
            smtp.EnableSsl = true;
            string[] destinos = correoDestino.Split(';');

            for (int i = 0; i < destinos.Length; i++)
                if (!string.IsNullOrEmpty(destinos[i]))
                    correo.To.Add(destinos[i]);

            smtp.Send(correo);
            correo.To.Clear();
            }
            catch (Exception e)
             {
                string errorMessage = "Error desconocido.";

                if (e != null)
                {
                    errorMessage = e.Message ?? string.Empty;

                    if (e.InnerException != null)
                    {
                        errorMessage += Environment.NewLine + "Inner Exception: " + e.InnerException.Message ?? string.Empty;
                    }

                    errorMessage += Environment.NewLine + "StackTrace: " + e.StackTrace ?? string.Empty;
                }

                throw new Exception(errorMessage);
            }
            //});
        }

        public static void EnviarCorreoAduntos(string asunto, string mensaje, string correoDestino, string nombreServerCorreo,
        string correoServer, string usuarioCorreo, string password, string urlAdjunto)
        {

           // Task.Run(() =>
            //{
               // try
                //{
                    string mensajeTexto = string.Empty;
                    string mensajeCorreo = string.Empty;
                    string htmlBody = htmlTemplate.Replace("\"\" + mensaje + \"\"", mensaje).Replace("\"\" + asunto + \"\"", asunto);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

                    string absolutePath = "C:\\\\FFC.jpg";
                    LinkedResource pic1 = new LinkedResource(absolutePath, MediaTypeNames.Image.Jpeg);
                    pic1.ContentId = "Pic1";
                    avHtml.LinkedResources.Add(pic1);
                    MailMessage correo = new MailMessage();

                    correo.AlternateViews.Add(avHtml);
                    correo.From = new MailAddress(correoServer);
                    correo.Subject = asunto;
                    correo.Priority = MailPriority.Normal;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = nombreServerCorreo;
                    smtp.Credentials = new System.Net.NetworkCredential(usuarioCorreo, password);
                    smtp.Port = 587;
                    smtp.EnableSsl = true;

                    Attachment attachment = new Attachment(urlAdjunto, MediaTypeNames.Application.Pdf);
                    correo.Attachments.Add(attachment);

                    string[] destinos = correoDestino.Split(';');

                    for (int i = 0; i < destinos.Length; i++)
                        if (!string.IsNullOrEmpty(destinos[i]))
                            correo.To.Add(destinos[i]);

                    smtp.Send(correo);
                    correo.To.Clear();

                //}
                //catch (Exception)
                //{
                //}
           // });
        }
    }
}
