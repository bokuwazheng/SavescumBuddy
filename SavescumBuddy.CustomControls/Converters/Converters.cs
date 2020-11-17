using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace SavescumBuddy.CustomControls.Converters
{
    public class ExistsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !File.Exists(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class EnumEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => parameter is Enum enumValue && enumValue.Equals(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value?.Equals(true) == true ? parameter : DependencyProperty.UnsetValue;
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is int val ? val != 0 : (bool?)null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool val ? val ? 1 : 0 : (int?)null;
    }

    public class LongToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new DateTime((long)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool val && !val;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool val && !val;
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool val && val ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseExistsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => File.Exists((string)value) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

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

    public class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is object;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class UriToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            try
            {
                if (value is string val && !string.IsNullOrEmpty(val))
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(val);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    //bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                    bi.EndInit();
                    return bi;
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

    ///Use:
    ///<Image x:Name="img">
    ///    <Image.Source>
    ///        <MultiBinding Converter = "{StaticResource UriToCachedResizedImageConverter}" >
    ///            <Binding Path="PathToYourImageFile"/>
    ///            <Binding Path="ActualHeight" ElementName="img"/>
    ///            <Binding Path="ActualWidth" ElementName="img"/>
    ///        </MultiBinding>
    ///    </Image.Source>
    ///</Image>
    public class UriToCachedResizedImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null)
                return null;

            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue ||
                values[2] == DependencyProperty.UnsetValue)
                return null;

            var xres = System.Convert.ToInt32(values[1]);
            var yres = System.Convert.ToInt32(values[2]);

            try
            {
                if (values[0] is string imagePath && !string.IsNullOrEmpty(imagePath))
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(imagePath);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                    bi.DecodePixelHeight = xres;
                    bi.DecodePixelWidth = yres;
                    bi.EndInit();
                    return bi;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class StringIsNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string stringVal
            ? string.IsNullOrWhiteSpace(stringVal)
            : throw new ArgumentException();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
