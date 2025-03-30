using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using iNKORE.UI.WPF.Modern.Controls;

namespace WinFlux.Pages
{
    public partial class AboutPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        public AboutPage()
        {
            InitializeComponent();
            
            // You can dynamically load the version number here if needed
            // For example:
            // versionTextBlock.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void SocialButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string url)
            {
                OpenUrl(url);
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                    ex.Message,
                    FindResource("MessageBox_Error").ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
} 