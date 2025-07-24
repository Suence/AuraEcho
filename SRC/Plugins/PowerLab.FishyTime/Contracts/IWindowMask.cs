using System;

namespace PowerLab.FishyTime.Contracts
{
    public interface IWindowMask
    {
        public nint Handle { get; }
        public event Action MaskClosed;
        public void Show();
        public void Close();
    }
}
