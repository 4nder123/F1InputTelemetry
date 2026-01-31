using F1InputTelemetry.Settings.Overlay;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace F1InputTelemetry.Settings
{
    public class AppSettings
    {
        private const string SettingsFile = "settings.yaml";
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        private static AppSettings? _instance;

        [YamlMember(Description = "IP address, port and send rate of the UDP telemetry server.")]
        public string IPAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 20777;
        public int SendRate { get; set; } = 20;
        public InputTelemetrySettings InputTelemetry { get; set; } = new InputTelemetrySettings();
        public RadarSettings Radar { get; set; } = new RadarSettings();

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                    return _instance = CreateAndSaveDefault();

                var yaml = File.ReadAllText(SettingsFile);

                if (NeedsMigration(yaml))
                    return _instance = Migrate(yaml);

                return _instance = Deserializer.Deserialize<AppSettings>(yaml);
            }
            catch
            {
                return _instance = CreateAndSaveDefault();
            }
        }
        private static void Save(AppSettings settings)
        {
            var yaml = Serializer.Serialize(settings);
            File.WriteAllText(SettingsFile, yaml);
        }

        public static void Update() 
        { 
            if (_instance != null) 
                Save(_instance); 
        }

        private static AppSettings CreateAndSaveDefault()
        {
            var settings = new AppSettings();
            Save(settings);
            return settings;
        }

        private static bool NeedsMigration(string yaml)
        {
            using var reader = new StringReader(yaml);
            var stream = new YamlStream();
            stream.Load(reader);

            if (stream.Documents.Count == 0)
                return true;

            var root = stream.Documents[0].RootNode as YamlMappingNode;
            return root is null || !root.Children.ContainsKey(new YamlScalarNode("InputTelemetry"));
        }
        private static AppSettings Migrate(string yaml)
        {
            var root = ParseYamlRoot(yaml);
            if (root is null)
                return CreateAndSaveDefault();

            var migrated = new AppSettings
            {
                IPAddress = root.GetValue("IPAddress", "127.0.0.1"),
                Port = root.GetValue("Port", 20777),
                SendRate = root.GetValue("SendRate", 20),
                InputTelemetry = new InputTelemetrySettings
                {
                    WindowX = root.GetValue("WindowX", 960),
                    WindowY = root.GetValue("WindowY", 815),
                    WindowScale = root.GetValue("WindowScale", 1.0f),
                    AutoHide = root.GetValue("AutoHide", false),
                    ShowClutch = root.GetValue("ShowClutch", true),
                    Enabled = true
                }
            };

            Save(migrated);
            return migrated;
        }

        private static YamlMappingNode? ParseYamlRoot(string yaml)
        {
            using var reader = new StringReader(yaml);
            var stream = new YamlStream();
            stream.Load(reader);

            return stream.Documents.Count > 0
                ? stream.Documents[0].RootNode as YamlMappingNode
                : null;
        }
    }
}
