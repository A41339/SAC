using System;
using System.ServiceModel;

namespace FGA.Utility
{
    /// <summary>
    /// Helper centralizado para invocación y administración segura del ciclo de vida de clientes WCF.
    /// Previene fugas de sockets y puertos TCP en IIS al asegurar que los canales en estado Faulted se aborten
    /// y los canales normales se cierren limpiamente sin lanzar excepciones secundarias en Dispose().
    /// </summary>
    public static class WcfHelper
    {
        /// <summary>
        /// Cierra de forma segura un cliente o canal WCF sin arrojar excepciones no controladas
        /// si el canal se encuentra en estado Faulted o ya cerrado.
        /// </summary>
        /// <param name="client">Instancia que implementa ICommunicationObject (ClientBase)</param>
        public static void SafeClose(this ICommunicationObject client)
        {
            if (client == null) return;

            try
            {
                if (client.State != CommunicationState.Faulted)
                {
                    client.Close();
                }
                else
                {
                    client.Abort();
                }
            }
            catch (CommunicationException)
            {
                client.Abort();
            }
            catch (TimeoutException)
            {
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
            }
        }

        /// <summary>
        /// Ejecuta una función sobre un cliente WCF nuevo, asegurando el cierre o aborto del canal al finalizar.
        /// </summary>
        public static TResult Execute<TClient, TResult>(Func<TClient, TResult> action)
            where TClient : ICommunicationObject, new()
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var client = new TClient();
            try
            {
                var result = action(client);
                client.SafeClose();
                return result;
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una acción sin retorno sobre un cliente WCF nuevo, asegurando el cierre o aborto del canal al finalizar.
        /// </summary>
        public static void Execute<TClient>(Action<TClient> action)
            where TClient : ICommunicationObject, new()
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var client = new TClient();
            try
            {
                action(client);
                client.SafeClose();
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
        }
    }
}
