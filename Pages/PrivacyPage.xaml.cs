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
            // Location
            int locationValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessLocation") ?? 1);
            toggleLocation.IsOn = locationValue != 0;

            // Camera
            int cameraValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessCamera") ?? 1);
            toggleCamera.IsOn = cameraValue != 0;

            // Microphone
            int microphoneValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessMicrophone") ?? 1);
            toggleMicrophone.IsOn = microphoneValue != 0;

            // System Files
            int systemFilesValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessDocuments") ?? 1);
            toggleSystemFiles.IsOn = systemFilesValue != 0;

            // Account Info
            int accountInfoValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessAccountInfo") ?? 1);
            toggleAccountInfo.IsOn = accountInfoValue != 0;

            // Contacts
            int contactsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessContacts") ?? 1);
            toggleContacts.IsOn = contactsValue != 0;

            // Call History
            int callHistoryValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessCallHistory") ?? 1);
            toggleCallHistory.IsOn = callHistoryValue != 0;

            // Messaging
            int messagingValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessMessaging") ?? 1);
            toggleMessaging.IsOn = messagingValue != 0;

            // Notifications
            int notificationsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessNotifications") ?? 1);
            toggleNotifications.IsOn = notificationsValue != 0;

            // Email
            int emailValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessEmail") ?? 1);
            toggleEmail.IsOn = emailValue != 0;

            // Tasks
            int tasksValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessTasks") ?? 1);
            toggleTasks.IsOn = tasksValue != 0;

            // Diagnostics
            int diagnosticsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsGetDiagnosticInfo") ?? 1);
            toggleDiagnostics.IsOn = diagnosticsValue != 0;

            // Voice Activation
            int voiceActivationValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoice") ?? 1);
            toggleVoiceActivation.IsOn = voiceActivationValue != 0;

            // Phone
            int phoneValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessPhone") ?? 1);
            togglePhone.IsOn = phoneValue != 0;

            // Trusted Devices
            int trustedDevicesValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessTrustedDevices") ?? 1);
            toggleTrustedDevices.IsOn = trustedDevicesValue != 0;

            // Calendar
            int calendarValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessCalendar") ?? 1);
            toggleCalendar.IsOn = calendarValue != 0;

            // Motion
            int motionValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessMotion") ?? 1);
            toggleMotion.IsOn = motionValue != 0;

            // Radio
            int radioValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessRadios") ?? 1);
            toggleRadio.IsOn = radioValue != 0;

            // Cloud Sync
            int cloudSyncValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC") ?? 0);
            toggleCloudSync.IsOn = cloudSyncValue != 1;

            // Activity Feed
            int activityFeedValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed") ?? 1);
            toggleActivityFeed.IsOn = activityFeedValue != 0;

            // Screen Recording
            int screenRecordingValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessGraphicsCaptureProgrammatic") ?? 1);
            toggleScreenRecording.IsOn = screenRecordingValue != 0;

            // Notification Tray
            int notificationsTrayValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter") ?? 0);
            toggleNotificationsTray.IsOn = notificationsTrayValue != 1;

            // Map Downloads
            int mapDownloadsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessLocation") ?? 1);
            toggleMapDownloads.IsOn = mapDownloadsValue != 0;

            // Lockscreen Camera
            int lockscreenCameraValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera") ?? 0);
            toggleLockscreenCamera.IsOn = lockscreenCameraValue != 1;

            // Biometrics
            int biometricsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Biometrics", "Enabled") ?? 1);
            int credProviderValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider", "Enabled") ?? 1);
            toggleBiometrics.IsOn = biometricsValue != 0 && credProviderValue != 0;

            // Explorer Gizlilik Ayarları
            // Online Tips
            int onlineTipsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "AllowOnlineTips") ?? 1);
            toggleOnlineTips.IsOn = onlineTipsValue != 0;

            // Internet File Association
            int fileAssociationValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoInternetOpenWith") ?? 0);
            toggleFileAssociation.IsOn = fileAssociationValue != 1;

            // Order Prints
            int orderPrintsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoOnlinePrintsWizard") ?? 0);
            toggleOrderPrints.IsOn = orderPrintsValue != 1;

            // Publish to Web
            int publishToWebValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoPublishingWizard") ?? 0);
            togglePublishToWeb.IsOn = publishToWebValue != 1;

            // Provider List
            int providerListValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoWebServices") ?? 0);
            toggleProviderList.IsOn = providerListValue != 1;

            // Recent Documents History
            int recentDocsHistoryValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRecentDocsHistory") ?? 0);
            toggleRecentDocsHistory.IsOn = recentDocsHistoryValue != 1;

            // Clear Recent Documents on Exit
            int clearRecentDocsValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "ClearRecentDocsOnExit") ?? 0);
            toggleClearRecentDocs.IsOn = clearRecentDocsValue == 1;

            // Lock Screen App Notifications
            int lockScreenNotifValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "DisableLockScreenAppNotifications") ?? 0);
            toggleLockScreenNotif.IsOn = lockScreenNotifValue != 1;

            // Live Tiles Notifications
            int liveTileNotifValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications", "NoTileApplicationNotification") ?? 0);
            toggleLiveTileNotif.IsOn = liveTileNotifValue != 1;

            // Store App Suggestions
            int storeOpenWithValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "NoUseStoreOpenWith") ?? 0);
            toggleStoreOpenWith.IsOn = storeOpenWithValue != 1;

            // Show Recent Files in Quick Access
            int recentFilesValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent") ?? 1);
            toggleRecentFiles.IsOn = recentFilesValue == 1;

            // Sync Provider Notifications
            int syncNotificationsValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications") ?? 1);
            toggleSyncNotifications.IsOn = syncNotificationsValue == 1;

            // Recent Apps
            int recentAppsValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\EdgeUI", "DisableRecentApps") ?? 0);
            toggleRecentApps.IsOn = recentAppsValue != 1;

            // App Usage Tracking
            int appTrackingValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\EdgeUI", "DisableMFUTracking") ?? 0);
            toggleAppTracking.IsOn = appTrackingValue != 1;

            // Backtracking
            int backtrackingValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\EdgeUI", "TurnOffBackstack") ?? 0);
            toggleBacktracking.IsOn = backtrackingValue != 1;
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
            
            // Explorer Gizlilik Ayarları
            // Online Tips
            toggleOnlineTips.Toggled += (s, e) => SetOnlineTipsPrivacy(toggleOnlineTips.IsOn);
            
            // File Association
            toggleFileAssociation.Toggled += (s, e) => SetFileAssociationPrivacy(toggleFileAssociation.IsOn);
            
            // Order Prints
            toggleOrderPrints.Toggled += (s, e) => SetOrderPrintsPrivacy(toggleOrderPrints.IsOn);
            
            // Publish to Web
            togglePublishToWeb.Toggled += (s, e) => SetPublishToWebPrivacy(togglePublishToWeb.IsOn);
            
            // Provider List
            toggleProviderList.Toggled += (s, e) => SetProviderListPrivacy(toggleProviderList.IsOn);
            
            // Recent Documents History
            toggleRecentDocsHistory.Toggled += (s, e) => SetRecentDocsHistoryPrivacy(toggleRecentDocsHistory.IsOn);
            
            // Clear Recent Documents on Exit
            toggleClearRecentDocs.Toggled += (s, e) => SetClearRecentDocsPrivacy(toggleClearRecentDocs.IsOn);
            
            // Lock Screen App Notifications
            toggleLockScreenNotif.Toggled += (s, e) => SetLockScreenNotifPrivacy(toggleLockScreenNotif.IsOn);
            
            // Live Tiles Notifications
            toggleLiveTileNotif.Toggled += (s, e) => SetLiveTileNotifPrivacy(toggleLiveTileNotif.IsOn);
            
            // Store App Suggestions
            toggleStoreOpenWith.Toggled += (s, e) => SetStoreOpenWithPrivacy(toggleStoreOpenWith.IsOn);
            
            // Show Recent Files in Quick Access
            toggleRecentFiles.Toggled += (s, e) => SetRecentFilesPrivacy(toggleRecentFiles.IsOn);
            
            // Sync Provider Notifications
            toggleSyncNotifications.Toggled += (s, e) => SetSyncNotificationsPrivacy(toggleSyncNotifications.IsOn);
            
            // Recent Apps
            toggleRecentApps.Toggled += (s, e) => SetRecentAppsPrivacy(toggleRecentApps.IsOn);
            
            // App Usage Tracking
            toggleAppTracking.Toggled += (s, e) => SetAppTrackingPrivacy(toggleAppTracking.IsOn);
            
            // Backtracking
            toggleBacktracking.Toggled += (s, e) => SetBacktrackingPrivacy(toggleBacktracking.IsOn);
        }

        private void SetLocationPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessLocation", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetCameraPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessCamera", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetMicrophonePrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessMicrophone", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetSystemFilesPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessDocuments", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetAccountInfoPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessAccountInfo", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetContactsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessContacts", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetCallHistoryPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessCallHistory", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetMessagingPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessMessaging", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetNotificationsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessNotifications", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetEmailPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessEmail", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetTasksPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessTasks", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetDiagnosticsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsGetDiagnosticInfo", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetVoiceActivationPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoice", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetPhonePrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessPhone", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetTrustedDevicesPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessTrustedDevices", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetCalendarPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessCalendar", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetMotionPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessMotion", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetRadioPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessRadios", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetCloudSyncPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC", enabled ? 0 : 1, RegistryValueKind.DWord);
        }

        private void SetActivityFeedPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetScreenRecordingPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessGraphicsCaptureProgrammatic", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetNotificationsTrayPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", enabled ? 0 : 1, RegistryValueKind.DWord);
        }

        private void SetMapDownloadsPrivacy(bool enabled)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessLocation", enabled ? 1 : 0, RegistryValueKind.DWord);
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

        // Explorer Gizlilik Ayarları için yeni metotlar
        private void SetOnlineTipsPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "AllowOnlineTips", enabled ? 1 : 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çevrimiçi İpuçları ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetFileAssociationPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoInternetOpenWith", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İnternet Dosya İlişkilendirmesi ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetOrderPrintsPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoOnlinePrintsWizard", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Baskı Siparişi Resim Görevi ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetPublishToWebPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoPublishingWizard", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Web'de Yayınla ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetProviderListPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoWebServices", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sağlayıcı Listesi İndirmeleri ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetRecentDocsHistoryPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRecentDocsHistory", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Son Belgeler Geçmişi ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetClearRecentDocsPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "ClearRecentDocsOnExit", enabled ? 1 : 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çıkışta Son Belge Geçmişini Temizle ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetLockScreenNotifPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "DisableLockScreenAppNotifications", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kilit Ekranı Uygulama Bildirimleri ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetLiveTileNotifPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications", "NoTileApplicationNotification", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Canlı Kutucuk Bildirimleri ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetStoreOpenWithPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "NoUseStoreOpenWith", enabled ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mağaza Uygulama Önerileri ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetRecentFilesPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", enabled ? 1 : 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hızlı Erişim'de Son Dosyaları Göster ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetSyncNotificationsPrivacy(bool enabled)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", enabled ? 1 : 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Senkronizasyon Sağlayıcı Bildirimleri ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetRecentAppsPrivacy(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\EdgeUI", true))
                {
                    key.SetValue("DisableRecentApps", enabled ? 0 : 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Son Kullanılan Uygulamalar ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetAppTrackingPrivacy(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\EdgeUI", true))
                {
                    key.SetValue("DisableMFUTracking", enabled ? 0 : 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama Kullanım Takibi ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetBacktrackingPrivacy(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\EdgeUI", true))
                {
                    key.SetValue("TurnOffBackstack", enabled ? 0 : 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Geri Takip ayarı değiştirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            settings.Add("OnlineTips", toggleOnlineTips.IsOn);
            settings.Add("FileAssociation", toggleFileAssociation.IsOn);
            settings.Add("OrderPrints", toggleOrderPrints.IsOn);
            settings.Add("PublishToWeb", togglePublishToWeb.IsOn);
            settings.Add("ProviderList", toggleProviderList.IsOn);
            settings.Add("RecentDocsHistory", toggleRecentDocsHistory.IsOn);
            settings.Add("ClearRecentDocs", toggleClearRecentDocs.IsOn);
            settings.Add("LockScreenNotif", toggleLockScreenNotif.IsOn);
            settings.Add("LiveTileNotif", toggleLiveTileNotif.IsOn);
            settings.Add("StoreOpenWith", toggleStoreOpenWith.IsOn);
            settings.Add("RecentFiles", toggleRecentFiles.IsOn);
            settings.Add("SyncNotifications", toggleSyncNotifications.IsOn);
            settings.Add("RecentApps", toggleRecentApps.IsOn);
            settings.Add("AppTracking", toggleAppTracking.IsOn);
            settings.Add("Backtracking", toggleBacktracking.IsOn);

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
                
            if (settings.TryGetValue("OnlineTips", out bool onlineTipsValue))
                toggleOnlineTips.IsOn = onlineTipsValue;
                
            if (settings.TryGetValue("FileAssociation", out bool fileAssociationValue))
                toggleFileAssociation.IsOn = fileAssociationValue;
                
            if (settings.TryGetValue("OrderPrints", out bool orderPrintsValue))
                toggleOrderPrints.IsOn = orderPrintsValue;
                
            if (settings.TryGetValue("PublishToWeb", out bool publishToWebValue))
                togglePublishToWeb.IsOn = publishToWebValue;
                
            if (settings.TryGetValue("ProviderList", out bool providerListValue))
                toggleProviderList.IsOn = providerListValue;
                
            if (settings.TryGetValue("RecentDocsHistory", out bool recentDocsHistoryValue))
                toggleRecentDocsHistory.IsOn = recentDocsHistoryValue;
                
            if (settings.TryGetValue("ClearRecentDocs", out bool clearRecentDocsValue))
                toggleClearRecentDocs.IsOn = clearRecentDocsValue;
                
            if (settings.TryGetValue("LockScreenNotif", out bool lockScreenNotifValue))
                toggleLockScreenNotif.IsOn = lockScreenNotifValue;
                
            if (settings.TryGetValue("LiveTileNotif", out bool liveTileNotifValue))
                toggleLiveTileNotif.IsOn = liveTileNotifValue;
                
            if (settings.TryGetValue("StoreOpenWith", out bool storeOpenWithValue))
                toggleStoreOpenWith.IsOn = storeOpenWithValue;
                
            if (settings.TryGetValue("RecentFiles", out bool recentFilesValue))
                toggleRecentFiles.IsOn = recentFilesValue;
                
            if (settings.TryGetValue("SyncNotifications", out bool syncNotificationsValue))
                toggleSyncNotifications.IsOn = syncNotificationsValue;
                
            if (settings.TryGetValue("RecentApps", out bool recentAppsValue))
                toggleRecentApps.IsOn = recentAppsValue;
                
            if (settings.TryGetValue("AppTracking", out bool appTrackingValue))
                toggleAppTracking.IsOn = appTrackingValue;
                
            if (settings.TryGetValue("Backtracking", out bool backtrackingValue))
                toggleBacktracking.IsOn = backtrackingValue;
                
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
            if (toggleOnlineTips.IsOn) SetOnlineTipsPrivacy(true);
            if (toggleFileAssociation.IsOn) SetFileAssociationPrivacy(true);
            if (toggleOrderPrints.IsOn) SetOrderPrintsPrivacy(true);
            if (togglePublishToWeb.IsOn) SetPublishToWebPrivacy(true);
            if (toggleProviderList.IsOn) SetProviderListPrivacy(true);
            if (toggleRecentDocsHistory.IsOn) SetRecentDocsHistoryPrivacy(true);
            if (toggleClearRecentDocs.IsOn) SetClearRecentDocsPrivacy(true);
            if (toggleLockScreenNotif.IsOn) SetLockScreenNotifPrivacy(true);
            if (toggleLiveTileNotif.IsOn) SetLiveTileNotifPrivacy(true);
            if (toggleStoreOpenWith.IsOn) SetStoreOpenWithPrivacy(true);
            if (toggleRecentFiles.IsOn) SetRecentFilesPrivacy(true);
            if (toggleSyncNotifications.IsOn) SetSyncNotificationsPrivacy(true);
            if (toggleRecentApps.IsOn) SetRecentAppsPrivacy(true);
            if (toggleAppTracking.IsOn) SetAppTrackingPrivacy(true);
            if (toggleBacktracking.IsOn) SetBacktrackingPrivacy(true);
        }
    }
}
