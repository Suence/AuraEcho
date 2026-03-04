using AuraEcho.FishyTime.Strings;
using Prism.Mvvm;
using System;
using System.Globalization;
using System.Threading;

namespace AuraEcho.FishyTime.Utils;

public class FishyTimeResources : BindableBase
{
    private static FishyTimeResources _current;
    public static FishyTimeResources Current
         => _current ??= new FishyTimeResources();

    private FishyTimeResources()
    {
        Strings = new FishyTimeStrings();
    }

    public FishyTimeStrings Strings { get; set; }
    private string _language;
    /// <summary>
    /// 获取或设置 Language 的值
    /// </summary>
    public string Language
    {
        get => _language;
        set
        {
            if (_language == value)
                return;

            _language = value;
            ChangeCulture(new CultureInfo(value));
        }
    }

    public static void ChangeCulture(CultureInfo cultureInfo)
    {
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        FishyTimeStrings.Culture = cultureInfo;

        Current?.RaisePropertyChanged(String.Empty);
    }

    public static string GetString(string name)
        => FishyTimeStrings.ResourceManager.GetString(name, FishyTimeStrings.Culture);
}
