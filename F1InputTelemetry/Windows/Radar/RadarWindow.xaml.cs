using F1InputTelemetry.Settings.Overlay;
using F1InputTelemetry.Telemetry;
using F1InputTelemetry.Windows.Base;
using F1UDP.Enums;
using F1UDP.Structs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace F1InputTelemetry.Windows
{
    public partial class RadarWindow : OverlayWindow
    {
        private const int RadarSize = 300;
        private const double FadeDistance = 25.0;
        private const double WorldSize = 50.0;
        private const double Scale = RadarSize / WorldSize;
        private const double CloseProximityThreshold = 5.0;
        private const int MaxCars = 22;

        private static readonly (double Width, double Length) F1CarDimensions = (2.0, 5.63);
        private static readonly (double Width, double Length) F2CarDimensions = (1.9, 5.22);
        private readonly record struct CarPosition(
            double RadarX,
            double RadarY,
            double Distance,
            double Angle,
            bool InLeftSector,
            bool InRightSector);

        private struct RadarState
        {
            public bool LeftHasCar;
            public bool RightHasCar;
            public bool LeftClose;
            public bool RightClose;
            public double MaxOpacity;
        }

        private readonly bool[] ValidCars = new bool[MaxCars];
        private readonly Rectangle[] CarsRectangles = new Rectangle[MaxCars];
        private byte SpectatorCarIndex = 255;
        private bool IsAllowedSession = true;
        private double CarWidth = F1CarDimensions.Width;
        private double CarLength = F1CarDimensions.Length;

        public RadarWindow(RadarSettings settings) : base(settings)
        {
            InitializeComponent();
            InitializeOverlay();
            InitializeCars();
        }

        private void InitializeCars()
        {
            double widthPx = CarWidth * Scale;
            double heightPx = CarLength * Scale;

            for (int i = 0; i < MaxCars; i++)
            {
                var car = new Rectangle
                {
                    Width = widthPx,
                    Height = heightPx,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    RadiusX = 3,
                    RadiusY = 3,
                    Opacity = 0,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new RotateTransform(0),
                    Visibility = Visibility.Collapsed
                };
                CarsRectangles[i] = car;
                Cars.Children.Add(car);
            }
        }

        public override void WireUp(ITelemetryHub hub)
        {
            hub.MotionReceived += OnMotionReceived;
            hub.SessionReceived += OnSessionData;
            hub.EventReceived += OnEvent;
            hub.LapReceived += OnLapData;
        }

        private void OnLapData(PacketLapData lap)
        {
            for (int i = 0; i < lap.Cars.Length; i++)
            {
                var status = lap.Cars[i].ResultStatus;
                ValidCars[i] = status == ResultType.Active || status == ResultType.Finished;
            }
        }

        private void OnSessionData(PacketSessionData session)
        {
            SpectatorCarIndex = session.SpectatorCarIndex;
            UpdateCarDimensions(session.Formula);
            UpdateSessionAllowedState(session.SessionType);
        }

        private void OnEvent(PacketEventData evt)
        {
            if (evt.EventType == EventType.SessionEnded)
            {
                Dispatcher.InvokeAsync(HideWindow);
            }
        }

        private void OnMotionReceived(PacketMotionData data)
        {
            if (!IsAllowedSession)
                return;

            var refCarIndex = GetReferenceCarIndex(data.Header.PlayerCarIndex);
            if (refCarIndex == 255 || refCarIndex > data.Cars.Length)
                return;

            var refCar = data.Cars[refCarIndex];
            Dispatcher.InvokeAsync(() => UpdateRadar(data, refCarIndex, refCar));
        }


        private void UpdateCarDimensions(FormulaType formula)
        {
            var dimensions = formula == FormulaType.F2 ? F2CarDimensions : F1CarDimensions;
            if (CarWidth == dimensions.Width && CarLength == dimensions.Length)
                return;
            CarWidth = dimensions.Width;
            CarLength = dimensions.Length;
        }

        private void UpdateSessionAllowedState(SessionType sessionType)
        {
            switch (sessionType)
            {
                case SessionType.OneShotQualifying:
                case SessionType.OneShotSprintShootout:
                case SessionType.TimeTrial:
                    if (!IsAllowedSession) break;
                    IsAllowedSession = false;
                    Dispatcher.InvokeAsync(HideWindow);
                    break;
                case SessionType.Unknown:
                    break;
                default:
                    IsAllowedSession = true;
                    break;
            }
        }

        private byte GetReferenceCarIndex(byte playerCarIndex)
        {
            return SpectatorCarIndex != 255 ? SpectatorCarIndex : playerCarIndex;
        }


        private void UpdateRadar(PacketMotionData data, byte refCarIndex, CarMotionData refCar)
        {
            var radarState = new RadarState();
            double center = RadarSize / 2.0;
            double maxRadius = center;
            double fadeStart = maxRadius - FadeDistance;

            double widthPx = CarWidth * Scale;
            double heightPx = CarLength * Scale;

            for (int i = 0; i < data.Cars.Length; i++)
            {
                var carRect = CarsRectangles[i];

                if (!ValidCars[i])
                {
                    carRect.Visibility = Visibility.Collapsed;
                    continue;
                }

                var carPosition = CalculateCarPosition(data.Cars[i], refCar, center);

                if (carPosition.Distance > maxRadius)
                {
                    carRect.Visibility = Visibility.Collapsed;
                    continue;
                }

                bool isReferenceCar = i == refCarIndex;
                UpdateSectorState(ref radarState ,carPosition, isReferenceCar);

                double opacity = CalculateOpacity(carPosition.Distance, maxRadius, fadeStart);
                if (!isReferenceCar && opacity > radarState.MaxOpacity)
                {
                    radarState.MaxOpacity = opacity;
                }

                double relativeYaw = CalculateRelativeYaw(data.Cars[i].Yaw, refCar.Yaw);
                carRect.Width = widthPx;
                carRect.Height = heightPx;
                carRect.Fill = isReferenceCar ? Brushes.LimeGreen : Brushes.White;
                carRect.Opacity = opacity;
                carRect.Visibility = Visibility.Visible;
                ((RotateTransform)carRect.RenderTransform).Angle = relativeYaw;

                Canvas.SetLeft(carRect, carPosition.RadarX - widthPx / 2);
                Canvas.SetTop(carRect, carPosition.RadarY - heightPx / 2);
            }

            ApplyRadarState(ref radarState);
        }


        private CarPosition CalculateCarPosition(CarMotionData car, CarMotionData refCar, double center)
        {
            float dx = car.WorldPositionX - refCar.WorldPositionX;
            float dz = car.WorldPositionZ - refCar.WorldPositionZ;

            double cos = Math.Cos(-refCar.Yaw);
            double sin = Math.Sin(-refCar.Yaw);

            double rotatedX = dx * cos + dz * sin;
            double rotatedZ = dz * cos - dx * sin;

            double radarX = center - rotatedX * Scale;
            double radarY = center - rotatedZ * Scale;

            double dxRadar = radarX - center;
            double dyRadar = radarY - center;
            double distance = Math.Sqrt(dxRadar * dxRadar + dyRadar * dyRadar);

            double angle = Math.Atan2(rotatedZ, rotatedX) * 180.0 / Math.PI;
            if (angle < 0) angle += 360;

            bool inLeftSector = angle >= 295.0 || angle <= 65.0;
            bool inRightSector = angle >= 115.0 && angle <= 245.0;

            return new CarPosition(radarX, radarY, distance, angle, inLeftSector, inRightSector);
        }


        private void UpdateSectorState(ref RadarState radarState, CarPosition position, bool isReferenceCar)
        {
            if (isReferenceCar)
                return;

            if (position.InLeftSector) radarState.LeftHasCar = true;
            if (position.InRightSector) radarState.RightHasCar = true;

            bool isClose = position.Distance <= CloseProximityThreshold * Scale;
            if (isClose)
            {
                if (position.InLeftSector) radarState.LeftClose = true;
                if (position.InRightSector) radarState.RightClose = true;
            }
        }

        private static double CalculateOpacity(double distance, double maxRadius, double fadeStart)
        {
            if (distance >= maxRadius)
                return 0;
            if (distance <= fadeStart)
                return 1;
            return (maxRadius - distance) / (maxRadius - fadeStart);
        }

        private static double CalculateRelativeYaw(float carYaw, float refYaw)
        {
            return -(carYaw - refYaw) * 180.0 / Math.PI;
        }

        private void ApplyRadarState(ref RadarState radarState)
        {
            LeftSector.Opacity = radarState.LeftHasCar ? radarState.MaxOpacity : 0;
            RightSector.Opacity = radarState.RightHasCar ? radarState.MaxOpacity : 0;

            LeftSectorColor.Color = radarState.LeftClose ? Colors.Red : Colors.Yellow;
            RightSectorColor.Color = radarState.RightClose ? Colors.Red : Colors.Yellow;

            Opacity = radarState.MaxOpacity;
        }
    }
}