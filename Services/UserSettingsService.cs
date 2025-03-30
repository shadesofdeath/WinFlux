using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace WinFlux.Services
{
    public class UserSettingsService
    {
        private static UserSettingsService _instance;
        public static UserSettingsService Instance => _instance ??= new UserSettingsService();

        private UserSettings _settings;
        private readonly string _settingsFilePath;

        private UserSettingsService()
        {
            // Set the file path in application data folder
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WinFlux");
                
            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            _settingsFilePath = Path.Combine(appDataFolder, "settings.json");
            
            // Load existing settings or create default ones
            LoadSettings();
        }

        public string PreferredLanguage
        {
            get => _settings.PreferredLanguage;
            set
            {
                if (_settings.PreferredLanguage != value)
                {
                    _settings.PreferredLanguage = value;
                    SaveSettings();
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<UserSettings>(json);
                }
                else
                {
                    // Create default settings
                    _settings = new UserSettings
                    {
                        PreferredLanguage = "English" // Default to English
                    };
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user settings: {ex.Message}");
                
                // Create default settings if loading fails
                _settings = new UserSettings
                {
                    PreferredLanguage = "English"
                };
            }
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user settings: {ex.Message}");
            }
        }
    }

    public class UserSettings
    {
        public string PreferredLanguage { get; set; } = "English";
    }
} 