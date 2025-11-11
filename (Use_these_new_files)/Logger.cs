using System;
using System.IO;

namespace HelpCleaner.Utils
{
    public static class Logger
    {
        private static readonly string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpCleaner.log");

        public static void Log(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.AppendAllText(logPath, $"[{timestamp}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public static string GetLogPath()
        {
            return logPath;
        }
    }
}
