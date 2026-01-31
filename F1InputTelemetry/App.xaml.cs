using F1InputTelemetry.Core;
using F1InputTelemetry.Settings;
using F1InputTelemetry.Telemetry;
using F1InputTelemetry.Windows;
using System.Windows;

namespace F1InputTelemetry;

public partial class App : Application
{
    private CancellationTokenSource? _cts;
    private readonly TelemetryHub _hub = new();
    private readonly WindowManager _windowManager = new();
    private HotkeyService? _hotkeyService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = AppSettings.Load();

        RegisterWindows(settings);
        StartTelemetryListener(settings);
        StartHotkeyService();
    }

    private void RegisterWindows(AppSettings settings)
    {
        _windowManager.Register(
            () => new InputTelemetryWindow(settings.InputTelemetry, settings.SendRate),
            settings.InputTelemetry.Enabled,
            _hub);

        _windowManager.Register(
            () => new RadarWindow(settings.Radar),
            settings.Radar.Enabled,
            _hub);
    }

    private void StartTelemetryListener(AppSettings settings)
    {
        _cts = new CancellationTokenSource();
        _ = F1UDPListener.StartListener(_hub, settings, _cts.Token);
    }

    private void StartHotkeyService()
    {
        _hotkeyService = new HotkeyService(
            onActivate: _windowManager.EnableMoveAll,
            onDeactivate: _windowManager.DisableMoveAll);
        _hotkeyService.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyService?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
        base.OnExit(e);
    }
}