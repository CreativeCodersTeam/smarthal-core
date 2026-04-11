using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;
using Spectre.Console.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartHal.CLI.Tests.Commands;

public class DeviceCommandTests
{
    [Fact]
    public async Task DeviceList_CallsRepositoryWithFilter()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => repo.ListDevicesAsync(A<DeviceFilter>._, A<CancellationToken>._))
            .Returns(new List<DeviceSummary>());

        var command = new CLI.Commands.Device.DeviceListCommand(repo, formatter, NullLogger<CLI.Commands.Device.DeviceListCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceListOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.ListDevicesAsync(
            A<DeviceFilter>.That.Matches(f => f.AdapterId == "hm1"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeviceList_EmptyType_SetsNullTypeFilter()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => repo.ListDevicesAsync(A<DeviceFilter>._, A<CancellationToken>._))
            .Returns(new List<DeviceSummary>());

        var command = new CLI.Commands.Device.DeviceListCommand(repo, formatter, NullLogger<CLI.Commands.Device.DeviceListCommand>.Instance);

        // Act
        await command.ExecuteAsync(new CLI.Commands.Device.DeviceListOptions { Type = "" });

        // Assert
        A.CallTo(() => repo.ListDevicesAsync(
            A<DeviceFilter>.That.Matches(f => f.Type == null),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeviceList_WithTypeFilter_SetsDeviceType()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => repo.ListDevicesAsync(A<DeviceFilter>._, A<CancellationToken>._))
            .Returns(new List<DeviceSummary>());

        var command = new CLI.Commands.Device.DeviceListCommand(repo, formatter, NullLogger<CLI.Commands.Device.DeviceListCommand>.Instance);

        // Act
        await command.ExecuteAsync(new CLI.Commands.Device.DeviceListOptions { Type = "switch_actuator" });

        // Assert
        A.CallTo(() => repo.ListDevicesAsync(
            A<DeviceFilter>.That.Matches(f => f.Type != null && f.Type.Value.Value == "switch_actuator"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeviceShow_LoadsFullDevice()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var device = CreateDevice("smhal-hm-lamp", "Lamp");

        A.CallTo(() => repo.GetDeviceAsync("smhal-hm-lamp", A<CancellationToken>._)).Returns(device);

        var command = new CLI.Commands.Device.DeviceShowCommand(repo, formatter, NullLogger<CLI.Commands.Device.DeviceShowCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceShowOptions { DeviceId = "smhal-hm-lamp" });

        // Assert
        result.Should().Be(CommandResult.Success);
        var output = console.Output;
        output.Should().Contain("smhal-hm-lamp");
        output.Should().Contain("Lamp");
    }

    [Fact]
    public async Task DeviceSet_WritesParameterAndSavesDevice()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var adapterMock = A.Fake<ITestWriterAdapter>();
        var device = CreateDevice("dev1", "Test");
        var adapterConfig = CreateAdapterConfig();

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapterMock);

        var command = new CLI.Commands.Device.DeviceSetCommand(repo, adapterFactory, formatter, NullLogger<CLI.Commands.Device.DeviceSetCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new CLI.Commands.Device.DeviceSetOptions
        {
            DeviceId = "dev1",
            Parameter = "level",
            Value = "0.5"
        });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => adapterMock.WriteDeviceParameterAsync(
            "123", "level", A<ParameterValue>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => repo.SaveDeviceAsync(
            A<Device>.That.Matches(d => d.Parameters.ContainsKey("level")),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeviceSet_AdapterWithoutWriter_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var adapter = A.Fake<ISmartHalAdapter>();
        var device = CreateDevice("dev1", "Test");
        var adapterConfig = CreateAdapterConfig();

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapter);

        var command = new CLI.Commands.Device.DeviceSetCommand(repo, adapterFactory, formatter, NullLogger<CLI.Commands.Device.DeviceSetCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new CLI.Commands.Device.DeviceSetOptions
        {
            DeviceId = "dev1",
            Parameter = "level",
            Value = "0.5"
        });

        // Assert
        result.ExitCode.Should().Be(1);
        console.Output.Should().Contain("does not support writing");
    }

    [Fact]
    public async Task DeviceDiscover_NoDiscoverySupport_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var idGenerator = A.Fake<IIdGenerator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var adapter = A.Fake<ISmartHalAdapter>();
        var adapterConfig = CreateAdapterConfig();

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapter);

        var command = new CLI.Commands.Device.DeviceDiscoverCommand(
            repo, adapterFactory, idGenerator, interaction, formatter, NullLogger<CLI.Commands.Device.DeviceDiscoverCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceDiscoverOptions { AdapterId = "hm1" });

        // Assert
        result.ExitCode.Should().Be(1);
        console.Output.Should().Contain("does not support device discovery");
    }

    [Fact]
    public async Task DeviceDiscover_NoDevicesFound_ReturnsSuccess()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var idGenerator = A.Fake<IIdGenerator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var discoveryAdapter = A.Fake<ITestDiscoveryAdapter>();
        var adapterConfig = CreateAdapterConfig();

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(discoveryAdapter);
        A.CallTo(() => discoveryAdapter.DiscoverDevicesAsync(A<CancellationToken>._))
            .Returns(new List<Device>());

        var command = new CLI.Commands.Device.DeviceDiscoverCommand(
            repo, adapterFactory, idGenerator, interaction, formatter, NullLogger<CLI.Commands.Device.DeviceDiscoverCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceDiscoverOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("No new devices");
    }

    [Fact]
    public async Task DeviceDiscover_ImportConfirmed_SavesDevice()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var idGenerator = A.Fake<IIdGenerator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var discoveryAdapter = A.Fake<ITestDiscoveryAdapter>();
        var adapterConfig = CreateAdapterConfig();

        var discovered = new List<Device>
        {
            new Device
            {
                Name = "New Lamp",
                Type = DeviceType.SwitchActuator,
                NativeId = "native-001",
                AdapterId = "",
                GroupIds = [],
                Parameters = new Dictionary<string, ParameterValue>(),
                Channels = [],
                Relations = []
            }
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(discoveryAdapter);
        A.CallTo(() => discoveryAdapter.DiscoverDevicesAsync(A<CancellationToken>._)).Returns(discovered);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);
        A.CallTo(() => idGenerator.GenerateDeviceIdAsync("homematic", "New Lamp", A<CancellationToken>._))
            .Returns("smhal-hm-new-lamp");

        var command = new CLI.Commands.Device.DeviceDiscoverCommand(
            repo, adapterFactory, idGenerator, interaction, formatter, NullLogger<CLI.Commands.Device.DeviceDiscoverCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceDiscoverOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveDeviceAsync(
            A<Device>.That.Matches(d => d.Id == "smhal-hm-new-lamp" && d.AdapterId == "hm1"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeviceDiscover_ImportDeclined_DoesNotSaveDevice()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var idGenerator = A.Fake<IIdGenerator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var discoveryAdapter = A.Fake<ITestDiscoveryAdapter>();
        var adapterConfig = CreateAdapterConfig();

        var discovered = new List<Device>
        {
            new Device
            {
                Name = "Sensor",
                Type = DeviceType.SwitchActuator,
                NativeId = "native-002",
                AdapterId = "",
                GroupIds = [],
                Parameters = new Dictionary<string, ParameterValue>(),
                Channels = [],
                Relations = []
            }
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(discoveryAdapter);
        A.CallTo(() => discoveryAdapter.DiscoverDevicesAsync(A<CancellationToken>._)).Returns(discovered);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new CLI.Commands.Device.DeviceDiscoverCommand(
            repo, adapterFactory, idGenerator, interaction, formatter, NullLogger<CLI.Commands.Device.DeviceDiscoverCommand>.Instance);

        // Act
        await command.ExecuteAsync(new CLI.Commands.Device.DeviceDiscoverOptions { AdapterId = "hm1" });

        // Assert
        A.CallTo(() => repo.SaveDeviceAsync(A<Device>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task DeviceReplace_Confirmed_UpdatesNativeIdAndApplies()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var snapshotManager = A.Fake<ISnapshotManager>();
        var applier = A.Fake<IConfigApplier>();
        var differ = A.Fake<IConfigDiffer>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var readerAdapter = A.Fake<ITestReaderAdapter>();
        var device = CreateDevice("dev1", "Lamp");
        var adapterConfig = CreateAdapterConfig();

        var diff = new DeviceDiff
        {
            Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "level", OldValue = "0", NewValue = "1" }]
        };

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(new SnapshotManifest { SnapshotId = "pre-snap", Entries = [] });
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(readerAdapter);
        A.CallTo(() => readerAdapter.ReadDeviceAsync("new-native-999", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => differ.ComputeDiff(A<Device>._, A<Device>._)).Returns(diff);

        var command = new CLI.Commands.Device.DeviceReplaceCommand(
            repo, snapshotManager, applier, differ, adapterFactory, interaction, formatter, NullLogger<CLI.Commands.Device.DeviceReplaceCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceReplaceOptions { DeviceId = "dev1", NewNativeId = "new-native-999" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Trigger == "pre-replace"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repo.SaveDeviceAsync(
            A<Device>.That.Matches(d => d.NativeId == "new-native-999"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => applier.ApplyDiffAsync(diff, readerAdapter, "new-native-999", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeviceReplace_Cancelled_DoesNotReplace()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var snapshotManager = A.Fake<ISnapshotManager>();
        var applier = A.Fake<IConfigApplier>();
        var differ = A.Fake<IConfigDiffer>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var device = CreateDevice("dev1", "Lamp");

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new CLI.Commands.Device.DeviceReplaceCommand(
            repo, snapshotManager, applier, differ, adapterFactory, interaction, formatter, NullLogger<CLI.Commands.Device.DeviceReplaceCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(
            new CLI.Commands.Device.DeviceReplaceOptions { DeviceId = "dev1", NewNativeId = "new-native" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("Cancelled");
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => repo.SaveDeviceAsync(A<Device>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task DeviceDiscover_AdapterConfigLookupFailure_ThrowsException()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var idGenerator = A.Fake<IIdGenerator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => repo.GetAdapterConfigAsync("missing", A<CancellationToken>._))
            .Throws(new KeyNotFoundException("Adapter config 'missing' not found"));

        var command = new CLI.Commands.Device.DeviceDiscoverCommand(
            repo, adapterFactory, idGenerator, interaction, formatter, NullLogger<CLI.Commands.Device.DeviceDiscoverCommand>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => command.ExecuteAsync(
                new CLI.Commands.Device.DeviceDiscoverOptions { AdapterId = "missing" }));
    }

    private static Device CreateDevice(string id, string name) =>
        new Device
    {
        Id = id,
        Name = name,
        Type = DeviceType.SwitchActuator,
        AdapterId = "hm1",
        NativeId = "123",
        GroupIds = [],
        Parameters = new Dictionary<string, ParameterValue>(),
        Channels = [],
        Relations = []
    };

    private static AdapterConfig CreateAdapterConfig() =>
        new AdapterConfig
    {
        AdapterId = "hm1",
        AdapterType = "homematic",
        Settings = new Dictionary<string, string>()
    };

    /// <summary>Test double that implements both ISmartHalAdapter and IDeviceWriter.</summary>
    public interface ITestWriterAdapter : ISmartHalAdapter, IDeviceWriter;

    /// <summary>Test double that implements both ISmartHalAdapter and IDeviceReader.</summary>
    public interface ITestReaderAdapter : ISmartHalAdapter, IDeviceReader;

    /// <summary>Test double that implements both ISmartHalAdapter and IDeviceDiscovery.</summary>
    public interface ITestDiscoveryAdapter : ISmartHalAdapter, IDeviceDiscovery;
}
