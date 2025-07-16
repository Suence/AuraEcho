using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace PowerLab.FishyTime.Converters
{
    public class IconToImageSourceConverter : MarkupExtension, IValueConverter
    {
        private static IconToImageSourceConverter _instance;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Icon icon) return null;

            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => _instance ??= new IconToImageSourceConverter();
    }
}
