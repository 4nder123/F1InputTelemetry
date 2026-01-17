using F1InputTelemetry.Telemetry;
using F1InputTelemetry.Windows;
using System.Windows;

namespace F1InputTelemetry;

public partial class App : Application
{
    private CancellationTokenSource? _cts;
    private TelemetryHub _hub = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = Settings.Load();
        
        var InputTelemetryWindow = new InputTelemetryWindow(settings);
        InputTelemetryWindow.WireUp(_hub);
        InputTelemetryWindow.Show();

        _cts = new CancellationTokenSource();
        _ = F1UDPListener.StartListener(_hub, settings, _cts.Token);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        base.OnExit(e);
    }
}

