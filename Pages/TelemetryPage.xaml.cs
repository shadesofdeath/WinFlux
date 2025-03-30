using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using iNKORE.UI.WPF.Modern.Controls;
using WinFlux.Helpers;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using System.Collections.Generic;
using WinFlux.Models;

namespace WinFlux.Pages
{
    public partial class TelemetryPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        // Add static reference for preset service to access
        public static TelemetryPage Instance { get; private set; }

        public TelemetryPage()
        {
            InitializeComponent();
            
            // Store the instance for preset service to use
            Instance = this;
            
            LoadSettings();
            AttachToggleEvents();
        }

        private void LoadSettings()
        {
            // Windows Telemetry - Düşük değer (0) = Devre dışı bırakılmış telemetri
            toggleWindowsTelemetry.IsOn = !((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry") ?? 1) == 0);

            // Windows Update - Düşük değer (0) = Devre dışı bırakılmış sürücü araması
            toggleWindowsUpdate.IsOn = !((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig") ?? 1) == 0);

            // Windows Search - Yüksek değer (1) = Devre dışı bırakılmış web araması
            toggleWindowsSearch.IsOn = !((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch") ?? 0) == 1);

            // Office Telemetry - Yüksek değer (1) = Devre dışı bırakılmış telemetri
            toggleOfficeTelemetry.IsOn = !((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\Common\ClientTelemetry", "DisableTelemetry") ?? 0) == 1);

            // App Experience - Disable görevi = Devre dışı bırakılmış görev
            bool disabledAppraisers = CheckIfScheduledTaskDisabled(@"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser");
            toggleAppExperience.IsOn = !disabledAppraisers;

            // Feedback - Düşük değer (0) = Devre dışı bırakılmış geri bildirim
            toggleFeedback.IsOn = !((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod") ?? 1) == 0);

            // Handwriting - Yüksek değer (1) = Devre dışı bırakılmış el yazısı toplama
            toggleHandwriting.IsOn = !((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "RestrictImplicitInkCollection") ?? 0) == 1);

            // Clipboard - Düşük değer (0) = Devre dışı bırakılmış kişiselleştirme
            toggleClipboard.IsOn = !((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "AllowInputPersonalization") ?? 1) == 0);

            // Targeted Ads - Düşük değer (0) = Devre dışı bırakılmış hedefli reklamlar
            toggleTargetedAds.IsOn = !((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled") ?? 1) == 0);

            // Privacy Consent - Düşük değer (0) = Devre dışı bırakılmış gizlilik onayı
            togglePrivacyConsent.IsOn = !((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy") ?? 1) == 0);

            // 3rd-party apps Telemetry - hosts engelleme = Devre dışı bırakılmış telemetri
            toggleThirdPartyApps.IsOn = !CheckHostsContainsAdobe();

            // NVIDIA Telemetry - Düşük değer (0) = Devre dışı bırakılmış telemetri
            toggleNvidia.IsOn = !((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client", "OptInOrOutPreference") ?? 1) == 0);

            // VS Code Telemetry - Düşük değer (0) = Devre dışı bırakılmış telemetri
            toggleVSCode.IsOn = !((int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\16.0\SQM", "OptIn") ?? 1) == 0);

            // Media Player Telemetry - Düşük değer (0) = Devre dışı bırakılmış kullanım takibi
            toggleMediaPlayer.IsOn = !((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\MediaPlayer\Preferences", "UsageTracking") ?? 1) == 0);

            // PowerShell Telemetry - "1" = Devre dışı bırakılmış telemetri
            string powershellTelemetry = Environment.GetEnvironmentVariable("POWERSHELL_TELEMETRY_OPTOUT");
            togglePowerShell.IsOn = !(powershellTelemetry == "1");

            // CCleaner Telemetry - Düşük değer (0) = Devre dışı bırakılmış izleme
            toggleCCleaner.IsOn = !((int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "Monitoring") ?? 1) == 0);

            // Google Updates - Devre dışı servis = Devre dışı bırakılmış güncellemeler
            toggleGoogleUpdates.IsOn = !CheckServiceDisabled("gupdate");

            // Adobe Updates - Devre dışı servis = Devre dışı bırakılmış güncellemeler
            toggleAdobeUpdates.IsOn = !CheckServiceDisabled("AdobeARMservice");
        }

        private void AttachToggleEvents()
        {
            // Windows Telemetry
            toggleWindowsTelemetry.Toggled += (s, e) => SetWindowsTelemetry(!toggleWindowsTelemetry.IsOn);

            // Windows Update
            toggleWindowsUpdate.Toggled += (s, e) => SetWindowsUpdate(!toggleWindowsUpdate.IsOn);

            // Windows Search
            toggleWindowsSearch.Toggled += (s, e) => SetWindowsSearch(!toggleWindowsSearch.IsOn);

            // Office Telemetry
            toggleOfficeTelemetry.Toggled += (s, e) => SetOfficeTelemetry(!toggleOfficeTelemetry.IsOn);

            // App Experience
            toggleAppExperience.Toggled += (s, e) => SetAppExperience(!toggleAppExperience.IsOn);

            // Feedback
            toggleFeedback.Toggled += (s, e) => SetFeedback(!toggleFeedback.IsOn);

            // Handwriting
            toggleHandwriting.Toggled += (s, e) => SetHandwriting(!toggleHandwriting.IsOn);

            // Clipboard
            toggleClipboard.Toggled += (s, e) => SetClipboard(!toggleClipboard.IsOn);

            // Targeted Ads
            toggleTargetedAds.Toggled += (s, e) => SetTargetedAds(!toggleTargetedAds.IsOn);

            // Privacy Consent
            togglePrivacyConsent.Toggled += (s, e) => SetPrivacyConsent(!togglePrivacyConsent.IsOn);

            // 3rd-party apps Telemetry
            toggleThirdPartyApps.Toggled += (s, e) => SetThirdPartyApps(!toggleThirdPartyApps.IsOn);

            // NVIDIA Telemetry
            toggleNvidia.Toggled += (s, e) => SetNvidia(!toggleNvidia.IsOn);

            // VS Code Telemetry
            toggleVSCode.Toggled += (s, e) => SetVSCode(!toggleVSCode.IsOn);

            // Media Player Telemetry
            toggleMediaPlayer.Toggled += (s, e) => SetMediaPlayer(!toggleMediaPlayer.IsOn);

            // PowerShell Telemetry
            togglePowerShell.Toggled += (s, e) => SetPowerShell(!togglePowerShell.IsOn);

            // CCleaner Telemetry
            toggleCCleaner.Toggled += (s, e) => SetCCleaner(!toggleCCleaner.IsOn);

            // Google Updates
            toggleGoogleUpdates.Toggled += (s, e) => SetGoogleUpdates(!toggleGoogleUpdates.IsOn);

            // Adobe Updates
            toggleAdobeUpdates.Toggled += (s, e) => SetAdobeUpdates(!toggleAdobeUpdates.IsOn);
        }

        private void SetWindowsTelemetry(bool disable)
        {
            if (disable)
            {
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\KernelCeipTask\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /DISABLE");
                RunPowershellCommand("sc config diagnosticshub.standardcollector.service start=demand");
                RunPowershellCommand("sc config diagsvc start=demand");
                RunPowershellCommand("sc config WerSvc start=demand");
                RunPowershellCommand("sc config wercplsupport start=demand");
                
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDesktopAnalyticsProcessing", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "MicrosoftEdgeDataOptIn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowWUfBCloudProcessing", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowUpdateComplianceProcessing", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowCommercialDataPipeline", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\DataCollection", "DisableOneSettingsDownloads", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows NT\CurrentVersion\Software Protection Platform", "NoGenTicket", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting\Consent", "DefaultConsent", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting\Consent", "DefaultOverrideBehavior", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting", "DontSendAdditionalData", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting", "LoggingDisabled", 1, RegistryValueKind.DWord);
            }
            else
            {
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\KernelCeipTask\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /ENABLE");
                RunPowershellCommand("sc config diagnosticshub.standardcollector.service start=auto");
                RunPowershellCommand("sc config diagsvc start=auto");
                RunPowershellCommand("sc config WerSvc start=auto");
                RunPowershellCommand("sc config wercplsupport start=auto");
                
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDesktopAnalyticsProcessing", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "MicrosoftEdgeDataOptIn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowWUfBCloudProcessing", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowUpdateComplianceProcessing", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowCommercialDataPipeline", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\DataCollection", "DisableOneSettingsDownloads", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows NT\CurrentVersion\Software Protection Platform", "NoGenTicket", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting\Consent", "DefaultConsent", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting\Consent", "DefaultOverrideBehavior", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting", "DontSendAdditionalData", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\Windows Error Reporting", "LoggingDisabled", 0, RegistryValueKind.DWord);
            }
        }

        private void SetWindowsUpdate(bool disable)
        {
            // Toggle switch açıkken (IsOn=true), disable=false olur ve Windows Update etkinleştirilir
            // Toggle switch kapalıyken (IsOn=false), disable=true olur ve Windows Update devre dışı bırakılır
            if (disable)
            {
                // Windows Update/Driver arama devre dışı bırakıldı (toggle kapalı)
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0, RegistryValueKind.DWord);
            }
            else
            {
                // Windows Update/Driver arama etkinleştirildi (toggle açık)
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 1, RegistryValueKind.DWord);
            }
        }

        private void SetWindowsSearch(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchPrivacy", 3, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchHistory", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowSearchToUseLocation", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "EnableDynamicContentInWSB", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWeb", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "PreventUnwantedAddIns", " ", RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "PreventRemoteQueries", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AlwaysUseAutoLangDetection", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowIndexingEncryptedStoresOrItems", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "CortanaInAmbientMode", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CanCortanaBeEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWebOverMeteredConnections", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortanaAboveLock", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\Experience\AllowCortana", "value", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "AllowSearchToUseLocation", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Preferences", "ModelDownloadAllowed", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDeviceSearchHistoryEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Preferences", "VoiceActivationOn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Preferences", "VoiceActivationEnableAboveLockscreen", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\OOBE", "DisableVoice", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "DeviceHistoryEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "HistoryViewEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Speech_OneCore\Preferences", "VoiceActivationDefaultOn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "CortanaEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings", "IsMSACloudSearchEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings", "IsAADCloudSearchEnabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "VoiceShortcut", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0, RegistryValueKind.DWord);
            }
            else
            {
                // Reset Windows Search settings to default values (enable)
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchPrivacy", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchHistory", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowSearchToUseLocation", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "EnableDynamicContentInWSB", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWeb", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 0, RegistryValueKind.DWord);
                RegHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "PreventUnwantedAddIns");
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "PreventRemoteQueries", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AlwaysUseAutoLangDetection", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowIndexingEncryptedStoresOrItems", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "DisableSearchBoxSuggestions", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "CortanaInAmbientMode", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CanCortanaBeEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWebOverMeteredConnections", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortanaAboveLock", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\Experience\AllowCortana", "value", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "AllowSearchToUseLocation", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Preferences", "ModelDownloadAllowed", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDeviceSearchHistoryEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Preferences", "VoiceActivationOn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Preferences", "VoiceActivationEnableAboveLockscreen", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\OOBE", "DisableVoice", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "DeviceHistoryEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "HistoryViewEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Microsoft\Speech_OneCore\Preferences", "VoiceActivationDefaultOn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "CortanaEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings", "IsMSACloudSearchEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings", "IsAADCloudSearchEnabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "VoiceShortcut", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 1, RegistryValueKind.DWord);
            }
        }

        private void SetOfficeTelemetry(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Outlook\Options\Mail", "EnableLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Outlook\Options\Mail", "EnableLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Outlook\Options\Calendar", "EnableCalendarLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Outlook\Options\Calendar", "EnableCalendarLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Word\Options", "EnableLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Word\Options", "EnableLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\15.0\OSM", "EnableLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\16.0\OSM", "EnableLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\15.0\OSM", "EnableUpload", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\16.0\OSM", "EnableUpload", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\Common\ClientTelemetry", "DisableTelemetry", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common\ClientTelemetry", "DisableTelemetry", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\Common\ClientTelemetry", "VerboseLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common\ClientTelemetry", "VerboseLogging", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Common", "QMEnable", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common", "QMEnable", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Common\Feedback", "Enabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common\Feedback", "Enabled", 0, RegistryValueKind.DWord);
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentFallBack\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentLogOn\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentFallBack2016\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentLogOn2016\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\Office 15 Subscription Heartbeat\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\Office 16 Subscription Heartbeat\" /DISABLE");
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Outlook\Options\Mail", "EnableLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Outlook\Options\Mail", "EnableLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Outlook\Options\Calendar", "EnableCalendarLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Outlook\Options\Calendar", "EnableCalendarLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Word\Options", "EnableLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Word\Options", "EnableLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\15.0\OSM", "EnableLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\16.0\OSM", "EnableLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\15.0\OSM", "EnableUpload", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Office\16.0\OSM", "EnableUpload", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\Common\ClientTelemetry", "DisableTelemetry", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common\ClientTelemetry", "DisableTelemetry", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\Common\ClientTelemetry", "VerboseLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common\ClientTelemetry", "VerboseLogging", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Common", "QMEnable", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common", "QMEnable", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\15.0\Common\Feedback", "Enabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Office\16.0\Common\Feedback", "Enabled", 1, RegistryValueKind.DWord);
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentFallBack\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentLogOn\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentFallBack2016\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\OfficeTelemetryAgentLogOn2016\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\Office 15 Subscription Heartbeat\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Office\\Office 16 Subscription Heartbeat\" /ENABLE");
            }
        }

        private void SetAppExperience(bool disable)
        {
            if (disable)
            {
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\ProgramDataUpdater\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\AitAgent\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\StartupAppTask\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\PcaPatchDbTask\" /DISABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\MareBackup\" /DISABLE");
            }
            else
            {
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\ProgramDataUpdater\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\AitAgent\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\StartupAppTask\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\PcaPatchDbTask\" /ENABLE");
                RunPowershellCommand("schtasks /change /TN \"\\Microsoft\\Windows\\Application Experience\\MareBackup\" /ENABLE");
            }
        }

        private void SetFeedback(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord);
                RegHelper.DeleteValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "PeriodInNanoSeconds");
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 3, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", 864000000000, RegistryValueKind.QWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "DoNotShowFeedbackNotifications", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 0, RegistryValueKind.DWord);
            }
        }

        private void SetHandwriting(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "AllowInputPersonalization", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\InputPersonalization", "AllowInputPersonalization", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 1, RegistryValueKind.DWord);
            }
        }

        private void SetClipboard(bool disable)
        {
            // Same as handwriting data collection for now
            SetHandwriting(disable);
        }

        private void SetTargetedAds(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableSoftLanding", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightFeatures", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableSoftLanding", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightFeatures", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 1, RegistryValueKind.DWord);
            }
        }

        private void SetPrivacyConsent(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 1, RegistryValueKind.DWord);
            }
        }

        private void SetThirdPartyApps(bool disable)
        {
            if (disable)
            {
                string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
                try
                {
                    // Read current hosts file
                    string hostsContent = File.ReadAllText(hostsPath);

                    // Check if Adobe entries already exist
                    if (!hostsContent.Contains("0.0.0.0 *.adobe.io"))
                    {
                        StringBuilder sb = new StringBuilder(hostsContent);
                        sb.AppendLine();
                        sb.AppendLine("# Adobe Telemetry blocking");
                        sb.AppendLine("0.0.0.0 *.adobe.io");
                        sb.AppendLine("0.0.0.0 *.adobe.com");
                        sb.AppendLine("0.0.0.0 adobeereg.com");
                        sb.AppendLine("0.0.0.0 wip.adobe.com");
                        sb.AppendLine("0.0.0.0 adobedtm.com");
                        sb.AppendLine("0.0.0.0 adobe-dns.adobe.com");
                        sb.AppendLine("0.0.0.0 adobe-dns-1.adobe.com");
                        sb.AppendLine("0.0.0.0 adobe-dns-2.adobe.com");
                        sb.AppendLine("0.0.0.0 adobe-dns-3.adobe.com");
                        sb.AppendLine("0.0.0.0 adobe-dns-4.adobe.com");
                        sb.AppendLine("0.0.0.0 wwwimages.adobe.com");
                        sb.AppendLine("0.0.0.0 wwwimages2.adobe.com");
                        sb.AppendLine("0.0.0.0 www.macromedia.com");
                        sb.AppendLine("0.0.0.0 fpdownload.macromedia.com");
                        sb.AppendLine("0.0.0.0 fpdownload2.macromedia.com");

                        File.WriteAllText(hostsPath, sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(FindResource("TelemetryPageMessageBox_HostsFileUpdateError") + " " + ex.Message, 
                        FindResource("MessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
                try
                {
                    // Read current hosts file
                    string hostsContent = File.ReadAllText(hostsPath);

                    // Remove Adobe entries if they exist
                    string[] linesToRemove = new string[]
                    {
                        "0.0.0.0 *.adobe.io",
                        "0.0.0.0 *.adobe.com",
                        "0.0.0.0 adobeereg.com",
                        "0.0.0.0 wip.adobe.com",
                        "0.0.0.0 adobedtm.com",
                        "0.0.0.0 adobe-dns.adobe.com",
                        "0.0.0.0 adobe-dns-1.adobe.com",
                        "0.0.0.0 adobe-dns-2.adobe.com",
                        "0.0.0.0 adobe-dns-3.adobe.com",
                        "0.0.0.0 adobe-dns-4.adobe.com",
                        "0.0.0.0 wwwimages.adobe.com",
                        "0.0.0.0 wwwimages2.adobe.com",
                        "0.0.0.0 www.macromedia.com",
                        "0.0.0.0 fpdownload.macromedia.com",
                        "0.0.0.0 fpdownload2.macromedia.com",
                        "# Adobe Telemetry blocking"
                    };

                    // Remove each line from the hosts file content
                    string[] lines = hostsContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    StringBuilder sb = new StringBuilder();
                    foreach (string line in lines)
                    {
                        bool shouldKeep = true;
                        foreach (string lineToRemove in linesToRemove)
                        {
                            if (line.Trim().Equals(lineToRemove.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                shouldKeep = false;
                                break;
                            }
                        }
                        if (shouldKeep)
                        {
                            sb.AppendLine(line);
                        }
                    }

                    File.WriteAllText(hostsPath, sb.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(FindResource("TelemetryPageMessageBox_HostsFileUpdateError") + " " + ex.Message, 
                        FindResource("MessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CheckHostsContainsAdobe()
        {
            string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
            try
            {
                string hostsContent = File.ReadAllText(hostsPath);
                return hostsContent.Contains("0.0.0.0 *.adobe.io");
            }
            catch
            {
                return false;
            }
        }

        private bool CheckIfScheduledTaskDisabled(string taskPath)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "schtasks";
                process.StartInfo.Arguments = $"/query /TN \"{taskPath}\" /FO LIST";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains("Status: Disabled");
            }
            catch
            {
                return false;
            }
        }

        private bool CheckServiceDisabled(string serviceName)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "sc";
                process.StartInfo.Arguments = $"qc {serviceName}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains("START_TYPE : 4") || output.Contains("START_TYPE : 4  DISABLED");
            }
            catch
            {
                return false;
            }
        }

        private void RunPowershellCommand(string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{command}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Verb = "runas"; // Run as administrator
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{FindResource("TelemetryPageMessageBox_CommandError")} {ex.Message}", 
                    FindResource("MessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetNvidia(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client", "OptInOrOutPreference", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\FTS", "EnableRID44231", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\FTS", "EnableRID64640", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\FTS", "EnableRID66610", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\Startup", "SendTelemetryData", 0, RegistryValueKind.DWord);
                RunPowershellCommand("schtasks /change /TN NvTmMon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8} /DISABLE");
                RunPowershellCommand("schtasks /change /TN NvTmRep_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8} /DISABLE");
                RunPowershellCommand("schtasks /change /TN NvTmRepOnLogon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8} /DISABLE");
            }
            else
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client", "OptInOrOutPreference", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\FTS", "EnableRID44231", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\FTS", "EnableRID64640", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\FTS", "EnableRID66610", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\Startup", "SendTelemetryData", 1, RegistryValueKind.DWord);
                RunPowershellCommand("schtasks /change /TN NvTmMon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8} /ENABLE");
                RunPowershellCommand("schtasks /change /TN NvTmRep_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8} /ENABLE");
                RunPowershellCommand("schtasks /change /TN NvTmRepOnLogon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8} /ENABLE");
            }
        }

        private void SetVSCode(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\14.0\SQM", "OptIn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\15.0\SQM", "OptIn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\16.0\SQM", "OptIn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\17.0\SQM", "OptIn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\VisualStudio\SQM", "OptIn", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\VisualStudio\Telemetry", "TurnOffSwitch", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\Feedback", "DisableFeedbackDialog", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\Feedback", "DisableEmailInput", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\Feedback", "DisableScreenshotCapture", 1, RegistryValueKind.DWord);
                RegHelper.DeleteValue(RegistryHive.LocalMachine, @"Software\Microsoft\VisualStudio\DiagnosticsHub", "LogLevel");
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\IntelliCode", "DisableRemoteAnalysis", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\VSCommon\16.0\IntelliCode", "DisableRemoteAnalysis", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\VSCommon\17.0\IntelliCode", "DisableRemoteAnalysis", 1, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\14.0\SQM", "OptIn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\15.0\SQM", "OptIn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\16.0\SQM", "OptIn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\VSCommon\17.0\SQM", "OptIn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\VisualStudio\SQM", "OptIn", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\VisualStudio\Telemetry", "TurnOffSwitch", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\Feedback", "DisableFeedbackDialog", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\Feedback", "DisableEmailInput", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\Feedback", "DisableScreenshotCapture", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\VisualStudio\IntelliCode", "DisableRemoteAnalysis", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\VSCommon\16.0\IntelliCode", "DisableRemoteAnalysis", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\VSCommon\17.0\IntelliCode", "DisableRemoteAnalysis", 0, RegistryValueKind.DWord);
            }
        }

        private void SetMediaPlayer(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\MediaPlayer\Preferences", "UsageTracking", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\WindowsMediaPlayer", "PreventCDDVDMetadataRetrieval", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\WindowsMediaPlayer", "PreventMusicFileMetadataRetrieval", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\WindowsMediaPlayer", "PreventRadioPresetsRetrieval", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\WMDRM", "DisableOnline", 1, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\MediaPlayer\Preferences", "UsageTracking", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\WindowsMediaPlayer", "PreventCDDVDMetadataRetrieval", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\WindowsMediaPlayer", "PreventMusicFileMetadataRetrieval", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\WindowsMediaPlayer", "PreventRadioPresetsRetrieval", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\WMDRM", "DisableOnline", 0, RegistryValueKind.DWord);
            }
        }

        private void SetPowerShell(bool disable)
        {
            if (disable)
            {
                RunPowershellCommand("setx POWERSHELL_TELEMETRY_OPTOUT 1");
            }
            else
            {
                RunPowershellCommand("setx POWERSHELL_TELEMETRY_OPTOUT 0");
            }
        }

        private void SetCCleaner(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "Monitoring", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "HelpImproveCCleaner", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "SystemMonitoring", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "UpdateAuto", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "UpdateCheck", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "CheckTrialOffer", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)HealthCheck", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)QuickClean", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)QuickCleanIpm", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)GetIpmForTrial", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)SoftwareUpdater", 0, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)SoftwareUpdaterIpm", 0, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "Monitoring", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "HelpImproveCCleaner", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "SystemMonitoring", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "UpdateAuto", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "UpdateCheck", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Piriform\CCleaner", "CheckTrialOffer", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)HealthCheck", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)QuickClean", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)QuickCleanIpm", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)GetIpmForTrial", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)SoftwareUpdater", 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"Software\Piriform\CCleaner", "(Cfg)SoftwareUpdaterIpm", 1, RegistryValueKind.DWord);
            }
        }

        private void SetGoogleUpdates(bool disable)
        {
            if (disable)
            {
                RunPowershellCommand("sc config gupdate start=disabled");
                RunPowershellCommand("sc config gupdatem start=disabled");
            }
            else
            {
                RunPowershellCommand("sc config gupdate start=auto");
                RunPowershellCommand("sc config gupdatem start=auto");
            }
        }

        private void SetAdobeUpdates(bool disable)
        {
            if (disable)
            {
                RunPowershellCommand("schtasks /change /TN \"\\Adobe Acrobat Update Task\" /DISABLE");
                RunPowershellCommand("sc config AdobeARMservice start=disabled");
                RunPowershellCommand("sc config adobeupdateservice start=disabled");
            }
            else
            {
                RunPowershellCommand("schtasks /change /TN \"\\Adobe Acrobat Update Task\" /ENABLE");
                RunPowershellCommand("sc config AdobeARMservice start=auto");
                RunPowershellCommand("sc config adobeupdateservice start=auto");
            }
        }

        // Method to collect all toggle settings for preset export
        public Dictionary<string, bool> GetToggleSettings()
        {
            var settings = new Dictionary<string, bool>();

            // Collect settings from all toggle switches
            settings.Add("WindowsTelemetry", toggleWindowsTelemetry.IsOn);
            settings.Add("WindowsUpdate", toggleWindowsUpdate.IsOn);
            settings.Add("WindowsSearch", toggleWindowsSearch.IsOn);
            settings.Add("OfficeTelemetry", toggleOfficeTelemetry.IsOn);
            settings.Add("AppExperience", toggleAppExperience.IsOn);
            settings.Add("Feedback", toggleFeedback.IsOn);
            settings.Add("Handwriting", toggleHandwriting.IsOn);
            settings.Add("Clipboard", toggleClipboard.IsOn);
            settings.Add("TargetedAds", toggleTargetedAds.IsOn);
            settings.Add("PrivacyConsent", togglePrivacyConsent.IsOn);
            settings.Add("ThirdPartyApps", toggleThirdPartyApps.IsOn);
            settings.Add("Nvidia", toggleNvidia.IsOn);
            settings.Add("VSCode", toggleVSCode.IsOn);
            settings.Add("MediaPlayer", toggleMediaPlayer.IsOn);
            settings.Add("PowerShell", togglePowerShell.IsOn);
            settings.Add("CCleaner", toggleCCleaner.IsOn);
            settings.Add("GoogleUpdates", toggleGoogleUpdates.IsOn);
            settings.Add("AdobeUpdates", toggleAdobeUpdates.IsOn);

            return settings;
        }

        // Method to get blocked hosts
        public List<string> GetBlockedHosts()
        {
            // Retrieve the current list of blocked hosts from your application
            var blockedHosts = new List<string>();

            // If you have a collection of blocked hosts in the app
            // Example: blockedHosts = blockedHostsListBox.Items.Cast<string>().ToList();
            
            // For demonstration purposes, returning a predefined list
            // You should replace this with your actual implementation
            blockedHosts.Add("telemetry.microsoft.com");
            blockedHosts.Add("vortex.data.microsoft.com");
            blockedHosts.Add("settings-win.data.microsoft.com");
            blockedHosts.Add("telemetry.appex.bing.net");
            
            return blockedHosts;
        }

        // Method to apply preset settings
        public void ApplyToggleSettings(Dictionary<string, bool> settings)
        {
            if (settings == null || settings.Count == 0) return;

            // Apply settings to toggle switches
            if (settings.TryGetValue("WindowsTelemetry", out bool windowsTelemetryValue))
                toggleWindowsTelemetry.IsOn = windowsTelemetryValue;
                
            if (settings.TryGetValue("WindowsUpdate", out bool windowsUpdateValue))
                toggleWindowsUpdate.IsOn = windowsUpdateValue;
                
            if (settings.TryGetValue("WindowsSearch", out bool windowsSearchValue))
                toggleWindowsSearch.IsOn = windowsSearchValue;
                
            if (settings.TryGetValue("OfficeTelemetry", out bool officeTelemetryValue))
                toggleOfficeTelemetry.IsOn = officeTelemetryValue;
                
            if (settings.TryGetValue("AppExperience", out bool appExperienceValue))
                toggleAppExperience.IsOn = appExperienceValue;
                
            if (settings.TryGetValue("Feedback", out bool feedbackValue))
                toggleFeedback.IsOn = feedbackValue;
                
            if (settings.TryGetValue("Handwriting", out bool handwritingValue))
                toggleHandwriting.IsOn = handwritingValue;
                
            if (settings.TryGetValue("Clipboard", out bool clipboardValue))
                toggleClipboard.IsOn = clipboardValue;
                
            if (settings.TryGetValue("TargetedAds", out bool targetedAdsValue))
                toggleTargetedAds.IsOn = targetedAdsValue;
                
            if (settings.TryGetValue("PrivacyConsent", out bool privacyConsentValue))
                togglePrivacyConsent.IsOn = privacyConsentValue;
                
            if (settings.TryGetValue("ThirdPartyApps", out bool thirdPartyAppsValue))
                toggleThirdPartyApps.IsOn = thirdPartyAppsValue;
                
            if (settings.TryGetValue("Nvidia", out bool nvidiaValue))
                toggleNvidia.IsOn = nvidiaValue;
                
            if (settings.TryGetValue("VSCode", out bool vsCodeValue))
                toggleVSCode.IsOn = vsCodeValue;
                
            if (settings.TryGetValue("MediaPlayer", out bool mediaPlayerValue))
                toggleMediaPlayer.IsOn = mediaPlayerValue;
                
            if (settings.TryGetValue("PowerShell", out bool powerShellValue))
                togglePowerShell.IsOn = powerShellValue;
                
            if (settings.TryGetValue("CCleaner", out bool cCleanerValue))
                toggleCCleaner.IsOn = cCleanerValue;
                
            if (settings.TryGetValue("GoogleUpdates", out bool googleUpdatesValue))
                toggleGoogleUpdates.IsOn = googleUpdatesValue;
                
            if (settings.TryGetValue("AdobeUpdates", out bool adobeUpdatesValue))
                toggleAdobeUpdates.IsOn = adobeUpdatesValue;
                
            // Apply toggles by applying the click events
            ApplyToggles();
        }

        // Method to apply blocked hosts
        public void ApplyBlockedHosts(List<string> blockedHosts)
        {
            if (blockedHosts == null || blockedHosts.Count == 0) return;

            // Apply the blockedHosts list here
            // This would typically involve adding these hosts to the hosts file
            // Example:
            foreach (var host in blockedHosts)
            {
                // Add the host to your blocking mechanism
                AddHostToHostsFile(host);
            }
        }
        
        // Method to apply all toggle changes (which will call the appropriate registry changes)
        private void ApplyToggles()
        {
            // Trigger methods directly instead of using click events
            if (toggleWindowsTelemetry.IsOn) SetWindowsTelemetry(!toggleWindowsTelemetry.IsOn);
            if (toggleWindowsUpdate.IsOn) SetWindowsUpdate(!toggleWindowsUpdate.IsOn);
            if (toggleWindowsSearch.IsOn) SetWindowsSearch(!toggleWindowsSearch.IsOn);
            if (toggleOfficeTelemetry.IsOn) SetOfficeTelemetry(!toggleOfficeTelemetry.IsOn);
            if (toggleAppExperience.IsOn) SetAppExperience(!toggleAppExperience.IsOn);
            if (toggleFeedback.IsOn) SetFeedback(!toggleFeedback.IsOn);
            if (toggleHandwriting.IsOn) SetHandwriting(!toggleHandwriting.IsOn);
            if (toggleClipboard.IsOn) SetClipboard(!toggleClipboard.IsOn);
            if (toggleTargetedAds.IsOn) SetTargetedAds(!toggleTargetedAds.IsOn);
            if (togglePrivacyConsent.IsOn) SetPrivacyConsent(!togglePrivacyConsent.IsOn);
            if (toggleThirdPartyApps.IsOn) SetThirdPartyApps(!toggleThirdPartyApps.IsOn);
            if (toggleNvidia.IsOn) SetNvidia(!toggleNvidia.IsOn);
            if (toggleVSCode.IsOn) SetVSCode(!toggleVSCode.IsOn);
            if (toggleMediaPlayer.IsOn) SetMediaPlayer(!toggleMediaPlayer.IsOn);
            if (togglePowerShell.IsOn) SetPowerShell(!togglePowerShell.IsOn);
            if (toggleCCleaner.IsOn) SetCCleaner(!toggleCCleaner.IsOn);
            if (toggleGoogleUpdates.IsOn) SetGoogleUpdates(!toggleGoogleUpdates.IsOn);
            if (toggleAdobeUpdates.IsOn) SetAdobeUpdates(!toggleAdobeUpdates.IsOn);
        }

        // Helper method to add host to hosts file
        private void AddHostToHostsFile(string host)
        {
            // Your implementation for adding a host to the hosts file
            // This should handle proper formatting and avoid duplicates
            // Example: AddHostEntry("0.0.0.0 " + host);
        }
    }
} 