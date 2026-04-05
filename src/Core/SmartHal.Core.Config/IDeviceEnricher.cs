using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Enriches devices with adapter-specific type information (phase 2 of two-stage loading).
/// </summary>
public interface IDeviceEnricher
{
    Task EnrichDeviceAsync(Device device, ISmartHalAdapter adapter, CancellationToken ct = default);
}
