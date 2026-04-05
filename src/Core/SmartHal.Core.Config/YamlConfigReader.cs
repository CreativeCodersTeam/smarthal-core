using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SmartHal.Core.Config;

/// <summary>
/// Reads SmartHal configuration from YAML files using YamlDotNet.
/// </summary>
public class YamlConfigReader : IConfigReader
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <inheritdoc />
    public async Task<MetaConfig> ReadMetaAsync(string configPath, CancellationToken ct = default)
    {
        var filePath = Path.Combine(configPath, "meta.yaml");
        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<MetaConfig>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<RoomsConfig> ReadRoomsAsync(string configPath, CancellationToken ct = default)
    {
        var filePath = Path.Combine(configPath, "rooms.yaml");
        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<RoomsConfig>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<AdapterConfig> ReadAdapterConfigAsync(string filePath, CancellationToken ct = default)
    {
        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<AdapterConfig>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<Device> ReadDeviceAsync(string filePath, CancellationToken ct = default)
    {
        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<Device>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<DeviceSummary> ReadDeviceSummaryAsync(string filePath, CancellationToken ct = default)
    {
        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);

        // Parse only the header fields for performance
        var summary = DeserializeYaml<DeviceSummary>(yaml, filePath);
        summary.FilePath = filePath;
        return summary;
    }

    private static async Task<string> ReadFileAsync(string filePath, CancellationToken ct)
    {
        if (!File.Exists(filePath))
        {
            throw new SmartHalConfigFileException($"Configuration file not found: '{filePath}'.", filePath);
        }

        return await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
    }

    private T DeserializeYaml<T>(string yaml, string filePath) where T : new()
    {
        try
        {
            return _deserializer.Deserialize<T>(yaml) ?? new T();
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new SmartHalConfigFileException(
                $"Failed to parse YAML file: {ex.Message}",
                filePath,
                (int)ex.Start.Line);
        }
    }
}
