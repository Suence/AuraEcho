using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PowerLab.Core.Enums;

namespace PowerLab.Core.Converters
{
    [ValueConversion(typeof(MessageSeverity), typeof(Brush))]
    public class MessageSeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            switch((MessageSeverity)value)
            {
                case MessageSeverity.Tips: return new SolidColorBrush(Color.FromRgb(8, 82, 156));
                case MessageSeverity.Warnning: return new SolidColorBrush(Color.FromRgb(184, 86, 0));
                case MessageSeverity.Error: return new SolidColorBrush(Color.FromRgb(160, 0, 0));
                case MessageSeverity.Succeeded: return new SolidColorBrush(Color.FromRgb(8, 112, 32));
                case MessageSeverity messageSeverity: throw new ArgumentException($"意外的枚举值：{messageSeverity}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
