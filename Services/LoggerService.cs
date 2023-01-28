using System;

namespace TransliteratorWPF_Version.Services
{
    public class LoggerService
    {
        private static LoggerService _instance;

        public event EventHandler<NewLogMessageEventArg> NewLogMessage;

        private LoggerService()
        {
        }

        public static LoggerService GetInstance()
        {
            _instance ??= new LoggerService();
            return _instance;
        }

        public void LogMessage(object sender, string message, string color = null)
        {
            NewLogMessage?.Invoke(sender, new NewLogMessageEventArg(message, color));
        }
    }
}