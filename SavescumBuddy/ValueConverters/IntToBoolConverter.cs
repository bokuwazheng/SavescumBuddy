using System;
using System.Globalization;
using System.Windows.Data;

namespace SavescumBuddy.ValueConverters
{
    class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is int val ? val != 0 : (bool?)null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool val ? val ? 1 : 0 : (int?)null;
    }
}
