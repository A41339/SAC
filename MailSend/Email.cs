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
                    background-color: #2F5597;
                    background: linear-gradient(135deg, #2F5597 0%, #204880 50%, #31859C 100%);
                    padding: 30px 36px;
                    border-bottom: 4px solid #ED7D31;
                    text-align: left;
                }
                .email-header-banner img {
                    width: 165px;
                    max-width: 165px;
                    height: auto;
                    display: block;
                    border: 0;
                }
                .email-body {
                    padding: 40px 48px;
                    color: #334155;
                    font-size: 15px;
                    line-height: 1.65;
                }
                .email-body a {
                    color: #31859C;
                    font-weight: 600;
                    text-decoration: none;
                }
                .email-body a:hover {
                    color: #2F5597;
                    text-decoration: underline;
                }
                .subject-title {
                    font-size: 21px;
                    font-weight: 700;
                    color: #2F5597;
                    margin-top: 0;
                    margin-bottom: 22px;
                    padding-left: 14px;
                    border-left: 4px solid #31859C;
                    line-height: 1.35;
                }
                .message-content {
                    color: #334155;
                    font-size: 15px;
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
                    font-size: 15px;
                }
                .button-container {
                    text-align: center;
                    margin: 32px 0 24px 0;
                }
                .btn-c2a {
                    display: inline-block;
                    padding: 12px 38px;
                    font-family: inherit;
                    font-size: 14.5px;
                    font-weight: 600;
                    letter-spacing: 0.4px;
                    color: #ffffff !important;
                    background-color: #2F5597;
                    background: linear-gradient(135deg, #2F5597 0%, #31859C 100%);
                    text-decoration: none !important;
                    border-radius: 8px;
                    border: 1px solid #24447a;
                    box-shadow: 0 4px 14px rgba(47, 85, 151, 0.28);
                    transition: all 0.2s ease;
                }
                .btn-c2a:hover {
                    background: linear-gradient(135deg, #24447a 0%, #276e8a 100%) !important;
                    color: #ffffff !important;
                }
                .no-reply-box {
                    margin-top: 30px;
                    padding: 16px 20px;
                    background-color: #f8fafc;
                    border: 1px solid #e2e8f0;
                    border-left: 4px solid #ED7D31;
                    border-radius: 8px;
                    text-align: center;
                    color: #475569;
                    font-size: 13.5px;
                    font-weight: 500;
                    line-height: 1.55;
                }
                .email-footer {
                    background-color: #1e293b;
                    padding: 24px 30px;
                    text-align: center;
                    color: #94a3b8;
                    font-size: 12px;
                    line-height: 1.6;
                    border-top: 1px solid #334155;
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
                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                            <tr>
                                <td valign='middle' style='width: 175px; text-align: left;'>
                                    <img src='cid:Pic1' alt='FFC - Fondo de Fortalecimiento Cooperativo' width='165' style='display: block; width: 165px; max-width: 165px; height: auto; border: 0;' />
                                </td>
                                <td valign='middle' style='border-left: 1px solid rgba(255, 255, 255, 0.35); padding-left: 22px; text-align: left;'>
                                    <div style='color: #ffffff; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; font-size: 16px; font-weight: 700; letter-spacing: 1.8px; text-transform: uppercase; line-height: 1.25;'>
                                        Sistema de Notificaci&oacute;n
                                    </div>
                                    <div style='color: #93CDDD; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; font-size: 11.5px; font-weight: 600; letter-spacing: 0.6px; margin-top: 4px;'>
                                        Sistema de An&aacute;lisis Cooperativo &bull; SAC
                                    </div>
                                </td>
                            </tr>
                        </table>
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
                            &#9888;&#65039; <strong style='color: #c2410c;'>Aviso Importante:</strong> Mensaje autom&aacute;tico del sistema. Por favor <u>no responda directamente a este correo</u>.
                        </div>
                    </div>
                    <div class='email-footer'>
                        <p style='font-size: 13.5px; font-weight: 700; color: #ffffff;'>FFC - Fondo de Fortalecimiento Cooperativo</p>
                        <p style='color: #93CDDD; font-size: 12px; font-weight: 600;'>Sistema de An&aacute;lisis Cooperativo (SAC)</p>
                        <p style='color: #64748b; font-size: 11.5px;'>San Jos&eacute;, Costa Rica &bull; <a href='https://www.ffc.co.cr/' style='color: #93CDDD; text-decoration: none;'>www.ffc.co.cr</a> &bull; Tel: (506) 2257-1111</p>
                    </div>
                </div>
            </div>
        </body>
        </html>";

        private static string ResolveLogoPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            string[] candidatePaths = new string[]
            {
                Path.Combine(baseDir, "Content", "images", "FFC_Logo_Blanco.png"),
                Path.Combine(baseDir, "..", "Content", "images", "FFC_Logo_Blanco.png"),
                Path.Combine(baseDir, "..", "FGA_En_Linea", "Content", "images", "FFC_Logo_Blanco.png"),
                Path.Combine(baseDir, "..", "..", "FGA_En_Linea", "Content", "images", "FFC_Logo_Blanco.png"),
                Path.Combine(baseDir, "bin", "Content", "images", "FFC_Logo_Blanco.png"),
                @"C:\Users\jcastro\Desktop\Solution\FGA_En_Linea\Content\images\FFC_Logo_Blanco.png",
                @"C:\FFC_Logo_Blanco.png",
                @"C:\Logo.png",
                @"C:\Logo.jpg"
            };

            foreach (string path in candidatePaths)
            {
                try
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        return Path.GetFullPath(path);
                    }
                }
                catch { }
            }
            return null;
        }

        private static void AttachLogoResource(AlternateView avHtml)
        {
            try
            {
                string logoPath = ResolveLogoPath();
                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    string mediaType = logoPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        ? "image/png"
                        : MediaTypeNames.Image.Jpeg;
                    LinkedResource pic1 = new LinkedResource(logoPath, mediaType);
                    pic1.ContentId = "Pic1";
                    avHtml.LinkedResources.Add(pic1);
                }
            }
            catch { }
        }

        public static void EnviarCorreoImagenes(string asunto, string mensaje, string correoDestino, string nombreServerCorreo,
            string correoServer, string usuarioCorreo, string password)
        {
            try
            {
                string htmlBody = htmlTemplate.Replace("Tmensaje", mensaje).Replace("Tasunto", asunto);
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                
                AttachLogoResource(avHtml);

                MailMessage correo = new MailMessage();
                correo.AlternateViews.Add(avHtml);
                correo.From = new MailAddress(correoServer, "FFC Fondo de Fortalecimiento Cooperativo");
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

                AttachLogoResource(avHtml);

                MailMessage correo = new MailMessage();
                correo.AlternateViews.Add(avHtml);
                correo.From = new MailAddress(correoServer, "FFC Fondo de Fortalecimiento Cooperativo");
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
