namespace TransliteratorWPF_Version.Services
{
    public class NewLogMessageEventArg
    {
        public string Message { get; set; }
        public string Color { get; set; }

        public NewLogMessageEventArg(string message, string color)
        {
            Message = message;
            Color = color;
        }
    }
}