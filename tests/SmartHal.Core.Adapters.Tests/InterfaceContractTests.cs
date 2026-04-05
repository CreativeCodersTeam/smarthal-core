using AwesomeAssertions;
using SmartHal.Core.Adapters.Fakes;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

public class InterfaceContractTests
{
    [Fact]
    public void FakeAdapter_ImplementsISmartHalAdapter()
    {
        var adapter = new FakeAdapter();

        adapter.Should().BeAssignableTo<ISmartHalAdapter>();
    }

    [Fact]
    public void CapabilityDetection_ViaIsPattern_Works()
    {
        ISmartHalAdapter adapter = new FakeAdapter();

        (adapter is IDeviceDiscovery).Should().BeTrue();
        (adapter is IDeviceReader).Should().BeTrue();
        (adapter is IDeviceWriter).Should().BeFalse();
        (adapter is IRelationManager).Should().BeFalse();
        (adapter is IBackupRestore).Should().BeFalse();
    }

    [Fact]
    public async Task FakeAdapter_TestConnection_ReturnsTrue()
    {
        var adapter = new FakeAdapter();

        var result = await adapter.TestConnectionAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public void FakeAdapter_EnrichDevice_UpgradesStringToEnum()
    {
        var adapter = new FakeAdapter();
        var device = new Device();
        device.Parameters["MODE"] = ParameterValue.FromString("AUTO");

        var schema = new DeviceParameterSchema { ["MODE"] = ParameterKind.Enum };

        adapter.EnrichDevice(device, schema);

        device.Parameters["MODE"].Kind.Should().Be(ParameterKind.Enum);
        device.Parameters["MODE"].StringValue.Should().Be("AUTO");
    }

    [Fact]
    public async Task FakeAdapter_CanBeDisposed()
    {
        var adapter = new FakeAdapter();

        await adapter.DisposeAsync();

        // No exception means success
    }
}
