using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Central facade for all configuration access.
/// </summary>
public interface IConfigRepository : IDisposable
{
    /// <summary>Reads the meta configuration.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The meta configuration.</returns>
    Task<MetaConfig> GetMetaAsync(CancellationToken ct = default);

    /// <summary>Saves the meta configuration.</summary>
    /// <param name="meta">The meta configuration to save.</param>
    /// <param name="ct">The cancellation token.</param>
    Task SaveMetaAsync(MetaConfig meta, CancellationToken ct = default);

    /// <summary>Initializes a new configuration directory with default structure and meta data.</summary>
    /// <param name="meta">The initial meta configuration.</param>
    /// <param name="ct">The cancellation token.</param>
    Task InitializeAsync(MetaConfig meta, CancellationToken ct = default);

    /// <summary>Reads the rooms and groups configuration.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The rooms configuration.</returns>
    Task<RoomsConfig> GetRoomsAsync(CancellationToken ct = default);

    /// <summary>Saves the rooms and groups configuration.</summary>
    /// <param name="rooms">The rooms configuration to save.</param>
    /// <param name="ct">The cancellation token.</param>
    Task SaveRoomsAsync(RoomsConfig rooms, CancellationToken ct = default);

    /// <summary>Reads the configuration for a specific adapter.</summary>
    /// <param name="adapterId">The adapter identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The adapter configuration.</returns>
    Task<AdapterConfig> GetAdapterConfigAsync(string adapterId, CancellationToken ct = default);

    /// <summary>Reads all adapter configurations.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of all adapter configurations.</returns>
    Task<IReadOnlyList<AdapterConfig>> GetAllAdapterConfigsAsync(CancellationToken ct = default);

    /// <summary>Saves an adapter configuration.</summary>
    /// <param name="config">The adapter configuration to save.</param>
    /// <param name="ct">The cancellation token.</param>
    Task SaveAdapterConfigAsync(AdapterConfig config, CancellationToken ct = default);

    /// <summary>Lists all devices as lightweight summaries.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of device summaries.</returns>
    Task<IReadOnlyList<DeviceSummary>> ListDevicesAsync(CancellationToken ct = default);

    /// <summary>Reads a full device by its identifier.</summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The full device data.</returns>
    Task<Device> GetDeviceAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Saves a device.</summary>
    /// <param name="device">The device to save.</param>
    /// <param name="ct">The cancellation token.</param>
    Task SaveDeviceAsync(Device device, CancellationToken ct = default);

    /// <summary>Deletes a device by its identifier.</summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    Task DeleteDeviceAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Lists devices matching the specified filter criteria.</summary>
    /// <param name="filter">The filter criteria to apply.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of matching device summaries.</returns>
    Task<IReadOnlyList<DeviceSummary>> ListDevicesAsync(DeviceFilter filter, CancellationToken ct = default);
}
