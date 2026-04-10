using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;
using Spectre.Console.Testing;

namespace SmartHal.CLI.Tests.Commands;

public class DeviceCommandTests
{
    [Fact]
    public async Task DeviceList_CallsRepositoryWithFilter()
    {
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => repo.ListDevicesAsync(A<DeviceFilter>._, A<CancellationToken>._))
            .Returns(new List<DeviceSummary>());

        var command = new CLI.Commands.Device.DeviceListCommand(repo, formatter);

        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceListOptions { AdapterId = "hm1" });
        result.Should().Be(CommandResult.Success);

        A.CallTo(() => repo.ListDevicesAsync(
            A<DeviceFilter>.That.Matches(f => f.AdapterId == "hm1"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeviceShow_LoadsFullDevice()
    {
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var device = new Device
        {
            Id = "smhal-hm-lamp",
            Name = "Lamp",
            Type = DeviceType.SwitchActuator,
            AdapterId = "hm1",
            NativeId = "001234",
            GroupIds = [],
            Parameters = new Dictionary<string, ParameterValue>(),
            Channels = [],
            Relations = []
        };

        A.CallTo(() => repo.GetDeviceAsync("smhal-hm-lamp", A<CancellationToken>._))
            .Returns(device);

        var command = new CLI.Commands.Device.DeviceShowCommand(repo, formatter);

        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceShowOptions { DeviceId = "smhal-hm-lamp" });

        result.Should().Be(CommandResult.Success);
        var output = console.Output;
        output.Should().Contain("smhal-hm-lamp");
        output.Should().Contain("Lamp");
    }

    [Fact]
    public async Task DeviceSet_WritesParameterAndSavesDevice()
    {
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<Core.Adapters.IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var adapterMock = A.Fake<ITestWriterAdapter>();
        var device = new Device
        {
            Id = "dev1",
            Name = "Test",
            Type = DeviceType.SwitchActuator,
            AdapterId = "hm1",
            NativeId = "123",
            GroupIds = [],
            Parameters = new Dictionary<string, ParameterValue>(),
            Channels = [],
            Relations = []
        };
        var adapterConfig = new Core.Adapters.AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string>()
        };

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapterMock);

        var command = new CLI.Commands.Device.DeviceSetCommand(repo, adapterFactory, formatter);

        var result = await command.ExecuteAsync(new CLI.Commands.Device.DeviceSetOptions
        {
            DeviceId = "dev1",
            Parameter = "level",
            Value = "0.5"
        });

        result.Should().Be(CommandResult.Success);

        A.CallTo(() => adapterMock.WriteDeviceParameterAsync(
            "123", "level", A<ParameterValue>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => repo.SaveDeviceAsync(
            A<Device>.That.Matches(d => d.Parameters.ContainsKey("level")),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    /// <summary>Test double that implements both ISmartHalAdapter and IDeviceWriter.</summary>
    public interface ITestWriterAdapter : Core.Adapters.ISmartHalAdapter, Core.Adapters.IDeviceWriter;
}
