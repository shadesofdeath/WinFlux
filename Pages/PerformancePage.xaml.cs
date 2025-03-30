using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using iNKORE.UI.WPF.Modern.Controls;
using WinFlux.Helpers;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using WinFlux.Models;

namespace WinFlux.Pages
{
    public partial class PerformancePage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        // Add static reference for preset service to access
        public static PerformancePage Instance { get; private set; }

        public PerformancePage()
        {
            InitializeComponent();
            
            // Store the instance for preset service to use
            Instance = this;
            
            LoadSettings();
            PopulateNetworkInterfaces();
            AttachEvents();
        }

        private void LoadSettings()
        {
            // Hibernation
            toggleHibernation.IsOn = false; // Hibernation is enabled by default, so IsOn = false means disable it

            // Prefetch
            string sysmainServiceStart = RegHelper.GetStringValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SysMain", "Start") ?? "2";
            togglePrefetch.IsOn = sysmainServiceStart == "4"; // 4 = Disabled

            // Storage Sense
            int storageSenseValue = (int)(RegHelper.GetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01") ?? 1);
            toggleStorageSense.IsOn = storageSenseValue == 0;

            // HAGS
            int hagsValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode") ?? 0);
            toggleHAGS.IsOn = hagsValue == 1;

            // Core Isolation
            int coreIsolationValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\CurrentControlSet\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled") ?? 1);
            toggleCoreIsolation.IsOn = coreIsolationValue == 0;

            // Windows Defender CPU Limit
            int defenderCpuLimitValue = (int)(RegHelper.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Scan", "AvgCPULoadFactor") ?? 50);
            toggleDefender.IsOn = defenderCpuLimitValue == 25;

            // Windows Search
            string searchServiceStart = RegHelper.GetStringValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSearch", "Start") ?? "2";
            toggleSearch.IsOn = searchServiceStart == "4"; // 4 = Disabled
        }

        private void PopulateNetworkInterfaces()
        {
            try
            {
                networkInterfaceComboBox.Items.Clear();
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && 
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                         ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        string description = $"{ni.Name} ({ni.NetworkInterfaceType})";
                        networkInterfaceComboBox.Items.Add(description);
                    }
                }

                if (networkInterfaceComboBox.Items.Count > 0)
                {
                    networkInterfaceComboBox.SelectedIndex = 0;
                }
                else
                {
                    networkInterfaceComboBox.Items.Add(FindResource("PerformancePageMessageBox_NoNetworkInterfaces").ToString());
                    networkInterfaceComboBox.SelectedIndex = 0;
                    networkInterfaceComboBox.IsEnabled = false;
                    applyDnsButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, FindResource("MessageBox_Error").ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AttachEvents()
        {
            // Hibernation
            toggleHibernation.Toggled += (s, e) => SetHibernation(toggleHibernation.IsOn);

            // Prefetch
            togglePrefetch.Toggled += (s, e) => SetPrefetch(togglePrefetch.IsOn);

            // Storage Sense
            toggleStorageSense.Toggled += (s, e) => SetStorageSense(toggleStorageSense.IsOn);

            // HAGS
            toggleHAGS.Toggled += (s, e) => SetHAGS(toggleHAGS.IsOn);

            // Core Isolation
            toggleCoreIsolation.Toggled += (s, e) => SetCoreIsolation(toggleCoreIsolation.IsOn);

            // Windows Defender CPU Limit
            toggleDefender.Toggled += (s, e) => SetDefenderCpuLimit(toggleDefender.IsOn);

            // Windows Search
            toggleSearch.Toggled += (s, e) => SetWindowsSearch(toggleSearch.IsOn);
        }

        private void ApplyDnsButton_Click(object sender, RoutedEventArgs e)
        {
            if (networkInterfaceComboBox.SelectedItem == null || !networkInterfaceComboBox.IsEnabled)
            {
                MessageBox.Show(FindResource("PerformancePageMessageBox_SelectNetworkInterface").ToString(), 
                    FindResource("MessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string selectedInterface = networkInterfaceComboBox.SelectedItem.ToString();
            // Extract just the interface name from the description format
            string interfaceName = selectedInterface.Split(' ')[0];
            
            string primaryDns = "";
            string secondaryDns = "";

            switch (dnsProviderComboBox.SelectedIndex)
            {
                case 0: // Google DNS
                    primaryDns = "8.8.8.8";
                    secondaryDns = "8.8.4.4";
                    break;
                case 1: // Cloudflare DNS
                    primaryDns = "1.1.1.1";
                    secondaryDns = "1.0.0.1";
                    break;
                case 2: // OpenDNS
                    primaryDns = "208.67.222.222";
                    secondaryDns = "208.67.220.220";
                    break;
                case 3: // Quad9 DNS
                    primaryDns = "9.9.9.9";
                    secondaryDns = "149.112.112.112";
                    break;
                default:
                    MessageBox.Show(FindResource("PerformancePageMessageBox_SelectDNSProvider").ToString(), 
                        FindResource("MessageBox_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            try
            {
                string setPrimaryCmd = $"netsh interface ip set dns name=\"{interfaceName}\" static {primaryDns}";
                string setSecondaryCmd = $"netsh interface ip add dns name=\"{interfaceName}\" {secondaryDns} index=2";
                
                RunCommand(setPrimaryCmd);
                RunCommand(setSecondaryCmd);

                MessageBox.Show(FindResource("PerformancePageMessageBox_DNSApplied").ToString(), 
                    FindResource("MessageBox_Information").ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, FindResource("MessageBox_Error").ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UltimatePerformanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string checkPlanCmd = "powershell -command \"$ultimatePerformance = powercfg -list | Select-String -Pattern 'Ultimate Performance'; if ($ultimatePerformance) { echo '-- - Power plan already exists' } else { echo '-- - Enabling Ultimate Performance'; $output = powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 2>&1; if ($output -match 'Unable to create a new power scheme' -or $output -match 'The power scheme, subgroup or setting specified does not exist') { powercfg -RestoreDefaultSchemes } }\"";
                string activatePlanCmd = "powershell -command \"$ultimatePlanGUID = (powercfg -list | Select-String -Pattern 'Ultimate Performance').Line.Split()[3]; echo '-- - Activating Ultimate Performance'; powercfg -setactive $ultimatePlanGUID\"";
                
                RunCommand(checkPlanCmd);
                RunCommand(activatePlanCmd);

                MessageBox.Show(FindResource("PerformancePageMessageBox_UltimatePerformanceEnabled").ToString(), 
                    FindResource("MessageBox_Information").ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, FindResource("MessageBox_Error").ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OptimizeServicesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(
                    FindResource("PerformancePageMessageBox_ServicesConfirmation").ToString(),
                    FindResource("MessageBox_Confirmation").ToString(),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // List of important services to optimize
                    List<string> servicesCommands = new List<string>
                    {
                        "sc config DiagTrack start=disabled",
                        "sc config AJRouter start=disabled", 
                        "sc config AppVClient start=disabled",
                        "sc config AssignedAccessManagerSvc start=disabled",
                        "sc config NetTcpPortSharing start=disabled",
                        "sc config RemoteAccess start=disabled", 
                        "sc config RemoteRegistry start=disabled",
                        "sc config shpamsvc start=disabled",
                        "sc config ssh-agent start=disabled",
                        "sc config tzautoupdate start=disabled"
                    };

                    foreach (string command in servicesCommands)
                    {
                        RunCommand(command);
                    }

                    MessageBox.Show(FindResource("PerformancePageMessageBox_ServicesOptimized").ToString(), 
                        FindResource("MessageBox_Information").ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, FindResource("MessageBox_Error").ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetHibernation(bool disable)
        {
            try
            {
                if (disable)
                {
                    RunCommand("powercfg.exe /hibernate off");
                }
                else
                {
                    RunCommand("powercfg.exe /hibernate on");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, FindResource("MessageBox_Error").ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetPrefetch(bool disable)
        {
            try
            {
                if (disable)
                {
                    RunCommand("sc stop sysmain");
                    RunCommand("sc config sysmain start=disabled");
                }
                else
                {
                    RunCommand("sc config sysmain start=auto");
                    RunCommand("sc start sysmain");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, FindResource("MessageBox_Error").ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetStorageSense(bool disable)
        {
            if (disable)
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", 0, RegistryValueKind.DWord);
            }
            else
            {
                RegHelper.SetValue(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", 1, RegistryValueKind.DWord);
            }
        }

        private void SetHAGS(bool enable)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", enable ? 1 : 0, RegistryValueKind.DWord);
        }

        private void SetCoreIsolation(bool disable)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\CurrentControlSet\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", disable ? 0 : 1, RegistryValueKind.DWord);
        }

        private void SetDefenderCpuLimit(bool limit)
        {
            RegHelper.SetValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Scan", "AvgCPULoadFactor", limit ? 25 : 50, RegistryValueKind.DWord);
        }

        private void SetWindowsSearch(bool disable)
        {
            try
            {
                if (disable)
                {
                    RunCommand("sc stop \"wsearch\"");
                    RunCommand("sc config \"wsearch\" start=disabled");
                }
                else
                {
                    RunCommand("sc config \"wsearch\" start=auto");
                    RunCommand("sc start \"wsearch\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, FindResource("MessageBox_Error").ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunCommand(string command)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;
                processInfo.Verb = "runas"; // Run as administrator

                Process process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception(FindResource("TelemetryPageMessageBox_CommandError") + " " + ex.Message);
            }
        }

        // Method to collect all toggle settings for preset export
        public Dictionary<string, bool> GetToggleSettings()
        {
            var settings = new Dictionary<string, bool>();

            // Collect settings from all toggle switches
            settings.Add("Hibernation", toggleHibernation.IsOn);
            settings.Add("Prefetch", togglePrefetch.IsOn);
            settings.Add("StorageSense", toggleStorageSense.IsOn);
            settings.Add("HAGS", toggleHAGS.IsOn);
            settings.Add("CoreIsolation", toggleCoreIsolation.IsOn);
            settings.Add("Defender", toggleDefender.IsOn);
            settings.Add("Search", toggleSearch.IsOn);

            return settings;
        }

        // Method to collect all slider settings for preset export
        public Dictionary<string, double> GetSliderValues()
        {
            var sliderValues = new Dictionary<string, double>();
            
            // Currently there are no sliders in this page
            // But if you add any in the future, you can collect them here
            // sliderValues.Add("SomeSetting", someSlider.Value);
            
            // For DNS settings, you can store the selected index
            if (dnsProviderComboBox != null)
                sliderValues.Add("DNSProvider", dnsProviderComboBox.SelectedIndex);
            
            return sliderValues;
        }

        // Method to apply preset settings
        public void ApplyToggleSettings(Dictionary<string, bool> settings)
        {
            if (settings == null || settings.Count == 0) return;

            // Apply settings to toggle switches
            if (settings.TryGetValue("Hibernation", out bool hibernationValue))
                toggleHibernation.IsOn = hibernationValue;
                
            if (settings.TryGetValue("Prefetch", out bool prefetchValue))
                togglePrefetch.IsOn = prefetchValue;
                
            if (settings.TryGetValue("StorageSense", out bool storageSenseValue))
                toggleStorageSense.IsOn = storageSenseValue;
                
            if (settings.TryGetValue("HAGS", out bool hagsValue))
                toggleHAGS.IsOn = hagsValue;
                
            if (settings.TryGetValue("CoreIsolation", out bool coreIsolationValue))
                toggleCoreIsolation.IsOn = coreIsolationValue;
                
            if (settings.TryGetValue("Defender", out bool defenderValue))
                toggleDefender.IsOn = defenderValue;
                
            if (settings.TryGetValue("Search", out bool searchValue))
                toggleSearch.IsOn = searchValue;
                
            // Apply toggles by applying the click events
            ApplyToggles();
        }
        
        // Method to apply slider settings
        public void ApplySliderValues(Dictionary<string, double> sliderValues)
        {
            if (sliderValues == null || sliderValues.Count == 0) return;
            
            // Apply slider values
            // Currently there are no sliders in this page
            // But if you add any in the future, you can apply them here
            // if (sliderValues.TryGetValue("SomeSetting", out double someValue))
            //     someSlider.Value = someValue;
            
            // For DNS settings, apply the selected index if exists
            if (sliderValues.TryGetValue("DNSProvider", out double dnsProviderIndex) && dnsProviderComboBox != null)
            {
                dnsProviderComboBox.SelectedIndex = (int)dnsProviderIndex;
                // Optionally trigger the selection change event
                ApplyDnsButton_Click(null, null);
            }
        }
        
        // Method to apply all toggle changes (which will call the appropriate registry changes)
        private void ApplyToggles()
        {
            // Trigger methods directly instead of using click events
            if (toggleHibernation.IsOn) SetHibernation(toggleHibernation.IsOn);
            if (togglePrefetch.IsOn) SetPrefetch(togglePrefetch.IsOn);
            if (toggleStorageSense.IsOn) SetStorageSense(toggleStorageSense.IsOn);
            if (toggleHAGS.IsOn) SetHAGS(toggleHAGS.IsOn);
            if (toggleCoreIsolation.IsOn) SetCoreIsolation(toggleCoreIsolation.IsOn);
            if (toggleDefender.IsOn) SetDefenderCpuLimit(toggleDefender.IsOn);
            if (toggleSearch.IsOn) SetWindowsSearch(toggleSearch.IsOn);
        }
    }
} 