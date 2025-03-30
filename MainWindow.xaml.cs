using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using iNKORE.UI.WPF.Modern.Controls;

namespace WinFlux
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Sayfa önbelleği - açılmış sayfaları burada tutacağız
        private Dictionary<string, iNKORE.UI.WPF.Modern.Controls.Page> pageCache = new Dictionary<string, iNKORE.UI.WPF.Modern.Controls.Page>();
        
        public MainWindow()
        {
            InitializeComponent();
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            
            // Pencerenin yüklenmesi tamamlandığında tüm sayfaları ön yükle
            this.Loaded += MainWindow_Loaded;
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // AppInstallerPage'i öncelikli olarak yükle
            await System.Threading.Tasks.Task.Run(() => {
                Application.Current.Dispatcher.Invoke(async () => {
                    // AppInstallerPage sayfasını önceden yükle
                    InitializePageIfNeeded("appInstallerPage");
                    
                    // Sayfanın tamamen yüklenmesi için biraz bekle
                    await System.Threading.Tasks.Task.Delay(500);
                });
            });
            
            // Diğer sayfaları arka planda yükle
            await System.Threading.Tasks.Task.Run(() => {
                Application.Current.Dispatcher.Invoke(() => {
                    // Diğer uygulama sayfaları
                    InitializePageIfNeeded("toolsPage");
                    InitializePageIfNeeded("debloatPage");
                    InitializePageIfNeeded("privacyPage");
                    InitializePageIfNeeded("telemetryPage");
                    InitializePageIfNeeded("gameOptimizationPage");
                    InitializePageIfNeeded("performancePage");
                    InitializePageIfNeeded("settings");
                    InitializePageIfNeeded("about");
                });
            });
        }
        
        // Sayfa henüz yüklenmemişse yükler
        private void InitializePageIfNeeded(string pageTag)
        {
            if (!pageCache.ContainsKey(pageTag))
            {
                iNKORE.UI.WPF.Modern.Controls.Page page = null;
                
                switch (pageTag)
                {
                    case "toolsPage":
                        page = new Pages.ToolsPage();
                        break;
                    case "debloatPage":
                        page = new Pages.DebloatPage();
                        break;
                    case "appInstallerPage":
                        page = new Pages.AppInstallerPage();
                        break;
                    case "privacyPage":
                        page = new Pages.PrivacyPage();
                        break;
                    case "telemetryPage":
                        page = new Pages.TelemetryPage();
                        break;
                    case "gameOptimizationPage":
                        page = new Pages.GameOptimizationPage();
                        break;
                    case "performancePage":
                        page = new Pages.PerformancePage();
                        break;
                    case "settings":
                        page = new Pages.SettingsPage();
                        break;
                    case "about":
                        page = new Pages.AboutPage();
                        break;
                }
                
                if (page != null)
                {
                    pageCache[pageTag] = page;
                }
            }
        }

        private void NavigationView_SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string tag = args.SelectedItemContainer.Tag.ToString();
                
                // Sayfa henüz oluşturulmamışsa, InitializePageIfNeeded metodunu kullanarak oluştur
                InitializePageIfNeeded(tag);
                
                // Sayfayı göster
                if (pageCache.TryGetValue(tag, out iNKORE.UI.WPF.Modern.Controls.Page page))
                {
                    ContentFrame.Navigate(page);
                }
            }
        }
    }
}