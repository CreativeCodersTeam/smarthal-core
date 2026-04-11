using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SmartHal.Core.Config;

/// <summary>
/// Reads SmartHal configuration from YAML files using YamlDotNet.
/// </summary>
public class YamlConfigReader(ILogger<YamlConfigReader> logger) : IConfigReader
{
    private readonly ILogger<YamlConfigReader> _logger = Ensure.NotNull(logger);

    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <inheritdoc />
    public async Task<MetaConfig> ReadMetaAsync(string configPath, CancellationToken ct = default)
    {
        _logger.LogDebug("Reading meta config from {ConfigPath}", configPath);

        var filePath = Path.Combine(configPath, "meta.yaml");
        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<MetaConfig>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<RoomsConfig> ReadRoomsAsync(string configPath, CancellationToken ct = default)
    {
        _logger.LogDebug("Reading rooms config from {ConfigPath}", configPath);

        var filePath = Path.Combine(configPath, "rooms.yaml");
        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<RoomsConfig>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<AdapterConfig> ReadAdapterConfigAsync(string filePath, CancellationToken ct = default)
    {
        _logger.LogDebug("Reading adapter config from {FilePath}", filePath);

        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<AdapterConfig>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<Device> ReadDeviceAsync(string filePath, CancellationToken ct = default)
    {
        _logger.LogDebug("Reading device from {FilePath}", filePath);

        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return DeserializeYaml<Device>(yaml, filePath);
    }

    /// <inheritdoc />
    public async Task<DeviceSummary> ReadDeviceSummaryAsync(string filePath, CancellationToken ct = default)
    {
        _logger.LogDebug("Reading device summary from {FilePath}", filePath);

        var yaml = await ReadFileAsync(filePath, ct).ConfigureAwait(false);

        // Parse only the header fields for performance
        var summary = DeserializeYaml<DeviceSummary>(yaml, filePath);
        summary.FilePath = filePath;
        return summary;
    }

    private async Task<string> ReadFileAsync(string filePath, CancellationToken ct)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Configuration file not found: {FilePath}", filePath);

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
            _logger.LogError("Failed to parse YAML file {FilePath}: {Message}", filePath, ex.Message);

            throw new SmartHalConfigFileException(
                $"Failed to parse YAML file: {ex.Message}",
                filePath,
                (int)ex.Start.Line);
        }
    }
}
