using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using iNKORE.UI.WPF.Modern.Controls;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using WinFlux.Models;
using WinFlux.Services;
using ikw = iNKORE.UI.WPF;

namespace WinFlux.Pages
{
    public partial class AppInstallerPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        // Add static reference for preset service to access
        public static AppInstallerPage Instance { get; private set; }
        
        private List<ToggleSwitch> allToggles = new List<ToggleSwitch>();
        private int selectedCount = 0;
        private string installScript = "";
        private System.Timers.Timer progressUpdateTimer;
        private Process currentInstallProcess;
        private DateTime installStartTime;
        private Dictionary<string, bool> appPresetStates = new Dictionary<string, bool>();
        private bool isInitialLoad = true;
        private Dictionary<string, bool> pendingPresetSettings = new Dictionary<string, bool>();
        
        // Application data
        private Dictionary<string, ApplicationInfo> applications;
        private Dictionary<string, string> categoryIcons = new Dictionary<string, string>
        {
            { "Browsers", "\uE774" },      // Web browser icon
            { "Development", "\uE943" },   // Code icon
            { "Utilities", "\uE8D2" },     // Tools icon
            { "Document", "\uE8A5" },      // Document icon
            { "Multimedia Tools", "\uE8B0" }, // Media icon
            { "Pro Tools", "\uE8C9" },     // Professional tools icon
            { "Communication", "\uE8F2" }, // Communication icon
            { "Gaming", "\uE7FC" },        // Gaming icon
            { "Microsoft Tools", "\uE8FC" } // Microsoft icon
        };

        // Paket yöneticileri için paket ID eşleştirmeleri
        private Dictionary<string, string> wingetPackageIds = new Dictionary<string, string>
        {
            { "chrome", "Google.Chrome" },
            { "firefox", "Mozilla.Firefox" },
            { "brave", "Brave.Brave" },
            { "vscode", "Microsoft.VisualStudioCode" },
            { "git", "Git.Git" },
            { "nodejs", "OpenJS.NodeJS" },
            { "7zip", "7zip.7zip" },
            { "vlc", "VideoLAN.VLC" },
            { "notepadplusplus", "Notepad++.Notepad++" },
            { "discord", "Discord.Discord" },
            { "microsoft-teams", "Microsoft.Teams" },
            { "slack", "SlackTechnologies.Slack" },
            { "steam", "Valve.Steam" },
            { "epic", "EpicGames.EpicGamesLauncher" }
        };

        private Dictionary<string, string> chocolateyPackageIds = new Dictionary<string, string>
        {
            { "chrome", "googlechrome" },
            { "firefox", "firefox" },
            { "brave", "brave" },
            { "vscode", "vscode" },
            { "git", "git" },
            { "nodejs", "nodejs" },
            { "7zip", "7zip" },
            { "vlc", "vlc" },
            { "notepadplusplus", "notepadplusplus" },
            { "discord", "discord" },
            { "microsoft-teams", "microsoft-teams" },
            { "slack", "slack" },
            { "steam", "steam" },
            { "epic", "epicgameslauncher" }
        };

        public AppInstallerPage()
        {
            InitializeComponent();
            
            // Store the instance for preset service to use
            Instance = this;
            
            // Event handlers için page loaded
            Loaded += AppInstallerPage_Loaded;
            Unloaded += AppInstallerPage_Unloaded;
            
            // İlerleme zamanlayıcısını oluşturma
            progressUpdateTimer = new System.Timers.Timer(500); // 500ms
            progressUpdateTimer.Elapsed += ProgressUpdateTimer_Elapsed;
        }

        private async void AppInstallerPage_Loaded(object sender, RoutedEventArgs e)
        {
            // İlk yükleme olduğunu işaretle
            isInitialLoad = true;
            
            // Sayfayı hazırla
            await LoadMainContent();
            
            // Set up the package manager
            await SetupPackageManager();
            
            // Tüm toggle'ları bul ve ayarla
            FindAllToggles();
            
            // Hide progress panel initially
            if (progressPanel != null)
            {
                progressPanel.Visibility = Visibility.Collapsed;
            }
            
            // Sayfa yükleme tamamlandıktan sonra, herhangi bir bekleyen preset ayarı varsa uygula
            if (pendingPresetSettings.Count > 0)
            {
                // UI'ın tamamen yüklenmesi için kısa bir bekleme
                await Task.Delay(200);
                
                // Preset ayarlarını uygula
                ApplyPendingPresetSettings();
            }
            
            // İlk yüklemenin tamamlandığını işaretle
            isInitialLoad = false;
        }

        private void AppInstallerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Timer'ı durdur
            if (progressUpdateTimer != null)
            {
                progressUpdateTimer.Stop();
            }
            
