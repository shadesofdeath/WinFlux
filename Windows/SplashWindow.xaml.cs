using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using WinFlux.Services;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace WinFlux.Windows
{
    /// <summary>
    /// SplashWindow için etkileşim mantığı
    /// </summary>
    public partial class SplashWindow : Window
    {
        private DispatcherTimer timer;
        private int totalSteps = 5;
        private int currentStep = 0;
        private double stepDuration = 600; // Her adım 600 ms sürsün, toplamda 3 saniye

        // Adımlarda gösterilecek mesajlar
        private string[] loadingMessages;
        
        // Tamamlanma olayı
        public event EventHandler LoadingComplete;

        public SplashWindow()
        {
            InitializeComponent();
            
            // Durum mesajlarını yükle
            loadingMessages = new string[totalSteps];
            for (int i = 0; i < totalSteps; i++)
            {
                string key = $"SplashScreen_Step{i+1}";
                loadingMessages[i] = FindResource(key) as string ?? $"Adım {i+1}";
            }
            
            // Pencere yüklendiğinde başlat
            this.Loaded += SplashWindow_Loaded;
        }

        private void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Logo için fade-in animasyonu
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            appLogo.Opacity = 0;
            appLogo.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            
            // Timer'ı başlat
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(stepDuration);
            timer.Tick += Timer_Tick;
            timer.Start();
            
            // İlk durum mesajını göster
            statusText.Text = loadingMessages[0];
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            currentStep++;
            
            // Progress değerini güncelle
            double progress = (double)currentStep / totalSteps * 100;
            
            // Yumuşak geçişli animasyon
            DoubleAnimation animation = new DoubleAnimation
            {
                To = progress,
                Duration = TimeSpan.FromMilliseconds(stepDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            loadingBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation);
            
            // Durum mesajını güncelle
            if (currentStep < loadingMessages.Length)
            {
                statusText.Text = loadingMessages[currentStep];
            }
            
            // Yükleme tamamlandı mı?
            if (currentStep >= totalSteps)
            {
                timer.Stop();
                
                // Güncellemeleri kontrol et
                CheckForUpdates();
            }
        }
        
        private async void CheckForUpdates()
        {
            try
            {
                // Güncelleme kontrol ediliyor mesajını göster
                statusText.Text = FindResource("UpdateNotification_CheckingUpdates") as string;
                
                // Güncelleme kontrolü yap
                var (hasUpdate, latestVersion, downloadUrl) = await UpdateService.CheckForUpdatesAsync();
                
                if (hasUpdate)
                {
                    // Kullanıcıya güncelleme bildirimi göster
                    ShowUpdateNotification(latestVersion, downloadUrl);
                }
                else
                {
                    // Güncelleme yoksa normal akışı devam ettir
                    CloseAndContinue();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda normal akışı devam ettir
                Console.WriteLine($"Error checking for updates: {ex.Message}");
                CloseAndContinue();
            }
        }
        
        private void ShowUpdateNotification(string latestVersion, string downloadUrl)
        {
            string title = FindResource("UpdateNotification_Title") as string;
            string message = string.Format(FindResource("UpdateNotification_Message") as string, latestVersion);
            
            // Kullanıcıya güncelleme bildirimini göster
            MessageBoxResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
                
            if (result == MessageBoxResult.Yes)
            {
                // Tarayıcıda indirme sayfasını aç
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadUrl,
                        UseShellExecute = true
                    });
                    
                    // Uygulama kapatılacak, kullanıcı yeni sürümü indirecek
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error opening download URL: {ex.Message}");
                    CloseAndContinue();
                }
            }
            else
            {
                // Kullanıcı güncellemeyi istemiyorsa normal akışı devam ettir
                CloseAndContinue();
            }
        }
        
        private void CloseAndContinue()
        {
            // Kısa bir gecikme ve pencereyi kapat
            var closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromMilliseconds(500);
            closeTimer.Tick += (s, args) =>
            {
                closeTimer.Stop();
                LoadingComplete?.Invoke(this, EventArgs.Empty);
                this.Close();
            };
            closeTimer.Start();
        }
    }
} 