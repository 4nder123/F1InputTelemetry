using F1InputTelemetry.Settings.Overlay;
using F1InputTelemetry.Telemetry;
using F1InputTelemetry.Windows.Base;
using F1UDP.Enums;
using F1UDP.Structs;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace F1InputTelemetry.Windows
{
    public partial class InputTelemetryWindow : OverlayWindow
    {
        private const int SecondsOfTelemetry = 5;
        private byte SpectatorCarIndex = 255;
        private int MaxSamples;

        private float[] GasData;
        private float[] BrakeData;
        private int SampleIndex = 0;
        private int SampleCount = 0;

        private InputTelemetrySettings Settings;

        private readonly Path gasPath = new()
        {
            Stroke = Brushes.LimeGreen,
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round,
        };

        private readonly Path brakePath = new()
        {
            Stroke = Brushes.Red,
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round,
        };

        public InputTelemetryWindow(InputTelemetrySettings settings, int SendRate) : base(settings)
        {
            InitializeComponent();
            InitializeOverlay();

            MaxSamples = SecondsOfTelemetry * SendRate;
            GasData = new float[MaxSamples];
            BrakeData = new float[MaxSamples];

            TelemetryCanvas.Children.Add(gasPath);
            TelemetryCanvas.Children.Add(brakePath);
            Settings = settings;

            ConfigureWindow();
        }

        private void ConfigureWindow()
        {
            if (Settings.AutoHide) HideWindow();
            if (!Settings.ShowClutch) clutch.Visibility = Visibility.Collapsed;
        }
        public override void WireUp(ITelemetryHub hub)
        {
            hub.CarTelemetryReceived += OnCarTelemetry;
            hub.SessionReceived += OnSessionData;
            if (Settings.AutoHide) hub.EventReceived += OnEvent;
        }
        private void OnCarTelemetry(PacketCarTelemetryData telemetry)
        {
            var playerIndex = telemetry.Header.PlayerCarIndex;
            if (SpectatorCarIndex != 255) playerIndex = SpectatorCarIndex;
            if (playerIndex == 255 || playerIndex > telemetry.Cars.Length) return;

            var playerData = telemetry.Cars[playerIndex];
            Dispatcher.InvokeAsync(() =>
            {
                Update(playerData.Throttle, playerData.Brake, playerData.Clutch, playerData.Steer);
            });
        }
        private void OnSessionData(PacketSessionData session)
        {
            SpectatorCarIndex = session.SpectatorCarIndex;
        }
        private void OnEvent(PacketEventData evt)
        {
            if (evt.EventType == EventType.SessionStarted) Dispatcher.InvokeAsync(ShowWindow);
            if (evt.EventType == EventType.SessionEnded) Dispatcher.InvokeAsync(HideWindow);
        }
        private void Update(float gas, float brake, float clutch, float steering)
        {
            AddSample(gas, brake);
            DrawTelemetry();

            int barHeight = 50;
            //Cluth is given in a range of 0 to 100.
            ClutchFill.Height = clutch / 2;
            GasFill.Height = gas * barHeight;
            BrakeFill.Height = brake * barHeight;
            SetSteering(steering);

            ClutchPercentText.Text = $"{(int)(clutch)}";
            GasPercentText.Text = $"{(int)(gas * 100)}";
            BrakePercentText.Text = $"{(int)(brake * 100)}";
        }

        private void SetSteering(float angle)
        {
            int offset = 45;
            SteeringDotRotation.Angle = (180 * angle) + offset;
        }
        private void AddSample(float gas, float brake)
        {
            GasData[SampleIndex] = Math.Clamp(gas, 0f, 1f);
            BrakeData[SampleIndex] = Math.Clamp(brake, 0f, 1f);
            SampleIndex = (SampleIndex + 1) % MaxSamples;
            if (SampleCount < MaxSamples) SampleCount++;
        }

        private void DrawTelemetry()
        {
            double width = TelemetryCanvas.ActualWidth;
            double height = TelemetryCanvas.ActualHeight;
            double stepX = width / MaxSamples;

            gasPath.Data = CreateGraphGeometry(GasData, stepX, height);
            brakePath.Data = CreateGraphGeometry(BrakeData, stepX, height);
        }

        private StreamGeometry? CreateGraphGeometry(float[] data, double stepX, double height)
        {
            if (SampleCount < 2 || height == 0 || double.IsNaN(height))
                return null;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                int startIndex = SampleCount < MaxSamples ? 0 : SampleIndex;                
                for (int i = 0; i < SampleCount; i++)
                {
                    int index = (startIndex + i) % MaxSamples;
                    double x = i * stepX;
                    double y = height * (1 - data[index]);

                    if (i == 0)
                        ctx.BeginFigure(new Point(x, y), false, false);
                    else
                        ctx.LineTo(new Point(x, y), true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }
    }
}
