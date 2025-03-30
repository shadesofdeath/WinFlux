using System;
using System.Windows;
using Microsoft.Win32;
using iNKORE.UI.WPF.Modern.Controls;
using WinFlux.Helpers;
using System.Diagnostics;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using System.Collections.Generic;
using WinFlux.Models;

namespace WinFlux.Pages
{
    public partial class GameOptimizationPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        // Add static reference for preset service to access
        public static GameOptimizationPage Instance { get; private set; }

        public GameOptimizationPage()
        {
            InitializeComponent();
            
            // Store the instance for preset service to use
            Instance = this;
            LoadSettings();
            AttachToggleEvents();
        }

        private void LoadSettings()
        {
            // Fullscreen Optimizations
            int fullscreenOptValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible") ?? 0);
            toggleFullscreenOpt.IsOn = fullscreenOptValue == 1;

            // Mouse Acceleration
            string mouseSpeed = RegHelper.GetStringValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed") ?? "1";
            string mouseThreshold1 = RegHelper.GetStringValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1") ?? "6";
            string mouseThreshold2 = RegHelper.GetStringValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2") ?? "10";
            toggleMouseAccel.IsOn = mouseSpeed == "0" && mouseThreshold1 == "0" && mouseThreshold2 == "0";

            // Game Bar
            int allowGameDVR = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR") ?? 1);
            int appCaptureEnabled = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled") ?? 1);
            int useNexusForGameBar = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\GameBar", "UseNexusForGameBarEnabled") ?? 1);
            int showStartupPanel = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\GameBar", "ShowStartupPanel") ?? 1);
            toggleGameBar.IsOn = allowGameDVR == 0 && appCaptureEnabled == 0 && useNexusForGameBar == 0 && showStartupPanel == 0;

            // Game Mode
            int autoGameModeEnabledLKM = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AutoGameModeEnabled") ?? 1);
            int autoGameModeEnabledCU = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\GameBar", "AutoGameModeEnabled") ?? 1);
            toggleGameMode.IsOn = autoGameModeEnabledLKM == 0 && autoGameModeEnabledCU == 0;
        }

        private void AttachToggleEvents()
        {
            // Fullscreen Optimizations
            toggleFullscreenOpt.Toggled += (s, e) => SetFullscreenOptimizations(toggleFullscreenOpt.IsOn);

            // Mouse Acceleration
            toggleMouseAccel.Toggled += (s, e) => SetMouseAcceleration(toggleMouseAccel.IsOn);

            // Game Bar
            toggleGameBar.Toggled += (s, e) => SetGameBar(toggleGameBar.IsOn);

            // Game Mode
            toggleGameMode.Toggled += (s, e) => SetGameMode(toggleGameMode.IsOn);
        }

        private void SetFullscreenOptimizations(bool disable)
        {
            RegHelper.SetValue(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", disable ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetMouseAcceleration(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "1", RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", "6", RegistryValueKind.String);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", "10", RegistryValueKind.String);
            }
        }

        private void SetGameBar(bool disable)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", disable ? 0 : 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\GameBar", "UseNexusForGameBarEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\GameBar", "ShowStartupPanel", disable ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindResource("MessageBox_Error") + ": " + ex.Message, FindResource("MessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetGameMode(bool disable)
        {
            try
            {
                RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AutoGameModeEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\GameBar", "AutoGameModeEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindResource("MessageBox_Error") + ": " + ex.Message, FindResource("MessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to collect all toggle settings for preset export
        public Dictionary<string, bool> GetToggleSettings()
        {
            var settings = new Dictionary<string, bool>();

            // Collect settings from all toggle switches
            settings.Add("FullscreenOpt", toggleFullscreenOpt.IsOn);
            settings.Add("MouseAccel", toggleMouseAccel.IsOn);
            settings.Add("GameBar", toggleGameBar.IsOn);
            settings.Add("GameMode", toggleGameMode.IsOn);

            return settings;
        }

        // Method to apply preset settings
        public void ApplyToggleSettings(Dictionary<string, bool> settings)
        {
            if (settings == null || settings.Count == 0) return;

            // Apply settings to toggle switches
            if (settings.TryGetValue("FullscreenOpt", out bool fullscreenOptValue))
                toggleFullscreenOpt.IsOn = fullscreenOptValue;
                
            if (settings.TryGetValue("MouseAccel", out bool mouseAccelValue))
                toggleMouseAccel.IsOn = mouseAccelValue;
                
            if (settings.TryGetValue("GameBar", out bool gameBarValue))
                toggleGameBar.IsOn = gameBarValue;
                
            if (settings.TryGetValue("GameMode", out bool gameModeValue))
                toggleGameMode.IsOn = gameModeValue;
                
            // Apply changes directly through the set methods
            if (toggleFullscreenOpt.IsOn) SetFullscreenOptimizations(true);
            if (toggleMouseAccel.IsOn) SetMouseAcceleration(true);
            if (toggleGameBar.IsOn) SetGameBar(true);
            if (toggleGameMode.IsOn) SetGameMode(true);
        }
    }
} 