using Prism.Mvvm;

namespace PluginInstaller.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region private members
        private string _title = "PlixInstaller";
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
}
