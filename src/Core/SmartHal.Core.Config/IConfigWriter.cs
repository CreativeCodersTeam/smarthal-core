using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Writes configuration data to YAML files.
/// </summary>
public interface IConfigWriter
{
    /// <summary>Writes the meta configuration to the specified directory.</summary>
    /// <param name="configPath">The root configuration directory path.</param>
    /// <param name="meta">The meta configuration to write.</param>
    /// <param name="ct">The cancellation token.</param>
    Task WriteMetaAsync(string configPath, MetaConfig meta, CancellationToken ct = default);

    /// <summary>Writes the rooms configuration to the specified directory.</summary>
    /// <param name="configPath">The root configuration directory path.</param>
    /// <param name="rooms">The rooms configuration to write.</param>
    /// <param name="ct">The cancellation token.</param>
    Task WriteRoomsAsync(string configPath, RoomsConfig rooms, CancellationToken ct = default);

    /// <summary>Writes an adapter configuration to the specified file.</summary>
    /// <param name="filePath">The path to the adapter configuration YAML file.</param>
    /// <param name="config">The adapter configuration to write.</param>
    /// <param name="ct">The cancellation token.</param>
    Task WriteAdapterConfigAsync(string filePath, AdapterConfig config, CancellationToken ct = default);

    /// <summary>Writes a device to the specified file.</summary>
    /// <param name="filePath">The path to the device YAML file.</param>
    /// <param name="device">The device to write.</param>
    /// <param name="ct">The cancellation token.</param>
    Task WriteDeviceAsync(string filePath, Device device, CancellationToken ct = default);

    /// <summary>Deletes a device YAML file.</summary>
    /// <param name="filePath">The path to the device YAML file to delete.</param>
    /// <param name="ct">The cancellation token.</param>
    Task DeleteDeviceFileAsync(string filePath, CancellationToken ct = default);

    /// <summary>Creates the initial directory structure for a new configuration.</summary>
    /// <param name="configPath">The root configuration directory path.</param>
    /// <param name="ct">The cancellation token.</param>
    Task InitializeDirectoryStructureAsync(string configPath, CancellationToken ct = default);
}
