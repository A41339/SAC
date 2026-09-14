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
                    padding: 36px 16px;
                }
                .email-card {
                    max-width: 780px;
                    margin: 0 auto;
                    background-color: #ffffff;
                    border-radius: 14px;
                    overflow: hidden;
                    box-shadow: 0 12px 28px -5px rgba(0, 0, 0, 0.09), 0 8px 10px -6px rgba(0, 0, 0, 0.02);
                    border: 1px solid #e2e8f0;
                }
                .email-header-banner {
                    background-color: #143750;
                    padding: 40px 28px;
                    text-align: center;
                    border-bottom: 5px solid #2563eb;
                }
                .email-header-banner img {
                    max-width: 640px;
                    width: 95%;
                    height: auto;
                    max-height: 175px;
                    display: inline-block;
                    vertical-align: middle;
                    object-fit: contain;
                }
                .email-body {
                    padding: 40px 48px;
                    color: #334155;
                    font-size: 16px;
                    line-height: 1.65;
                }
                .subject-title {
                    font-size: 22px;
                    font-weight: 700;
                    color: #0f172a;
                    margin-top: 0;
                    margin-bottom: 22px;
                    padding-left: 14px;
                    border-left: 5px solid #2563eb;
                    line-height: 1.35;
                }
                .message-content {
                    color: #334155;
                    font-size: 16px;
                    line-height: 1.65;
                    margin-bottom: 28px;
                }
                .message-content ul {
                    background-color: #f8fafc;
                    border-radius: 10px;
                    padding: 18px 24px 18px 40px;
                    border: 1px solid #e2e8f0;
                }
                .message-content li {
                    margin-bottom: 10px;
                    color: #1e293b;
                    font-size: 16px;
                }
                .button-container {
                    text-align: center;
                    margin: 32px 0 24px 0;
                }
                .btn-c2a {
                    display: inline-block;
                    padding: 12px 36px;
                    font-family: inherit;
                    font-size: 15px;
                    font-weight: 600;
                    letter-spacing: 0.5px;
                    color: #ffffff !important;
                    background-color: #2563eb;
                    background: linear-gradient(180deg, #2563eb 0%, #1d4ed8 100%);
                    text-decoration: none !important;
                    border-radius: 8px;
                    border: 1px solid #1d4ed8;
                    box-shadow: 0 4px 12px rgba(37, 99, 235, 0.22);
                    transition: all 0.2s ease;
                }
                .no-reply-box {
                    margin-top: 32px;
                    padding: 18px 22px;
                    background-color: #fff1f2;
                    border: 1px solid #fecdd3;
                    border-left: 5px solid #e11d48;
                    border-radius: 10px;
                    text-align: center;
                    color: #9f1239;
                    font-size: 15px;
                    font-weight: 600;
                    line-height: 1.6;
                }
                .email-footer {
                    background-color: #143750;
                    padding: 24px 30px;
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
                            <a href='https://www.ffc.co.cr/FFC/Account/Login' class='btn-c2a'>Ingrese Aqu&iacute;</a>
                        </div>
                        <div class='no-reply-box'>
                            &#9888;&#65039; <strong>Aviso Importante:</strong> Mensaje automático del sistema. Por favor <u>no responda directamente a este correo</u>.
                        </div>
                    </div>
                    <div class='email-footer'>
                        <p style='font-size: 14px;'><strong>FFC - Fondo de Fortalecimiento Cooperativo</strong></p>
                        <p style='color: #cbd5e1; font-size: 13px;'>Sistema de Análisis Cooperativo (SAC)</p>
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
