using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using iNKORE.UI.WPF.Modern.Controls;
using WinFlux.Helpers;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using System.Collections.Generic;
using WinFlux.Models;

namespace WinFlux.Pages
{
    public partial class PrivacyPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        // Add static reference for preset service to access
        public static PrivacyPage Instance { get; private set; }

        public PrivacyPage()
        {
            InitializeComponent();
            
            // Store the instance for preset service to use
            Instance = this;
            
            LoadRegistrySettings();
            AttachToggleEvents();
        }

        private void LoadRegistrySettings()
        {
            // Location Access
            toggleLocation.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value");

            // Camera Access
            toggleCamera.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam", "Value");

            // Microphone Access
            toggleMicrophone.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone", "Value");

            // System Files Access
            toggleSystemFiles.IsOn = !(
                IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\documentsLibrary", "Value") &&
                IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\picturesLibrary", "Value") &&
                IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\videosLibrary", "Value") &&
                IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\broadFileSystemAccess", "Value")
            );

            // Account Info Access
            toggleAccountInfo.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", "Value");

            // Contacts Access
            toggleContacts.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\contacts", "Value");

            // Call History Access
            toggleCallHistory.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\phoneCallHistory", "Value");

            // Messaging Access
            toggleMessaging.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\chat", "Value");

            // Notification Access
            toggleNotifications.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userNotificationListener", "Value");

            // Email Access
            toggleEmail.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\email", "Value");

            // Tasks Access
            toggleTasks.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userDataTasks", "Value");

            // Diagnostics Access
            toggleDiagnostics.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics", "Value");

            // Voice Activation Access
            int voiceActivationValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Settings\VoiceActivation\UserPreferenceForAllApps", "AgentActivationEnabled") ?? 1);
            toggleVoiceActivation.IsOn = voiceActivationValue != 0;

            // Phone Access
            int phoneValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessPhone") ?? 0);
            togglePhone.IsOn = phoneValue != 2;

            // Trusted Devices Access
            toggleTrustedDevices.IsOn = !IsRegistryValueDeny(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DeviceAccess\Global\{C1D23ACC-752B-43E5-8448-8D0E519CD6D6}", "Value");

            // Calendar Access
            toggleCalendar.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appointments", "Value");

            // Motion Access
            toggleMotion.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\activity", "Value");

            // Radio Access
            toggleRadio.IsOn = !IsRegistryValueDeny(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\radios", "Value");

            // Cloud Sync
            int settingSyncValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableSettingSync") ?? 0);
            toggleCloudSync.IsOn = settingSyncValue != 2;

            // Activity Feed
            int activityFeedValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed") ?? 1);
            toggleActivityFeed.IsOn = activityFeedValue != 0;

            // Screen Recording (Game DVR)
            int gameDvrValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled") ?? 1);
            int allowGameDvrValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR") ?? 1);
            toggleScreenRecording.IsOn = gameDvrValue != 0 || allowGameDvrValue != 0;

            // Notifications Tray
            int notificationCenterValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter") ?? 0);
            int toastEnabledValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled") ?? 1);
            toggleNotificationsTray.IsOn = notificationCenterValue != 1 && toastEnabledValue != 0;

            // Map Downloads
            int mapTrafficValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Maps", "AllowUntriggeredNetworkTrafficOnSettingsPage") ?? 1);
            int mapAutoDownloadValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Maps", "AutoDownloadAndUpdateMapData") ?? 1);
            toggleMapDownloads.IsOn = mapTrafficValue != 0 || mapAutoDownloadValue != 0;

            // Lockscreen Camera
            int lockScreenCameraValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera") ?? 0);
            toggleLockscreenCamera.IsOn = lockScreenCameraValue != 1;

            // Biometrics
            int biometricsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Biometrics", "Enabled") ?? 1);
            int credProviderValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider", "Enabled") ?? 1);
            toggleBiometrics.IsOn = biometricsValue != 0 && credProviderValue != 0;
        }

        private bool IsRegistryValueDeny(RegistryHive hive, string path, string name)
        {
            string value = RegHelper.GetStringValue(hive, path, name) ?? string.Empty;
            return value.Equals("Deny", StringComparison.OrdinalIgnoreCase);
        }

        private void AttachToggleEvents()
        {
            // Location Access
            toggleLocation.Toggled += (s, e) => SetLocationPrivacy(toggleLocation.IsOn);

            // Camera Access
            toggleCamera.Toggled += (s, e) => SetCameraPrivacy(toggleCamera.IsOn);

            // Microphone Access
            toggleMicrophone.Toggled += (s, e) => SetMicrophonePrivacy(toggleMicrophone.IsOn);

            // System Files Access
            toggleSystemFiles.Toggled += (s, e) => SetSystemFilesPrivacy(toggleSystemFiles.IsOn);

            // Account Info Access
            toggleAccountInfo.Toggled += (s, e) => SetAccountInfoPrivacy(toggleAccountInfo.IsOn);

            // Contacts Access
            toggleContacts.Toggled += (s, e) => SetContactsPrivacy(toggleContacts.IsOn);

            // Call History Access
            toggleCallHistory.Toggled += (s, e) => SetCallHistoryPrivacy(toggleCallHistory.IsOn);

            // Messaging Access
            toggleMessaging.Toggled += (s, e) => SetMessagingPrivacy(toggleMessaging.IsOn);

            // Notification Access
            toggleNotifications.Toggled += (s, e) => SetNotificationsPrivacy(toggleNotifications.IsOn);

            // Email Access
            toggleEmail.Toggled += (s, e) => SetEmailPrivacy(toggleEmail.IsOn);

            // Tasks Access
            toggleTasks.Toggled += (s, e) => SetTasksPrivacy(toggleTasks.IsOn);

            // Diagnostics Access
            toggleDiagnostics.Toggled += (s, e) => SetDiagnosticsPrivacy(toggleDiagnostics.IsOn);

            // Voice Activation Access
            toggleVoiceActivation.Toggled += (s, e) => SetVoiceActivationPrivacy(toggleVoiceActivation.IsOn);

            // Phone Access
            togglePhone.Toggled += (s, e) => SetPhonePrivacy(togglePhone.IsOn);

            // Trusted Devices Access
            toggleTrustedDevices.Toggled += (s, e) => SetTrustedDevicesPrivacy(toggleTrustedDevices.IsOn);

            // Calendar Access
            toggleCalendar.Toggled += (s, e) => SetCalendarPrivacy(toggleCalendar.IsOn);

            // Motion Access
            toggleMotion.Toggled += (s, e) => SetMotionPrivacy(toggleMotion.IsOn);

            // Radio Access
            toggleRadio.Toggled += (s, e) => SetRadioPrivacy(toggleRadio.IsOn);

            // Cloud Sync
            toggleCloudSync.Toggled += (s, e) => SetCloudSyncPrivacy(toggleCloudSync.IsOn);

            // Activity Feed
            toggleActivityFeed.Toggled += (s, e) => SetActivityFeedPrivacy(toggleActivityFeed.IsOn);

            // Screen Recording
            toggleScreenRecording.Toggled += (s, e) => SetScreenRecordingPrivacy(toggleScreenRecording.IsOn);

            // Notifications Tray
            toggleNotificationsTray.Toggled += (s, e) => SetNotificationsTrayPrivacy(toggleNotificationsTray.IsOn);

            // Map Downloads
            toggleMapDownloads.Toggled += (s, e) => SetMapDownloadsPrivacy(toggleMapDownloads.IsOn);

            // Lockscreen Camera
            toggleLockscreenCamera.Toggled += (s, e) => SetLockscreenCameraPrivacy(toggleLockscreenCamera.IsOn);

            // Biometrics
            toggleBiometrics.Toggled += (s, e) => SetBiometricsPrivacy(toggleBiometrics.IsOn);
        }

        private void SetLocationPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetCameraPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetMicrophonePrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetSystemFilesPrivacy(bool enabled)
        {
            string value = enabled ? "Allow" : "Deny";
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\documentsLibrary", "Value", value, RegistryValueKind.String);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\picturesLibrary", "Value", value, RegistryValueKind.String);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\videosLibrary", "Value", value, RegistryValueKind.String);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\broadFileSystemAccess", "Value", value, RegistryValueKind.String);
        }

