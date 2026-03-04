using AuraEcho.ExternalTools.Strings;
using Prism.Mvvm;
using System;
using System.Globalization;
using System.Threading;

namespace AuraEcho.ExternalTools.Utils;

public class ExternalToolsResources : BindableBase
{
    private static ExternalToolsResources _current;
    public static ExternalToolsResources Current
         => _current ??= new ExternalToolsResources();

    private ExternalToolsResources()
    {
        Strings = new ExternalToolsStrings();
    }

    public ExternalToolsStrings Strings { get; set; }
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
        ExternalToolsStrings.Culture = cultureInfo;

        Current?.RaisePropertyChanged(String.Empty);
    }

    public static string GetString(string name)
        => ExternalToolsStrings.ResourceManager.GetString(name, ExternalToolsStrings.Culture);
}
