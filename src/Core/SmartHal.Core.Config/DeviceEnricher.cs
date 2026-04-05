using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Enriches devices with adapter-specific type information.
/// If the adapter implements <see cref="IDeviceReader"/>, it uses the adapter's schema
/// to upgrade parameter kinds (e.g. String to Enum).
/// </summary>
public class DeviceEnricher : IDeviceEnricher
{
    /// <inheritdoc />
    public Task EnrichDeviceAsync(Device device, ISmartHalAdapter adapter, CancellationToken ct = default)
    {
        if (adapter is not IDeviceReader reader)
        {
            return Task.CompletedTask;
        }

        var schema = new DeviceParameterSchema();
        reader.EnrichDevice(device, schema);
        return Task.CompletedTask;
    }
}
