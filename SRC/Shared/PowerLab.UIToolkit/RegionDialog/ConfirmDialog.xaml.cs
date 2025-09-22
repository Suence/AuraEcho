using Prism.Ioc;
using System.Windows.Controls;

namespace PowerLab.UIToolkit.RegionDialog;

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
