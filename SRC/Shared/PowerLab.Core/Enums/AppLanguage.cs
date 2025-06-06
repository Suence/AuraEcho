using System.ComponentModel;
using PowerLab.Core.Converters.TypeConverters;
using PowerLab.Core.Strings;

namespace PowerLab.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AppLanguage
    {
        [Description(nameof(Labels.AppLanguage_zh_CN))]
        zh_CN,
        [Description(nameof(Labels.AppLanguage_en_US))]
        en_US
    }
}
