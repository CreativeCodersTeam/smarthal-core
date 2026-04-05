using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Writes configuration data to YAML files.
/// </summary>
public interface IConfigWriter
{
    Task WriteMetaAsync(string configPath, MetaConfig meta, CancellationToken ct = default);
    Task WriteRoomsAsync(string configPath, RoomsConfig rooms, CancellationToken ct = default);
    Task WriteAdapterConfigAsync(string filePath, AdapterConfig config, CancellationToken ct = default);
    Task WriteDeviceAsync(string filePath, Device device, CancellationToken ct = default);
    Task DeleteDeviceFileAsync(string filePath, CancellationToken ct = default);
    Task InitializeDirectoryStructureAsync(string configPath, CancellationToken ct = default);
}
