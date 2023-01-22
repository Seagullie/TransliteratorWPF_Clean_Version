using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransliteratorWPF_Version.Services
{
    public static class LoggerService
    {
        public delegate void LogMessageDelegate(string message, string? color = "Black");

        public static event LogMessageDelegate NewLogMessageEvent;

        public static void LogMessage(string message, string? color = null)
        {
            NewLogMessageEvent?.Invoke(message, color);
        }
    }
}