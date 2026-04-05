using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Enriches devices with adapter-specific type information (phase 2 of two-stage loading).
/// </summary>
public interface IDeviceEnricher
{
    /// <summary>Enriches a device with adapter-specific type information.</summary>
    /// <param name="device">The device to enrich.</param>
    /// <param name="adapter">The adapter that provides the type information.</param>
    /// <param name="ct">The cancellation token.</param>
    Task EnrichDeviceAsync(Device device, ISmartHalAdapter adapter, CancellationToken ct = default);
}
