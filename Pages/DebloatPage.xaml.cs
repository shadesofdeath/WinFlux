using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls;
using MessageBox = System.Windows.MessageBox;
using WinFlux.Services;

namespace WinFlux.Pages
{
    public partial class DebloatPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        public static DebloatPage Instance { get; private set; }
        private ContentDialog processingDialog;

        public DebloatPage()
        {
            InitializeComponent();
            Instance = this;
            
            // ToggleSwitch için olay dinleyicisini ekle
            tglEnableFeatures.Toggled += tglEnableFeatures_Toggled;
            
            // Dil değişikliği olayını dinle
            LanguageService.LanguageChanged += LanguageService_LanguageChanged;
            
            // Sayfa kapandığında olay dinleyicilerini kaldırmak için Unloaded olayını dinle
            this.Unloaded += DebloatPage_Unloaded;
            
            // Başlangıçta buton metnini güncelle
            UpdateButtonText();
        }

        private void DebloatPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Olası bellek sızıntılarını önlemek için olay dinleyicilerini kaldır
            LanguageService.LanguageChanged -= LanguageService_LanguageChanged;
            this.Unloaded -= DebloatPage_Unloaded;
        }

        private void LanguageService_LanguageChanged(object sender, EventArgs e)
        {
            // Dil değiştiğinde buton metinlerini güncelle
            UpdateButtonText();
        }
        
        private void tglEnableFeatures_Toggled(object sender, RoutedEventArgs e)
        {
            UpdateButtonText();
        }
        
        private void UpdateButtonText()
        {
            // Buton içeriği XAML'de Dynamic Resource'a bağlı olduğu için
            // burada sadece DataContext değiştiriyoruz, böylece dil değiştirildiğinde
            // buton direkt olarak yeni dil dosyasındaki metni alacak
            if (tglEnableFeatures.IsOn)
            {
                btnRemoveSelected.SetResourceReference(Button.ContentProperty, "DebloatPageEnableSelected");
            }
            else
            {
                btnRemoveSelected.SetResourceReference(Button.ContentProperty, "DebloatPageRemoveSelected");
            }
        }

        private void RunCommandAsAdmin(string scriptPath)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = $"-ExecutionPolicy Unrestricted -WindowStyle Normal -Command \"& {{ chcp 65001 | Out-Null; & '{scriptPath}'}}\"";
            process.StartInfo.Verb = "runas";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = false;

            try
            {
                HideProcessingDialog();
                process.Start();
                // Don't wait for exit to avoid UI freezing
            }
            catch (Exception ex)
            {
                HideProcessingDialog();
                MessageBox.Show($"Error: {ex.Message}", GetLocalizedString("MessageBox_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowProcessingDialog(string message)
        {
            if (processingDialog != null)
            {
                HideProcessingDialog();
            }

            processingDialog = new ContentDialog
            {
                Title = GetLocalizedString("DebloatPageProcessing"),
                Content = message,
                CloseButtonText = null
            };

            _ = processingDialog.ShowAsync();
        }

        private void HideProcessingDialog()
        {
            if (processingDialog != null)
            {
                processingDialog.Hide();
                processingDialog = null;
            }
        }

        private void ShowCompletionDialog(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = GetLocalizedString("DebloatPageCompleted"),
                Content = message,
                CloseButtonText = "OK"
            };

            _ = dialog.ShowAsync();
        }

        #region Helper Methods
        private IEnumerable<CheckBox> GetAllCheckBoxes()
        {
            List<CheckBox> allCheckboxes = new List<CheckBox>();
            allCheckboxes.AddRange(GetWindowsAppCheckboxes());
            allCheckboxes.AddRange(GetMicrosoftAppCheckboxes());
            allCheckboxes.AddRange(GetXboxAppCheckboxes());
            return allCheckboxes;
        }

        private IEnumerable<CheckBox> GetWindowsAppCheckboxes()
        {
            return new List<CheckBox>
            {
                chkCandyCrush,
                chkCandyCrushSoda,
                chkShazam,
                chkFlipboard,
                chkTwitter,
                chkIHeartRadio,
                chkDuolingo,
                chkAdobePhotoshop,
                chkPandora,
                chkEclipseManager,
                chkActiproSoftware,
                chkSpotify
            };
        }

        private IEnumerable<CheckBox> GetMicrosoftAppCheckboxes()
        {
            return new List<CheckBox>
            {
                chkMicrosoftFamily,
                chkOutlook,
                chkClipchamp,
                chk3DBuilder,
                chk3DViewer,
                chkBingWeather,
                chkBingSports,
                chkBingFinance,
                chkOfficeHub,
                chkBingNews,
                chkOneNote,
                chkSway,
                chkWindowsPhone,
                chkCommsPhone,
                chkYourPhone,
                chkGetStarted,
                chkSolitaire,
                chkStickyNotes,
                chkCommsApps,
                chkSkype,
                chkGroupMe,
                chkToDo,
                chkMixedReality,
                chkFeedbackHub,
                chkAlarmsClock,
                chkCamera,
                chkMSPaint,
                chkMaps,
                chkHeifImageExtension,
                chkWebpImageExtension,
                chkHEVCVideoExtension,
                chkRawImageExtension,
                chkWebMediaExtensions
            };
        }

        private IEnumerable<CheckBox> GetXboxAppCheckboxes()
        {
            return new List<CheckBox>
            {
                chkXboxApp,
                chkXboxTCUI,
                chkXboxGamingOverlay,
                chkXboxGameOverlay,
                chkXboxIdentityProvider,
                chkXboxSpeechToTextOverlay,
                chkGamingApp
            };
        }

        private void SelectAll(IEnumerable<CheckBox> checkboxes)
        {
            foreach (var checkbox in checkboxes)
            {
                checkbox.IsChecked = true;
            }
        }

        private void DeselectAll(IEnumerable<CheckBox> checkboxes)
        {
            foreach (var checkbox in checkboxes)
            {
                checkbox.IsChecked = false;
            }
        }

        private string GetAppPackageName(CheckBox checkbox)
        {
            switch (checkbox.Name)
            {
                // 3rd Party Apps
                case "chkCandyCrush": return "king.com.CandyCrushSaga";
                case "chkCandyCrushSoda": return "king.com.CandyCrushSodaSaga";
                case "chkShazam": return "ShazamEntertainmentLtd.Shazam";
                case "chkFlipboard": return "Flipboard.Flipboard";
                case "chkTwitter": return "*.Twitter";
                case "chkIHeartRadio": return "iHeartRadio.iHeartRadio";
                case "chkDuolingo": return "D5EA27B7.Duolingo-LearnLanguagesforFree";
                case "chkAdobePhotoshop": return "AdobeSystemsIncorporated.AdobePhotoshopExpress";
                case "chkPandora": return "PandoraMediaInc.29680B314EFC2";
                case "chkEclipseManager": return "46928bounde.EclipseManager";
                case "chkActiproSoftware": return "ActiproSoftwareLLC.562882FEEB491";
                case "chkSpotify": return "SpotifyAB.SpotifyMusic";

                // Microsoft Apps
                case "chkMicrosoftFamily": return "MicrosoftCorporationII.MicrosoftFamily";
                case "chkOutlook": return "Microsoft.OutlookForWindows";
                case "chkClipchamp": return "Clipchamp.Clipchamp";
                case "chk3DBuilder": return "Microsoft.3DBuilder";
                case "chk3DViewer": return "Microsoft.Microsoft3DViewer";
                case "chkBingWeather": return "Microsoft.BingWeather";
                case "chkBingSports": return "Microsoft.BingSports";
                case "chkBingFinance": return "Microsoft.BingFinance";
                case "chkOfficeHub": return "Microsoft.MicrosoftOfficeHub";
                case "chkBingNews": return "Microsoft.BingNews";
                case "chkOneNote": return "Microsoft.Office.OneNote";
                case "chkSway": return "Microsoft.Office.Sway";
                case "chkWindowsPhone": return "Microsoft.WindowsPhone";
                case "chkCommsPhone": return "Microsoft.CommsPhone";
                case "chkYourPhone": return "Microsoft.YourPhone";
                case "chkGetStarted": return "Microsoft.Getstarted";
                case "chkSolitaire": return "Microsoft.MicrosoftSolitaireCollection";
                case "chkStickyNotes": return "Microsoft.MicrosoftStickyNotes";
                case "chkCommsApps": return "microsoft.windowscommunicationsapps";
                case "chkSkype": return "Microsoft.SkypeApp";
                case "chkGroupMe": return "Microsoft.GroupMe10";
                case "chkToDo": return "Microsoft.Todos";
                case "chkMixedReality": return "Microsoft.MixedReality.Portal";
                case "chkFeedbackHub": return "Microsoft.WindowsFeedbackHub";
                case "chkAlarmsClock": return "Microsoft.WindowsAlarms";
                case "chkCamera": return "Microsoft.WindowsCamera";
                case "chkMSPaint": return "Microsoft.MSPaint";
                case "chkMaps": return "Microsoft.WindowsMaps";
                case "chkHeifImageExtension": return "Microsoft.HEIFImageExtension";
                case "chkWebpImageExtension": return "Microsoft.WebpImageExtension";
                case "chkHEVCVideoExtension": return "Microsoft.HEVCVideoExtension";
                case "chkRawImageExtension": return "Microsoft.RawImageExtension";
                case "chkWebMediaExtensions": return "Microsoft.WebMediaExtensions";

                // Xbox Apps
                case "chkXboxApp": return "Microsoft.XboxApp";
                case "chkXboxTCUI": return "Microsoft.Xbox.TCUI";
                case "chkXboxGamingOverlay": return "Microsoft.XboxGamingOverlay";
                case "chkXboxGameOverlay": return "Microsoft.XboxGameOverlay";
                case "chkXboxIdentityProvider": return "Microsoft.XboxIdentityProvider";
                case "chkXboxSpeechToTextOverlay": return "Microsoft.XboxSpeechToTextOverlay";
                case "chkGamingApp": return "Microsoft.GamingApp";

                default: return string.Empty;
            }
        }

        private string CreatePowerShellScript()
        {
            var scriptBuilder = new StringBuilder();
            
            // Add script header
            scriptBuilder.AppendLine("# Windows App Debloater Script");
            scriptBuilder.AppendLine("# Generated by WinFlux on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("# Functions for colored console output");
            scriptBuilder.AppendLine("function Write-Cyan { param([string]$Text) Write-Host $Text -ForegroundColor Cyan }");
            scriptBuilder.AppendLine("function Write-Green { param([string]$Text) Write-Host $Text -ForegroundColor Green }");
            scriptBuilder.AppendLine("function Write-Yellow { param([string]$Text) Write-Host $Text -ForegroundColor Yellow }");
            scriptBuilder.AppendLine("function Write-Red { param([string]$Text) Write-Host $Text -ForegroundColor Red }");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("Write-Green \"" + GetLocalizedString("DebloatPageStartingRemoval") + "\"");
            scriptBuilder.AppendLine();

            // Process selected checkboxes
            var selectedCheckboxes = GetAllCheckBoxes().Where(cb => cb.IsChecked == true);
            
            if (!selectedCheckboxes.Any())
            {
                scriptBuilder.AppendLine("Write-Yellow \"" + GetLocalizedString("DebloatPageNoAppSelected") + "\"");
                scriptBuilder.AppendLine("Write-Yellow \"" + GetLocalizedString("DebloatPageExitPrompt") + "\"");
                scriptBuilder.AppendLine("$host.UI.RawUI.ReadKey(\"NoEcho,IncludeKeyDown\") | Out-Null");
            }
            else
            {
                scriptBuilder.AppendLine("$totalApps = " + selectedCheckboxes.Count() + "");
                scriptBuilder.AppendLine("$currentApp = 0");
                scriptBuilder.AppendLine();

                foreach (var checkbox in selectedCheckboxes)
                {
                    string packageName = GetAppPackageName(checkbox);
                    if (!string.IsNullOrEmpty(packageName))
                    {
                        scriptBuilder.AppendLine("# " + checkbox.Content);
                        scriptBuilder.AppendLine("$currentApp++");
                        scriptBuilder.AppendLine("Write-Cyan \"[$currentApp/$totalApps] " + checkbox.Content + "...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    Get-AppxPackage \"" + packageName + "\" | Remove-AppxPackage -ErrorAction Stop");
                        scriptBuilder.AppendLine("    Write-Green \"  " + GetLocalizedString("DebloatPageRemovalSuccess") + "\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  " + GetLocalizedString("DebloatPageRemovalFailed") + " $\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }

                scriptBuilder.AppendLine("Write-Green \"" + GetLocalizedString("DebloatPageRemovalCompleted") + "\"");
                scriptBuilder.AppendLine("Write-Yellow \"" + GetLocalizedString("DebloatPageExitPrompt") + "\"");
                scriptBuilder.AppendLine("$host.UI.RawUI.ReadKey(\"NoEcho,IncludeKeyDown\") | Out-Null");
            }

            // Create temp directory if it doesn't exist
            string tempDir = Path.Combine(Path.GetTempPath(), "WinFlux");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // Write script to temp file
            string scriptPath = Path.Combine(tempDir, "debloat_script.ps1");
            File.WriteAllText(scriptPath, scriptBuilder.ToString());

            return scriptPath;
        }

        private string GetLocalizedString(string key)
        {
            return Application.Current.Resources[key] as string;
        }
        #endregion

        #region Button Events
        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAll(GetAllCheckBoxes());
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            DeselectAll(GetAllCheckBoxes());
        }

        private void btnRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            // Handle both Windows app removal and feature disabling
            ProcessSelectedItems();
        }
        #endregion

        private void ProcessSelectedItems()
        {
            bool hasAppsToRemove = GetAllCheckBoxes().Any(cb => cb.IsChecked == true);
            bool hasFeaturesToManage = HasWindowsFeaturesToManage();
            bool hasAppsToRemoveFromFeatures = HasWindowsAppsToRemove();
            bool isEnableMode = tglEnableFeatures.IsOn;

            // Kullanıcı mesajını güncelleyelim - Şimdi lokalize dizeleri kullanıyoruz
            string confirmMessage = GetLocalizedString("DebloatPageNoSelectedConfirmEnable");

            // Değiştirildi: Hiçbir öğe seçili olmasa bile Windows özelliklerini işlemek için devam et
            if (!hasAppsToRemove && !hasFeaturesToManage && !hasAppsToRemoveFromFeatures)
            {
                // Kullanıcıya soralım: Hiçbir özellik seçilmedi, etkinleştirmek istiyor mu?
                MessageBoxResult result = MessageBox.Show(
                    confirmMessage,
                    GetLocalizedString("MessageBox_Confirmation"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Kullanıcı onayladıysa, Windows özelliklerini işlemek için devam et
                    ShowProcessingDialog(GetLocalizedString("DebloatPagePreparingScript"));
                    string enableScriptPath = CreateCombinedScript(true, false, false);
                    HideProcessingDialog();
                    RunCommandAsAdmin(enableScriptPath);
                    ShowCompletionDialog(GetLocalizedString("DebloatPageCompleted"));
                }
                return;
            }

            ShowProcessingDialog(GetLocalizedString("DebloatPagePreparingScript"));
            
            // Create a combined script
            string scriptPath = CreateCombinedScript(hasFeaturesToManage || !hasAppsToRemove, hasAppsToRemoveFromFeatures, hasAppsToRemove);
            
            HideProcessingDialog();

            // Run combined script
            RunCommandAsAdmin(scriptPath);
            
            ShowCompletionDialog(GetLocalizedString("DebloatPageCompleted"));
        }

        private bool HasWindowsFeaturesToManage()
        {
            // Bu metot Windows özelliklerini yönetmek istiyorsak true döndürecek
            // Kullanıcı özellikler kısmını görüntülediği anda bu fonksiyon true dönecek
            // Bu sayede checkbox'ları seçmeden de Windows özellikleri işlenecek
            // Eğer seçili checkbox varsa, seçilenleri işlem yapacağız
            // Eğer hiçbir checkbox seçili değilse, tümünü işleyeceğiz
            return true;
        }

        private bool HasWindowsAppsToRemove()
        {
            return chkStore.IsChecked == true ||
                   chkOneDrive.IsChecked == true ||
                   chkEdge.IsChecked == true ||
                   chkEdgeRemove.IsChecked == true ||
                   chkCopilot.IsChecked == true ||
                   chkWidgets.IsChecked == true ||
                   chkTaskbarWidgets.IsChecked == true;
        }

        private string CreateCombinedScript(bool includeWindowsFeatures, bool includeSystemApps, bool includeStandardApps)
        {
            var scriptBuilder = new StringBuilder();
            bool isEnableMode = tglEnableFeatures.IsOn;
            
            // Add script header
            scriptBuilder.AppendLine("# WinFlux Combined Optimization Script");
            scriptBuilder.AppendLine("# Generated by WinFlux on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            // Ensure UTF-8 encoding for proper display of characters
            scriptBuilder.AppendLine("# Ensure UTF-8 encoding for proper display of characters");
            scriptBuilder.AppendLine("chcp 65001 | Out-Null");
            scriptBuilder.AppendLine("[Console]::OutputEncoding = [System.Text.Encoding]::UTF8");
            scriptBuilder.AppendLine("[Console]::InputEncoding = [System.Text.Encoding]::UTF8");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("# Check if running as administrator");
            scriptBuilder.AppendLine("if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {");
            scriptBuilder.AppendLine("    Write-Host \"This script requires administrator privileges.\" -ForegroundColor Red");
            scriptBuilder.AppendLine("    Write-Host \"Please run this script as an administrator.\" -ForegroundColor Red");
            scriptBuilder.AppendLine("    Read-Host \"Press Enter to exit\"");
            scriptBuilder.AppendLine("    exit");
            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("# Functions for colored console output");
            scriptBuilder.AppendLine("function Write-Cyan { param([string]$Text) Write-Host $Text -ForegroundColor Cyan }");
            scriptBuilder.AppendLine("function Write-Green { param([string]$Text) Write-Host $Text -ForegroundColor Green }");
            scriptBuilder.AppendLine("function Write-Yellow { param([string]$Text) Write-Host $Text -ForegroundColor Yellow }");
            scriptBuilder.AppendLine("function Write-Red { param([string]$Text) Write-Host $Text -ForegroundColor Red }");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("Write-Green \"Starting optimization process...\"");
            scriptBuilder.AppendLine();

            // Windows Features Section
            if (includeWindowsFeatures)
            {
                scriptBuilder.AppendLine("# ==========================================");
                scriptBuilder.AppendLine("# WINDOWS FEATURES SECTION");
                scriptBuilder.AppendLine("# ==========================================");
                scriptBuilder.AppendLine("Write-Cyan \"Processing Windows Features...\"");
                scriptBuilder.AppendLine();

                // Consumer Features - Registry based so handle differently
                if (chkConsumerFeatures.IsChecked == true)
                {
                    if (!isEnableMode)
                    {
                        scriptBuilder.AppendLine("# Disable Consumer Features");
                        scriptBuilder.AppendLine("Write-Cyan \"Disabling Consumer Features...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    New-Item -Path \"HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\CloudContent\" -Force -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("    New-ItemProperty -Path \"HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\CloudContent\" -Name \"DisableWindowsConsumerFeatures\" -Value 1 -PropertyType DWORD -Force | Out-Null");
                        scriptBuilder.AppendLine("    Write-Green \"  Feature successfully disabled.\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                    else 
                    {
                        scriptBuilder.AppendLine("# Enable Consumer Features");
                        scriptBuilder.AppendLine("Write-Cyan \"Enabling Consumer Features...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    Remove-ItemProperty -Path \"HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\CloudContent\" -Name \"DisableWindowsConsumerFeatures\" -Force -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("    Write-Green \"  Feature successfully enabled.\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }

                // Recall feature
                if (chkRecall.IsChecked == true)
                {
                    if (!isEnableMode)
                    {
                        scriptBuilder.AppendLine("# Disable Recall");
                        scriptBuilder.AppendLine("Write-Cyan \"Disabling Recall...\"");
                        scriptBuilder.AppendLine("try {");
                        // Düzeltildi: Recall feature'ı bazı Windows sürümlerinde olmayabilir
                        scriptBuilder.AppendLine("    $feature = Get-WindowsOptionalFeature -Online -FeatureName \"Recall\" -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("    if ($feature) {");
                        scriptBuilder.AppendLine("        Disable-WindowsOptionalFeature -Online -FeatureName \"Recall\" -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("        Write-Green \"  Feature successfully disabled.\"");
                        scriptBuilder.AppendLine("    } else {");
                        scriptBuilder.AppendLine("        Write-Yellow \"  Feature not found.\"");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                    else
                    {
                        scriptBuilder.AppendLine("# Enable Recall");
                        scriptBuilder.AppendLine("Write-Cyan \"Enabling Recall...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    $feature = Get-WindowsOptionalFeature -Online -FeatureName \"Recall\" -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("    if ($feature) {");
                        scriptBuilder.AppendLine("        Enable-WindowsOptionalFeature -Online -FeatureName \"Recall\" -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("        Write-Green \"  Feature successfully enabled.\"");
                        scriptBuilder.AppendLine("    } else {");
                        scriptBuilder.AppendLine("        Write-Yellow \"  Feature not found.\"");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }

                if (chkInternetExplorer.IsChecked == true)
                {
                    if (!isEnableMode)
                    {
                        scriptBuilder.AppendLine("# Disable Internet Explorer");
                        scriptBuilder.AppendLine("Write-Cyan \"Disabling Internet Explorer...\"");
                        scriptBuilder.AppendLine("try {");
                        // Windows 11 ve 10'da IE özelliği farklı isimlerde olabilir
                        scriptBuilder.AppendLine("    $ieFeatureNames = @(");
                        scriptBuilder.AppendLine("        'Internet-Explorer-Optional-amd64',");
                        scriptBuilder.AppendLine("        'Internet-Explorer-Optional-x86',");
                        scriptBuilder.AppendLine("        'Internet-Explorer-Optional-x64',");
                        scriptBuilder.AppendLine("        'InternetExplorer',");
                        scriptBuilder.AppendLine("        'Internet-Explorer',");
                        scriptBuilder.AppendLine("        'Microsoft-Windows-InternetExplorer-Optional-Package'");
                        scriptBuilder.AppendLine("    )");
                        scriptBuilder.AppendLine("    $found = $false");
                        scriptBuilder.AppendLine("    foreach ($featureName in $ieFeatureNames) {");
                        scriptBuilder.AppendLine("        $feature = Get-WindowsOptionalFeature -Online -FeatureName $featureName -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("        if ($feature -and $feature.State -eq 'Enabled') {");
                        scriptBuilder.AppendLine("            Disable-WindowsOptionalFeature -Online -FeatureName $featureName -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("            $found = $true");
                        scriptBuilder.AppendLine("            Write-Green \"  Feature successfully disabled ($featureName).\"");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        # Alternatif olarak DISM komutu deneyelim");
                        scriptBuilder.AppendLine("        try {");
                        scriptBuilder.AppendLine("            Start-Process -FilePath \"DISM.exe\" -ArgumentList \"/Online /Disable-Feature /FeatureName:Internet-Explorer-Optional-amd64 /NoRestart\" -Wait -WindowStyle Hidden");
                        scriptBuilder.AppendLine("            $found = $true");
                        scriptBuilder.AppendLine("        } catch {");
                        scriptBuilder.AppendLine("            # DISM komutu da çalışmazsa hata mesajı verelim");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        Write-Yellow \"  Feature not found.\"");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                    else
                    {
                        scriptBuilder.AppendLine("# Enable Internet Explorer");
                        scriptBuilder.AppendLine("Write-Cyan \"Enabling Internet Explorer...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    $ieFeatureNames = @(");
                        scriptBuilder.AppendLine("        'Internet-Explorer-Optional-amd64',");
                        scriptBuilder.AppendLine("        'Internet-Explorer-Optional-x86',");
                        scriptBuilder.AppendLine("        'Internet-Explorer-Optional-x64',");
                        scriptBuilder.AppendLine("        'InternetExplorer',");
                        scriptBuilder.AppendLine("        'Internet-Explorer',");
                        scriptBuilder.AppendLine("        'Microsoft-Windows-InternetExplorer-Optional-Package'");
                        scriptBuilder.AppendLine("    )");
                        scriptBuilder.AppendLine("    $found = $false");
                        scriptBuilder.AppendLine("    foreach ($featureName in $ieFeatureNames) {");
                        scriptBuilder.AppendLine("        $feature = Get-WindowsOptionalFeature -Online -FeatureName $featureName -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("        if ($feature -and $feature.State -eq 'Disabled') {");
                        scriptBuilder.AppendLine("            Enable-WindowsOptionalFeature -Online -FeatureName $featureName -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("            $found = $true");
                        scriptBuilder.AppendLine("            Write-Green \"  Feature successfully enabled ($featureName).\"");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        # Alternatif olarak DISM komutu deneyelim");
                        scriptBuilder.AppendLine("        try {");
                        scriptBuilder.AppendLine("            Start-Process -FilePath \"DISM.exe\" -ArgumentList \"/Online /Enable-Feature /FeatureName:Internet-Explorer-Optional-amd64 /NoRestart\" -Wait -WindowStyle Hidden");
                        scriptBuilder.AppendLine("            $found = $true");
                        scriptBuilder.AppendLine("        } catch {");
                        scriptBuilder.AppendLine("            # DISM komutu da çalışmazsa hata mesajı verelim");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        Write-Yellow \"  Feature not found.\"");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }

                if (chkHyperV.IsChecked == true)
                {
                    if (!isEnableMode)
                    {
                        scriptBuilder.AppendLine("# Disable Hyper-V");
                        scriptBuilder.AppendLine("Write-Cyan \"Disabling Hyper-V...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    Disable-WindowsOptionalFeature -FeatureName 'Microsoft-Hyper-V-All' -Online -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("    Write-Green \"  Feature successfully disabled.\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                    else
                    {
                        scriptBuilder.AppendLine("# Enable Hyper-V");
                        scriptBuilder.AppendLine("Write-Cyan \"Enabling Hyper-V...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    Enable-WindowsOptionalFeature -FeatureName 'Microsoft-Hyper-V-All' -Online -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("    Write-Green \"  Feature successfully enabled.\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }

                if (chkFaxScan.IsChecked == true)
                {
                    if (!isEnableMode)
                    {
                        scriptBuilder.AppendLine("# Disable Fax and Scan");
                        scriptBuilder.AppendLine("Write-Cyan \"Disabling Fax and Scan...\"");
                        scriptBuilder.AppendLine("try {");
                        // Farklı Windows sürümlerinde Fax özellikleri değişik isimlerde olabilir
                        scriptBuilder.AppendLine("    $faxFeatureNames = @(");
                        scriptBuilder.AppendLine("        'FaxServicesClientPackage',");
                        scriptBuilder.AppendLine("        'Printing-FaxServicesClientPackage',");
                        scriptBuilder.AppendLine("        'Scan-UI-Fax',");
                        scriptBuilder.AppendLine("        'Windows-Fax-And-Scan'");
                        scriptBuilder.AppendLine("    )");
                        scriptBuilder.AppendLine("    $found = $false");
                        scriptBuilder.AppendLine("    foreach ($featureName in $faxFeatureNames) {");
                        scriptBuilder.AppendLine("        $feature = Get-WindowsOptionalFeature -Online -FeatureName $featureName -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("        if ($feature -and $feature.State -eq 'Enabled') {");
                        scriptBuilder.AppendLine("            Disable-WindowsOptionalFeature -Online -FeatureName $featureName -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("            $found = $true");
                        scriptBuilder.AppendLine("            Write-Green \"  Feature successfully disabled ($featureName).\"");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        # Servis bazlı devre dışı bırakma deneyelim");
                        scriptBuilder.AppendLine("        $services = @('Fax')");
                        scriptBuilder.AppendLine("        foreach ($service in $services) {");
                        scriptBuilder.AppendLine("            if (Get-Service -Name $service -ErrorAction SilentlyContinue) {");
                        scriptBuilder.AppendLine("                Stop-Service -Name $service -Force -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("                Set-Service -Name $service -StartupType Disabled -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("                $found = $true");
                        scriptBuilder.AppendLine("                Write-Green \"  Feature successfully disabled (Service: $service).\"");
                        scriptBuilder.AppendLine("            }");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        Write-Yellow \"  Feature not found.\"");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                    else
                    {
                        scriptBuilder.AppendLine("# Enable Fax and Scan");
                        scriptBuilder.AppendLine("Write-Cyan \"Enabling Fax and Scan...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    $faxFeatureNames = @(");
                        scriptBuilder.AppendLine("        'FaxServicesClientPackage',");
                        scriptBuilder.AppendLine("        'Printing-FaxServicesClientPackage',");
                        scriptBuilder.AppendLine("        'Scan-UI-Fax',");
                        scriptBuilder.AppendLine("        'Windows-Fax-And-Scan'");
                        scriptBuilder.AppendLine("    )");
                        scriptBuilder.AppendLine("    $found = $false");
                        scriptBuilder.AppendLine("    foreach ($featureName in $faxFeatureNames) {");
                        scriptBuilder.AppendLine("        $feature = Get-WindowsOptionalFeature -Online -FeatureName $featureName -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("        if ($feature -and $feature.State -eq 'Disabled') {");
                        scriptBuilder.AppendLine("            Enable-WindowsOptionalFeature -Online -FeatureName $featureName -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("            $found = $true");
                        scriptBuilder.AppendLine("            Write-Green \"  Feature successfully enabled ($featureName).\"");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        # Servis bazlı etkinleştirme deneyelim");
                        scriptBuilder.AppendLine("        $services = @('Fax')");
                        scriptBuilder.AppendLine("        foreach ($service in $services) {");
                        scriptBuilder.AppendLine("            if (Get-Service -Name $service -ErrorAction SilentlyContinue) {");
                        scriptBuilder.AppendLine("                Set-Service -Name $service -StartupType Manual -ErrorAction SilentlyContinue");
                        scriptBuilder.AppendLine("                $found = $true");
                        scriptBuilder.AppendLine("                Write-Green \"  Feature successfully enabled (Service: $service).\"");
                        scriptBuilder.AppendLine("            }");
                        scriptBuilder.AppendLine("        }");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("    if (-not $found) {");
                        scriptBuilder.AppendLine("        Write-Yellow \"  Feature not found.\"");
                        scriptBuilder.AppendLine("    }");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }

                if (chkMediaPlayer.IsChecked == true)
                {
                    if (!isEnableMode)
                    {
                        scriptBuilder.AppendLine("# Disable Windows Media Player");
                        scriptBuilder.AppendLine("Write-Cyan \"Disabling Windows Media Player...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    Disable-WindowsOptionalFeature -FeatureName 'WindowsMediaPlayer' -Online -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("    Write-Green \"  Feature successfully disabled.\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                    else
                    {
                        scriptBuilder.AppendLine("# Enable Windows Media Player");
                        scriptBuilder.AppendLine("Write-Cyan \"Enabling Windows Media Player...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine("    Enable-WindowsOptionalFeature -FeatureName 'WindowsMediaPlayer' -Online -NoRestart -ErrorAction SilentlyContinue | Out-Null");
                        scriptBuilder.AppendLine("    Write-Green \"  Feature successfully enabled.\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }
                
                scriptBuilder.AppendLine("Write-Green \"Windows Features processing completed.\"");
                scriptBuilder.AppendLine();
            }

            // System Apps Section
            if (includeSystemApps)
            {
                scriptBuilder.AppendLine("# ==========================================");
                scriptBuilder.AppendLine("# SYSTEM APPS SECTION");
                scriptBuilder.AppendLine("# ==========================================");
                scriptBuilder.AppendLine("Write-Cyan \"Processing System Apps...\"");
                scriptBuilder.AppendLine();
                
                // Add script header for removing Microsoft Store
                if (chkStore.IsChecked == true)
                {
                    scriptBuilder.AppendLine("# Remove Microsoft Store");
                    scriptBuilder.AppendLine("Write-Cyan \"Removing Microsoft Store...\"");
                    scriptBuilder.AppendLine("try {");
                    scriptBuilder.AppendLine("    Get-AppxPackage -Name 'Microsoft.WindowsStore' | Remove-AppxPackage -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -eq 'Microsoft.WindowsStore' } | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Write-Green \"  Feature successfully removed.\"");
                    scriptBuilder.AppendLine("} catch {");
                    scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                    scriptBuilder.AppendLine("}");
                    scriptBuilder.AppendLine();
                }

                // Add OneDrive removal
                if (chkOneDrive.IsChecked == true)
                {
                    scriptBuilder.AppendLine("# Remove OneDrive");
                    scriptBuilder.AppendLine("Write-Cyan \"Removing OneDrive...\"");
                    scriptBuilder.AppendLine("try {");
                    scriptBuilder.AppendLine("    # Kill OneDrive process");
                    scriptBuilder.AppendLine("    Get-Process -Name \"OneDrive\" -ErrorAction SilentlyContinue | Stop-Process -Force");
                    scriptBuilder.AppendLine("    # Uninstall OneDrive");
                    scriptBuilder.AppendLine("    if (Test-Path \"$env:SystemRoot\\System32\\OneDriveSetup.exe\") {");
                    scriptBuilder.AppendLine("        Write-Host \"    Uninstalling OneDrive from System32...\"");
                    scriptBuilder.AppendLine("        & \"$env:SystemRoot\\System32\\OneDriveSetup.exe\" /uninstall");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    if (Test-Path \"$env:SystemRoot\\SysWOW64\\OneDriveSetup.exe\") {");
                    scriptBuilder.AppendLine("        Write-Host \"    Uninstalling OneDrive from SysWOW64...\"");
                    scriptBuilder.AppendLine("        & \"$env:SystemRoot\\SysWOW64\\OneDriveSetup.exe\" /uninstall");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    # Remove OneDrive from explorer sidebar");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"HKLM:\\SOFTWARE\\WOW6432Node\\Classes\\CLSID\\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\" -Recurse -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"HKLM:\\SOFTWARE\\Classes\\CLSID\\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\" -Recurse -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    # Remove OneDrive shortcut");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"$env:APPDATA\\Microsoft\\Windows\\Start Menu\\Programs\\OneDrive.lnk\" -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    # Remove scheduled tasks");
                    scriptBuilder.AppendLine("    Get-ScheduledTask -TaskPath '\\' -TaskName 'OneDrive*' -ErrorAction SilentlyContinue | Unregister-ScheduledTask -Confirm:$false -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    # Remove OneDrive leftovers");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"$env:USERPROFILE\\OneDrive\" -Recurse -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"$env:LOCALAPPDATA\\OneDrive\" -Recurse -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"$env:LOCALAPPDATA\\Microsoft\\OneDrive\" -Recurse -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"$env:PROGRAMDATA\\Microsoft OneDrive\" -Recurse -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"C:\\OneDriveTemp\" -Recurse -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    # Remove OneDrive registry entries");
                    scriptBuilder.AppendLine("    Remove-Item -Path \"HKCU:\\Software\\Microsoft\\OneDrive\" -Recurse -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    # Restore default folders");
                    scriptBuilder.AppendLine("    New-ItemProperty -Path \"HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders\" -Name \"AppData\" -Value \"$env:USERPROFILE\\AppData\\Roaming\" -PropertyType ExpandString -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("    Write-Green \"  OneDrive successfully removed.\"");
                    scriptBuilder.AppendLine("} catch {");
                    scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                    scriptBuilder.AppendLine("}");
                    scriptBuilder.AppendLine();
                }

                // Fix Edge registry edits
                if (chkEdge.IsChecked == true)
                {
                    scriptBuilder.AppendLine("# Debloat Edge");
                    scriptBuilder.AppendLine("Write-Cyan \"Debloating Microsoft Edge...\"");
                    scriptBuilder.AppendLine("try {");
                    scriptBuilder.AppendLine("    # Create registry keys if they don't exist");
                    scriptBuilder.AppendLine("    $edgePath = 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Edge'");
                    scriptBuilder.AppendLine("    if (-not (Test-Path $edgePath)) {");
                    scriptBuilder.AppendLine("        New-Item -Path $edgePath -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    # Set registry values");
                    scriptBuilder.AppendLine("    $edgeSettings = @{");
                    scriptBuilder.AppendLine("        'EdgeEnhanceImagesEnabled' = 0;");
                    scriptBuilder.AppendLine("        'PersonalizationReportingEnabled' = 0;");
                    scriptBuilder.AppendLine("        'ShowRecommendationsEnabled' = 0;");
                    scriptBuilder.AppendLine("        'HideFirstRunExperience' = 1;");
                    scriptBuilder.AppendLine("        'UserFeedbackAllowed' = 0;");
                    scriptBuilder.AppendLine("        'ConfigureDoNotTrack' = 1;");
                    scriptBuilder.AppendLine("        'AlternateErrorPagesEnabled' = 0;");
                    scriptBuilder.AppendLine("        'EdgeCollectionsEnabled' = 0;");
                    scriptBuilder.AppendLine("        'EdgeFollowEnabled' = 0;");
                    scriptBuilder.AppendLine("        'EdgeShoppingAssistantEnabled' = 0;");
                    scriptBuilder.AppendLine("        'MicrosoftEdgeInsiderPromotionEnabled' = 0;");
                    scriptBuilder.AppendLine("        'ShowMicrosoftRewards' = 0;");
                    scriptBuilder.AppendLine("        'WebWidgetAllowed' = 0;");
                    scriptBuilder.AppendLine("        'DiagnosticData' = 0;");
                    scriptBuilder.AppendLine("        'EdgeAssetDeliveryServiceEnabled' = 0;");
                    scriptBuilder.AppendLine("        'CryptoWalletEnabled' = 0;");
                    scriptBuilder.AppendLine("        'WalletDonationEnabled' = 0");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    $edgeSettings.GetEnumerator() | ForEach-Object {");
                    scriptBuilder.AppendLine("        New-ItemProperty -Path $edgePath -Name $_.Key -Value $_.Value -PropertyType DWORD -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    Write-Green \"  Feature successfully disabled.\"");
                    scriptBuilder.AppendLine("} catch {");
                    scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                    scriptBuilder.AppendLine("}");
                    scriptBuilder.AppendLine();
                }

                // Fix Edge Remover script
                if (chkEdgeRemove.IsChecked == true)
                {
                    scriptBuilder.AppendLine("# Remove Edge");
                    scriptBuilder.AppendLine("Write-Cyan \"Removing Microsoft Edge...\"");
                    scriptBuilder.AppendLine("try {");
                    scriptBuilder.AppendLine("    # Use direct commands instead of downloading scripts for security");
                    scriptBuilder.AppendLine("    Get-AppxPackage *MicrosoftEdge* | Remove-AppxPackage -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Get-WindowsCapability -Online -Name 'Microsoft.Windows.EdgeChromium*' | Remove-WindowsCapability -Online -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Write-Green \"  Feature successfully removed.\"");
                    scriptBuilder.AppendLine("} catch {");
                    scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                    scriptBuilder.AppendLine("}");
                    scriptBuilder.AppendLine();
                }

                // Fix Copilot registry edits
                if (chkCopilot.IsChecked == true)
                {
                    scriptBuilder.AppendLine("# Remove Copilot");
                    scriptBuilder.AppendLine("Write-Cyan \"Removing Copilot...\"");
                    scriptBuilder.AppendLine("try {");
                    scriptBuilder.AppendLine("    # Remove Copilot app");
                    scriptBuilder.AppendLine("    Get-AppxPackage *Microsoft.Windows.Copilot* | Remove-AppxPackage -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    Get-AppxPackage *Microsoft.CoPilot* | Remove-AppxPackage -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    # Disable Copilot in registry");
                    scriptBuilder.AppendLine("    $paths = @(");
                    scriptBuilder.AppendLine("        @{Path = 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsCopilot'; Name = 'TurnOffWindowsCopilot'; Value = 1},");
                    scriptBuilder.AppendLine("        @{Path = 'HKCU:\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot'; Name = 'TurnOffWindowsCopilot'; Value = 1},");
                    scriptBuilder.AppendLine("        @{Path = 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced'; Name = 'ShowCopilotButton'; Value = 0},");
                    scriptBuilder.AppendLine("        @{Path = 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Edge'; Name = 'HubsSidebarEnabled'; Value = 0}");
                    scriptBuilder.AppendLine("    )");
                    scriptBuilder.AppendLine("    foreach ($item in $paths) {");
                    scriptBuilder.AppendLine("        $path = $item.Path");
                    scriptBuilder.AppendLine("        if (-not (Test-Path $path)) {");
                    scriptBuilder.AppendLine("            New-Item -Path $path -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("        }");
                    scriptBuilder.AppendLine("        New-ItemProperty -Path $path -Name $item.Name -Value $item.Value -PropertyType DWORD -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    # Try to create BingChat path");
                    scriptBuilder.AppendLine("    $bingChatPath = 'HKCU:\\Software\\Microsoft\\Windows\\Shell\\Copilot\\BingChat'");
                    scriptBuilder.AppendLine("    if (-not (Test-Path $bingChatPath)) {");
                    scriptBuilder.AppendLine("        New-Item -Path $bingChatPath -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("        New-ItemProperty -Path $bingChatPath -Name 'IsUserEligible' -Value 0 -PropertyType DWORD -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    Write-Green \"  " + GetLocalizedString("DebloatPageFeatureRemoved") + "\"");
                    scriptBuilder.AppendLine("} catch {");
                    scriptBuilder.AppendLine("    Write-Red \"  " + GetLocalizedString("DebloatPageError") + " $_\"");
                    scriptBuilder.AppendLine("}");
                    scriptBuilder.AppendLine();
                }

                // Fix Widgets registry edits
                if (chkWidgets.IsChecked == true)
                {
                    scriptBuilder.AppendLine("# Remove Widgets");
                    scriptBuilder.AppendLine("Write-Cyan \"" + GetLocalizedString("DebloatPageRemovingWidgets") + "\"");
                    scriptBuilder.AppendLine("try {");
                    scriptBuilder.AppendLine("    # Remove widgets app with error handling");
                    scriptBuilder.AppendLine("    $widgets = Get-AppxPackage *WebExperience* -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("    if ($widgets) {");
                    scriptBuilder.AppendLine("        foreach ($widget in $widgets) {");
                    scriptBuilder.AppendLine("            try {");
                    scriptBuilder.AppendLine("                Remove-AppxPackage -Package $widget.PackageFullName -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("                Write-Green \"  Widget app removed successfully.\"");
                    scriptBuilder.AppendLine("            } catch {");
                    scriptBuilder.AppendLine("                Write-Yellow \"  Could not remove widget app: $_\"");
                    scriptBuilder.AppendLine("            }");
                    scriptBuilder.AppendLine("        }");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    # Disable widgets in registry with safer approach");
                    scriptBuilder.AppendLine("    $dshPath = 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Dsh'");
                    scriptBuilder.AppendLine("    try {");
                    scriptBuilder.AppendLine("        if (-not (Test-Path $dshPath)) {");
                    scriptBuilder.AppendLine("            New-Item -Path $dshPath -Force -ErrorAction SilentlyContinue | Out-Null");
                    scriptBuilder.AppendLine("        }");
                    scriptBuilder.AppendLine("        Set-ItemProperty -Path $dshPath -Name 'AllowNewsAndInterests' -Value 0 -Type DWord -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("        Write-Green \"  Registry settings updated successfully.\"");
                    scriptBuilder.AppendLine("    } catch {");
                    scriptBuilder.AppendLine("        Write-Yellow \"  Could not update registry settings: $_\"");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    # Try to disable the service");
                    scriptBuilder.AppendLine("    try {");
                    scriptBuilder.AppendLine("        $service = Get-Service -Name 'WebExperience' -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("        if ($service) {");
                    scriptBuilder.AppendLine("            Stop-Service -Name 'WebExperience' -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("            Set-Service -Name 'WebExperience' -StartupType Disabled -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("            Write-Green \"  Widget service disabled successfully.\"");
                    scriptBuilder.AppendLine("        }");
                    scriptBuilder.AppendLine("    } catch {");
                    scriptBuilder.AppendLine("        Write-Yellow \"  Could not disable widget service: $_\"");
                    scriptBuilder.AppendLine("    }");
                    scriptBuilder.AppendLine("    Write-Green \"  Widgets removal process completed.\"");
                    scriptBuilder.AppendLine("} catch {");
                    scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                    scriptBuilder.AppendLine("}");
                    scriptBuilder.AppendLine();
                }

                // Fix Taskbar Widgets registry edits
                if (chkTaskbarWidgets.IsChecked == true)
                {
                    scriptBuilder.AppendLine("# Remove Taskbar Widgets");
                    scriptBuilder.AppendLine("Write-Cyan \"Removing Taskbar Widgets...\"");
                    scriptBuilder.AppendLine("try {");
                    // Daha güvenli registry düzenlemesi
                    scriptBuilder.AppendLine("    # Safer registry modifications");
                    scriptBuilder.AppendLine("    $explorerKeyExists = Test-Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced'");
                    
                    scriptBuilder.AppendLine("    # Modify Explorer Advanced registry keys safely");
                    scriptBuilder.AppendLine("    if ($explorerKeyExists) {");
                    scriptBuilder.AppendLine("        try {");
                    scriptBuilder.AppendLine("            # Disable TaskbarDa and ShowTaskViewButton by setting them to 0");
                    scriptBuilder.AppendLine("            Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'TaskbarDa' -Value 0 -Type DWord -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("            Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'ShowTaskViewButton' -Value 0 -Type DWord -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("            Write-Green \"  Feature successfully removed (Taskbar Widgets).\"");
                    scriptBuilder.AppendLine("        } catch {");
                    scriptBuilder.AppendLine("            # Inform the user if registry modification fails");
                    scriptBuilder.AppendLine("            Write-Yellow \"  Taskbar widgets registry modification failed. This may be normal.\"");
                    scriptBuilder.AppendLine("        }");
                    scriptBuilder.AppendLine("    }");
                    
                    scriptBuilder.AppendLine("    # Use Local Group Policy settings as an alternative");
                    scriptBuilder.AppendLine("    $policyPath = 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Feeds'");
                    scriptBuilder.AppendLine("    if (-not (Test-Path $policyPath)) {");
                    scriptBuilder.AppendLine("        try {");
                    scriptBuilder.AppendLine("            New-Item -Path $policyPath -Force | Out-Null");
                    scriptBuilder.AppendLine("        } catch {");
                    scriptBuilder.AppendLine("            Write-Yellow \"  Unable to create Windows Feeds policy key. This may be normal.\"");
                    scriptBuilder.AppendLine("        }");
                    scriptBuilder.AppendLine("    }");
                    
                    scriptBuilder.AppendLine("    try {");
                    scriptBuilder.AppendLine("        Set-ItemProperty -Path $policyPath -Name 'EnableFeeds' -Value 0 -Type DWord -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("        Write-Green \"  Feature successfully removed (Windows Feeds).\"");
                    scriptBuilder.AppendLine("    } catch {");
                    scriptBuilder.AppendLine("        Write-Yellow \"  Unable to update Windows Feeds policy. This may be normal.\"");
                    scriptBuilder.AppendLine("    }");
                    
                    scriptBuilder.AppendLine("    # Check Windows services too");
                    scriptBuilder.AppendLine("    $services = @('Feeds', 'TabletInputService')");
                    scriptBuilder.AppendLine("    foreach ($service in $services) {");
                    scriptBuilder.AppendLine("        if (Get-Service -Name $service -ErrorAction SilentlyContinue) {");
                    scriptBuilder.AppendLine("            try {");
                    scriptBuilder.AppendLine("                Stop-Service -Name $service -Force -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("                Set-Service -Name $service -StartupType Disabled -ErrorAction SilentlyContinue");
                    scriptBuilder.AppendLine("                Write-Green \"  Feature successfully disabled (Service: $service).\"");
                    scriptBuilder.AppendLine("            } catch {");
                    scriptBuilder.AppendLine("                Write-Yellow \"  Service $service could not be disabled. This may be normal.\"");
                    scriptBuilder.AppendLine("            }");
                    scriptBuilder.AppendLine("        }");
                    scriptBuilder.AppendLine("    }");
                    
                    scriptBuilder.AppendLine("    Write-Green \"  Feature successfully removed.\"");
                    scriptBuilder.AppendLine("} catch {");
                    scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                    scriptBuilder.AppendLine("}");
                    scriptBuilder.AppendLine();
                }
                
                scriptBuilder.AppendLine("Write-Green \"System Apps processing completed.\"");
                scriptBuilder.AppendLine();
            }

            // Third Party and Microsoft Apps Section
            if (includeStandardApps)
            {
                scriptBuilder.AppendLine("# ==========================================");
                scriptBuilder.AppendLine("# THIRD PARTY AND MICROSOFT APPS SECTION");
                scriptBuilder.AppendLine("# ==========================================");
                scriptBuilder.AppendLine("Write-Cyan \"Processing Third-Party and Microsoft Apps...\"");
                scriptBuilder.AppendLine();
                
                // Build PowerShell commands for regular app removal
                foreach (var checkbox in GetAllCheckBoxes().Where(c => c.IsChecked == true))
                {
                    string packageName = GetAppPackageName(checkbox);
                    if (!string.IsNullOrEmpty(packageName))
                    {
                        scriptBuilder.AppendLine($"# Remove {checkbox.Content}");
                        scriptBuilder.AppendLine($"Write-Cyan \"Removing {checkbox.Content}...\"");
                        scriptBuilder.AppendLine("try {");
                        scriptBuilder.AppendLine($"    Get-AppxPackage *{packageName}* | Remove-AppxPackage");
                        scriptBuilder.AppendLine("    Write-Green \"  App successfully removed.\"");
                        scriptBuilder.AppendLine("} catch {");
                        scriptBuilder.AppendLine("    Write-Red \"  Error: $_\"");
                        scriptBuilder.AppendLine("}");
                        scriptBuilder.AppendLine();
                    }
                }
                
                scriptBuilder.AppendLine("Write-Green \"Third-Party and Microsoft Apps processing completed.\"");
                scriptBuilder.AppendLine();
            }

            // Final summary
            scriptBuilder.AppendLine("# ==========================================");
            scriptBuilder.AppendLine("# SUMMARY");
            scriptBuilder.AppendLine("# ==========================================");
            scriptBuilder.AppendLine("Write-Green \"Optimization process completed.\"");
            scriptBuilder.AppendLine("Write-Yellow \"Press any key to exit...\"");
            scriptBuilder.AppendLine("Read-Host | Out-Null");

            // Create temp directory if it doesn't exist
            string tempDir = Path.Combine(Path.GetTempPath(), "WinFlux");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // Write script to temp file
            string scriptPath = Path.Combine(tempDir, "winflux_optimization.ps1");
            File.WriteAllText(scriptPath, scriptBuilder.ToString());

            return scriptPath;
        }

        // Method to get selected apps for preset service
        public List<string> GetAppsToRemove()
        {
            List<string> appsToRemove = new List<string>();
            
            // Windows Features
            if (chkConsumerFeatures.IsChecked == true) appsToRemove.Add("ConsumerFeatures");
            if (chkRecall.IsChecked == true) appsToRemove.Add("Recall");
            if (chkInternetExplorer.IsChecked == true) appsToRemove.Add("InternetExplorer");
            if (chkHyperV.IsChecked == true) appsToRemove.Add("HyperV");
            if (chkFaxScan.IsChecked == true) appsToRemove.Add("FaxScan");
            if (chkMediaPlayer.IsChecked == true) appsToRemove.Add("MediaPlayer");
            
            // System Apps
            if (chkStore.IsChecked == true) appsToRemove.Add("Store");
            if (chkOneDrive.IsChecked == true) appsToRemove.Add("OneDrive");
            if (chkEdge.IsChecked == true) appsToRemove.Add("Edge");
            if (chkEdgeRemove.IsChecked == true) appsToRemove.Add("EdgeRemove");
            if (chkCopilot.IsChecked == true) appsToRemove.Add("Copilot");
            if (chkWidgets.IsChecked == true) appsToRemove.Add("Widgets");
            if (chkTaskbarWidgets.IsChecked == true) appsToRemove.Add("TaskbarWidgets");
            
            // Add all checkboxes from other sections
            foreach (var checkbox in GetAllCheckBoxes().Where(c => c.IsChecked == true))
            {
                string appName = checkbox.Name.Replace("chk", "");
                appsToRemove.Add(appName);
            }
            
            return appsToRemove;
        }
        
        // Method to apply preset settings
        public void ApplyAppSettings(List<string> appsToRemove)
        {
            if (appsToRemove == null || appsToRemove.Count == 0) return;
            
            // First uncheck all checkboxes
            DeselectAll(GetAllCheckBoxes());
            
            // Windows Features
            chkConsumerFeatures.IsChecked = appsToRemove.Contains("ConsumerFeatures");
            chkRecall.IsChecked = appsToRemove.Contains("Recall");
            chkInternetExplorer.IsChecked = appsToRemove.Contains("InternetExplorer");
            chkHyperV.IsChecked = appsToRemove.Contains("HyperV");
            chkFaxScan.IsChecked = appsToRemove.Contains("FaxScan");
            chkMediaPlayer.IsChecked = appsToRemove.Contains("MediaPlayer");
            
            // System Apps
            chkStore.IsChecked = appsToRemove.Contains("Store");
            chkOneDrive.IsChecked = appsToRemove.Contains("OneDrive");
            chkEdge.IsChecked = appsToRemove.Contains("Edge");
            chkEdgeRemove.IsChecked = appsToRemove.Contains("EdgeRemove");
            chkCopilot.IsChecked = appsToRemove.Contains("Copilot");
            chkWidgets.IsChecked = appsToRemove.Contains("Widgets");
            chkTaskbarWidgets.IsChecked = appsToRemove.Contains("TaskbarWidgets");
            
            // Standard Apps
            foreach (var checkbox in GetAllCheckBoxes())
            {
                string appName = checkbox.Name.Replace("chk", "");
                if (appsToRemove.Contains(appName))
                {
                    checkbox.IsChecked = true;
                }
            }
        }
    }
} 