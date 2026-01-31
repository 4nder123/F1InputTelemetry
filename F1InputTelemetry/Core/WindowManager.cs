using F1InputTelemetry.Telemetry;
using F1InputTelemetry.Windows.Base;

public class WindowManager
{
    private readonly List<OverlayWindow> _windows = new();
    public void Register(Func<OverlayWindow> factory, bool enabled, ITelemetryHub hub)
    {
        if (!enabled)
            return;

        var window = factory();
        window.WireUp(hub);
        window.Show();

        _windows.Add(window);
    }
    public void EnableMoveAll()
    {
        foreach (var w in _windows)
            w.EnableMove();
    }
    public void DisableMoveAll()
    {
        foreach (var w in _windows)
            w.DisableMove();
    }
}
