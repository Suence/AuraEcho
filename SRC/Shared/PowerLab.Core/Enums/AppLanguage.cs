using PowerLab.Core.Converters.TypeConverters;
using PowerLab.Core.Strings;
using System.ComponentModel;

namespace PowerLab.Core.Enums;

/// <summary>
/// 程序语言枚举
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum AppLanguage
{
    [Description(nameof(Labels.AppLanguage_zh_CN))]
    zh_CN,
    [Description(nameof(Labels.AppLanguage_en_US))]
    en_US
}