        private void SetAccountInfoPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetContactsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\contacts", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetCallHistoryPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\phoneCallHistory", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetMessagingPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\chat", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetNotificationsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userNotificationListener", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetEmailPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\email", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetTasksPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userDataTasks", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetDiagnosticsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetVoiceActivationPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Settings\VoiceActivation\UserPreferenceForAllApps", "AgentActivationEnabled", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetPhonePrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessPhone", enabled ? 0 : 2, RegistryValueKind.DWord);
            if (!enabled)
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessPhone_UserInControlOfTheseApps", string.Empty, RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessPhone_ForceAllowTheseApps", string.Empty, RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessPhone_ForceDenyTheseApps", string.Empty, RegistryValueKind.String);
            }
        }

        private void SetTrustedDevicesPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DeviceAccess\Global\{C1D23ACC-752B-43E5-8448-8D0E519CD6D6}", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetCalendarPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appointments", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetMotionPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\activity", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetRadioPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\radios", "Value", enabled ? "Allow" : "Deny", RegistryValueKind.String);
        }

        private void SetCloudSyncPrivacy(bool enabled)
        {
            int value = enabled ? 0 : 2;
            int userOverride = enabled ? 0 : 1;

            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableSyncOnPaidNetwork", enabled ? 0 : 1, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync", "SyncPolicy", enabled ? 1 : 5, RegistryValueKind.DWord);
            
            // Application settings sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableApplicationSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableApplicationSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            
            // App sync settings sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableAppSyncSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableAppSyncSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            
            // Credentials setting sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableCredentialsSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableCredentialsSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", enabled ? 1 : 0, RegistryValueKind.DWord);
            
            // Desktop theme setting sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableDesktopThemeSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableDesktopThemeSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            
            // Personalization setting sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisablePersonalizationSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisablePersonalizationSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            
            // Start layout setting sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableStartLayoutSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableStartLayoutSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            
            // Web browser setting sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableWebBrowserSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableWebBrowserSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            
            // Windows setting sync
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableWindowsSettingSync", value, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableWindowsSettingSyncUserOverride", userOverride, RegistryValueKind.DWord);
            
            // Language setting sync
            RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetActivityFeedPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetScreenRecordingPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", enabled ? 1 : 0, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetNotificationsTrayPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", enabled ? 0 : 1, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetMapDownloadsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Maps", "AllowUntriggeredNetworkTrafficOnSettingsPage", enabled ? 1 : 0, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Maps", "AutoDownloadAndUpdateMapData", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetLockscreenCameraPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", enabled ? 0 : 1, RegistryValueKind.DWord);
        }

        private void SetBiometricsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Biometrics", "Enabled", enabled ? 1 : 0, RegistryValueKind.DWord);
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider", "Enabled", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        // Method to collect all toggle settings for preset export
        public Dictionary<string, bool> GetToggleSettings()
        {
            var settings = new Dictionary<string, bool>();

            // Collect settings from all toggle switches
            settings.Add("Location", toggleLocation.IsOn);
            settings.Add("Camera", toggleCamera.IsOn);
            settings.Add("Microphone", toggleMicrophone.IsOn);
            settings.Add("SystemFiles", toggleSystemFiles.IsOn);
            settings.Add("AccountInfo", toggleAccountInfo.IsOn);
            settings.Add("Contacts", toggleContacts.IsOn);
            settings.Add("CallHistory", toggleCallHistory.IsOn);
            settings.Add("Messaging", toggleMessaging.IsOn);
            settings.Add("Notifications", toggleNotifications.IsOn);
            settings.Add("Email", toggleEmail.IsOn);
            settings.Add("Tasks", toggleTasks.IsOn);
            settings.Add("Diagnostics", toggleDiagnostics.IsOn);
            settings.Add("VoiceActivation", toggleVoiceActivation.IsOn);
            settings.Add("Phone", togglePhone.IsOn);
            settings.Add("TrustedDevices", toggleTrustedDevices.IsOn);
            settings.Add("Calendar", toggleCalendar.IsOn);
            settings.Add("Motion", toggleMotion.IsOn);
            settings.Add("Radio", toggleRadio.IsOn);
            settings.Add("CloudSync", toggleCloudSync.IsOn);
            settings.Add("ActivityFeed", toggleActivityFeed.IsOn);
            settings.Add("ScreenRecording", toggleScreenRecording.IsOn);
            settings.Add("NotificationsTray", toggleNotificationsTray.IsOn);
            settings.Add("MapDownloads", toggleMapDownloads.IsOn);
            settings.Add("LockscreenCamera", toggleLockscreenCamera.IsOn);
            settings.Add("Biometrics", toggleBiometrics.IsOn);

            return settings;
        }

        // Method to apply preset settings
        public void ApplyToggleSettings(Dictionary<string, bool> settings)
        {
            if (settings == null || settings.Count == 0) return;

            // Apply settings to toggle switches
            if (settings.TryGetValue("Location", out bool locationValue))
                toggleLocation.IsOn = locationValue;
                
            if (settings.TryGetValue("Camera", out bool cameraValue))
                toggleCamera.IsOn = cameraValue;
                
            if (settings.TryGetValue("Microphone", out bool microphoneValue))
                toggleMicrophone.IsOn = microphoneValue;
                
            if (settings.TryGetValue("SystemFiles", out bool systemFilesValue))
                toggleSystemFiles.IsOn = systemFilesValue;
                
            if (settings.TryGetValue("AccountInfo", out bool accountInfoValue))
                toggleAccountInfo.IsOn = accountInfoValue;
                
            if (settings.TryGetValue("Contacts", out bool contactsValue))
                toggleContacts.IsOn = contactsValue;
                
            if (settings.TryGetValue("CallHistory", out bool callHistoryValue))
                toggleCallHistory.IsOn = callHistoryValue;
                
            if (settings.TryGetValue("Messaging", out bool messagingValue))
                toggleMessaging.IsOn = messagingValue;
                
            if (settings.TryGetValue("Notifications", out bool notificationsValue))
                toggleNotifications.IsOn = notificationsValue;
                
            if (settings.TryGetValue("Email", out bool emailValue))
                toggleEmail.IsOn = emailValue;
                
            if (settings.TryGetValue("Tasks", out bool tasksValue))
                toggleTasks.IsOn = tasksValue;
                
            if (settings.TryGetValue("Diagnostics", out bool diagnosticsValue))
                toggleDiagnostics.IsOn = diagnosticsValue;
                
            if (settings.TryGetValue("VoiceActivation", out bool voiceActivationValue))
                toggleVoiceActivation.IsOn = voiceActivationValue;
                
            if (settings.TryGetValue("Phone", out bool phoneValue))
                togglePhone.IsOn = phoneValue;
                
            if (settings.TryGetValue("TrustedDevices", out bool trustedDevicesValue))
                toggleTrustedDevices.IsOn = trustedDevicesValue;
                
            if (settings.TryGetValue("Calendar", out bool calendarValue))
                toggleCalendar.IsOn = calendarValue;
                
            if (settings.TryGetValue("Motion", out bool motionValue))
                toggleMotion.IsOn = motionValue;
                
            if (settings.TryGetValue("Radio", out bool radioValue))
                toggleRadio.IsOn = radioValue;
                
            if (settings.TryGetValue("CloudSync", out bool cloudSyncValue))
                toggleCloudSync.IsOn = cloudSyncValue;
                
            if (settings.TryGetValue("ActivityFeed", out bool activityFeedValue))
                toggleActivityFeed.IsOn = activityFeedValue;
                
            if (settings.TryGetValue("ScreenRecording", out bool screenRecordingValue))
                toggleScreenRecording.IsOn = screenRecordingValue;
                
            if (settings.TryGetValue("NotificationsTray", out bool notificationsTrayValue))
                toggleNotificationsTray.IsOn = notificationsTrayValue;
                
            if (settings.TryGetValue("MapDownloads", out bool mapDownloadsValue))
                toggleMapDownloads.IsOn = mapDownloadsValue;
                
            if (settings.TryGetValue("LockscreenCamera", out bool lockscreenCameraValue))
                toggleLockscreenCamera.IsOn = lockscreenCameraValue;
                
            if (settings.TryGetValue("Biometrics", out bool biometricsValue))
                toggleBiometrics.IsOn = biometricsValue;
                
            // Apply toggles by applying the click events
            ApplyToggles();
        }
        
        // Method to apply all toggle changes (which will call the appropriate registry changes)
        private void ApplyToggles()
        {
            // Trigger methods directly instead of using click events
            if (toggleLocation.IsOn) SetLocationPrivacy(true);
            if (toggleCamera.IsOn) SetCameraPrivacy(true);
            if (toggleMicrophone.IsOn) SetMicrophonePrivacy(true);
            if (toggleSystemFiles.IsOn) SetSystemFilesPrivacy(true);
            if (toggleAccountInfo.IsOn) SetAccountInfoPrivacy(true);
            if (toggleContacts.IsOn) SetContactsPrivacy(true);
            if (toggleCallHistory.IsOn) SetCallHistoryPrivacy(true);
            if (toggleMessaging.IsOn) SetMessagingPrivacy(true);
            if (toggleNotifications.IsOn) SetNotificationsPrivacy(true);
            if (toggleEmail.IsOn) SetEmailPrivacy(true);
            if (toggleTasks.IsOn) SetTasksPrivacy(true);
            if (toggleDiagnostics.IsOn) SetDiagnosticsPrivacy(true);
            if (toggleVoiceActivation.IsOn) SetVoiceActivationPrivacy(true);
            if (togglePhone.IsOn) SetPhonePrivacy(true);
            if (toggleTrustedDevices.IsOn) SetTrustedDevicesPrivacy(true);
            if (toggleCalendar.IsOn) SetCalendarPrivacy(true);
            if (toggleMotion.IsOn) SetMotionPrivacy(true);
            if (toggleRadio.IsOn) SetRadioPrivacy(true);
            if (toggleCloudSync.IsOn) SetCloudSyncPrivacy(true);
            if (toggleActivityFeed.IsOn) SetActivityFeedPrivacy(true);
            if (toggleScreenRecording.IsOn) SetScreenRecordingPrivacy(true);
            if (toggleNotificationsTray.IsOn) SetNotificationsTrayPrivacy(true);
            if (toggleMapDownloads.IsOn) SetMapDownloadsPrivacy(true);
            if (toggleLockscreenCamera.IsOn) SetLockscreenCameraPrivacy(true);
            if (toggleBiometrics.IsOn) SetBiometricsPrivacy(true);
        }
    }
}
