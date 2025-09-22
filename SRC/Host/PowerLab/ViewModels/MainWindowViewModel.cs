using Prism.Mvvm;

namespace PowerLab.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #region private members
    private string _title = "PowerLab";
    #endregion

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public MainWindowViewModel()
    {

    }
}
