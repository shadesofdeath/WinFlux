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

        private void btnRemoveAdobeCC_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("powershell.exe", @"-command ""
                function Invoke-WPFRunAdobeCCCleanerTool {
                    [string]$url='https://swupmf.adobe.com/webfeed/CleanerTool/win/AdobeCreativeCloudCleanerTool.exe'
                    try {
                        $ProgressPreference='SilentlyContinue'
                        $tempFolder = $env:TEMP
                        Invoke-WebRequest -Uri $url -OutFile ""$tempFolder\AdobeCreativeCloudCleanerTool.exe"" -UseBasicParsing -ErrorAction SilentlyContinue
                        $ProgressPreference='Continue'
                        Start-Process -FilePath ""$tempFolder\AdobeCreativeCloudCleanerTool.exe"" -Wait -ErrorAction SilentlyContinue
                    } catch {
                        Write-Error $_.Exception.Message
                    } finally {
                        if (Test-Path -Path ""$tempFolder\AdobeCreativeCloudCleanerTool.exe"") {
                            Remove-Item -Path ""$tempFolder\AdobeCreativeCloudCleanerTool.exe""
                        }
                    }
                }
                Invoke-WPFRunAdobeCCCleanerTool
            """);
        }
        
        private void btnResetNetworkSettings_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("powershell.exe", @"-command ""
                function Invoke-WPFFixesNetwork {
                    # Reset WinSock catalog to a clean state
                    Start-Process -NoNewWindow -FilePath 'netsh' -ArgumentList 'winsock', 'reset'
                    # Resets WinHTTP proxy setting to DIRECT
                    Start-Process -NoNewWindow -FilePath 'netsh' -ArgumentList 'winhttp', 'reset', 'proxy'
                    # Removes all user configured IP settings
                    Start-Process -NoNewWindow -FilePath 'netsh' -ArgumentList 'int', 'ip', 'reset'
                    
                    $ButtonType = [System.Windows.MessageBoxButton]::OK
                    $MessageboxTitle = 'Network Reset'
                    $Messageboxbody = ('Stock settings loaded.`n Please reboot your computer')
                    $MessageIcon = [System.Windows.MessageBoxImage]::Information
                    
                    [System.Windows.MessageBox]::Show($Messageboxbody, $MessageboxTitle, $ButtonType, $MessageIcon)
                }
                Invoke-WPFFixesNetwork
            """);
        }
        
        private void btnSetupAutologin_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("powershell.exe", @"-command ""
                function Invoke-WPFPanelAutologin {
                    # Official Microsoft recommendation: https://learn.microsoft.com/en-us/sysinternals/downloads/autologon
                    $tempFolder = $env:TEMP
                    Invoke-WebRequest -Uri 'https://live.sysinternals.com/Autologon.exe' -OutFile ""$tempFolder\autologin.exe""
                    cmd /c ""$tempFolder\autologin.exe"" /accepteula
                }
                Invoke-WPFPanelAutologin
            """);
        }
        
        private void btnReinstallWinget_Click(object sender, RoutedEventArgs e)
        {
            RunCommandAsAdmin("powershell.exe", @"-command ""
                try {
                    Write-Host 'GitHub''dan Winget indiriliyor ve kuruluyor...'
                    
                    # GitHub API'den en son winget sürümünü al
                    $API_URL = 'https://api.github.com/repos/microsoft/winget-cli/releases/latest'
                    $apiResult = Invoke-RestMethod $API_URL
                    $DOWNLOAD_URL = $apiResult.assets.browser_download_url | Where-Object {$_.EndsWith('.msixbundle')}
                    
                    # Yükleyiciyi indir
                    Write-Host 'Winget yükleyicisi indiriliyor...'
                    $tempFolder = $env:TEMP
                    Invoke-WebRequest -URI $DOWNLOAD_URL -OutFile ""$tempFolder\winget.msixbundle"" -UseBasicParsing
                    
                    # Winget'i yüklemeyi dene
                    try {
                        Write-Host 'Winget yükleniyor...'
                        Add-AppxPackage ""$tempFolder\winget.msixbundle""
                        Write-Host 'Winget başarıyla yüklendi!'
                    } catch {
                        $errorMsg = $_.Exception.Message
                        
                        # Microsoft.UI.Xaml.2.7 eksikse kur
                        if ($errorMsg -like '*Microsoft.UI.Xaml.2.7*') {
                            Write-Host 'Microsoft.UI.Xaml.2.7 bağımlılığı kuruluyor...'
                            Invoke-WebRequest -URI 'https://www.nuget.org/api/v2/package/Microsoft.UI.Xaml/2.7.3' -OutFile ""$tempFolder\xaml.zip"" -UseBasicParsing
                            Expand-Archive -Path ""$tempFolder\xaml.zip"" -DestinationPath ""$tempFolder\xaml"" -Force
                            Add-AppxPackage -Path ""$tempFolder\xaml\tools\AppX\x64\Release\Microsoft.UI.Xaml.2.7.appx""
                            
                            # Winget'i tekrar kurmayı dene
                            try {
                                Write-Host 'Winget tekrar kuruluyor...'
                                Add-AppxPackage ""$tempFolder\winget.msixbundle""
                            } catch {
                                $secondErrorMsg = $_.Exception.Message
                                
                                # Microsoft.VCLibs.140.00.UWPDesktop eksikse kur
                                if ($secondErrorMsg -like '*Microsoft.VCLibs.140.00.UWPDesktop*') {
                                    Write-Host 'Microsoft.VCLibs.140.00 bağımlılığı kuruluyor...'
                                    Invoke-WebRequest -URI 'https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx' -OutFile ""$tempFolder\UWPDesktop.appx"" -UseBasicParsing
                                    Add-AppxPackage ""$tempFolder\UWPDesktop.appx""
                                    
                                    # Winget'i son kez kurmayı dene
                                    Write-Host 'Winget son kez kuruluyor...'
                                    Add-AppxPackage ""$tempFolder\winget.msixbundle""
                                }
                            }
                        }
                    }
                    
                    # Geçici dosyaları temizle
                    if (Test-Path -Path ""$tempFolder\winget.msixbundle"") { Remove-Item ""$tempFolder\winget.msixbundle"" }
                    if (Test-Path -Path ""$tempFolder\xaml.zip"") { Remove-Item ""$tempFolder\xaml.zip"" }
                    if (Test-Path -Path ""$tempFolder\xaml"" -PathType Container) { Remove-Item ""$tempFolder\xaml"" -Recurse }
                    if (Test-Path -Path ""$tempFolder\UWPDesktop.appx"") { Remove-Item ""$tempFolder\UWPDesktop.appx"" }
                    
                    [System.Windows.MessageBox]::Show(
                        'Winget başarıyla kuruldu! Kullanmak için yeni bir terminal penceresi açın ve ""winget"" komutunu yazın.',
                        'Winget Kurulumu',
                        [System.Windows.MessageBoxButton]::OK,
                        [System.Windows.MessageBoxImage]::Information
                    )
                }
                catch {
                    Write-Error $_.Exception.Message
                    [System.Windows.MessageBox]::Show(
                        'Winget kurulumu sırasında bir hata oluştu: ' + $_.Exception.Message,
                        'Hata',
                        [System.Windows.MessageBoxButton]::OK,
                        [System.Windows.MessageBoxImage]::Error
                    )
                }
            """);
        }
    }
}
