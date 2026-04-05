using AwesomeAssertions;
using FakeItEasy;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

public class DeviceEnricherTests
{
    private readonly DeviceEnricher _sut = new();

    [Fact]
    public async Task EnrichDeviceAsync_AdapterIsDeviceReader_CallsEnrichDevice()
    {
        // Arrange
        var adapter = A.Fake<IReadableAdapter>();
        var device = new Device { Id = "dev-001" };

        // Act
        await _sut.EnrichDeviceAsync(device, adapter);

        // Assert
        A.CallTo(() => adapter.EnrichDevice(device, A<DeviceParameterSchema>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task EnrichDeviceAsync_AdapterIsNotDeviceReader_DoesNothing()
    {
        // Arrange
        var adapter = A.Fake<ISmartHalAdapter>();
        var device = new Device { Id = "dev-001" };

        // Act
        await _sut.EnrichDeviceAsync(device, adapter);

        // Assert — no exception, just completes
        device.Id.Should().Be("dev-001");
    }

    /// <summary>Combined interface for FakeItEasy to create a fake that is both adapter and reader.</summary>
    public interface IReadableAdapter : ISmartHalAdapter, IDeviceReader;
}
