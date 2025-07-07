using System.Text.Json;
using System.Text.Json.Serialization;

namespace mmvp.src;

public class LaserTagConfig
{
    [JsonPropertyName("globals")] public required Globals Globals { get; set; }
    [JsonPropertyName("layers")] public List<LayerConfig> Layers { get; set; } = [];

    private static DirectoryInfo? FindRootDirectory(DirectoryInfo currentDir)
    {
        if (Directory.Exists(Path.Combine(currentDir.FullName, "LaserTagBox")))
        {
            return currentDir;
        }

        return currentDir.Parent is not null
            ? FindRootDirectory(currentDir.Parent)
            : null;
    }

    public static string? GetConfigPath()
    {
        var rootDir = FindRootDirectory(new DirectoryInfo(Directory.GetCurrentDirectory()));
        if (rootDir is null) return null;
        var pathToConfigFile = Path.Combine(rootDir.FullName, "LaserTagBox", "config.json");
        if (!File.Exists(pathToConfigFile)) return null;

        return pathToConfigFile;
    }

    public static string? GetMapPath(string configPath)
    {
        var rootDir = Directory.GetParent(configPath)?.Parent;
        if (rootDir is null) return null;

        var configJson = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<LaserTagConfig>(configJson);
        var playerBodyLayer = config?.Layers.Find(l => l.Name == "PlayerBodyLayer") ??
                              throw new Exception("PlayerBodyLayer not found in config.");
        return Path.Combine(rootDir.FullName, "LaserTagBox", playerBodyLayer.File);
    }
}

public record Globals([property: JsonPropertyName("steps")] int Steps);

public class LayerConfig
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("file")] public string File { get; set; } = "";
}
