using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SavescumBuddy.ValueConverters
{
    public class UriToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) 
                return null;

            try
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(value.ToString());
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    return (object)bi;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
