using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using WinFlux.Models;
using System.Windows;

namespace WinFlux.Services
{
    public class ApplicationDataService
    {
        private static ApplicationDataService _instance;
        public static ApplicationDataService Instance => _instance ??= new ApplicationDataService();

        private Dictionary<string, ApplicationInfo> _applications;
        private Dictionary<string, List<ApplicationInfo>> _categorizedApplications;
        
        private ApplicationDataService()
        {
            // Initialize empty collections
            _applications = new Dictionary<string, ApplicationInfo>();
            _categorizedApplications = new Dictionary<string, List<ApplicationInfo>>();
        }

        public async Task LoadApplicationsDataAsync()
        {
            try
            {
                // Try loading the embedded resource from the assembly
                Uri resourceUri = new Uri("pack://application:,,,/Config/applications.json", UriKind.Absolute);
                string jsonContent;

                try
                {
                    using (Stream stream = Application.GetResourceStream(resourceUri).Stream)
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        jsonContent = await reader.ReadToEndAsync();
                    }
                    
                    // Parse the JSON string
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    _applications = JsonSerializer.Deserialize<Dictionary<string, ApplicationInfo>>(jsonContent, options);
                    
                    // Organize applications by category
                    OrganizeByCategory();
                    return; // Successfully loaded from embedded resource
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading applications data as embedded resource: {ex.Message}");
                    // Continue to fallback method
                }

                // Fallback: Try loading from resource stream directly using GetManifestResourceStream
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = assembly.GetManifestResourceNames()
                        .FirstOrDefault(name => name.EndsWith("applications.json", StringComparison.OrdinalIgnoreCase));

                    if (resourceName != null)
                    {
                        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            jsonContent = await reader.ReadToEndAsync();
                        }
                        
                        // Parse the JSON string
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        
                        _applications = JsonSerializer.Deserialize<Dictionary<string, ApplicationInfo>>(jsonContent, options);
                        
                        // Organize applications by category
                        OrganizeByCategory();
                        return; // Successfully loaded from manifest resource
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading applications data from manifest resource: {ex.Message}");
                    // Continue to next fallback method
                }

                // Last resort: Try to load from the file system
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "applications.json");
                if (!File.Exists(jsonFilePath))
                {
                    throw new FileNotFoundException($"Applications data file not found at: {jsonFilePath}");
                }

                jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                
                // Parse the JSON string
                var fileOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                _applications = JsonSerializer.Deserialize<Dictionary<string, ApplicationInfo>>(jsonContent, fileOptions);
                
                // Organize applications by category
                OrganizeByCategory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading applications data: {ex.Message}");
                throw;
            }
        }

        private void OrganizeByCategory()
        {
            _categorizedApplications.Clear();
            
            foreach (var app in _applications.Values)
            {
                if (!_categorizedApplications.ContainsKey(app.Category))
                {
                    _categorizedApplications[app.Category] = new List<ApplicationInfo>();
                }
                
                _categorizedApplications[app.Category].Add(app);
            }
            
            // Sort the apps in each category by name
            foreach (var category in _categorizedApplications.Keys.ToList())
            {
                _categorizedApplications[category] = _categorizedApplications[category]
                    .OrderBy(app => app.Name)
                    .ToList();
            }
        }

        public Dictionary<string, ApplicationInfo> GetAllApplications()
        {
            return _applications;
        }

        public Dictionary<string, List<ApplicationInfo>> GetCategorizedApplications()
        {
            return _categorizedApplications;
        }

        public List<string> GetCategories()
        {
            return _categorizedApplications.Keys.OrderBy(c => c).ToList();
        }

        public ApplicationInfo GetApplication(string appId)
        {
            if (_applications.TryGetValue(appId, out var appInfo))
            {
                return appInfo;
            }
            
            return null;
        }

        public List<ApplicationInfo> GetApplicationsByCategory(string category)
        {
            if (_categorizedApplications.TryGetValue(category, out var appsList))
            {
                return appsList;
            }
            
            return new List<ApplicationInfo>();
        }
    }
} 