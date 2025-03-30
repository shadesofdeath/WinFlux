using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace WinFlux.Pages
{
    public enum AppCategory
    {
        SystemCritical,    // Kırmızı - Windows için kritik sistem uygulamaları
        SystemCore,        // Turuncu - Temel sistem bileşenleri
        Driver,            // Mor - Sürücü paketleri
        Language,          // Pembe - Dil paketleri
        Store,            // Mavi - Microsoft Store uygulamaları
        Gaming,           // Yeşil - Xbox ve oyun uygulamaları
        DefaultApps,      // Açık Yeşil - Windows varsayılan uygulamaları
        Extension,        // Açık Mavi - Eklentiler
        Optional          // Gri - İsteğe bağlı uygulamalar
    }

    public class AppInfo : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public string AppName { get; set; }
        public string Version { get; set; }
        public string PackageFullName { get; set; }
        public AppCategory Category { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class AppsPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        private ObservableCollection<AppInfo> AppsList { get; set; }

        // Add static reference for preset service to access
        public static AppsPage Instance { get; private set; }

        public AppsPage()
        {
            InitializeComponent();
            AppsList = new ObservableCollection<AppInfo>();
            lvApps.ItemsSource = AppsList;
            
            btnRefresh.Click += BtnRefresh_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSelectAll.Click += BtnSelectAll_Click;
            btnColorInfo.Click += BtnColorInfo_Click;

            // Store the instance for preset service to use
            Instance = this;

            LoadAppsAsync();
        }

        private string ExecutePowerShellCommand(string command)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                return output;
            }
        }

        private AppCategory DetermineCategory(string appName, string signatureKind, string publisherId)
        {
            // Sistem imzalı uygulamalar her zaman kritik olarak işaretlenir
            if (signatureKind.Equals("System", StringComparison.OrdinalIgnoreCase))
            {
                return AppCategory.SystemCritical;
            }

            // Windows varsayılan uygulamaları için publisherId kontrolü
            if (publisherId.Equals("8wekyb3d8bbwe", StringComparison.OrdinalIgnoreCase))
            {
                // Kritik sistem uygulamaları ve runtime bileşenleri (Kırmızı)
                if (Regex.IsMatch(appName, @"Windows\.Shell|Windows\.UI|Microsoft\.Windows\.StartMenuExperienceHost|Microsoft\.Windows\.ContentDeliveryManager|VCLibs|NET\.Native|Microsoft\.NET|Framework|WindowsAppRuntime|Microsoft\.UI\.Xaml|DotNet|RuntimeBroker", RegexOptions.IgnoreCase))
                {
                    return AppCategory.SystemCritical;
                }

                // Microsoft'un varsayılan uygulamaları (Açık Yeşil)
                if (appName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) && 
                    !appName.Contains("Framework", StringComparison.OrdinalIgnoreCase) && 
                    !appName.Contains("Native", StringComparison.OrdinalIgnoreCase) && 
                    !appName.Contains("Runtime", StringComparison.OrdinalIgnoreCase))
                {
                    return AppCategory.DefaultApps;
                }
            }

            // Eklentiler (Açık Mavi)
            if (Regex.IsMatch(appName, @"Extension|Plugin|\.Extension|\.Plugin|MicrosoftEdge\.Extension", RegexOptions.IgnoreCase))
            {
                return AppCategory.Extension;
            }

            // Dil paketleri (Pembe)
            if (Regex.IsMatch(appName, @"LanguageExperiencePack|Language|Dil|Localization", RegexOptions.IgnoreCase))
            {
                return AppCategory.Language;
            }
            
            // Sürücü uygulamaları (Mor)
            if (Regex.IsMatch(appName, @"NVIDIA|Realtek|Intel|AMD|Driver", RegexOptions.IgnoreCase))
            {
                return AppCategory.Driver;
            }
            
            // Store uygulamaları (Mavi)
            if (Regex.IsMatch(appName, @"Microsoft\.WindowsStore|Microsoft\.StorePurchaseApp|Microsoft\.MicrosoftEdge|WebExperience|Winget", RegexOptions.IgnoreCase))
            {
                return AppCategory.Store;
            }
            
            // Gaming uygulamaları (Yeşil)
            if (Regex.IsMatch(appName, @"Xbox|GamingApp|Microsoft\.GamingServices", RegexOptions.IgnoreCase))
            {
                return AppCategory.Gaming;
            }
            
            // Diğer tüm uygulamalar (Gri)
            return AppCategory.Optional;
        }

        private async Task LoadAppsAsync()
        {
            try
            {
                loadingGrid.Visibility = Visibility.Visible;
                AppsList.Clear();
                
                await Task.Run(() =>
                {
                    string output = ExecutePowerShellCommand("Get-AppxPackage | Format-List Name,Version,PackageFullName,SignatureKind,PublisherId");
                    
                    var currentApp = new Dictionary<string, string>();
                    foreach (var line in output.Split('\n'))
                    {
                        var trimmedLine = line.Trim();
                        if (string.IsNullOrEmpty(trimmedLine))
                        {
                            if (currentApp.Count > 0)
                            {
                                var appName = currentApp.GetValueOrDefault("Name", "Unknown");
                                var signatureKind = currentApp.GetValueOrDefault("SignatureKind", "");
                                var publisherId = currentApp.GetValueOrDefault("PublisherId", "");
                                
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    AppsList.Add(new AppInfo
                                    {
                                        IsSelected = false,
                                        AppName = appName,
                                        Version = currentApp.GetValueOrDefault("Version", "Unknown"),
                                        PackageFullName = currentApp.GetValueOrDefault("PackageFullName", ""),
                                        Category = DetermineCategory(appName, signatureKind, publisherId)
                                    });
                                });
                                
                                currentApp.Clear();
                            }
                            continue;
                        }

                        var parts = trimmedLine.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            currentApp[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                string errorMessage = $"{FindResource("AppsPageMessageBox_LoadError")} {ex.Message}";
                MessageBox.Show(errorMessage, FindResource("AppsPageMessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loadingGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedApps = AppsList.Where(app => app.IsSelected).ToList();
            if (selectedApps.Any())
            {
                bool hasCriticalApps = selectedApps.Any(app => app.Category == AppCategory.SystemCritical);
                string warningMessage = hasCriticalApps
                    ? FindResource("AppsPageMessageBox_CriticalAppsWarning").ToString()
                    : FindResource("AppsPageMessageBox_DeleteConfirmation").ToString();

                var messageIcon = hasCriticalApps ? MessageBoxImage.Warning : MessageBoxImage.Question;
                var result = MessageBox.Show(warningMessage, FindResource("AppsPageMessageBox_Confirmation").ToString(), MessageBoxButton.YesNo, messageIcon);
                
                if (result == MessageBoxResult.Yes)
                {
                    loadingGrid.Visibility = Visibility.Visible;
                    foreach (var app in selectedApps)
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                ExecutePowerShellCommand($"Remove-AppxPackage '{app.PackageFullName}'");
                            });
                        }
                        catch (Exception ex)
                        {
                            string errorMessage = $"{app.AppName} {FindResource("AppsPageMessageBox_DeleteError")} {ex.Message}";
                            MessageBox.Show(errorMessage, FindResource("AppsPageMessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    loadingGrid.Visibility = Visibility.Collapsed;
                    await LoadAppsAsync(); // Listeyi yenile
                }
            }
            else
            {
                MessageBox.Show(FindResource("AppsPageMessageBox_SelectApps").ToString(), FindResource("AppsPageMessageBox_Warning").ToString(), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAppsAsync();
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (AppsList != null && AppsList.Any())
            {
                bool currentState = AppsList.First().IsSelected;
                foreach (var app in AppsList)
                {
                    app.IsSelected = !currentState;
                }
            }
        }

        private void BtnColorInfo_Click(object sender, RoutedEventArgs e)
        {
            // Artık colorInfoPopup.IsOpen kullanılmayacak, Flyout otomatik olarak çalışacak
        }

        // New method to get apps list for preset export
        public List<string> GetSelectedAppsForPreset()
        {
            return AppsList
                .Where(app => app.IsSelected)
                .Select(app => app.PackageFullName)
                .ToList();
        }

        // New method to apply preset and remove apps
        public async Task<bool> ApplyPresetAndUninstallApps(List<string> appsToRemove)
        {
            try
            {
                if (appsToRemove == null || appsToRemove.Count == 0)
                {
                    return true;
                }

                // Ask for confirmation before proceeding with app removal
                var confirmMessage = Application.Current.Resources["PresetMessageBox_AppsPageWarning"] as string;
                var result = MessageBox.Show(
                    confirmMessage,
                    Application.Current.Resources["MessageBox_Warning"] as string,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return false; // User canceled the operation
                }

                // Check for critical apps
                var criticalApps = AppsList
                    .Where(app => appsToRemove.Contains(app.PackageFullName) && app.Category == AppCategory.SystemCritical)
                    .ToList();

                if (criticalApps.Count > 0)
                {
                    var warningMessage = Application.Current.Resources["AppsPageMessageBox_CriticalAppsWarning"] as string;
                    var criticalWarningResult = MessageBox.Show(
                        warningMessage,
                        Application.Current.Resources["AppsPageMessageBox_Warning"] as string,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (criticalWarningResult != MessageBoxResult.Yes)
                    {
                        return false; // User canceled after seeing critical apps warning
                    }
                }

                // Start removing apps
                loadingGrid.Visibility = Visibility.Visible;

                var errors = new List<string>();
                var successCount = 0;

                // Process each app for removal
                foreach (var appFullName in appsToRemove)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            ExecutePowerShellCommand($"Remove-AppxPackage -Package \"{appFullName}\"");
                        });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        var app = AppsList.FirstOrDefault(a => a.PackageFullName == appFullName);
                        var appName = app != null ? app.AppName : appFullName;
                        errors.Add($"{appName}: {ex.Message}");
                    }
                }

                // Refresh the apps list after removal
                await LoadAppsAsync();

                // Show results
                if (errors.Count > 0)
                {
                    var errorMsg = Application.Current.Resources["AppsPageMessageBox_DeleteError"] as string;
                    MessageBox.Show(
                        $"{successCount} uygulamalar kaldırıldı, {errors.Count} uygulamalar {errorMsg}\n\n{string.Join("\n", errors)}",
                        Application.Current.Resources["AppsPageMessageBox_Error"] as string,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    Application.Current.Resources["AppsPageMessageBox_Error"] as string,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            finally
            {
                loadingGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
