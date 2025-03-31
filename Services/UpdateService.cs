using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinFlux.Services
{
    public static class UpdateService
    {
        private const string GitHubReleasesUrl = "https://api.github.com/repos/shadesofdeath/WinFlux/releases/latest";
        
        public static async Task<(bool HasUpdate, string LatestVersion, string DownloadUrl)> CheckForUpdatesAsync()
        {
            try
            {
                // Get current version from assembly
                Version currentVersion = GetCurrentVersion();
                
                // Get latest release info from GitHub
                var (latestVersion, releaseUrl) = await GetLatestVersionInfoAsync();
                
                if (latestVersion != null && currentVersion != null)
                {
                    Version parsedLatestVersion = Version.Parse(latestVersion.TrimStart('v'));
                    
                    // Compare versions
                    bool hasUpdate = parsedLatestVersion > currentVersion;
                    
                    return (hasUpdate, latestVersion, releaseUrl);
                }
                
                return (false, null, null);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error checking for updates: {ex.Message}");
                return (false, null, null);
            }
        }
        
        private static Version GetCurrentVersion()
        {
            try
            {
                // Get the current assembly version
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                
                // For custom version format (e.g. FileVersion)
                // var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                // string fileVersion = fileVersionInfo.FileVersion;
                // return Version.Parse(fileVersion);
                
                return version;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current version: {ex.Message}");
                return null;
            }
        }
        
        private static async Task<(string Version, string Url)> GetLatestVersionInfoAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Add user agent as GitHub API requires it
                    client.DefaultRequestHeaders.Add("User-Agent", "WinFlux-Update-Checker");
                    
                    // Get response from GitHub API
                    HttpResponseMessage response = await client.GetAsync(GitHubReleasesUrl);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        
                        // Parse JSON response using Newtonsoft.Json
                        JObject releaseInfo = JObject.Parse(jsonResponse);
                        
                        string latestVersion = releaseInfo["tag_name"]?.ToString();
                        string releaseUrl = releaseInfo["html_url"]?.ToString();
                        
                        return (latestVersion, releaseUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching latest version: {ex.Message}");
            }
            
            return (null, null);
        }
    }
} 