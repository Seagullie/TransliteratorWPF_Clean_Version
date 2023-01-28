using CommunityToolkit.WinUI.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransliteratorWPF_Version.Services
{
    // https://learn.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=uwp

    internal class SystemNotificationService
    {
        private static SystemNotificationService _instance;

        // TODO: adapt this snippet from LoggerService
        public event EventHandler<NewLogMessageEventArg> NewNotification;

        public void CreateNotification(object sender, string message, string color = null)
        {
            NewNotification?.Invoke(sender, new NewLogMessageEventArg(message, color));
        }

        // at this point with so many singletons we need a singleton class to inherit from, haha
        public static SystemNotificationService GetInstance()
        {
            _instance ??= new SystemNotificationService();
            return _instance;
        }

        // TODO: figure out how to enable .Show()
        private void CreateNotification(string text)
        {
            // Requires Microsoft.Toolkit.Uwp.Notifications NuGet package version 7.0 or greater
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText("Andrew sent you a picture")
                .AddText("Check this out, The Enchantments in Washington!")
                .Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 6 (or later), then your TFM must be net6.0-windows10.0.17763.0 or greater
        }
    }
}