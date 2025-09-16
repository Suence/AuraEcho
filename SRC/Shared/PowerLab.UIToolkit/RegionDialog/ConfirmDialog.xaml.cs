using System.Windows.Controls;
using Prism.Ioc;

namespace PowerLab.UIToolkit.ContentDialog
{
    /// <summary>
    /// ConfirmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmDialog : UserControl
    {
        public ConfirmDialog(IContainerProvider containerProvider)
        {
            InitializeComponent();
            DataContext = containerProvider.Resolve<ConfirmDialogViewModel>();
        }
    }
}
