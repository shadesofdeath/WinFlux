using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WinFlux.Pages;

namespace WinFlux.Converters
{
    public class AppCategoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppCategory category)
            {
                return category switch
                {
                    AppCategory.SystemCritical => new SolidColorBrush(Color.FromRgb(220, 53, 69)),     // Kırmızı - Çok kritik, silme!
                    AppCategory.SystemCore => new SolidColorBrush(Color.FromRgb(255, 128, 0)),         // Turuncu - Temel bileşenler
                    AppCategory.Driver => new SolidColorBrush(Color.FromRgb(138, 43, 226)),            // Mor - Sürücüler
                    AppCategory.Language => new SolidColorBrush(Color.FromRgb(255, 105, 180)),         // Pembe - Dil paketleri
                    AppCategory.Store => new SolidColorBrush(Color.FromRgb(0, 123, 255)),             // Mavi - Store uygulamaları
                    AppCategory.Gaming => new SolidColorBrush(Color.FromRgb(40, 167, 69)),            // Yeşil - Oyun uygulamaları
                    AppCategory.DefaultApps => new SolidColorBrush(Color.FromRgb(98, 203, 102)),      // Açık Yeşil - Varsayılan uygulamalar
                    AppCategory.Extension => new SolidColorBrush(Color.FromRgb(91, 192, 222)),        // Açık Mavi - Eklentiler
                    AppCategory.Optional => new SolidColorBrush(Color.FromRgb(108, 117, 125)),        // Gri - İsteğe bağlı uygulamalar
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}