using System;

namespace SavescumBuddy.ValueConverters
{
    class IntToBoolConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is int)
            {
                var val = (int)value;
                return (val == 0) ? false : true;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is bool)
            {
                var val = (bool)value;
                return val ? 1 : 0;
            }
            else
            {
                return null;
            }
        }
    }
}
