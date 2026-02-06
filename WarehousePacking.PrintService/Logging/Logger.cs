using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.PrintService.Logging
{
    public static class Logger
    {
        private static readonly object _lock = new object();

        private static readonly string logDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        private static readonly string logFilePath =
            Path.Combine(logDir, "PrintingService.log");

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";

                lock (_lock)
                {
                    File.AppendAllText(logFilePath, logLine + Environment.NewLine);
                }
            }
            catch
            {
                // silently ignore logging errors to avoid crashing service
            }
        }
    }
}