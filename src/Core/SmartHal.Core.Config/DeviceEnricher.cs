using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Enriches devices with adapter-specific type information.
/// If the adapter implements <see cref="IDeviceReader"/>, it uses the adapter's schema
/// to upgrade parameter kinds (e.g. String to Enum).
/// </summary>
public class DeviceEnricher(ILogger<DeviceEnricher> logger) : IDeviceEnricher
{
    private readonly ILogger<DeviceEnricher> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public Task EnrichDeviceAsync(Device device, ISmartHalAdapter adapter, CancellationToken ct = default)
    {
        _logger.LogDebug("Enriching device {DeviceId} with adapter schema", device.Id);

        if (adapter is not IDeviceReader reader)
        {
            _logger.LogDebug("Adapter {AdapterId} does not support device reading, skipping enrichment", adapter.AdapterId);
            return Task.CompletedTask;
        }

        var schema = new DeviceParameterSchema();
        reader.EnrichDevice(device, schema);
        return Task.CompletedTask;
    }
}
