using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using WinFlux.Services;

namespace WinFlux
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;
        private Windows.SplashWindow splashWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                // Initialize language based on saved preference
                LanguageService.InitializeLanguage();
                
                // Ana pencereyi oluştur ama gösterme
                mainWindow = new MainWindow();
                
                // Özel splash window'u göster
                splashWindow = new Windows.SplashWindow();
                
                // Splash ekranı tamamlandığında ana pencereyi göster
                splashWindow.LoadingComplete += (s, args) => 
                {
                    mainWindow.Show();
                    mainWindow.Activate();
                };
                
                // Splash window'u göster
                splashWindow.Show();
            }
            catch (Exception ex)
            {

                if (mainWindow == null)
                    mainWindow = new MainWindow();
                
                mainWindow.Show();
            }
        }
    }
}
