using F1InputTelemetry.Settings;
using F1InputTelemetry.Settings.Overlay.Base;
using F1InputTelemetry.Telemetry;
using System.Windows;
using System.Windows.Input;

namespace F1InputTelemetry.Windows.Base
{
    public abstract class OverlayWindow: Window
    {
        public const int SnapDistance = 20;

        private bool _isDragging = false;
        private Point _dragStart;
        private bool _isMoveEnabled = false;
        private double _currentOpacity = 1.0;
        private OverlayBaseSettings OverlayBaseSettings;

        public OverlayWindow(OverlayBaseSettings settings)
        {
            OverlayBaseSettings = settings;
        }

        protected void InitializeOverlay()
        {
            Height = Math.Ceiling(Height * OverlayBaseSettings.WindowScale);
            Width = Math.Ceiling(Width * OverlayBaseSettings.WindowScale);
            Left = Math.Max(OverlayBaseSettings.WindowX - Width / 2, 0);
            Top = Math.Max(OverlayBaseSettings.WindowY - Height / 2, 0);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e); 
            if (!_isMoveEnabled) return;
            _isDragging = true; 
            _dragStart = e.GetPosition(this); 
            CaptureMouse();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging && _isMoveEnabled)
            {
                var mouse = PointToScreen(e.GetPosition(this));

                double targetX = mouse.X - _dragStart.X; 
                double targetY = mouse.Y - _dragStart.Y;

                Left = SnapToVerticalCenter(targetX);
                Top = targetY;
            }
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                SavePosition();
            }
        }

        private double SnapToVerticalCenter(double targetX)
        {
            double screenCenterX = SystemParameters.PrimaryScreenWidth / 2;
            double windowCenter = Width / 2;

            if (Math.Abs(screenCenterX - (targetX + windowCenter)) < SnapDistance)
            {
                return screenCenterX - windowCenter;
            }
            return targetX;
        }
        public void SavePosition()
        {
            OverlayBaseSettings.WindowX = (int)(Left + Width / 2);
            OverlayBaseSettings.WindowY = (int)(Top + Height / 2);
            AppSettings.Update();
        }
        public abstract void WireUp(ITelemetryHub hub);

        public void EnableMove()
        {
            if (Opacity < 1 || !_isMoveEnabled)
            {
                _currentOpacity = Opacity;
                Opacity = 1;
            }
            _isMoveEnabled = true;
        }

        public void DisableMove()
        {
            _isMoveEnabled = false;
            Opacity = _currentOpacity;
        }

        public void HideWindow() 
        { 
            _currentOpacity = 0;
            Opacity = 0; 
        }
        public void ShowWindow() 
        { 
            _currentOpacity = 1;
            Opacity = 1; 
        }
    }
}
