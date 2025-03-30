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

        private void btnExportPresets_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml",
                Title = Application.Current.Resources["PresetMessageBox_SavePresetTitle"] as string,
                DefaultExt = "xml",
                AddExtension = true
            };

            // Show save file dialog
            if (saveFileDialog.ShowDialog() == true)
            {
                // Export preset to selected file
                if (PresetService.ExportPreset(saveFileDialog.FileName))
                {
                    MessageBox.Show(
                        Application.Current.Resources["PresetMessageBox_ExportSuccess"] as string,
                        Application.Current.Resources["PresetMessageBox_Success"] as string,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        private void btnImportPresets_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml",
                Title = Application.Current.Resources["PresetMessageBox_LoadPresetTitle"] as string
            };

            // Show open file dialog
            if (openFileDialog.ShowDialog() == true)
            {
                // Ask for confirmation before importing
                var confirmResult = MessageBox.Show(
                    Application.Current.Resources["PresetMessageBox_ImportConfirmation"] as string,
                    Application.Current.Resources["MessageBox_Confirmation"] as string,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    // Import and apply preset
                    var preset = PresetService.ImportPreset(openFileDialog.FileName);
                    if (preset != null && PresetService.ApplyPreset(preset))
                    {
                        MessageBox.Show(
                            Application.Current.Resources["PresetMessageBox_ImportSuccess"] as string,
                            Application.Current.Resources["PresetMessageBox_Success"] as string,
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}
