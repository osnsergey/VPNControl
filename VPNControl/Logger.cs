using System;
using System.IO;

namespace VPNControl
{
    static class Logger
    {
        private static readonly string LogFile = "VPNControl.log";
        private static readonly object Lock = new object();
        private static bool logEnabled = false;

        public static void setEnabled(bool enabled = true)
        {
            logEnabled = enabled;
        }

        public static void log(string message)
        {
            if (!logEnabled) return;

            lock (Lock)
            {
                File.AppendAllText(
                    LogFile,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
        }

        public static void log(string direction, string text)
        {
            if (!logEnabled) return;

            lock (Lock)
            {
                File.AppendAllText(
                    LogFile,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{direction}] {text}{Environment.NewLine}");
            }
        }
    }
}
