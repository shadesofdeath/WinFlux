using System.Windows.Controls;
using WinFlux.Services;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Linq;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Microsoft.Win32;
using WinFlux.Models;

namespace WinFlux.Pages
{
    public partial class SettingsPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LanguageComboBox.ItemsSource = LanguageService.AvailableLanguages;
            
            // Get the current language from settings
            string currentLanguage = UserSettingsService.Instance.PreferredLanguage;
            
            // Set selected index based on the saved language
            LanguageComboBox.SelectedIndex = currentLanguage == "English" ? 1 : 0;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is string selectedLanguage)
            {
                LanguageService.ChangeLanguage(selectedLanguage);
            }
        }
    }
}
