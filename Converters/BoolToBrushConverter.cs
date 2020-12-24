using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DropZone.Converters
{
    internal class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parts = parameter?.ToString().Split(',');
            if (parts == null || parts.Length != 2)
                return null;

            var converter = new BrushConverter();

            return value != null && (bool) value
                ? converter.ConvertFrom(parts[0])
                : converter.ConvertFrom(parts[1]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}