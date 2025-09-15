using Prism.Mvvm;
using Prism.Navigation;

namespace PowerLab.UIToolkit.Mvvm
{
    public abstract class ViewModelBase : BindableBase, IDestructible
    {
        protected ViewModelBase()
        {

        }

        public virtual void Destroy()
        {

        }
    }
}
