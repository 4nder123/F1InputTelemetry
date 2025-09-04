using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.IO;

namespace TelemetryUI
{
    public class Settings
    {
        private const string SettingsFile = "settings.yaml";
        [YamlMember(Description = "IP address, port and send rate of the UDP telemetry server.")]
        public String IPAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 20777;
        public int SendRate { get; set; } = 20;
        [YamlMember(Description = "Screen coordinates of the window, measured from its center point.")]
        public int WindowX { get; set; } = 960;
        public int WindowY { get; set; } = 815;

        [YamlMember(Description = "Scale factor for resizing the overlay.")]
        public float WindowScale { get; set; } = 1.0f;
        [YamlMember(Description = "If true, the overlay is shown only when a session is active; otherwise it stays hidden.")]
        public bool AutoHide { get; set; } = false;
        [YamlMember(Description = "If true, displays clutch pedal input on the overlay.")]
        public bool ShowClutch { get; set; } = true;

        public static Settings Load()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                {
                    var settings = new Settings();
                    Save(settings);
                    return settings;
                }

                var input = File.ReadAllText(SettingsFile);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();

                return deserializer.Deserialize<Settings>(input) ?? new Settings();
            }
            catch
            {
                var settings = new Settings();
                Save(settings);
                return settings;
            }
        }
        private static void Save(Settings settings)
        {
            try
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();

                var yaml = serializer.Serialize(settings);

                File.WriteAllText(SettingsFile, yaml);
            }
            catch (Exception ex)
            {
                // Log or handle exceptions related to file writing
                throw new IOException("Failed to save settings.", ex);
            }
        }
    }
}
