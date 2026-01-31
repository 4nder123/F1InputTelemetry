using F1InputTelemetry.Settings.Overlay.Base;
using YamlDotNet.Serialization;

namespace F1InputTelemetry.Settings.Overlay
{
    public class InputTelemetrySettings : OverlayBaseSettings
    {
        [YamlMember(Order = 5, Description = "If true, the overlay is shown only when a session is active; otherwise it stays hidden.")]
        public bool AutoHide { get; set; } = false;

        [YamlMember(Order = 6, Description = "If true, displays clutch pedal input on the overlay.")]
        public bool ShowClutch { get; set; } = true;

        // base parameters: enabled, x, y, scale
        public InputTelemetrySettings() : base(true, 960, 815, 1.0f) { }
    }
}
