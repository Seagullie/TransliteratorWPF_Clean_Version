using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransliteratorWPF_Version.Helpers;

namespace TransliteratorWPF_Version.Services.KeyboardServices
{
    public class KeyInjectorBase
    {
        // KBE == Keyboard Event
        public int KBEInjectionsIntervalMilliseconds = 0;

        protected LoggerService loggerService;

        protected KeyInjectorBase()
        {
            loggerService = LoggerService.GetInstance();
        }

        public async void Write(string text)
        {
            loggerService.LogMessage(this, "Writing this: " + text);

            foreach (char character in text)
            {
                if (KBEInjectionsIntervalMilliseconds != 0)
                {
                    await Task.Delay(KBEInjectionsIntervalMilliseconds);
                }

                WinAPI.WriteStringThroughSendInput(character.ToString());
            }
        }
    }
}