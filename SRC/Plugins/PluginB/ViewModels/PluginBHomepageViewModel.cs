using Prism.Mvvm;

namespace PluginB.ViewModels
{
    public class PluginBHomepageViewModel : BindableBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public PluginBHomepageViewModel()
        {
            Message = "PluginB default view";
        }
    }
}
