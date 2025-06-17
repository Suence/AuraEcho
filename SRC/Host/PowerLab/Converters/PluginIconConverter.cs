using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using PowerLab.Core.Converters;
using PowerLab.Host.Core.Models;

namespace PowerLab.Converters
{
    public class PluginIconConverter : MarkupExtension, IValueConverter
    {
        public PluginIconConverter _instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not PluginRegistry pr) return null;

            var iconPath = new PathCombineConverter().Convert([pr.PluginFolder, pr.Manifest.Icon], null, null, null);
            var imageSource = new StringToImageSourceConverter().Convert(iconPath, null, null, null);
            return imageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => _instance ??= new PluginIconConverter();
    }
}
