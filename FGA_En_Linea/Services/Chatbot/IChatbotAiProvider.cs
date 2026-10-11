using System;
using System.Threading.Tasks;

namespace FGA.Services.Chatbot
{
    public interface IChatbotAiProvider
    {
        Task<string> GenerarRespuestaAsync(string promptSistema, string historial, string preguntaUsuario);
    }
}
