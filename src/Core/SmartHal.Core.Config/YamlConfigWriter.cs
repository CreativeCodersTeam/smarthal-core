using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SmartHal.Core.Config;

/// <summary>
/// Writes SmartHal configuration to YAML files using YamlDotNet.
/// </summary>
public class YamlConfigWriter : IConfigWriter
{
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    /// <inheritdoc />
    public async Task WriteMetaAsync(string configPath, MetaConfig meta, CancellationToken ct = default)
    {
        var filePath = Path.Combine(configPath, "meta.yaml");
        await WriteYamlAsync(filePath, meta, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteRoomsAsync(string configPath, RoomsConfig rooms, CancellationToken ct = default)
    {
        var filePath = Path.Combine(configPath, "rooms.yaml");
        await WriteYamlAsync(filePath, rooms, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteAdapterConfigAsync(string filePath, AdapterConfig config, CancellationToken ct = default)
    {
        await WriteYamlAsync(filePath, config, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteDeviceAsync(string filePath, Device device, CancellationToken ct = default)
    {
        await WriteYamlAsync(filePath, device, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DeleteDeviceFileAsync(string filePath, CancellationToken ct = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task InitializeDirectoryStructureAsync(string configPath, CancellationToken ct = default)
    {
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
        }

        var yaml = _serializer.Serialize(data);
        await File.WriteAllTextAsync(filePath, yaml, ct).ConfigureAwait(false);
    }
}
