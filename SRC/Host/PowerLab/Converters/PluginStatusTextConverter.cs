using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using PowerLab.Core.Models;

namespace PowerLab.Converters
{
    /// <summary>
    /// 插件状态文本转换器
    /// </summary>
    public class PluginStatusTextConverter : MarkupExtension, IMultiValueConverter
    {
        private PluginStatusTextConverter _instance;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return "未知状态";

            if (values[0] is PluginStatus status && values[1] is PluginPlanStatus planStatus)
            {
                return planStatus switch
                {
                    PluginPlanStatus.EnablePending => "启用挂起",
                    PluginPlanStatus.DisablePending => "禁用挂起",
                    PluginPlanStatus.UninstallPending => "卸载挂起",
                    _ => status switch
                    {
                        PluginStatus.Enabled => "已启用",
                        PluginStatus.Disabled => "已禁用",
                        _ => "未知状态"
                    }
                };
            }

            return "状态错误";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => _instance ??= new PluginStatusTextConverter();
    }

}
