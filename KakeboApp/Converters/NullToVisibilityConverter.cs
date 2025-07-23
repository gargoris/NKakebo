using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace KakeboApp.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(value == null || (value is string s && string.IsNullOrWhiteSpace(s)));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
