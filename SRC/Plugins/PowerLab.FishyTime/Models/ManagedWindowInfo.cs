using System;
using System.Windows;
using Prism.Mvvm;

using Drawing = System.Drawing;
namespace PowerLab.FishyTime.Models
{
    public class ManagedWindowInfo : BindableBase
    {
        #region private members
        private string _name;
        private double _width;
        private double _height;
        private Point _position;
        private IntPtr _handle;
        private double _opacity;
        private bool _isTopmost;
        private ShowWindowCommands _windowState;
        private bool _isMouseOver;
        private bool _isVisible = true;
        private Drawing::Icon _icon;
        private bool _isMasked = false;
        #endregion



        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Drawing::Icon Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public Point Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public bool IsMasked
        {
            get => _isMasked;
            set => SetProperty(ref _isMasked, value);
        }

        public IntPtr Handle
        {
            get => _handle;
            set => SetProperty(ref _handle, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public bool IsTopmost
        {
            get => _isTopmost;
            set => SetProperty(ref _isTopmost, value);
        }

        public bool IsMouseOver
        {
            get => _isMouseOver;
            set => SetProperty(ref _isMouseOver, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ShowWindowCommands WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        public Rect Bounds => new(Position, new Size(Width, Height));

        public bool CanSetOpacity { get; init; }

    }
}
