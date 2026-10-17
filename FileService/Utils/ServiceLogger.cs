using System;
using System.IO;

namespace FileService.Utils
{
    public static class ServiceLogger
    {
        private static readonly object _lock = new object();
        private static readonly string _logPath;

        static ServiceLogger()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _logPath = Path.Combine(baseDir, "FileService.log");
            }
            catch
            {
                _logPath = @"C:\Logs\FileService.log";
            }
        }

        public static void Log(string message)
        {
            string line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message);

            try
            {
                if (Environment.UserInteractive)
                {
                    Console.WriteLine(line);
                }
            }
            catch
            {
            }

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logPath, line + Environment.NewLine);
                }
            }
            catch
            {
            }
        }

        public static void LogError(string context, Exception ex)
        {
            Log(string.Format("ERROR en {0}: {1}{2}{3}", context, ex.Message, Environment.NewLine, ex.StackTrace));
        }
    }
}
