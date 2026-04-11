using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SmartHal.Core.Config;

/// <summary>
/// Writes SmartHal configuration to YAML files using YamlDotNet.
/// </summary>
public class YamlConfigWriter(ILogger<YamlConfigWriter> logger) : IConfigWriter
{
    private readonly ILogger<YamlConfigWriter> _logger = Ensure.NotNull(logger);

    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    /// <inheritdoc />
    public async Task WriteMetaAsync(string configPath, MetaConfig meta, CancellationToken ct = default)
    {
        _logger.LogDebug("Writing meta config to {ConfigPath}", configPath);

        var filePath = Path.Combine(configPath, "meta.yaml");
        await WriteYamlAsync(filePath, meta, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteRoomsAsync(string configPath, RoomsConfig rooms, CancellationToken ct = default)
    {
        _logger.LogDebug("Writing rooms config to {ConfigPath}", configPath);

        var filePath = Path.Combine(configPath, "rooms.yaml");
        await WriteYamlAsync(filePath, rooms, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteAdapterConfigAsync(string filePath, AdapterConfig config, CancellationToken ct = default)
    {
        _logger.LogDebug("Writing adapter config to {FilePath}", filePath);

        await WriteYamlAsync(filePath, config, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteDeviceAsync(string filePath, Device device, CancellationToken ct = default)
    {
        _logger.LogDebug("Writing device to {FilePath}", filePath);

        await WriteYamlAsync(filePath, device, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DeleteDeviceFileAsync(string filePath, CancellationToken ct = default)
    {
        _logger.LogDebug("Deleting device file {FilePath}", filePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task InitializeDirectoryStructureAsync(string configPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Initializing directory structure at {ConfigPath}", configPath);

        Directory.CreateDirectory(configPath);
        Directory.CreateDirectory(Path.Combine(configPath, "adapters"));
        Directory.CreateDirectory(Path.Combine(configPath, "devices"));

        var metaPath = Path.Combine(configPath, "meta.yaml");
        if (!File.Exists(metaPath))
        {
            await WriteMetaAsync(configPath, new MetaConfig(), ct).ConfigureAwait(false);
        }

        var roomsPath = Path.Combine(configPath, "rooms.yaml");
        if (!File.Exists(roomsPath))
        {
            await WriteRoomsAsync(configPath, new RoomsConfig(), ct).ConfigureAwait(false);
        }
    }

    private async Task WriteYamlAsync<T>(string filePath, T data, CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogDebug("Created directory {Directory}", directory);
        }

        var yaml = _serializer.Serialize(data);
        await File.WriteAllTextAsync(filePath, yaml, ct).ConfigureAwait(false);
    }
}
