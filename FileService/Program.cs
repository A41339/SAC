using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace FileService
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// Soporta ejecución como Servicio de Windows y también ejecución interactiva de consola para depuración.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine("==========================================================");
                Console.WriteLine("    FileService - Modo Interactivo / Consola de Pruebas    ");
                Console.WriteLine("==========================================================");

                FileService service = new FileService();

                // Si se pasa el parámetro /runonce o -once, ejecuta una pasada sincrónica y sale
                if (args != null && args.Any(a => a.Equals("/runonce", StringComparison.OrdinalIgnoreCase) || a.Equals("-once", StringComparison.OrdinalIgnoreCase)))
                {
                    service.EjecutarUnaVez();
                    return;
                }

                service.StartInteractive(args);

                Console.WriteLine();
                Console.WriteLine("El servicio está ejecutándose activamente.");
                Console.WriteLine("Presione ENTER o Ctrl+C en cualquier momento para detener el servicio...");

                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };

                if (!Console.IsInputRedirected)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            Console.ReadLine();
                            exitEvent.Set();
                        }
                        catch { }
                    });
                }

                exitEvent.WaitOne();

                service.StopInteractive();
                Console.WriteLine("Servicio detenido correctamente.");
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new FileService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
