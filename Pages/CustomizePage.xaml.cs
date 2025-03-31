using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Win32;
using WinFlux.Helpers;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace WinFlux.Pages
{
    public partial class CustomizePage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        // Add static reference for preset service to access
        public static CustomizePage Instance { get; private set; }

        public CustomizePage()
        {
            InitializeComponent();
            
            // Store the instance for preset service to use
            Instance = this;
            
            LoadSettings();
            AttachToggleEvents();
        }

        private void LoadSettings()
        {
            // Bing Search - Enabled (1) = Bing search enabled, Disabled (0) = Bing search disabled
            toggleBingSearch.IsOn = !((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled") ?? 1) == 0);

            // Taskbar Alignment - Center (1) = Center alignment, Left (0) = Left alignment
            toggleTaskbarAlignment.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl") ?? 1) == 1);

            // Dark Mode - Light (1) = Light mode, Dark (0) = Dark mode
            int lightThemeValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme") ?? 1);
            toggleDarkMode.IsOn = lightThemeValue == 0; // 0 = Dark mode açık, 1 = Light mode açık

            // Detailed BSoD - Enabled (1) = Detailed BSoD, Disabled (0) = Simple BSoD
            toggleDetailedBSoD.IsOn = ((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters") ?? 0) == 1);

            // Mouse Acceleration - 1 = enabled, 0 = disabled
            toggleMouseAcceleration.IsOn = (RegHelper.GetStringValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed") ?? "1") == "1";

            // NumLock on Startup - 2 = enabled, 0 = disabled
            toggleNumLock.IsOn = (RegHelper.GetStringValue(RegistryHive.Users, @".DEFAULT\Control Panel\Keyboard", "InitialKeyboardIndicators") ?? "0") == "2";

            // Search Button in Taskbar - 0 = hidden, 1 = show icon, 2 = show search box
            toggleSearchButton.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode") ?? 1) != 0);

            // Show File Extensions - 0 = show, 1 = hide
            toggleShowFileExt.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt") ?? 1) == 0);

            // Show Hidden Files - 1 = show, 2 = hide
            toggleShowHiddenFiles.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden") ?? 2) == 1);

            // Snap Assist Flyout - 0 = enabled, 1 = disabled
            toggleSnapAssistFlyout.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout") ?? 0) == 0);

            // Snap Assist Suggestion - 1 = enabled, 0 = disabled
            toggleSnapAssistSuggestion.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SnapAssist") ?? 1) == 1);

            // Snap Window - 1 = enabled, 0 = disabled
            toggleSnapWindow.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapBar") ?? 1) == 1);

            // Sticky Keys - 510 = enabled, 26 = disabled
            toggleStickyKeys.IsOn = (RegHelper.GetStringValue(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags") ?? "26") == "510";

            // Task View Button in Taskbar - 0 = hide, 1 = show
            toggleTaskViewButton.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton") ?? 1) == 1);

            // Verbose Messages During Logon - 1 = enabled, 0 = disabled
            toggleVerboseLogon.IsOn = ((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus") ?? 0) == 1);

            // Widgets Button in Taskbar - 0 = hide, 1 = show
            toggleWidgetsButton.IsOn = ((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa") ?? 1) == 1);

            // Classic Right-Click Menu - InprocServer32 key exists = enabled
            try
            {
                bool keyExists = false;
                using (RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"))
                {
                    keyExists = baseKey != null;
                }
                toggleClassicRightClick.IsOn = keyExists;
            }
            catch
            {
                toggleClassicRightClick.IsOn = false;
            }
            
            // Gallery in Navigation Pane - 1 = shown, 0 = hidden
            try
            {
                using (RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}"))
                {
                    if (baseKey != null)
                    {
                        object value = baseKey.GetValue("System.IsPinnedToNameSpaceTree");
                        // IsOn = true ise galeri kaldırılacak, false ise gösterilecek
                        // Registry'de 1 = göster, 0 = gizle olduğu için tersini alıyoruz
                        toggleGalleryNavigationPane.IsOn = value == null || (int)value != 1;
                    }
                    else
                    {
                        toggleGalleryNavigationPane.IsOn = true; // Varsayılan olarak kaldır
                    }
                }
            }
            catch
            {
                toggleGalleryNavigationPane.IsOn = true; // Hata durumunda varsayılan olarak kaldır
            }
        }

        private void AttachToggleEvents()
        {
            // Bing Search
            toggleBingSearch.Toggled += (s, e) => SetBingSearch(!toggleBingSearch.IsOn);

            // Taskbar Alignment
            toggleTaskbarAlignment.Toggled += (s, e) => SetTaskbarAlignment(toggleTaskbarAlignment.IsOn);

            // Dark Mode
            toggleDarkMode.Toggled += (s, e) => SetDarkMode(toggleDarkMode.IsOn);

            // Detailed BSoD
            toggleDetailedBSoD.Toggled += (s, e) => SetDetailedBSoD(toggleDetailedBSoD.IsOn);

            // Mouse Acceleration
            toggleMouseAcceleration.Toggled += (s, e) => SetMouseAcceleration(toggleMouseAcceleration.IsOn);

            // NumLock on Startup
            toggleNumLock.Toggled += (s, e) => SetNumLockOnStartup(toggleNumLock.IsOn);

            // Search Button in Taskbar
            toggleSearchButton.Toggled += (s, e) => SetSearchButtonInTaskbar(toggleSearchButton.IsOn);

            // Show File Extensions
            toggleShowFileExt.Toggled += (s, e) => SetShowFileExtensions(toggleShowFileExt.IsOn);

            // Show Hidden Files
            toggleShowHiddenFiles.Toggled += (s, e) => SetShowHiddenFiles(toggleShowHiddenFiles.IsOn);

            // Snap Assist Flyout
            toggleSnapAssistFlyout.Toggled += (s, e) => SetSnapAssistFlyout(toggleSnapAssistFlyout.IsOn);

            // Snap Assist Suggestion
            toggleSnapAssistSuggestion.Toggled += (s, e) => SetSnapAssistSuggestion(toggleSnapAssistSuggestion.IsOn);

            // Snap Window
            toggleSnapWindow.Toggled += (s, e) => SetSnapWindow(toggleSnapWindow.IsOn);

            // Sticky Keys
            toggleStickyKeys.Toggled += (s, e) => SetStickyKeys(toggleStickyKeys.IsOn);

            // Task View Button in Taskbar
            toggleTaskViewButton.Toggled += (s, e) => SetTaskViewButtonInTaskbar(toggleTaskViewButton.IsOn);

            // Verbose Messages During Logon
            toggleVerboseLogon.Toggled += (s, e) => SetVerboseMessagesOnLogon(toggleVerboseLogon.IsOn);

            // Widgets Button in Taskbar
            toggleWidgetsButton.Toggled += (s, e) => SetWidgetsButtonInTaskbar(toggleWidgetsButton.IsOn);

            // Classic Right-Click Menu
            toggleClassicRightClick.Toggled += (s, e) => SetClassicRightClickMenu(toggleClassicRightClick.IsOn);
            
            // Gallery in Navigation Pane
            toggleGalleryNavigationPane.Toggled += (s, e) => SetGalleryNavigationPane(toggleGalleryNavigationPane.IsOn);
        }

        private void SetBingSearch(bool disable)
        {
            try
            {
                if (disable)
                {
                    RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, RegistryValueKind.DWord);
                }
                else
                {
                    RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetTaskbarAlignment(bool center)
        {
            try
            {
                int value = center ? 1 : 0;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetDarkMode(bool darkModeEnabled)
        {
            try
            {
                // darkModeEnabled true ise 0 (karanlık), false ise 1 (aydınlık) değeri ayarla
                int lightThemeValue = darkModeEnabled ? 0 : 1;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", lightThemeValue, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", lightThemeValue, RegistryValueKind.DWord);

                // Explorer'ı yeniden başlatarak tema değişikliğini hemen uygulayalım
                try
                {
                    Process[] explorerProcesses = Process.GetProcessesByName("explorer");
                    foreach (Process explorerProcess in explorerProcesses)
                    {
                        explorerProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Explorer sonlandırılırken hata: {ex.Message}");
                    // Hata işlemi durdurmamalı, devam etmeli
                }

                // Sonra yeniden başlat
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetDetailedBSoD(bool enabled)
        {
            try
            {
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetMouseAcceleration(bool enabled)
        {
            try
            {
                // Mouse Acceleration - 1 = enabled, 0 = disabled
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", value, RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", value == 1 ? "6" : "0", RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", value == 1 ? "10" : "0", RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetNumLockOnStartup(bool enabled)
        {
            try
            {
                // NumLock on Startup - 2 = enabled, 0 = disabled
                int value = enabled ? 2 : 0;
                RegHelper.SetValue(RegistryHive.Users, @".DEFAULT\Control Panel\Keyboard", "InitialKeyboardIndicators", value, RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetSearchButtonInTaskbar(bool enabled)
        {
            try
            {
                // Search Button in Taskbar - 0 = hidden, 1 = show icon, 2 = show search box
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetShowFileExtensions(bool enabled)
        {
            try
            {
                // Show File Extensions - 0 = show, 1 = hide
                int value = enabled ? 0 : 1;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetShowHiddenFiles(bool enabled)
        {
            try
            {
                // Show Hidden Files - 1 = show, 2 = hide
                int value = enabled ? 1 : 2;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetSnapAssistFlyout(bool enabled)
        {
            try
            {
                // Snap Assist Flyout - 0 = enabled, 1 = disabled
                int value = enabled ? 0 : 1;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetSnapAssistSuggestion(bool enabled)
        {
            try
            {
                // Snap Assist Suggestion - 1 = enabled, 0 = disabled
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SnapAssist", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetSnapWindow(bool enabled)
        {
            try
            {
                // Snap Window - 1 = enabled, 0 = disabled
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapBar", value, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Desktop", "WindowArrangementActive", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetStickyKeys(bool enabled)
        {
            try
            {
                // Sticky Keys - 510 = enabled, 26 = disabled
                int value = enabled ? 510 : 26;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", value, RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetTaskViewButtonInTaskbar(bool enabled)
        {
            try
            {
                // Task View Button in Taskbar - 0 = hide, 1 = show
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetVerboseMessagesOnLogon(bool enabled)
        {
            try
            {
                // Verbose Messages During Logon - 1 = enabled, 0 = disabled
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetWidgetsButtonInTaskbar(bool enabled)
        {
            try
            {
                // Widgets Button in Taskbar - 0 = hide, 1 = show
                int value = enabled ? 1 : 0;
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", value, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetClassicRightClickMenu(bool enabled)
        {
            try
            {
                if (enabled)
                {
                    // Klasik (eski) sağ tık menüsünü aktifleştir
                    // Önce ana klasörü oluştur
                    if (!RegHelper.SubKeyExist(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"))
                    {
                        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"))
                        {
                            // Sadece anahtarı oluştur
                        }
                    }

                    // Sonra InprocServer32 alt klasörünü oluştur
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"))
                    {
                        // Boş değer ata
                        key.SetValue("", "", RegistryValueKind.String);
                    }
                }
                else
                {
                    // Modern (yeni) menüyü aktifleştir (klasik menüyü devre dışı bırak)
                    try
                    {
                        // Ağaç yapısını kaldır
                        Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", false);
                    }
                    catch (Exception ex)
                    {
                        // Anahtar yoksa veya başka bir hata varsa atla
                        Debug.WriteLine($"Registry anahtarı silinirken hata: {ex.Message}");
                    }
                }

                // Explorer'ı yeniden başlat
                try
                {
                    Process[] explorerProcesses = Process.GetProcessesByName("explorer");
                    foreach (Process explorerProcess in explorerProcesses)
                    {
                        explorerProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Explorer sonlandırılırken hata: {ex.Message}");
                }

                // Kısa bir süre bekle
                System.Threading.Thread.Sleep(500);

                // Explorer'ı yeniden başlat
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        private void SetGalleryNavigationPane(bool remove)
        {
            try
            {
                // true = kaldır (0), false = göster (1)
                int value = remove ? 0 : 1;
                
                // Anahtarı oluştur veya aç
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}"))
                {
                    // Değeri ayarla
                    key.SetValue("System.IsPinnedToNameSpaceTree", value, RegistryValueKind.DWord);
                }
                
                // Explorer'ı yeniden başlat
                try
                {
                    Process[] explorerProcesses = Process.GetProcessesByName("explorer");
                    foreach (Process explorerProcess in explorerProcesses)
                    {
                        explorerProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Explorer sonlandırılırken hata: {ex.Message}");
                }

                // Kısa bir süre bekle
                System.Threading.Thread.Sleep(500);

                // Explorer'ı yeniden başlat
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Application.Current.Resources["TelemetryPageMessageBox_CommandError"]} {ex.Message}",
                    Application.Current.Resources["MessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Method to get toggle settings for preset service
        public Dictionary<string, bool> GetToggleSettings()
        {
            return new Dictionary<string, bool>
            {
                { "BingSearch", toggleBingSearch.IsOn },
                { "TaskbarAlignment", toggleTaskbarAlignment.IsOn },
                { "DarkMode", toggleDarkMode.IsOn },
                { "DetailedBSoD", toggleDetailedBSoD.IsOn },
                { "MouseAcceleration", toggleMouseAcceleration.IsOn },
                { "NumLock", toggleNumLock.IsOn },
                { "SearchButton", toggleSearchButton.IsOn },
                { "ShowFileExt", toggleShowFileExt.IsOn },
                { "ShowHiddenFiles", toggleShowHiddenFiles.IsOn },
                { "SnapAssistFlyout", toggleSnapAssistFlyout.IsOn },
                { "SnapAssistSuggestion", toggleSnapAssistSuggestion.IsOn },
                { "SnapWindow", toggleSnapWindow.IsOn },
                { "StickyKeys", toggleStickyKeys.IsOn },
                { "TaskViewButton", toggleTaskViewButton.IsOn },
                { "VerboseLogon", toggleVerboseLogon.IsOn },
                { "WidgetsButton", toggleWidgetsButton.IsOn },
                { "ClassicRightClick", toggleClassicRightClick.IsOn },
                { "GalleryNavigationPane", toggleGalleryNavigationPane.IsOn }
            };
        }

        // Method to apply preset settings
        public void ApplyToggleSettings(Dictionary<string, bool> settings)
        {
            if (settings.TryGetValue("BingSearch", out bool bingSearch))
            {
                toggleBingSearch.IsOn = bingSearch;
            }

            if (settings.TryGetValue("TaskbarAlignment", out bool taskbarAlignment))
            {
                toggleTaskbarAlignment.IsOn = taskbarAlignment;
            }

            if (settings.TryGetValue("DarkMode", out bool darkMode))
            {
                toggleDarkMode.IsOn = darkMode;
            }

            if (settings.TryGetValue("DetailedBSoD", out bool detailedBSoD))
            {
                toggleDetailedBSoD.IsOn = detailedBSoD;
            }

            if (settings.TryGetValue("MouseAcceleration", out bool mouseAcceleration))
            {
                toggleMouseAcceleration.IsOn = mouseAcceleration;
            }

            if (settings.TryGetValue("NumLock", out bool numLock))
            {
                toggleNumLock.IsOn = numLock;
            }

            if (settings.TryGetValue("SearchButton", out bool searchButton))
            {
                toggleSearchButton.IsOn = searchButton;
            }

            if (settings.TryGetValue("ShowFileExt", out bool showFileExt))
            {
                toggleShowFileExt.IsOn = showFileExt;
            }

            if (settings.TryGetValue("ShowHiddenFiles", out bool showHiddenFiles))
            {
                toggleShowHiddenFiles.IsOn = showHiddenFiles;
            }

            if (settings.TryGetValue("SnapAssistFlyout", out bool snapAssistFlyout))
            {
                toggleSnapAssistFlyout.IsOn = snapAssistFlyout;
            }

            if (settings.TryGetValue("SnapAssistSuggestion", out bool snapAssistSuggestion))
            {
                toggleSnapAssistSuggestion.IsOn = snapAssistSuggestion;
            }

            if (settings.TryGetValue("SnapWindow", out bool snapWindow))
            {
                toggleSnapWindow.IsOn = snapWindow;
            }

            if (settings.TryGetValue("StickyKeys", out bool stickyKeys))
            {
                toggleStickyKeys.IsOn = stickyKeys;
            }

            if (settings.TryGetValue("TaskViewButton", out bool taskViewButton))
            {
                toggleTaskViewButton.IsOn = taskViewButton;
            }

            if (settings.TryGetValue("VerboseLogon", out bool verboseLogon))
            {
                toggleVerboseLogon.IsOn = verboseLogon;
            }

            if (settings.TryGetValue("WidgetsButton", out bool widgetsButton))
            {
                toggleWidgetsButton.IsOn = widgetsButton;
            }

            if (settings.TryGetValue("ClassicRightClick", out bool classicRightClick))
            {
                toggleClassicRightClick.IsOn = classicRightClick;
            }
            
            if (settings.TryGetValue("GalleryNavigationPane", out bool galleryNavigationPane))
            {
                toggleGalleryNavigationPane.IsOn = galleryNavigationPane;
            }
            
            // Apply all toggles
            ApplyToggles();
        }

        private void ApplyToggles()
        {
            SetBingSearch(!toggleBingSearch.IsOn);
            SetTaskbarAlignment(toggleTaskbarAlignment.IsOn);
            SetDarkMode(toggleDarkMode.IsOn);
            SetDetailedBSoD(toggleDetailedBSoD.IsOn);
            SetMouseAcceleration(toggleMouseAcceleration.IsOn);
            SetNumLockOnStartup(toggleNumLock.IsOn);
            SetSearchButtonInTaskbar(toggleSearchButton.IsOn);
            SetShowFileExtensions(toggleShowFileExt.IsOn);
            SetShowHiddenFiles(toggleShowHiddenFiles.IsOn);
            SetSnapAssistFlyout(toggleSnapAssistFlyout.IsOn);
            SetSnapAssistSuggestion(toggleSnapAssistSuggestion.IsOn);
            SetSnapWindow(toggleSnapWindow.IsOn);
            SetStickyKeys(toggleStickyKeys.IsOn);
            SetTaskViewButtonInTaskbar(toggleTaskViewButton.IsOn);
            SetVerboseMessagesOnLogon(toggleVerboseLogon.IsOn);
            SetWidgetsButtonInTaskbar(toggleWidgetsButton.IsOn);
            SetClassicRightClickMenu(toggleClassicRightClick.IsOn);
            SetGalleryNavigationPane(toggleGalleryNavigationPane.IsOn);
        }
    }
}