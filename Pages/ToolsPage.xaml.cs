using System.Windows.Controls;
using WinFlux.Services;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Linq;
using System.Diagnostics;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace WinFlux.Pages
{
    public partial class ToolsPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        public ToolsPage()
        {
            InitializeComponent();
        }

        private void RunCommandAsAdmin(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas"; // Yönetici olarak çalıştır
            Process.Start(startInfo);
        }

        private void btnCleanTemp_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("cmd.exe", "/c del /s /f /q c:\\windows\\temp\\. & del /s /f /q C:\\WINDOWS\\Prefetch & del /s /f /q %temp%\\.");
        }

        private void btnDiskCleanup_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("cmd.exe", "/c cleanmgr /verylowdisk /sagerun:5");
        }

        private void btnEmptyRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("powershell.exe", "-ExecutionPolicy Unrestricted -Command \"$bin = (New-Object -ComObject Shell.Application).NameSpace(10); $bin.items() | ForEach {Write-Host \\\"Deleting $($_.Name) from Recycle Bin\\\"; Remove-Item $_.Path -Recurse -Force}\"");
        }

        private void btnCreateRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("powershell.exe", "-command \"Enable-ComputerRestore -Drive $env:SystemDrive ; Checkpoint-Computer -Description \\\"RestorePoint1\\\" -RestorePointType \\\"MODIFY_SETTINGS\\\"\"");
        }

        private void btnScanSystem_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("cmd.exe", "/c sfc /scannow");
        }

        private void btnClearBrowserData_Click(object sender, RoutedEventArgs e)
        {
            string command = @"/c del /q /s ""%LocalAppData%\Google\Chrome\User Data\Default\History"" & " +
                           @"del /q /s ""%LocalAppData%\Google\Chrome\User Data\Default\Cache\*.*"" & " +
                           @"del /q /s ""%LocalAppData%\Google\Chrome\User Data\Default\Cookies"" & " +
                           @"del /q /s ""%LocalAppData%\Microsoft\Edge\User Data\Default\History"" & " +
                           @"del /q /s ""%LocalAppData%\Microsoft\Edge\User Data\Default\Cache\*.*"" & " +
                           @"del /q /s ""%LocalAppData%\Microsoft\Edge\User Data\Default\Cookies"" & " +
                           @"del /q /s ""%APPDATA%\Mozilla\Firefox\Profiles\*.default\places.sqlite"" & " +
                           @"del /q /s ""%APPDATA%\Mozilla\Firefox\Profiles\*.default\cache2\entries\*.*"" & " +
                           @"del /q /s ""%LocalAppData%\BraveSoftware\Brave-Browser\User Data\Default\History"" & " +
                           @"del /q /s ""%LocalAppData%\BraveSoftware\Brave-Browser\User Data\Default\Cache\*.*"" & " +
                           @"del /q /s ""%LocalAppData%\BraveSoftware\Brave-Browser\User Data\Default\Cookies""";
            
            RunCommandAsAdmin("cmd.exe", command);
        }

        private void btnResetNetwork_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("cmd.exe", "/c ipconfig /flushdns & ipconfig /release & ipconfig /renew");
        }

        private void btnRunMAS_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("powershell.exe", "-command \"irm https://get.activated.win | iex\"");
        }
    }
}
