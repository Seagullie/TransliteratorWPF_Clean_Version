using Newtonsoft.Json;
using System;
using System.IO;
using TransliteratorWPF_Version.Models;


namespace TransliteratorWPF_Version.Services
{
    public partial class SettingsService
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public bool ShouldPlayToggleSound { get; set; }

        public bool ShouldStartMinimized { get; set; }

        public bool ShouldRunAtStartup { get; set; }

        public bool IsStateOverlayEnabled { get; set; }

        public bool IsTranslitEnabledOnStartup { get; set; }

        public bool IsAltShiftGlobalShortcutDisabled { get; set; }

        public bool IsDisplayOfBufferCharactersEnabled { get; set; }

        // HotKeys
        public HotKey ToggleHotKey { get; set; }

        // Events
        public event EventHandler SettingsReset;

        public event EventHandler SettingsLoaded;

        public event EventHandler SettingsSaved;

        public SettingsService()
        {
        }

        // TODO: Write the implementation
        public void Reset()
        {
            SettingsReset?.Invoke(this, EventArgs.Empty);
        }

        // TODO: Improve, catch exceptions
        public void Load()
        {
            var configurationFilePath = "Settings.json";

            if (File.Exists(configurationFilePath))
            {
                var jsonFile = File.ReadAllText(configurationFilePath);
                JsonConvert.PopulateObject(jsonFile, this, JsonSerializerSettings);

                SettingsLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        // TODO: Improve, catch exceptions
        public void Save()
        {
            string contents = JsonConvert.SerializeObject(this, JsonSerializerSettings);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.json", contents);

            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}
