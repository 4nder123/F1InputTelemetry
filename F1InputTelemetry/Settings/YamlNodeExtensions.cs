using System.Globalization;
using YamlDotNet.RepresentationModel;

namespace F1InputTelemetry.Settings;

internal static class YamlNodeExtensions
{
    public static string GetValue(this YamlMappingNode node, string key, string fallback)
    {
        return node.Children.TryGetValue(new YamlScalarNode(key), out var value)
            ? value.ToString()
            : fallback;
    }

    public static int GetValue(this YamlMappingNode node, string key, int fallback)
    {
        return node.Children.TryGetValue(new YamlScalarNode(key), out var value)
            && int.TryParse(value.ToString(), out var result)
            ? result
            : fallback;
    }

    public static float GetValue(this YamlMappingNode node, string key, float fallback)
    {
        return node.Children.TryGetValue(new YamlScalarNode(key), out var value)
            && float.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : fallback;
    }

    public static bool GetValue(this YamlMappingNode node, string key, bool fallback)
    {
        return node.Children.TryGetValue(new YamlScalarNode(key), out var value)
            && bool.TryParse(value.ToString(), out var result)
            ? result
            : fallback;
    }
}