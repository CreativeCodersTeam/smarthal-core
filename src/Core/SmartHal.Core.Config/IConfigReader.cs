using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Reads configuration data from YAML files.
/// </summary>
public interface IConfigReader
{
    /// <summary>Reads the meta configuration from the specified directory.</summary>
    /// <param name="configPath">The root configuration directory path.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed meta configuration.</returns>
    Task<MetaConfig> ReadMetaAsync(string configPath, CancellationToken ct = default);

    /// <summary>Reads the rooms configuration from the specified directory.</summary>
    /// <param name="configPath">The root configuration directory path.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed rooms configuration.</returns>
    Task<RoomsConfig> ReadRoomsAsync(string configPath, CancellationToken ct = default);

    /// <summary>Reads an adapter configuration from the specified file.</summary>
    /// <param name="filePath">The path to the adapter configuration YAML file.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed adapter configuration.</returns>
    Task<AdapterConfig> ReadAdapterConfigAsync(string filePath, CancellationToken ct = default);

    /// <summary>Reads a full device from the specified file.</summary>
    /// <param name="filePath">The path to the device YAML file.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed device.</returns>
    Task<Device> ReadDeviceAsync(string filePath, CancellationToken ct = default);

    /// <summary>Reads only the header data of a device for efficient listing.</summary>
    /// <param name="filePath">The path to the device YAML file.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A lightweight device summary.</returns>
    Task<DeviceSummary> ReadDeviceSummaryAsync(string filePath, CancellationToken ct = default);
}
