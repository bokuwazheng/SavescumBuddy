using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;

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

    //Use:
    //<Image x:Name="img">
    //    <Image.Source>
    //        <MultiBinding Converter = "{StaticResource UriToCachedResizedImageConverter}" >
    //            <Binding Path="PathToYourImageFile"/>
    //            <Binding Path="ActualHeight" ElementName="img"/>
    //            <Binding Path="ActualWidth" ElementName="img"/>
    //        </MultiBinding>
    //    </Image.Source>
    //</Image>
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
}
