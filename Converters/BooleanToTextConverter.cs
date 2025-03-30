using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WinFlux.Converters
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            string[] resourceKeys = ((string)parameter).Split('|');
            
            string key = isChecked ? resourceKeys[1] : resourceKeys[0];
            return Application.Current.TryFindResource(key) ?? key;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
