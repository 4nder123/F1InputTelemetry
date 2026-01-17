using F1InputTelemetry.Telemetry;
using F1UDP.Enums;
using F1UDP.Structs;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace F1InputTelemetry.Windows
{
    public partial class InputTelemetryWindow : Window
    {
        private const int SecondsOfTelemetry = 5;
        private byte spectatorCarIndex = 255;
        private int MaxSamples;

        private float[] gasData;
        private float[] brakeData;
        private int sampleIndex = 0;
        private int sampleCount = 0;

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

        public InputTelemetryWindow(Settings settings)
        {
            InitializeComponent();
            MaxSamples = SecondsOfTelemetry * settings.SendRate;
            gasData = new float[MaxSamples];
            brakeData = new float[MaxSamples];

            TelemetryCanvas.Children.Add(gasPath);
            TelemetryCanvas.Children.Add(brakePath);

            SetupWindow(settings);
        }

        private void SetupWindow(Settings settings)
        {
            if (settings.AutoHide) HideWindow();
            if (!settings.ShowClutch) clutch.Visibility = Visibility.Collapsed;
            Height = Math.Ceiling(Height * settings.WindowScale);
            Width = Math.Ceiling(Width * settings.WindowScale);
            Left = Math.Max(settings.WindowX - (Width / 2), 0);
            Top = Math.Max(settings.WindowY - (Height / 2), 0);
        }
        public void WireUp(ITelemetryHub hub)
        {
            hub.CarTelemetryReceived += OnCarTelemetry;
            hub.SessionReceived += OnSessionData;
            hub.EventReceived += OnEvent;
        }
        private void OnCarTelemetry(PacketCarTelemetryData telemetry)
        {
            var playerIndex = telemetry.Header.PlayerCarIndex;
            if (spectatorCarIndex != 255) playerIndex = spectatorCarIndex;
            if (playerIndex == 255) return;

            var playerData = telemetry.Cars[playerIndex];
            Dispatcher.InvokeAsync(() =>
            {
                Update(playerData.Throttle, playerData.Brake, playerData.Clutch, playerData.Steer);
            });
        }
        private void OnSessionData(PacketSessionData session)
        {
            spectatorCarIndex = session.SpectatorCarIndex;
        }
        private void OnEvent(PacketEventData evt)
        {
            if (evt.EventType == EventType.SessionStarted) Dispatcher.InvokeAsync(ShowWindow);
            if (evt.EventType == EventType.SessionEnded) Dispatcher.InvokeAsync(HideWindow);
        }

        private void HideWindow() => Opacity = 0;
        private void ShowWindow() => Opacity = 1;
        

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
            gasData[sampleIndex] = Math.Clamp(gas, 0f, 1f);
            brakeData[sampleIndex] = Math.Clamp(brake, 0f, 1f);
            sampleIndex = (sampleIndex + 1) % MaxSamples;
            if (sampleCount < MaxSamples) sampleCount++;
        }

        private void DrawTelemetry()
        {
            double width = TelemetryCanvas.ActualWidth;
            double height = TelemetryCanvas.ActualHeight;
            double stepX = width / MaxSamples;

            gasPath.Data = CreateGraphGeometry(gasData, stepX, height);
            brakePath.Data = CreateGraphGeometry(brakeData, stepX, height);
        }

        private StreamGeometry? CreateGraphGeometry(float[] data, double stepX, double height)
        {
            if (sampleCount < 2 || height == 0 || double.IsNaN(height))
                return null;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                int startIndex = sampleCount < MaxSamples ? 0 : sampleIndex;                
                for (int i = 0; i < sampleCount; i++)
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
