using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using System.Collections.Generic;
using WinFlux.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using iNKORE.UI.WPF.Modern.Controls;

namespace WinFlux.Services
{
    public class PresetService
    {
        // Save preset to XML file
        public static bool ExportPreset(string filePath)
        {
            try
            {
                // Create preset object with all settings
                var preset = new SystemPreset
                {
                    // Gather settings from all pages
                    PrivacySettings = CollectPrivacySettings(),
                    TelemetrySettings = CollectTelemetrySettings(),
                    PerformanceSettings = CollectPerformanceSettings(),
                    GameSettings = CollectGameSettings(),
                    AppSettings = CollectAppSettings(),
                    AppInstallerSettings = CollectAppInstallerSettings()
                };

                // Serialize to XML
                var serializer = new XmlSerializer(typeof(SystemPreset));
                using (var writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, preset);
                }

                return true;
            }
            catch (Exception ex)
            {
                var errorMsg = Application.Current.Resources["PresetMessageBox_ExportError"] as string;
                MessageBox.Show($"{errorMsg} {ex.Message}", 
                    Application.Current.Resources["MessageBox_Error"] as string, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return false;
            }
        }

        // Load preset from XML file
        public static SystemPreset ImportPreset(string filePath)
        {
            try
            {
                // Deserialize from XML
                var serializer = new XmlSerializer(typeof(SystemPreset));
                using (var reader = new StreamReader(filePath))
                {
                    return (SystemPreset)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = Application.Current.Resources["PresetMessageBox_ImportError"] as string;
                MessageBox.Show($"{errorMsg} {ex.Message}", 
                    Application.Current.Resources["MessageBox_Error"] as string, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return null;
            }
        }

        // Apply the imported preset to all pages
        public static bool ApplyPreset(SystemPreset preset)
        {
            try
            {
                // Check if there are apps to uninstall in the preset
                if (preset.AppSettings != null && preset.AppSettings.AppsToRemove.Count > 0)
                {
                    var confirmMessage = Application.Current.Resources["PresetMessageBox_AppsPageWarning"] as string;
                    var result = MessageBox.Show(confirmMessage,
                        Application.Current.Resources["MessageBox_Warning"] as string,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result != MessageBoxResult.Yes)
                    {
                        preset.AppSettings.AppsToRemove.Clear();
                    }
                }

                // Apply settings to each page
                ApplyPrivacySettings(preset.PrivacySettings);
                ApplyTelemetrySettings(preset.TelemetrySettings);
                ApplyPerformanceSettings(preset.PerformanceSettings);
                ApplyGameSettings(preset.GameSettings);
                ApplyAppSettings(preset.AppSettings);
                ApplyAppInstallerSettings(preset.AppInstallerSettings);

                return true;
            }
            catch (Exception ex)
            {
                var errorMsg = Application.Current.Resources["PresetMessageBox_ImportError"] as string;
                MessageBox.Show($"{errorMsg} {ex.Message}", 
                    Application.Current.Resources["MessageBox_Error"] as string, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return false;
            }
        }

        #region Collect Settings Methods
        private static PrivacySettings CollectPrivacySettings()
        {
            var settings = new PrivacySettings();
            
            // Collect settings from PrivacyPage if available
            if (Pages.PrivacyPage.Instance != null)
            {
                Dictionary<string, bool> toggles = Pages.PrivacyPage.Instance.GetToggleSettings();
                settings.ToggleSettings = new SerializableDictionary<string, bool>(toggles);
            }
            
            return settings;
        }

        private static TelemetrySettings CollectTelemetrySettings()
        {
            var settings = new TelemetrySettings();
            
            // Collect settings from TelemetryPage if available
            if (Pages.TelemetryPage.Instance != null)
            {
                Dictionary<string, bool> toggles = Pages.TelemetryPage.Instance.GetToggleSettings();
                settings.ToggleSettings = new SerializableDictionary<string, bool>(toggles);
                
                // Get list of blocked hosts if available
                settings.BlockedHosts = Pages.TelemetryPage.Instance.GetBlockedHosts();
            }
            
            return settings;
        }

        private static PerformanceSettings CollectPerformanceSettings()
        {
            var settings = new PerformanceSettings();
            
            // Collect settings from PerformancePage if available
            if (Pages.PerformancePage.Instance != null)
            {
                Dictionary<string, bool> toggles = Pages.PerformancePage.Instance.GetToggleSettings();
                settings.ToggleSettings = new SerializableDictionary<string, bool>(toggles);
                
                Dictionary<string, double> sliderValues = Pages.PerformancePage.Instance.GetSliderValues();
                settings.SliderValues = new SerializableDictionary<string, double>(sliderValues);
            }
            
            return settings;
        }

        private static GameSettings CollectGameSettings()
        {
            var settings = new GameSettings();
            
            // Collect settings from GameOptimizationPage if available
            if (Pages.GameOptimizationPage.Instance != null)
            {
                Dictionary<string, bool> toggles = Pages.GameOptimizationPage.Instance.GetToggleSettings();
                settings.ToggleSettings = new SerializableDictionary<string, bool>(toggles);
            }
            
            return settings;
        }

        private static AppSettings CollectAppSettings()
        {
            // Implement this to gather current app settings and list of apps to remove
            var appSettings = new AppSettings();
            
            // Get selected apps from AppsPage if available
            if (Pages.AppsPage.Instance != null)
            {
                appSettings.AppsToRemove = Pages.AppsPage.Instance.GetSelectedAppsForPreset();
            }
            
            return appSettings;
        }
        
        private static AppInstallerSettings CollectAppInstallerSettings()
        {
            var settings = new AppInstallerSettings();
            
            // Collect settings from AppInstallerPage if available
            if (Pages.AppInstallerPage.Instance != null)
            {
                Dictionary<string, bool> toggles = Pages.AppInstallerPage.Instance.GetToggleSettings();
                settings.AppsToInstall = new SerializableDictionary<string, bool>(toggles);
                settings.PreferredPackageManager = Pages.AppInstallerPage.Instance.GetPreferredPackageManager();
            }
            
            return settings;
        }
        #endregion

        #region Apply Settings Methods
        private static void ApplyPrivacySettings(PrivacySettings settings)
        {
            if (settings == null) return;
            
            // Apply settings to PrivacyPage if available
            if (Pages.PrivacyPage.Instance != null && settings.ToggleSettings.Count > 0)
            {
                Dictionary<string, bool> toggles = new Dictionary<string, bool>(settings.ToggleSettings);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Pages.PrivacyPage.Instance.ApplyToggleSettings(toggles);
                });
            }
        }

        private static void ApplyTelemetrySettings(TelemetrySettings settings)
        {
            if (settings == null) return;
            
            // Apply settings to TelemetryPage if available
            if (Pages.TelemetryPage.Instance != null)
            {
                // Apply toggle settings if there are any
                if (settings.ToggleSettings.Count > 0)
                {
                    Dictionary<string, bool> toggles = new Dictionary<string, bool>(settings.ToggleSettings);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Pages.TelemetryPage.Instance.ApplyToggleSettings(toggles);
                    });
                }
                
                // Apply blocked hosts if there are any
                if (settings.BlockedHosts.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Pages.TelemetryPage.Instance.ApplyBlockedHosts(settings.BlockedHosts);
                    });
                }
            }
        }

        private static void ApplyPerformanceSettings(PerformanceSettings settings)
        {
            if (settings == null) return;
            
            // Apply settings to PerformancePage if available
            if (Pages.PerformancePage.Instance != null)
            {
                // Apply toggle settings if there are any
                if (settings.ToggleSettings.Count > 0)
                {
                    Dictionary<string, bool> toggles = new Dictionary<string, bool>(settings.ToggleSettings);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Pages.PerformancePage.Instance.ApplyToggleSettings(toggles);
                    });
                }
                
                // Apply slider values if there are any
                if (settings.SliderValues.Count > 0)
                {
                    Dictionary<string, double> sliderValues = new Dictionary<string, double>(settings.SliderValues);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Pages.PerformancePage.Instance.ApplySliderValues(sliderValues);
                    });
                }
            }
        }

        private static void ApplyGameSettings(GameSettings settings)
        {
            if (settings == null) return;
            
            // Apply settings to GameOptimizationPage if available
            if (Pages.GameOptimizationPage.Instance != null && settings.ToggleSettings.Count > 0)
            {
                Dictionary<string, bool> toggles = new Dictionary<string, bool>(settings.ToggleSettings);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Pages.GameOptimizationPage.Instance.ApplyToggleSettings(toggles);
                });
            }
        }

        private static void ApplyAppSettings(AppSettings settings)
        {
            if (settings == null) return;
            
            // If there are apps to remove, use the AppsPage instance to handle it
            if (settings.AppsToRemove.Count > 0 && Pages.AppsPage.Instance != null)
            {
                // Need to dispatch to UI thread since we might be running on background thread
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await Pages.AppsPage.Instance.ApplyPresetAndUninstallApps(settings.AppsToRemove);
                });
            }
        }
        
        private static void ApplyAppInstallerSettings(AppInstallerSettings settings)
        {
            if (settings == null) return;
            
            // Eğer AppInstallerPage henüz oluşturulmamışsa, zorla oluştur
            if (Pages.AppInstallerPage.Instance == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Bu zorla sayfayı oluşturacak
                    var appInstallerPage = new Pages.AppInstallerPage();
                    
                    // Sayfa yükleme tamamlanması için kısa bir süre bekle
                    System.Threading.Thread.Sleep(500);
                });
            }
            
            // Apply settings to AppInstallerPage if available
            if (Pages.AppInstallerPage.Instance != null)
            {
                // Apply toggle settings if there are any
                if (settings.AppsToInstall.Count > 0)
                {
                    Dictionary<string, bool> toggles = new Dictionary<string, bool>(settings.AppsToInstall);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Pages.AppInstallerPage.Instance.ApplyToggleSettings(toggles);
                    });
                }
                
                // Apply preferred package manager
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Pages.AppInstallerPage.Instance.SetPreferredPackageManager(settings.PreferredPackageManager);
                });
            }
        }
        #endregion
    }
} 