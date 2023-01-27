using Newtonsoft.Json;
using System;
using System.IO;
using TransliteratorWPF_Version.Models;

namespace TransliteratorWPF_Version.Services
{
    public partial class SettingsService
    {
        private const string configurationFilePath = "Settings.json";

        private static SettingsService _instance;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public bool IsToggleSoundOn { get; set; } = true;

        public bool IsMinimizedStartEnabled { get; set; }

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

        public void Load()
        {
            if (File.Exists(configurationFilePath))
            {
                var settings = File.ReadAllText(configurationFilePath);
                JsonConvert.PopulateObject(settings, this, JsonSerializerSettings);          
            }

            SynchronizeJSONAndWindowsStartupSettings();

            SettingsLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void Save()
        {
            string settings = JsonConvert.SerializeObject(this, JsonSerializerSettings);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + configurationFilePath, settings);

            SynchronizeJSONAndWindowsStartupSettings();

            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        private void SynchronizeJSONAndWindowsStartupSettings()
        {
            bool isHasStartProgEntry = StartupMethods.HasStartProgEntry();

            if (IsAutoStartEnabled && !isHasStartProgEntry)
                StartupMethods.WriteStartProgEntry();
            if (!IsAutoStartEnabled && isHasStartProgEntry)
                StartupMethods.DeleteStartProgEntry();
        }
    }
}