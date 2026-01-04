using System;
using System.Globalization;
using System.Windows.Data;
using PowerLab.Core.Constants;

namespace PowerLab.Converters;

public class FileIdToUrlConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Guid fileId)
        {
            return $"{Urls.ServerUrl}/api/file/download?fileId={fileId}";
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
