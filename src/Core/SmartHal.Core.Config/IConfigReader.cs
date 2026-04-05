using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Reads configuration data from YAML files.
/// </summary>
public interface IConfigReader
{
    Task<MetaConfig> ReadMetaAsync(string configPath, CancellationToken ct = default);
    Task<RoomsConfig> ReadRoomsAsync(string configPath, CancellationToken ct = default);
    Task<AdapterConfig> ReadAdapterConfigAsync(string filePath, CancellationToken ct = default);
    Task<Device> ReadDeviceAsync(string filePath, CancellationToken ct = default);
    Task<DeviceSummary> ReadDeviceSummaryAsync(string filePath, CancellationToken ct = default);
}
