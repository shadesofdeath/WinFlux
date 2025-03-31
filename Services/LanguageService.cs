using System;
using System.Linq;
using System.Windows;

namespace WinFlux.Services
{
    public class LanguageService
    {
        // Dil değiştiğinde tetiklenecek olay
        public static event EventHandler LanguageChanged;
        
        public static string[] AvailableLanguages => new[] { "Türkçe", "English" };
        
        public static void ChangeLanguage(string language)
        {
            var dict = new ResourceDictionary();
            try
            {
                switch (language)
                {
                    case "English":
                        dict.Source = new Uri("/Resources/en-US.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("/Resources/tr-TR.xaml", UriKind.Relative);
                        break;
                }

                // Mevcut dil sözlüğünü kaldır
                var oldDict = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source?.ToString().Contains("/Resources/") ?? false);
                if (oldDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                }

                // Yeni dil sözlüğünü ekle
                Application.Current.Resources.MergedDictionaries.Add(dict);
                
                // Save language preference
                UserSettingsService.Instance.PreferredLanguage = language;
                
                // Dil değişikliği olayını tetikle
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dil değiştirme hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initializes the application language based on saved preferences
        /// </summary>
        public static void InitializeLanguage()
        {
            // Get the saved language preference
            string preferredLanguage = UserSettingsService.Instance.PreferredLanguage;
            
            // Apply the language
            ChangeLanguage(preferredLanguage);
        }
    }
}
