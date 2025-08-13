namespace PowerLab.Core.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media.Imaging;

    public class StringToImageSourceConverter : MarkupExtension, IValueConverter
    {
        public StringToImageSourceConverter _instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
                return null!;

            try
            {
                // 自动加 file:// 前缀（如果需要）
                Uri uri;
                if (Uri.TryCreate(path, UriKind.Absolute, out uri) && uri.IsFile && File.Exists(uri.LocalPath))
                {
                    return new BitmapImage(uri);
                }

                // 如果是纯文件路径
                if (File.Exists(path))
                {
                    return new BitmapImage(new Uri(path, UriKind.Absolute));
                }
            }
            catch
            {
                // 可选：记录日志或返回默认图像
            }

            return null!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("StringToImageSourceConverter does not support ConvertBack.");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => _instance ??= new StringToImageSourceConverter();
    }

}
