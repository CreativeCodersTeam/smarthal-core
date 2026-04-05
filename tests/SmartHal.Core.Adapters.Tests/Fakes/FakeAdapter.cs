using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters.Fakes;

/// <summary>
/// A fake adapter for testing that implements ISmartHalAdapter plus optional sub-interfaces.
/// </summary>
[AdapterMetadata("fake", "Fake Test Adapter")]
public class FakeAdapter : ISmartHalAdapter, IDeviceDiscovery, IDeviceReader
{
    public string AdapterId { get; set; } = "fake-001";
    public string DisplayName { get; set; } = "Fake Adapter";

    public Task<bool> TestConnectionAsync(CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<IReadOnlyList<Device>> DiscoverDevicesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Device>>([]);

    public Task<Device> ReadDeviceAsync(string nativeId, CancellationToken ct = default) =>
        Task.FromResult(new Device { NativeId = nativeId });

    public Task<IReadOnlyList<Device>> ReadAllDevicesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Device>>([]);

    public void EnrichDevice(Device device, DeviceParameterSchema schema)
    {
        foreach (var (key, kind) in schema)
        {
            if (device.Parameters.TryGetValue(key, out var value))
            {
                device.Parameters[key] = value.WithKind(kind);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
