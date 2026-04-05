using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Central facade for all configuration access.
/// </summary>
public interface IConfigRepository : IDisposable
{
    // Meta
    Task<MetaConfig> GetMetaAsync(CancellationToken ct = default);
    Task SaveMetaAsync(MetaConfig meta, CancellationToken ct = default);
    Task InitializeAsync(MetaConfig meta, CancellationToken ct = default);

    // Rooms & Groups
    Task<RoomsConfig> GetRoomsAsync(CancellationToken ct = default);
    Task SaveRoomsAsync(RoomsConfig rooms, CancellationToken ct = default);

    // Adapter configurations
    Task<AdapterConfig> GetAdapterConfigAsync(string adapterId, CancellationToken ct = default);
    Task<IReadOnlyList<AdapterConfig>> GetAllAdapterConfigsAsync(CancellationToken ct = default);
    Task SaveAdapterConfigAsync(AdapterConfig config, CancellationToken ct = default);

    // Devices — lazy loading
    Task<IReadOnlyList<DeviceSummary>> ListDevicesAsync(CancellationToken ct = default);
    Task<Device> GetDeviceAsync(string deviceId, CancellationToken ct = default);
    Task SaveDeviceAsync(Device device, CancellationToken ct = default);
    Task DeleteDeviceAsync(string deviceId, CancellationToken ct = default);

    // Filtered listing
    Task<IReadOnlyList<DeviceSummary>> ListDevicesAsync(DeviceFilter filter, CancellationToken ct = default);
}
