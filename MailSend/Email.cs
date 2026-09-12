using System;
using System.Diagnostics;
using System.IO;
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
        <html lang='es' xmlns='http://www.w3.org/1999/xhtml'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Notificación FFC</title>
            <style>
                body {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                    background-color: #f1f5f9;
                    margin: 0;
                    padding: 0;
                    -webkit-font-smoothing: antialiased;
                }
                .email-wrapper {
                    width: 100%;
                    background-color: #f1f5f9;
                    padding: 30px 10px;
                }
                .email-card {
                    max-width: 620px;
                    margin: 0 auto;
                    background-color: #ffffff;
                    border-radius: 12px;
                    overflow: hidden;
                    box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.08), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
                    border: 1px solid #e2e8f0;
                }
                .email-header-banner {
                    background-color: #0b2341;
                    padding: 24px 30px;
                    text-align: center;
                    border-bottom: 3px solid #0d7682;
                }
                .email-header-banner img {
                    max-width: 180px;
                    height: auto;
                }
                .email-body {
                    padding: 32px 35px;
                    color: #334155;
                    font-size: 15px;
                    line-height: 1.6;
                }
                .subject-title {
                    font-size: 20px;
                    font-weight: 700;
                    color: #0f172a;
                    margin-top: 0;
                    margin-bottom: 20px;
                    padding-left: 12px;
                    border-left: 4px solid #0d7682;
                    line-height: 1.3;
                }
                .message-content {
                    color: #334155;
                    font-size: 14px;
                    line-height: 1.6;
                    margin-bottom: 25px;
                }
                .message-content ul {
                    background-color: #f8fafc;
                    border-radius: 8px;
                    padding: 15px 20px 15px 35px;
                    border: 1px solid #e2e8f0;
                }
                .message-content li {
                    margin-bottom: 8px;
                    color: #1e293b;
                }
                .button-container {
                    text-align: center;
                    margin: 30px 0 15px 0;
                }
                .btn-c2a {
                    display: inline-block;
                    padding: 12px 28px;
                    font-family: inherit;
                    font-size: 14px;
                    font-weight: 700;
                    color: #ffffff !important;
                    background-color: #0d7682;
                    text-decoration: none !important;
                    border-radius: 8px;
                    box-shadow: 0 4px 12px rgba(13, 118, 130, 0.25);
                }
                .email-footer {
                    background-color: #0b2341;
                    padding: 20px 30px;
                    text-align: center;
                    color: #94a3b8;
                    font-size: 12px;
                    line-height: 1.5;
                    border-top: 1px solid rgba(255, 255, 255, 0.1);
                }
                .email-footer p {
                    margin: 4px 0;
                }
            </style>
        </head>
        <body>
            <div class='email-wrapper'>
                <div class='email-card'>
                    <div class='email-header-banner'>
                        <img src='cid:Pic1' alt='FFC Sistema Empresarial' />
                    </div>
                    <div class='email-body'>
                        <div class='subject-title'>
                            Tasunto
                        </div>
                        <div class='message-content'>
                            Tmensaje
                        </div>
                        <div class='button-container'>
                            <a href='https://www.ffc.co.cr/FFC/Account/Login' class='btn-c2a'>INGRESE AQUÍ</a>
                        </div>
                    </div>
                    <div class='email-footer'>
                        <p><strong>FFC - Fondo de Fortalecimiento Cooperativo</strong></p>
                        <p>Mensaje automático del sistema. Por favor no responda directamente a este correo.</p>
                    </div>
                </div>
            </div>
        </body>
        </html>";

        public static void EnviarCorreoImagenes(string asunto, string mensaje, string correoDestino, string nombreServerCorreo,
            string correoServer, string usuarioCorreo, string password)
        {
            try
            {
                string htmlBody = htmlTemplate.Replace("Tmensaje", mensaje).Replace("Tasunto", asunto);
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                
                string absolutePath = "C:\\Logo.jpg";
                if (File.Exists(absolutePath))
                {
                    try
                    {
                        LinkedResource pic1 = new LinkedResource(absolutePath, MediaTypeNames.Image.Jpeg);
                        pic1.ContentId = "Pic1";
                        avHtml.LinkedResources.Add(pic1);
                    }
                    catch { }
                }

                MailMessage correo = new MailMessage();
                correo.AlternateViews.Add(avHtml);
                correo.From = new MailAddress(correoServer, "FFC Sistema Empresarial");
                correo.Subject = asunto;
                correo.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = nombreServerCorreo;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(usuarioCorreo, password);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                smtp.Port = 587;
                smtp.EnableSsl = true;

                string[] destinos = correoDestino.Split(';');

                for (int i = 0; i < destinos.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(destinos[i]))
                        correo.To.Add(destinos[i].Trim());
                }

                smtp.Send(correo);
                correo.To.Clear();
            }
            catch (Exception e)
            {
                string errorMessage = "Error enviando correo.";

                if (e != null)
                {
                    errorMessage = e.Message ?? string.Empty;
                    if (e.InnerException != null)
                    {
                        errorMessage += Environment.NewLine + "Inner Exception: " + e.InnerException.Message;
                    }
                }

                throw new Exception(errorMessage);
            }
        }

        public static void EnviarCorreoAduntos(string asunto, string mensaje, string correoDestino, string nombreServerCorreo,
        string correoServer, string usuarioCorreo, string password, string urlAdjunto)
        {
            try
            {
                string htmlBody = htmlTemplate.Replace("Tmensaje", mensaje).Replace("Tasunto", asunto);
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

                string absolutePath = "C:\\Logo.jpg";
                if (File.Exists(absolutePath))
                {
                    try
                    {
                        LinkedResource pic1 = new LinkedResource(absolutePath, MediaTypeNames.Image.Jpeg);
                        pic1.ContentId = "Pic1";
                        avHtml.LinkedResources.Add(pic1);
                    }
                    catch { }
                }

                MailMessage correo = new MailMessage();
                correo.AlternateViews.Add(avHtml);
                correo.From = new MailAddress(correoServer, "FFC Sistema Empresarial");
                correo.Subject = asunto;
                correo.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = nombreServerCorreo;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(usuarioCorreo, password);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                smtp.Port = 587;
                smtp.EnableSsl = true;

                if (File.Exists(urlAdjunto))
                {
                    Attachment attachment = new Attachment(urlAdjunto, MediaTypeNames.Application.Pdf);
                    correo.Attachments.Add(attachment);
                }

                string[] destinos = correoDestino.Split(';');

                for (int i = 0; i < destinos.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(destinos[i]))
                        correo.To.Add(destinos[i].Trim());
                }

                smtp.Send(correo);
                correo.To.Clear();
            }
            catch (Exception e)
            {
                string errorMessage = "Error enviando correo con adjunto.";
                if (e != null)
                {
                    errorMessage = e.Message ?? string.Empty;
                }
                throw new Exception(errorMessage);
            }
        }
    }
}
