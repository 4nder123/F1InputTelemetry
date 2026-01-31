using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace F1InputTelemetry.Settings.Overlay.Base
{
    public class OverlayBaseSettings
    {
        [YamlMember(Order = 1, Description = "Enable or disable the overlay.")]
        public bool Enabled { get; set; }
        [YamlMember(Order = 2, Description = "Screen coordinates of the window, measured from its center point.")]
        public int WindowX { get; set; }
        [YamlMember(Order = 3)]
        public int WindowY { get; set; }

        [YamlMember(Order = 4, Description = "Scale factor for resizing the overlay.")]
        public float WindowScale { get; set; }

        public OverlayBaseSettings(bool enabled, int x, int y, float scale)
        {
            Enabled = enabled;
            WindowX = x;
            WindowY = y;
            WindowScale = scale;
        }
    }
}
