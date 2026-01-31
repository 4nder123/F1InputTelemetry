using System.Windows.Input;
using System.Windows.Media;

namespace F1InputTelemetry.Core
{
    public sealed class HotkeyService : IDisposable
    {
        private readonly Action _onActivate;
        private readonly Action _onDeactivate;
        private bool _isActive;

        public HotkeyService(Action onActivate, Action onDeactivate)
        {
            _onActivate = onActivate;
            _onDeactivate = onDeactivate;
        }

        public void Start()
        {
            CompositionTarget.Rendering += OnFrame;
        }

        public void Stop()
        {
            CompositionTarget.Rendering -= OnFrame;
        }

        public void Dispose() => Stop();

        private void OnFrame(object? sender, EventArgs e)
        {
            bool hotkeyDown = Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift);

            if (hotkeyDown && !_isActive)
            {
                _isActive = true;
                _onActivate();
            }
            else if (!hotkeyDown && _isActive)
            {
                _isActive = false;
                _onDeactivate();
            }
        }
    }
}
