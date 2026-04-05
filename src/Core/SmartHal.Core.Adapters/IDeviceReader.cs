using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that can read device data.
/// </summary>
public interface IDeviceReader
{
    /// <summary>Reads a single device by its native identifier.</summary>
    /// <param name="nativeId">The native device identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The device data.</returns>
    Task<Device> ReadDeviceAsync(string nativeId, CancellationToken ct = default);

    /// <summary>Reads all devices managed by this adapter.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of all devices.</returns>
    Task<IReadOnlyList<Device>> ReadAllDevicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Enriches a device with adapter-specific type information based on the given schema.
    /// This is phase 2 of the two-stage loading process: after YAML is loaded with inferred types,
    /// the adapter upgrades parameter kinds (e.g. String to Enum) using its schema.
    /// </summary>
    /// <param name="device">The device to enrich.</param>
    /// <param name="schema">The parameter schema defining adapter-specific type information.</param>
    void EnrichDevice(Device device, DeviceParameterSchema schema);
}
