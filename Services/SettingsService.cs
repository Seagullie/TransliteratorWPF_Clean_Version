using Newtonsoft.Json;
using System;
using System.IO;
using TransliteratorWPF_Version.Models;

namespace TransliteratorWPF_Version.Services
{
    public partial class SettingsService
    {
        private static SettingsService _instance;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public bool IsToggleSoundOn { get; set; } = true;

        public bool IsMinimizedStartEnabled { get; set; }

        [JsonIgnore]
        public bool IsAutoStartEnabled { get; set; }

        public bool IsStateOverlayEnabled { get; set; } = true;

        public bool IsTranslitEnabledAtStartup { get; set; }

        public bool IsAltShiftGlobalShortcutEnabled { get; set; } = true;

        public bool IsBufferInputEnabled { get; set; }

        public ModernWpf.ApplicationTheme ApplicationTheme { get; set; } = ModernWpf.ApplicationTheme.Light;

        public string LastSelectedTranslitTable { get; set; }

        public string PathToCustomToggleOnSound { get; set; }

        public string PathToCustomToggleOffSound { get; set; }

        // HotKeys
        public HotKey ToggleHotKey { get; set; }

        // Events
        public event EventHandler SettingsReset;

        public event EventHandler SettingsLoaded;

        public event EventHandler SettingsSaved;

        private SettingsService()
        {
        }

        public static SettingsService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SettingsService();
            }
            return _instance;
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

            IsAutoStartEnabled = StartupMethods.HasStartProgEntry();
        }

        // TODO: Improve, catch exceptions
        public void Save()
        {
            string contents = JsonConvert.SerializeObject(this, JsonSerializerSettings);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.json", contents);

            SettingsSaved?.Invoke(this, EventArgs.Empty);

            if (IsAutoStartEnabled)
                StartupMethods.WriteStartProgEntry();
            else
                StartupMethods.DeleteStartProgEntry();
        }
    }
}