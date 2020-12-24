using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DropZone.Converters
{
    internal class IsLeftToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var margin = parameter == null ? 0 : int.Parse(parameter.ToString());
            return value != null && (bool)value
                ? new Thickness(0, 0, margin, 0)
                : new Thickness(margin, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}