using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Commands.Config;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;
using Spectre.Console.Testing;

namespace SmartHal.CLI.Commands;

public class ConfigCommandTests
{
    [Fact]
    public async Task ConfigInit_InitializesRepository()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("env");

        var command = new ConfigInitCommand(cliContext, repo, interaction, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigInitOptions());

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.InitializeAsync(
            A<MetaConfig>.That.Matches(m => m.SecretsProvider == "env"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ConfigInit_EmptyInput_DefaultsToAuto()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("");

        var command = new ConfigInitCommand(cliContext, repo, interaction, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigInitOptions());

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.InitializeAsync(
            A<MetaConfig>.That.Matches(m => m.SecretsProvider == "auto"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ConfigInit_CustomPath_OverridesCliContextPath()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("file");

        var command = new ConfigInitCommand(cliContext, repo, interaction, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigInitOptions { Path = "/custom/path" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.InitializeAsync(
            A<MetaConfig>.That.Matches(m => m.ConfigPath == "/custom/path"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ConfigValidate_Valid_ReturnsSuccess()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var validator = A.Fake<IConfigValidator>();
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => validator.ValidateStructureAsync(cliContext.ConfigPath, A<CancellationToken>._))
            .Returns(new ValidationResult());

        var command = new ConfigValidateCommand(cliContext, validator, repo, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigValidateOptions());

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("valid");
    }

    [Fact]
    public async Task ConfigValidate_WithErrors_ReturnsConfigValidationError()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var validator = A.Fake<IConfigValidator>();
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationError { Code = "CFG-001", Message = "Bad config" });
        A.CallTo(() => validator.ValidateStructureAsync(cliContext.ConfigPath, A<CancellationToken>._))
            .Returns(validationResult);

        var command = new ConfigValidateCommand(cliContext, validator, repo, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigValidateOptions());

        // Assert
        result.ExitCode.Should().Be(Core.ExitCodes.ConfigValidationError);
        console.Output.Should().Contain("1 error(s)");
    }

    [Fact]
    public async Task ConfigValidate_Full_RunsSemanticValidation()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var validator = A.Fake<IConfigValidator>();
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => validator.ValidateStructureAsync(cliContext.ConfigPath, A<CancellationToken>._))
            .Returns(new ValidationResult());
        A.CallTo(() => validator.ValidateSemanticAsync(repo, A<CancellationToken>._))
            .Returns(new ValidationResult());

        var command = new ConfigValidateCommand(cliContext, validator, repo, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigValidateOptions { Full = true });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => validator.ValidateSemanticAsync(repo, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ConfigValidate_FullWithStructureErrors_SkipsSemantic()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var validator = A.Fake<IConfigValidator>();
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);

        var structureResult = new ValidationResult();
        structureResult.Errors.Add(new ValidationError { Code = "CFG-001", Message = "Missing file" });
        A.CallTo(() => validator.ValidateStructureAsync(cliContext.ConfigPath, A<CancellationToken>._))
            .Returns(structureResult);

        var command = new ConfigValidateCommand(cliContext, validator, repo, formatter);

        // Act
        await command.ExecuteAsync(new ConfigValidateOptions { Full = true });

        // Assert
        A.CallTo(() => validator.ValidateSemanticAsync(A<IConfigRepository>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ConfigValidate_WarningsOnly_ReturnsSuccess()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var validator = A.Fake<IConfigValidator>();
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);

        var validationResult = new ValidationResult();
        validationResult.Warnings.Add(new ValidationWarning { Code = "CFG-W01", Message = "Empty room" });
        A.CallTo(() => validator.ValidateStructureAsync(cliContext.ConfigPath, A<CancellationToken>._))
            .Returns(validationResult);

        var command = new ConfigValidateCommand(cliContext, validator, repo, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigValidateOptions());

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("1 warning(s)");
    }

    [Fact]
    public async Task ConfigApply_NoChanges_ReturnsSuccess()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var applier = A.Fake<IConfigApplier>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var readerAdapter = A.Fake<ITestReaderAdapter>();

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(readerAdapter);
        A.CallTo(() => readerAdapter.ReadDeviceAsync("native1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => differ.ComputeDiff(A<Core.Devices.Device>._, A<Core.Devices.Device>._)).Returns(new DeviceDiff());

        var command = new ConfigApplyCommand(repo, differ, applier, adapterFactory, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigApplyOptions { DeviceId = "dev1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("No changes");
    }

    [Fact]
    public async Task ConfigApply_AdapterWithoutReader_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var applier = A.Fake<IConfigApplier>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var adapter = A.Fake<ISmartHalAdapter>();

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapter);

        var command = new ConfigApplyCommand(repo, differ, applier, adapterFactory, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigApplyOptions { DeviceId = "dev1" });

        // Assert
        result.ExitCode.Should().Be(1);
        console.Output.Should().Contain("does not support reading");
    }

    [Fact]
    public async Task ConfigApply_DryRun_DoesNotApply()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var applier = A.Fake<IConfigApplier>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var readerAdapter = A.Fake<ITestReaderAdapter>();
        var diff = new DeviceDiff
        {
            Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "level", OldValue = "0", NewValue = "1" }]
        };

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(readerAdapter);
        A.CallTo(() => readerAdapter.ReadDeviceAsync("native1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => differ.ComputeDiff(A<Core.Devices.Device>._, A<Core.Devices.Device>._)).Returns(diff);

        var command = new ConfigApplyCommand(repo, differ, applier, adapterFactory, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigApplyOptions { DeviceId = "dev1", DryRun = true });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("Dry run");
        A.CallTo(() => applier.ApplyDiffAsync(A<DeviceDiff>._, A<ISmartHalAdapter>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ConfigApply_WithChanges_AppliesDiff()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var applier = A.Fake<IConfigApplier>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var readerAdapter = A.Fake<ITestReaderAdapter>();
        var diff = new DeviceDiff
        {
            Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "level", OldValue = "0", NewValue = "1" }]
        };

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(readerAdapter);
        A.CallTo(() => readerAdapter.ReadDeviceAsync("native1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => differ.ComputeDiff(A<Core.Devices.Device>._, A<Core.Devices.Device>._)).Returns(diff);

        var command = new ConfigApplyCommand(repo, differ, applier, adapterFactory, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigApplyOptions { DeviceId = "dev1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => applier.ApplyDiffAsync(diff, readerAdapter, "native1", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        console.Output.Should().Contain("1 change(s)");
    }

    [Fact]
    public async Task ConfigDiff_NoChanges_ReturnsSuccess()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var readerAdapter = A.Fake<ITestReaderAdapter>();

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(readerAdapter);
        A.CallTo(() => readerAdapter.ReadDeviceAsync("native1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => differ.ComputeDiff(A<Core.Devices.Device>._, A<Core.Devices.Device>._)).Returns(new DeviceDiff());

        var command = new ConfigDiffCommand(repo, differ, adapterFactory, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigDiffOptions { DeviceId = "dev1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("No differences");
    }

    [Fact]
    public async Task ConfigDiff_AdapterWithoutReader_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var adapter = A.Fake<ISmartHalAdapter>();

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapter);

        var command = new ConfigDiffCommand(repo, differ, adapterFactory, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigDiffOptions { DeviceId = "dev1" });

        // Assert
        result.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task ConfigDiff_WithChanges_DisplaysTable()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var readerAdapter = A.Fake<ITestReaderAdapter>();
        var diff = new DeviceDiff
        {
            Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "brightness", OldValue = "50", NewValue = "100" }]
        };

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(readerAdapter);
        A.CallTo(() => readerAdapter.ReadDeviceAsync("native1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => differ.ComputeDiff(A<Core.Devices.Device>._, A<Core.Devices.Device>._)).Returns(diff);

        var command = new ConfigDiffCommand(repo, differ, adapterFactory, formatter);

        // Act
        var result = await command.ExecuteAsync(new ConfigDiffOptions { DeviceId = "dev1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("brightness");
    }

    [Fact]
    public async Task ConfigApply_DeviceLookupFailure_ThrowsException()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var applier = A.Fake<IConfigApplier>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => repo.GetDeviceAsync("missing", A<CancellationToken>._))
            .Throws(new KeyNotFoundException("Device 'missing' not found"));

        var command = new ConfigApplyCommand(repo, differ, applier, adapterFactory, formatter);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => command.ExecuteAsync(new ConfigApplyOptions { DeviceId = "missing" }));
    }

    [Fact]
    public async Task ConfigApply_AdapterConfigLookupFailure_ThrowsException()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var applier = A.Fake<IConfigApplier>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._))
            .Throws(new KeyNotFoundException("Adapter config 'hm1' not found"));

        var command = new ConfigApplyCommand(repo, differ, applier, adapterFactory, formatter);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => command.ExecuteAsync(new ConfigApplyOptions { DeviceId = "dev1" }));
    }

    [Fact]
    public async Task ConfigApply_ApplyDiffFailure_ThrowsException()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var differ = A.Fake<IConfigDiffer>();
        var applier = A.Fake<IConfigApplier>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var device = CreateDevice();
        var adapterConfig = CreateAdapterConfig();
        var readerAdapter = A.Fake<ITestReaderAdapter>();
        var diff = new DeviceDiff
        {
            Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "level", OldValue = "0", NewValue = "1" }]
        };

        A.CallTo(() => repo.GetDeviceAsync("dev1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(readerAdapter);
        A.CallTo(() => readerAdapter.ReadDeviceAsync("native1", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => differ.ComputeDiff(A<Core.Devices.Device>._, A<Core.Devices.Device>._)).Returns(diff);
        A.CallTo(() => applier.ApplyDiffAsync(A<DeviceDiff>._, A<ISmartHalAdapter>._, A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("Adapter communication failed"));

        var command = new ConfigApplyCommand(repo, differ, applier, adapterFactory, formatter);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => command.ExecuteAsync(new ConfigApplyOptions { DeviceId = "dev1" }));
    }

    private static Core.Devices.Device CreateDevice() =>
        new Core.Devices.Device
    {
        Id = "dev1",
        Name = "Test",
        Type = DeviceType.SwitchActuator,
        AdapterId = "hm1",
        NativeId = "native1",
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

    /// <summary>Test double that implements both ISmartHalAdapter and IDeviceReader.</summary>
    public interface ITestReaderAdapter : ISmartHalAdapter, IDeviceReader;
}
