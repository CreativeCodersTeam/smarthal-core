using AwesomeAssertions;
using SmartHal.Core.Adapters.Fakes;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

public class InterfaceContractTests
{
    [Fact]
    public void FakeAdapter_ImplementsISmartHalAdapter()
    {
        // Act
        var adapter = new FakeAdapter();

        // Assert
        adapter.Should().BeAssignableTo<ISmartHalAdapter>();
    }

    [Fact]
    public void CapabilityDetection_ViaIsPattern_Works()
    {
        // Arrange
        ISmartHalAdapter adapter = new FakeAdapter();

        // Act & Assert
        (adapter is IDeviceDiscovery).Should().BeTrue();
        (adapter is IDeviceReader).Should().BeTrue();
        (adapter is IDeviceWriter).Should().BeFalse();
        (adapter is IRelationManager).Should().BeFalse();
        (adapter is IBackupRestore).Should().BeFalse();
    }

    [Fact]
    public async Task FakeAdapter_TestConnection_ReturnsTrue()
    {
        // Arrange
        var adapter = new FakeAdapter();

        // Act
        var result = await adapter.TestConnectionAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FakeAdapter_EnrichDevice_UpgradesStringToEnum()
    {
        // Arrange
        var adapter = new FakeAdapter();
        var device = new Device();
        device.Parameters["MODE"] = ParameterValue.FromString("AUTO");
        var schema = new DeviceParameterSchema { ["MODE"] = ParameterKind.Enum };

        // Act
        adapter.EnrichDevice(device, schema);

        // Assert
        device.Parameters["MODE"].Kind.Should().Be(ParameterKind.Enum);
        device.Parameters["MODE"].StringValue.Should().Be("AUTO");
    }

    [Fact]
    public async Task FakeAdapter_CanBeDisposed()
    {
        // Arrange
        var adapter = new FakeAdapter();

        // Act & Assert
        await adapter.DisposeAsync();

        // No exception means success
    }
}
