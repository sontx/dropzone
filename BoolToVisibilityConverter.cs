using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DropZone
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parts = parameter?.ToString().Split(',');
            if (parts == null || parts.Length != 2)
                return null;
            return value != null && (bool)value
                ? Enum.Parse(typeof(Visibility), parts[0])
                : Enum.Parse(typeof(Visibility), parts[1]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}