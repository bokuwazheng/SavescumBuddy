using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace SavescumBuddy.ValueConverters
{
    class InverseExistsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return File.Exists((string)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}