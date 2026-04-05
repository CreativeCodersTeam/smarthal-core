using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that support device discovery.
/// </summary>
public interface IDeviceDiscovery
{
    /// <summary>Discovers all devices available through this adapter.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of discovered devices.</returns>
    Task<IReadOnlyList<Device>> DiscoverDevicesAsync(CancellationToken ct = default);
}
