using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace TelemetryUI
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        private const int SecondsOfTelemetry = 5;
        private int MaxSamples;

        private readonly ObservableCollection<float> gasData = new();
        private readonly ObservableCollection<float> brakeData = new();

        public MainWindow()
        {
            InitializeComponent();
            Settings settings = Settings.Load();
            MaxSamples = SecondsOfTelemetry * settings.SendRate;
            SetupWindow(settings);
            _ = F1UDPListener.StartListener(this, settings, cts.Token);
        }

        public void SetupWindow(Settings settings)
        {
            if (settings.AutoHide) HideWindow();
            if (!settings.ShowClutch) clutch.Visibility = Visibility.Collapsed;
            Height = Math.Ceiling(Height * settings.WindowScale);
            Width = Math.Ceiling(Width * settings.WindowScale);
            Left = Math.Max(settings.WindowX - (Width / 2), 0);
            Top = Math.Max(settings.WindowY - (Height / 2), 0);
        }

        public void HideWindow() 
        {
            Opacity = 0;
        }
        public void ShowWindow()
        {
            Opacity = 1;
        }

        public void Update(float gas, float brake, float clutch, float steering)
        {
            AddSample(gasData, gas);
            AddSample(brakeData, brake);

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
        private void AddSample(ObservableCollection<float> collection, float value)
        {
            if (collection.Count >= MaxSamples)
                collection.RemoveAt(0);
            collection.Add(value);
        }

        private void DrawTelemetry()
        {
            TelemetryCanvas.Children.Clear();
            double width = TelemetryCanvas.ActualWidth;
            double height = TelemetryCanvas.ActualHeight;
            double stepX = width / MaxSamples;

            DrawGraph(gasData, Colors.LimeGreen, stepX, height);
            DrawGraph(brakeData, Colors.Red, stepX, height);
        }

        private void DrawGraph(ObservableCollection<float> data, Color color, double stepX, double height)
        {
            if (data.Count < 2 || height == 0 || double.IsNaN(height)) return;

            var geometry = new StreamGeometry();
            using var ctx = geometry.Open();

            // Clamp all values first to avoid out-of-bounds
            List<Point> points = new();
            for (int i = 0; i < data.Count; i++)
            {
                double x = i * stepX;
                double clampedValue = Math.Clamp(data[i], 0.0f, 1.0f);
                double y = height * (1 - clampedValue);
                points.Add(new Point(x, y));
            }

            // Draw straight lines between points
            ctx.BeginFigure(points[0], false, false);
            for (int i = 1; i < points.Count; i++)
            {
                ctx.LineTo(points[i], true, false);
            }

            var path = new Path
            {
                Data = geometry,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round,
            };

            TelemetryCanvas.Children.Add(path);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            cts.Cancel();
            base.OnClosing(e);
        }

    }
}
