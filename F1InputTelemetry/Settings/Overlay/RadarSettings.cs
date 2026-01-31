using F1InputTelemetry.Settings.Overlay.Base;
using YamlDotNet.Serialization;

namespace F1InputTelemetry.Settings.Overlay
{
    public class RadarSettings : OverlayBaseSettings
    {
        // base parameters: enabled, x, y, scale
        public RadarSettings() : base(false, 960, 315, 1.0f) { }
        
    }
}