            // Çalışan process varsa sonlandır
            if (currentInstallProcess != null && !currentInstallProcess.HasExited)
            {
                try
                {
                    currentInstallProcess.Kill();
                }
                catch { /* İşlem sonlandırma hatalarını yoksay */ }
            }
        }

        private async Task InstallMissingPackageManagersIfNeeded(bool wingetInstalled, bool chocolateyInstalled)
        {
            if (!wingetInstalled || !chocolateyInstalled)
            {
                string message = "";
                
                if (!wingetInstalled && !chocolateyInstalled)
                    message = (string)Application.Current.Resources["AppInstallerBothMissing"];
                else if (!wingetInstalled)
                    message = (string)Application.Current.Resources["AppInstallerWingetMissing"];
                else
                    message = (string)Application.Current.Resources["AppInstallerChocolateyMissing"];
                
                var result = MessageBox.Show(
                    message,
                    (string)Application.Current.Resources["MessageBox_Information"],
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (result == MessageBoxResult.Yes)
                {
                    // İlerleme göstergesini göster
                    ShowProgressPanel();
                    progressBar.IsIndeterminate = true;
                    currentStatusTextBlock.Text = (string)Application.Current.Resources["AppInstallerStatusRunning"];
                    
                    try
                    {
                        if (!wingetInstalled)
                        {
                            await InstallWingetAsync();
                            wingetInstalled = await IsWingetInstalledAsync();
                        }
                        
                        if (!chocolateyInstalled && wingetInstalled)
                        {
                            await InstallChocolateyWithWingetAsync();
                            chocolateyInstalled = await IsChocolateyInstalledAsync();
                        }
                        else if (!chocolateyInstalled)
                        {
                            await InstallChocolateyDirectAsync();
                            chocolateyInstalled = await IsChocolateyInstalledAsync();
                        }
                        
                        // Başarı mesajı göster
                        MessageBox.Show(
                            (string)Application.Current.Resources["AppInstallerPackageManagersInstalled"],
                            (string)Application.Current.Resources["MessageBox_Information"],
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"{(string)Application.Current.Resources["AppInstallerPackageManagerInstallError"]}\n\n{ex.Message}",
                            (string)Application.Current.Resources["MessageBox_Error"],
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    finally
                    {
                        // İlerleme göstergesini gizle
                        HideProgressPanel();
                    }
                }
            }
        }

        private void FindAllToggles()
        {
            // Sayfadaki tüm toggle switch'leri bul
            allToggles.Clear();
            FindAllToggleSwitchesInVisualTree(this);
            
            // Event handler'ları kaydet
            foreach (var toggle in allToggles)
            {
                toggle.Toggled -= Toggle_Toggled; // Çift kayıt olmaması için önce kaldır
                toggle.Toggled += Toggle_Toggled;
            }
            
            // Apply any pending preset settings
            if (pendingPresetSettings.Count > 0 && allToggles.Count > 0)
            {
                ApplyPendingPresetSettings();
            }
            
            // Mark that initial load is complete
            isInitialLoad = false;
        }

        private void FindAllToggleSwitchesInVisualTree(DependencyObject parent)
        {
            if (parent == null) return;
            
            // First check if this element is a ToggleSwitch
            if (parent is ToggleSwitch toggle && toggle.Name != null && toggle.Name.StartsWith("toggle"))
            {
                if (!allToggles.Contains(toggle))
                {
                    allToggles.Add(toggle);
                }
            }
            
            // Get all child elements
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                FindAllToggleSwitchesInVisualTree(child);
            }
            
            // For ContentControl (like Expander), also check logical children
            if (parent is ContentControl contentControl && contentControl.Content is DependencyObject content)
            {
                FindAllToggleSwitchesInVisualTree(content);
            }
            
            // For ItemsControl (which might contain multiple items)
            if (parent is ItemsControl itemsControl)
            {
                foreach (var item in itemsControl.Items)
                {
                    if (item is DependencyObject itemObj)
                    {
                        FindAllToggleSwitchesInVisualTree(itemObj);
                    }
                }
            }
        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            // Seçili sayısını güncelle
            UpdateSelectedCount();
            
            // Kurulum scriptini güncelle
            GenerateInstallScript();
        }

        private void UpdateSelectedCount()
        {
            selectedCount = allToggles.Count(t => t.IsOn);
            
            // UI'ı güncelle
            Dispatcher.Invoke(() =>
            {
                // Buton metnini ve durumunu güncelle
                var btnText = (string)Application.Current.Resources["AppInstallerInstallBtn"];
                installButton.Content = $"{btnText} ({selectedCount})";
                installButton.IsEnabled = selectedCount > 0;
                
                // Seçilen uygulamalara göre kategorileri güncelle
                UpdateCategoryHeaders();
            });
        }

        private void UpdateCategoryHeaders()
        {
            // Her kategori için seçilen uygulama sayısını güncelle
            // Not: Bu fonksiyon XAML yapınıza göre değiştirilmelidir
            try
            {
                Dictionary<string, int> categoryCounts = new Dictionary<string, int>
                {
                    { "browsers", allToggles.Count(t => t.IsOn && t.Name.StartsWith("toggleBrowser")) },
                    { "development", allToggles.Count(t => t.IsOn && t.Name.StartsWith("toggleDev")) },
                    { "utilities", allToggles.Count(t => t.IsOn && t.Name.StartsWith("toggleUtil")) },
                    { "communication", allToggles.Count(t => t.IsOn && t.Name.StartsWith("toggleComm")) },
                    { "gaming", allToggles.Count(t => t.IsOn && t.Name.StartsWith("toggleGame")) }
                };
                
                // UI'da kategori başlıklarını güncelleme işlemi...
                // Bu kısım XAML yapınıza göre düzenlenmelidir
            }
            catch (Exception)
            {
                // Kategori sayma hatalarını yoksay
            }
        }

        private void GenerateInstallScript()
        {
            var selectedApps = allToggles.Where(t => t.IsOn).ToList();
            StringBuilder scriptBuilder = new StringBuilder();
            
            if (selectedApps.Count == 0)
            {
                installScript = "";
                return;
            }
            
            // Hangi paket yöneticisinin kullanılacağını belirle
            bool useWinget = packageManagerComboBox.SelectedIndex == 0;
            
            // Seçilen paketlerin hangi paket yönetici tarafından desteklendiğini kontrol et
            var unavailablePackages = new List<string>();
            var availablePackages = new List<(ToggleSwitch Toggle, ApplicationInfo AppInfo)>();
            
            foreach (var appToggle in selectedApps)
            {
                string toggleName = appToggle.Name.Substring("toggle".Length);
                
                // Find the application in our data
                ApplicationInfo appInfo = null;
                foreach (var app in applications.Values)
                {
                    if (GetValidXamlName(app.Name) == toggleName)
                    {
                        appInfo = app;
                        break;
                    }
                }
                
                if (appInfo == null)
                {
                    unavailablePackages.Add(toggleName);
                    continue;
                }
                
                bool isAvailable = useWinget 
                    ? appInfo.Winget != "na" 
                    : appInfo.Chocolatey != "na";
                    
                if (isAvailable)
                {
                    availablePackages.Add((appToggle, appInfo));
                }
                else
                {
                    unavailablePackages.Add(appInfo.Name);
                }
            }
            
            // Kullanıcıyı desteklenmeyen paketler hakkında bilgilendir
            if (unavailablePackages.Count > 0)
            {
                string packageManager = useWinget ? "Winget" : "Chocolatey";
                string warningMessage = $"Some applications cannot be installed with {packageManager}: {string.Join(", ", unavailablePackages)}";
                
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        warningMessage,
                        (string)Application.Current.Resources["MessageBox_Warning"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    
                    // Diğer paket yöneticisine geçmeyi öner
                    if (MessageBox.Show(
                        $"Would you like to use the other package manager ({(useWinget ? (string)Application.Current.Resources["AppInstallerChocolatey"] : (string)Application.Current.Resources["AppInstallerWinget"])})?",
                        (string)Application.Current.Resources["MessageBox_Question"],
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        packageManagerComboBox.SelectedIndex = useWinget ? 1 : 0;
                        return; // Script oluşturmayı durdur, yeni seçimle tekrar çağrılacak
                    }
                });
            }
            
            // Konsol arkaplan rengini siyah yap ve yazı rengini beyaz yap
            scriptBuilder.AppendLine("$host.UI.RawUI.BackgroundColor = 'Black'");
            scriptBuilder.AppendLine("$host.UI.RawUI.ForegroundColor = 'White'");
            scriptBuilder.AppendLine("Clear-Host");
            
            // PowerShell pencere başlığını ayarla
            scriptBuilder.AppendLine("$host.UI.RawUI.WindowTitle = \"WinFlux - " + (string)Application.Current.Resources["AppInstallerWindowTitle"] + "\"");
            
            // Başlık ve renkli çıktı
            scriptBuilder.AppendLine("Write-Host \"\" # Boş satır");
            scriptBuilder.AppendLine("Write-Host \"WinFlux " + (string)Application.Current.Resources["AppInstallerScriptTitle"] + " - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\" -ForegroundColor Cyan");
            scriptBuilder.AppendLine("Write-Host \"" + string.Format((string)Application.Current.Resources["AppInstallerGeneratedBy"], (useWinget ? "Winget" : "Chocolatey")) + "\" -ForegroundColor Cyan");
            scriptBuilder.AppendLine("Write-Host \"=================================================\" -ForegroundColor Cyan");
            scriptBuilder.AppendLine("Write-Host \"\" # Boş satır");
            
            // Kullanıcıyı uyarma - kapatma durumu
            scriptBuilder.AppendLine("Write-Host \"" + (string)Application.Current.Resources["AppInstallerDontCloseWarning"] + "\" -ForegroundColor Yellow");
            scriptBuilder.AppendLine("Write-Host \"\" # Boş satır");
            
            // Progress output için ayarlar
            scriptBuilder.AppendLine("$progressPreference = 'Continue'");
            scriptBuilder.AppendLine("$ProgressActivity = \"" + (string)Application.Current.Resources["AppInstallerProgressActivity"] + "\"");
            
            // Try-Catch bloğu ekle - erken kapatılma durumunu yönetmek için
            scriptBuilder.AppendLine("try {");
            
            if (!useWinget)
            {
                // Chocolatey verbose çıktıyı azalt
                scriptBuilder.AppendLine("    $env:ChocolateyEnvironmentDebug = 'false'");
                scriptBuilder.AppendLine("    $env:ChocolateyEnvironmentVerbose = 'false'");
            }
            
            scriptBuilder.AppendLine();
            
            // Her uygulama için
            int count = 0;
            int total = availablePackages.Count;
            
            foreach (var (toggle, appInfo) in availablePackages)
            {
                string packageId = useWinget ? appInfo.Winget : appInfo.Chocolatey;
                
                count++;
                scriptBuilder.AppendLine($"    Write-Progress -Activity $ProgressActivity -Status \"" + 
                    string.Format((string)Application.Current.Resources["AppInstallerInstallingStatus"], appInfo.Name) + 
                    $"\" -PercentComplete {(count * 100) / total}");
                
                scriptBuilder.AppendLine($"    Write-Host \"`n[{count}/{total}] " + 
                    string.Format((string)Application.Current.Resources["AppInstallerInstalling"], appInfo.Name) + 
                    "\" -ForegroundColor Yellow");
                
                if (useWinget)
                {
                    scriptBuilder.AppendLine($"    winget install --id={packageId} --silent --accept-package-agreements --accept-source-agreements");
                }
                else
                {
                    scriptBuilder.AppendLine($"    choco install {packageId} -y --limit-output --force");
                }
                
                scriptBuilder.AppendLine($"    if ($LASTEXITCODE -eq 0) {{ " + 
                    $"Write-Host \"{string.Format((string)Application.Current.Resources["AppInstallerSuccessMessage"], appInfo.Name)}\" -ForegroundColor Green " + 
                    $"}} else {{ Write-Host \"{string.Format((string)Application.Current.Resources["AppInstallerFailedMessage"], appInfo.Name)}\" -ForegroundColor Red }}");
                
                scriptBuilder.AppendLine("    Write-Host \"\" # Boş satır");
            }
            
            scriptBuilder.AppendLine("    Write-Progress -Activity $ProgressActivity -Completed");
            scriptBuilder.AppendLine("    Write-Host \"`n" + (string)Application.Current.Resources["AppInstallerAllCompleted"] + "\" -ForegroundColor Green");
            scriptBuilder.AppendLine("    Write-Host \"\" # Boş satır");
            
            // Temizlik işlemleri ekle
            scriptBuilder.AppendLine("    # Temizlik işlemleri");
            scriptBuilder.AppendLine("    Write-Host \"" + (string)Application.Current.Resources["AppInstallerCleanup"] + "\" -ForegroundColor Cyan");
            
            // Geçici dosyaları temizle
            scriptBuilder.AppendLine("    # Kurulum önbelleğini temizle");
            if (useWinget) {
                scriptBuilder.AppendLine("    if (Test-Path \"$env:LOCALAPPDATA\\Packages\\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\\LocalCache\\*\") {");
                scriptBuilder.AppendLine("        Write-Host \"Winget önbelleğini temizleme...\" -ForegroundColor Gray");
                scriptBuilder.AppendLine("        Remove-Item -Path \"$env:LOCALAPPDATA\\Packages\\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\\LocalCache\\*\" -Recurse -Force -ErrorAction SilentlyContinue");
                scriptBuilder.AppendLine("    }");
            } else {
                scriptBuilder.AppendLine("    if (Test-Path \"$env:TEMP\\chocolatey\\*\") {");
                scriptBuilder.AppendLine("        Write-Host \"Chocolatey önbelleğini temizleme...\" -ForegroundColor Gray");
                scriptBuilder.AppendLine("        Remove-Item -Path \"$env:TEMP\\chocolatey\\*\" -Recurse -Force -ErrorAction SilentlyContinue");
                scriptBuilder.AppendLine("    }");
            }
            
            // Gereksiz kurulum dosyalarını sil
            scriptBuilder.AppendLine("    # İndirilen kurulum dosyalarını temizle");
            scriptBuilder.AppendLine("    Get-ChildItem -Path \"$env:TEMP\" -Filter \"*.exe\" -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.CreationTime -gt (Get-Date).AddHours(-1) } | ForEach-Object {");
            scriptBuilder.AppendLine("        Remove-Item -Path $_.FullName -Force -ErrorAction SilentlyContinue");
            scriptBuilder.AppendLine("    }");
            
            // Catch bloğunu ekle - erken kapatılma durumunu yönetmek için
            scriptBuilder.AppendLine("} catch {");
            scriptBuilder.AppendLine("    Write-Host \"`n" + (string)Application.Current.Resources["AppInstallerEarlyTermination"] + "\" -ForegroundColor Red");
            scriptBuilder.AppendLine("    Write-Host $_.Exception.Message -ForegroundColor Red");
            scriptBuilder.AppendLine("    Write-Host \"\" # Boş satır");
            scriptBuilder.AppendLine("}");
            
            scriptBuilder.AppendLine("Write-Host \"" + (string)Application.Current.Resources["AppInstallerPressAnyKey"] + "\" -ForegroundColor Cyan");
            scriptBuilder.AppendLine("$null = $host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')");
            
            installScript = scriptBuilder.ToString();
        }

        private void ApplyPendingPresetSettings()
        {
            // Preset değerlerini toggle'lara uygula
            foreach (var toggle in allToggles)
            {
                if (toggle.Name != null && pendingPresetSettings.ContainsKey(toggle.Name))
                {
                    toggle.IsOn = pendingPresetSettings[toggle.Name];
                }
            }
            
            // Update selected count based on the applied settings
            UpdateSelectedCount();
            
            // Preset değerlerini script'e de yansıt
            GenerateInstallScript();
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedApps = allToggles.Where(t => t.IsOn).ToList();
            
            if (selectedApps.Count == 0)
            {
                MessageBox.Show(
                    (string)Application.Current.Resources["AppInstallerNoAppsSelected"],
                    (string)Application.Current.Resources["MessageBox_Warning"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            
            // Script'i tekrar oluştur
            GenerateInstallScript();
            
            // Hangi paket yöneticisinin kullanılacağını belirle
            bool useWinget = packageManagerComboBox.SelectedIndex == 0;
            
            // Kurulumu onayla
            var packageManagerName = useWinget ? "Winget" : "Chocolatey";
            var confirmMessage = string.Format(
                (string)Application.Current.Resources["AppInstallerConfirmInstall"],
                selectedApps.Count,
                packageManagerName);
            
            var result = MessageBox.Show(
                confirmMessage,
                (string)Application.Current.Resources["MessageBox_Confirmation"],
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes)
                return;
            
            // İlerleme panelini göster
            ShowProgressPanel();
            
            try
            {
                // Uygulamaları kur
                await InstallSelectedAppsAsync(useWinget);
                
                // İşlem başarılı olduğunda mesaj göster
                MessageBox.Show(
                    (string)Application.Current.Resources["AppInstallerInstallComplete"],
                    (string)Application.Current.Resources["MessageBox_Information"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                
                // UI durumunu sıfırla - tüm toggle'ları kapat
                foreach (var toggle in allToggles)
                {
                    toggle.IsOn = false;
                }
                
                // Seçili sayısını güncelle
                UpdateSelectedCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{(string)Application.Current.Resources["AppInstallerInstallError"]}\n\n{ex.Message}",
                    (string)Application.Current.Resources["MessageBox_Error"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // İlerleme panelini gizle
                HideProgressPanel();
            }
        }

        private void ShowProgressPanel()
        {
            mainContentScrollViewer.Visibility = Visibility.Collapsed;
            progressPanel.Visibility = Visibility.Visible;
            installButton.IsEnabled = false;
            packageManagerComboBox.IsEnabled = false;
            
            // İlerleme bilgisini sıfırla
            progressBar.Value = 0;
            progressBar.IsIndeterminate = true;
            currentInstallingTextBlock.Text = "";
            currentStatusTextBlock.Text = (string)Application.Current.Resources["AppInstallerStatusRunning"];
            
            // Timer'ı başlat
            progressUpdateTimer.Start();
            installStartTime = DateTime.Now;
        }

        private void HideProgressPanel()
        {
            mainContentScrollViewer.Visibility = Visibility.Visible;
            progressPanel.Visibility = Visibility.Collapsed;
            installButton.IsEnabled = selectedCount > 0;
            packageManagerComboBox.IsEnabled = true;
            
            // Timer'ı durdur
            progressUpdateTimer.Stop();
        }

        private void ProgressUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Bu metot timer thread'inde çalışır, UI güncellemeleri için Dispatcher kullan
            Dispatcher.Invoke(() => 
            {
                if (currentInstallProcess != null && !currentInstallProcess.HasExited)
                {
                    // Process devam ediyor
                    string processingMessage = (string)Application.Current.Resources["AppInstallerStatusRunning"];
                    
                    // Animasyon efekti ekle
                    int seconds = DateTime.Now.Second % 3;
                    string dots = new string('.', seconds + 1);
                    currentStatusTextBlock.Text = $"{processingMessage}{dots}";
                    
                    // Progress bar gösterimini güncelle
                    if (progressBar.IsIndeterminate)
                    {
                        // İlk 3 saniye sonra indeterminate modu kapat
                        if (DateTime.Now.Subtract(installStartTime).TotalSeconds > 3)
                        {
                            progressBar.IsIndeterminate = false;
                            progressBar.Value = 5; // Başlangıç değeri
                        }
                    }
                    else if (progressBar.Value < 95) // Maksimum %95'e kadar git, tamamı bitince %100 yapılacak
                    {
                        // Yavaşça progress barı ilerlet
                        TimeSpan elapsed = DateTime.Now.Subtract(installStartTime);
                        double estimatedProgress = Math.Min(90, elapsed.TotalSeconds * 0.5);
                        progressBar.Value = Math.Max(progressBar.Value, estimatedProgress);
                    }
                }
                else if (currentInstallProcess == null || currentInstallProcess.HasExited)
                {
                    // Process tamamlandı veya hata ile sonlandı
                    if (progressBar.IsIndeterminate)
                    {
                        progressBar.IsIndeterminate = false;
                    }
                    
                    // Süreç tamamlandıysa %100 göster, yoksa mevcut değerde bırak
                    if (currentInstallProcess != null && currentInstallProcess.ExitCode == 0)
                    {
                        progressBar.Value = 100;
                        currentStatusTextBlock.Text = (string)Application.Current.Resources["AppInstallerStatusCompleted"];
                    }
                    else if (currentInstallProcess != null)
                    {
                        currentStatusTextBlock.Text = (string)Application.Current.Resources["AppInstallerStatusFailed"];
                    }
                }
            });
        }

        #region Package Manager Installation Methods
        
        private async Task<bool> IsWingetInstalledAsync(bool silent = false)
        {
            try
            {
                var result = await RunPowerShellCommandAsync("winget -v", silent);
                return !string.IsNullOrEmpty(result) && !result.Contains("not recognized");
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> IsChocolateyInstalledAsync(bool silent = false)
        {
            try
            {
                var result = await RunPowerShellCommandAsync("choco -v", silent);
                return !string.IsNullOrEmpty(result) && !result.Contains("not recognized");
            }
            catch
            {
                return false;
            }
        }
        
        private async Task InstallWingetAsync()
        {
            // Winget normal olarak Microsoft Store aracılığıyla kurulur, ancak AppInstaller'ı kullanabiliriz
            await RunPowerShellCommandAsync(@"
                Add-AppxPackage -RegisterByFamilyName -MainPackage Microsoft.DesktopAppInstaller_8wekyb3d8bbwe
            ");
        }
        
        private async Task InstallChocolateyWithWingetAsync()
        {
            // Winget kullanarak Chocolatey'i kur
            await RunPowerShellCommandAsync("winget install -e --id=Chocolatey.Chocolatey");
        }
        
        private async Task InstallChocolateyDirectAsync()
        {
            // Chocolatey'i doğrudan kur
            await RunPowerShellCommandAsync(@"
                Set-ExecutionPolicy Bypass -Scope Process -Force; 
                [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; 
                iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
            ");
        }
        
        #endregion
        
        #region App Installation Methods
        
        private async Task InstallSelectedAppsAsync(bool useWinget)
        {
            // Geçici script dosyası oluştur
            string tempPath = Path.GetTempPath();
            string scriptPath = Path.Combine(tempPath, "winflux_install.ps1");
            
            try
            {
                // Script'i dosyaya UTF-8 ile yaz
                File.WriteAllText(scriptPath, installScript, Encoding.UTF8);
                
                // PowerShell penceresi görünür şekilde çalıştır
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                
                using (currentInstallProcess = new Process())
                {
                    currentInstallProcess.StartInfo = startInfo;
                    currentInstallProcess.EnableRaisingEvents = true;
                    
                    // Process kapandığında bildirmek için event ekle
                    var tcs = new TaskCompletionSource<bool>();
                    
                    currentInstallProcess.Exited += (sender, e) => 
                    {
                        try
                        {
                            tcs.TrySetResult(true);
                        }
                        catch { }
                    };
                    
                    // Process başlatma
                    if (!currentInstallProcess.Start())
                    {
                        throw new Exception((string)Application.Current.Resources["AppInstallerProcessStartError"]);
                    }
                    
                    // İlerleme bilgisini güncelle
                    currentStatusTextBlock.Text = (string)Application.Current.Resources["AppInstallerStatusRunning"];
                    
                    // Process'in bitimini bekle
                    await tcs.Task;
                    
                    // Eğer process hata kodu ile çıktıysa
                    if (currentInstallProcess.ExitCode != 0)
                    {
                        if (currentInstallProcess.ExitCode == -1)
                        {
                            throw new Exception((string)Application.Current.Resources["AppInstallerProcessTerminated"]);
                        }
                        else
                        {
                            throw new Exception(string.Format(
                                (string)Application.Current.Resources["AppInstallerProcessExitCode"], 
                                currentInstallProcess.ExitCode));
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Kullanıcı işlemi iptal etti
                throw new Exception((string)Application.Current.Resources["AppInstallerUserCancelled"]);
            }
            catch (Exception ex)
            {
                // Diğer hataları yeniden fırlat
                throw new Exception($"{(string)Application.Current.Resources["AppInstallerError"]}: {ex.Message}", ex);
            }
            finally
            {
                // Geçici script dosyasını sil
                if (File.Exists(scriptPath))
                {
                    try
                    {
                        File.Delete(scriptPath);
                    }
                    catch { /* Silme hatalarını yoksay */ }
                }
                
                currentInstallProcess = null;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private async Task<string> RunPowerShellCommandAsync(string command, bool silent = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = silent ? false : true,
                RedirectStandardOutput = silent ? true : false,
                RedirectStandardError = silent ? true : false,
                CreateNoWindow = silent,
                WindowStyle = silent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };
            
            if (silent)
            {
                using (var process = Process.Start(startInfo))
                {
                    if (process == null) return string.Empty;
                    
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();
                    
                    string output = await outputTask;
                    string error = await errorTask;
                    
                    return output;
                }
            }
            else
            {
                using (var process = Process.Start(startInfo))
                {
                    // Process'in bitmesini bekle
                    if (process != null)
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        process.EnableRaisingEvents = true;
                        process.Exited += (sender, e) => tcs.TrySetResult(true);
                        
                        await tcs.Task;
                        
                        // Çıkış kodu kontrolü - görünür pencerede çıktı görüldüğü için string dönme gereksinimi yok
                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"Process exited with code {process.ExitCode}");
                        }
                        
                        return "Command completed";
                    }
                    
                    return string.Empty;
                }
            }
        }
        
        #endregion

        // Public methods for system-wide preset functionality

        // Get the toggle settings for all app installation toggles
        public Dictionary<string, bool> GetToggleSettings()
        {
            var settings = new Dictionary<string, bool>();
            
            foreach (var toggle in allToggles)
            {
                if (toggle.Name != null)
                {
                    settings[toggle.Name] = toggle.IsOn;
                }
            }
            
            return settings;
        }
        
        // Get the selected package manager index (0 = Winget, 1 = Chocolatey)
        public int GetPreferredPackageManager()
        {
            return packageManagerComboBox.SelectedIndex;
        }
        
        // Apply toggle settings from a preset
        public void ApplyToggleSettings(Dictionary<string, bool> settings)
        {
            if (settings == null || settings.Count == 0) return;
            
            // Always store these settings in pendingPresetSettings as a backup
            pendingPresetSettings.Clear();
            foreach (var kvp in settings)
            {
                pendingPresetSettings[kvp.Key] = kvp.Value;
            }
            
            // If the UI is not yet fully loaded, just save the settings for later application
            if (isInitialLoad || allToggles.Count == 0)
            {
                // Değişiklikleri sonra uygulanmak üzere kaydettik, şimdi çıkalım
                return;
            }
            
            // Apply settings to toggles if the UI is ready
            foreach (var toggle in allToggles)
            {
                if (toggle.Name != null && settings.ContainsKey(toggle.Name))
                {
                    toggle.IsOn = settings[toggle.Name];
                }
            }
            
            // Update selected count
            UpdateSelectedCount();
        }
        
        // Set the preferred package manager
        public void SetPreferredPackageManager(int index)
        {
            if (index >= 0 && index <= 1)
            {
                packageManagerComboBox.SelectedIndex = index;
            }
        }

        private void CreateApplicationCards()
        {
            // Clear existing content
            categoriesPanel.Children.Clear();
            
            // Get categorized applications
            var categorizedApps = ApplicationDataService.Instance.GetCategorizedApplications();
            
            // Sort categories
            var categories = categorizedApps.Keys.OrderBy(c => c).ToList();
            
            // Create UI for each category
            foreach (var category in categories)
            {
                // Create the expander for this category
                var expander = new Expander
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    Header = category,
                    IsExpanded = category == "Browsers" // Expand the first category by default
                };
                
                // Create a stack panel for the applications in this category
                var appsPanel = new StackPanel
                {
                    Margin = new Thickness(0, 10, 0, 0)
                };
                
                // Add applications to the panel
                foreach (var app in categorizedApps[category])
                {
                    // Get icon for the category
                    string iconGlyph = categoryIcons.ContainsKey(category) 
                        ? categoryIcons[category] 
                        : "\uE8D2"; // Default to tools icon
                    
                    // Create a settings card for this application
                    var card = new SettingsCard
                    {
                        Margin = new Thickness(0, 0, 0, 5),
                        Header = app.Name,
                        Description = app.Description
                    };
                    
                    // Create header icon
                    var icon = new FontIcon
                    {
                        Glyph = iconGlyph
                    };
                    card.HeaderIcon = icon;
                    
                    // Create toggle switch
                    var toggle = new ToggleSwitch
                    {
                        Name = $"toggle{GetValidXamlName(app.Name)}",
                        Tag = app.Winget != "na" && app.Chocolatey != "na" 
                            ? app.Name.ToLowerInvariant().Replace(" ", "") 
                            : (app.Winget != "na" ? app.Winget : app.Chocolatey),
                        OnContent = (string)Application.Current.Resources["ToggleOn"],
                        OffContent = (string)Application.Current.Resources["ToggleOff"],
                        IsOn = false
                    };
                    card.Content = toggle;
                    
                    // Add the card to the panel
                    appsPanel.Children.Add(card);
                }
                
                // Set the content of the expander
                expander.Content = appsPanel;
                
                // Add the expander to the main panel
                categoriesPanel.Children.Add(expander);
            }
        }
        
        private string GetValidXamlName(string name)
        {
            // Convert application name to a valid XAML name
            // Remove special characters and spaces
            return new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray());
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchBox.Text.Trim().ToLowerInvariant();
            
            if (string.IsNullOrEmpty(searchText))
            {
                // Show all apps if search box is empty
                ShowAllApps();
                return;
            }
            
            // Hide all expanders first
            foreach (Expander expander in categoriesPanel.Children)
            {
                expander.IsExpanded = false;
            }
            
            bool foundAnyMatch = false;

            // Go through each category
            foreach (Expander expander in categoriesPanel.Children)
            {
                bool matchInThisCategory = false;
                
                // Check each app card in this category
                var appsPanel = expander.Content as Panel;
                if (appsPanel != null)
                {
                    foreach (SettingsCard card in appsPanel.Children)
                    {
                        string appName = card.Header?.ToString()?.ToLowerInvariant() ?? "";
                        string appDesc = card.Description?.ToString()?.ToLowerInvariant() ?? "";
                        
                        bool isMatch = appName.Contains(searchText) || appDesc.Contains(searchText);
                        
                        // Show/hide based on match
                        card.Visibility = isMatch ? Visibility.Visible : Visibility.Collapsed;
                        
                        if (isMatch)
                        {
                            matchInThisCategory = true;
                            foundAnyMatch = true;
                        }
                    }
                }
                
                // Show and expand category if it has matches
                expander.Visibility = matchInThisCategory ? Visibility.Visible : Visibility.Collapsed;
                
                if (matchInThisCategory)
                {
                    expander.IsExpanded = true;
                }
            }
            
            // If no matches found, show a message
            if (!foundAnyMatch)
            {
                // This could be enhanced to show a "no results" message
                // For now, just show all apps
                ShowAllApps();
            }
        }
        
        private void ShowAllApps()
        {
            // Show all categories and apps
            foreach (Expander expander in categoriesPanel.Children)
            {
                expander.Visibility = Visibility.Visible;
                
                var appsPanel = expander.Content as Panel;
                if (appsPanel != null)
                {
                    foreach (UIElement element in appsPanel.Children)
                    {
                        element.Visibility = Visibility.Visible;
                    }
                }
                
                // Keep first category expanded as default
                expander.IsExpanded = expander == categoriesPanel.Children[0];
            }
        }
        
        private void PackageManagerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (packageManagerComboBox != null)
            {
                int index = packageManagerComboBox.SelectedIndex;
                // Regenerate script
                GenerateInstallScript();
            }
        }

        private async Task LoadMainContent()
        {
            try
            {
                // Load application data
                await ApplicationDataService.Instance.LoadApplicationsDataAsync();
                applications = ApplicationDataService.Instance.GetAllApplications();
                
                // Create UI for applications
                await Task.Delay(100); // Tüm UI elementlerinin yüklenmesi için kısa bir bekleme
                CreateApplicationCards();
                
                // Başlangıçta seçili sayısını güncelle
                UpdateSelectedCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading application data: {ex.Message}",
                    (string)Application.Current.Resources["MessageBox_Error"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        private async Task SetupPackageManager()
        {
            try
            {
                // Paket yöneticilerinin durumunu sessizce kontrol et
                bool wingetInstalled = await IsWingetInstalledAsync(true);
                bool chocolateyInstalled = await IsChocolateyInstalledAsync(true);

                // UI'ı duruma göre güncelle
                if (packageManagerComboBox != null)
                {
                    packageManagerComboBox.SelectedIndex = wingetInstalled ? 0 : chocolateyInstalled ? 1 : 0;
                }
                
                // Paket yöneticileri kurulu değilse kur
                await InstallMissingPackageManagersIfNeeded(wingetInstalled, chocolateyInstalled);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error setting up package manager: {ex.Message}",
                    (string)Application.Current.Resources["MessageBox_Error"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
} 