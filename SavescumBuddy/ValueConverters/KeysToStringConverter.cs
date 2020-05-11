using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace SavescumBuddy.ValueConverters
{
    public class KeysToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
                return null;

            var key = (Keys)values[0];
            var mod = (Keys)values[1];

            if (mod == Keys.None)
                return key.ToString().ToUpper();

            return mod.ToString().ToUpper() + " + " + key.ToString().ToUpper();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
