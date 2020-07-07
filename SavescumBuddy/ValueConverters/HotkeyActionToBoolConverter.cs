using SavescumBuddy.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SavescumBuddy.ValueConverters
{
    public class HotkeyActionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => parameter is Enum enumVal && enumVal.Equals(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (HotkeyAction)parameter;
    }
}
