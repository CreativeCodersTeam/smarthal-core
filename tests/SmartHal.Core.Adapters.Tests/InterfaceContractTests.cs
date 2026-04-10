using AwesomeAssertions;
using SmartHal.Core.Adapters.Fakes;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

public class InterfaceContractTests
{
    [Fact]
    public void FakeAdapter_Implements_ISmartHalAdapter()
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
    public async Task TestConnectionAsync_FakeAdapter_ReturnsTrue()
    {
        // Arrange
        var adapter = new FakeAdapter();

        // Act
        var result = await adapter.TestConnectionAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EnrichDevice_StringParameterWithEnumSchema_UpgradesToEnum()
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
    public async Task DisposeAsync_FakeAdapter_DoesNotThrow()
    {
        // Arrange
        var adapter = new FakeAdapter();

        // Act & Assert
        await adapter.DisposeAsync();

        // No exception means success
    }

    [Fact]
    public void EnrichDevice_EmptySchema_LeavesParametersUnchanged()
    {
        // Arrange
        var adapter = new FakeAdapter();
        var device = new Device();
        device.Parameters["MODE"] = ParameterValue.FromString("AUTO");
        var schema = new DeviceParameterSchema();

        // Act
        adapter.EnrichDevice(device, schema);

        // Assert
        device.Parameters["MODE"].Kind.Should().Be(ParameterKind.String);
    }

    [Fact]
    public void EnrichDevice_SchemaKeyNotInDevice_LeavesDeviceUnchanged()
    {
        // Arrange
        var adapter = new FakeAdapter();
        var device = new Device();
        var schema = new DeviceParameterSchema { ["BRIGHTNESS"] = ParameterKind.Number };

        // Act
        adapter.EnrichDevice(device, schema);

        // Assert
        device.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void EnrichDevice_MultipleParameters_OnlyMatchingKeysConverted()
    {
        // Arrange
        var adapter = new FakeAdapter();
        var device = new Device();
        device.Parameters["MODE"] = ParameterValue.FromString("AUTO");
        device.Parameters["LEVEL"] = ParameterValue.FromNumber(50);
        var schema = new DeviceParameterSchema { ["MODE"] = ParameterKind.Enum };

        // Act
        adapter.EnrichDevice(device, schema);

        // Assert
        device.Parameters["MODE"].Kind.Should().Be(ParameterKind.Enum);
        device.Parameters["LEVEL"].Kind.Should().Be(ParameterKind.Number);
    }
}
