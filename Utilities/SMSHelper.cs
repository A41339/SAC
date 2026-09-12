using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FGA.Utilities
{
    public static class SMSHelper
    {
        // NOTA: Estos valores deberían venir de un Web.config o Base de Datos
        private static readonly string ApiUrl = "https://api.sms-provider.com/send"; 
        private static readonly string ApiKey = "YOUR_API_KEY";

        public static async Task<bool> SendSMS(string telefono, string mensaje)
        {
            // MOCK: Para propósitos de desarrollo, siempre retorna true si el número no es vacío
            if (string.IsNullOrEmpty(telefono)) return false;

            try
            {
                // Aquí iría la integración real (ej. Twilio, Infobip, etc.)
                // using (var client = new HttpClient())
                // {
                //     var content = new StringContent($"{{\"to\":\"{telefono}\", \"msg\":\"{mensaje}\"}}", Encoding.UTF8, "application/json");
                //     var response = await client.PostAsync(ApiUrl, content);
                //     return response.IsSuccessStatusCode;
                // }
                
                System.Diagnostics.Debug.WriteLine(string.Format("[SMS MOCK] Enviando a {0}: {1}", telefono, mensaje));
                return true; 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("[SMS ERROR] {0}", ex.Message));
                return false;
            }
        }
    }
}
