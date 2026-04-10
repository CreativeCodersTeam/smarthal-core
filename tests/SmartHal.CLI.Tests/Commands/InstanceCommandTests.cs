using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Commands.Instance;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using Spectre.Console.Testing;

namespace SmartHal.CLI.Commands;

public class InstanceCommandTests
{
    [Fact]
    public async Task InstanceList_DisplaysConfigs()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig>
            {
                new AdapterConfig { AdapterId = "hm1", AdapterType = "homematic", Settings = new Dictionary<string, string> { ["host"] = "localhost" } },
                new AdapterConfig { AdapterId = "zb1", AdapterType = "zigbee", Settings = new Dictionary<string, string>() }
            });

        var command = new InstanceListCommand(repo, formatter);

        // Act
        var result = await command.ExecuteAsync();

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("hm1");
        console.Output.Should().Contain("zb1");
    }

    [Fact]
    public async Task InstanceAdd_UnknownType_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => adapterFactory.GetAvailableAdapterTypes())
            .Returns(new List<string> { "homematic" });

        var command = new InstanceAddCommand(repo, adapterFactory, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceAddOptions
        {
            AdapterType = "unknown_type",
            InstanceId = "inst1"
        });

        // Assert
        result.ExitCode.Should().Be(1);
        console.Output.Should().Contain("Unknown adapter type");
        console.Output.Should().Contain("homematic");
    }

    [Fact]
    public async Task InstanceAdd_ValidType_SavesConfig()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => adapterFactory.GetAvailableAdapterTypes())
            .Returns(new List<string> { "homematic" });

        // Simulate: enter key "host", value "192.168.1.1", then empty to finish
        var readLineCallIndex = 0;
        var readLineResponses = new[] { "host", "192.168.1.1", "" };
        A.CallTo(() => interaction.ReadLine(A<string>._))
            .ReturnsLazily(() => readLineResponses[readLineCallIndex++]);

        var command = new InstanceAddCommand(repo, adapterFactory, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceAddOptions
        {
            AdapterType = "homematic",
            InstanceId = "hm1"
        });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c =>
                c.AdapterId == "hm1" &&
                c.AdapterType == "homematic" &&
                c.Settings["host"] == "192.168.1.1"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InstanceAdd_NoSettings_SavesEmptyConfig()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => adapterFactory.GetAvailableAdapterTypes())
            .Returns(new List<string> { "homematic" });

        // Immediately press enter to finish settings
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("");

        var command = new InstanceAddCommand(repo, adapterFactory, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceAddOptions
        {
            AdapterType = "homematic",
            InstanceId = "hm1"
        });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c => c.Settings.Count == 0),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InstanceAdd_SecretKeyInput_UsesReadSecret()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => adapterFactory.GetAvailableAdapterTypes())
            .Returns(new List<string> { "homematic" });

        // Enter "api_key" as key, then empty to finish
        var readLineCallIndex = 0;
        A.CallTo(() => interaction.ReadLine(A<string>._))
            .ReturnsLazily(() => readLineCallIndex++ == 0 ? "api_key" : "");
        A.CallTo(() => interaction.ReadSecret(A<string>._)).Returns("s3cret");

        var command = new InstanceAddCommand(repo, adapterFactory, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceAddOptions
        {
            AdapterType = "homematic",
            InstanceId = "hm1"
        });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => interaction.ReadSecret(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c => c.Settings["api_key"] == "s3cret"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InstanceAdd_WhitespaceOnlyKey_TreatedAsEmpty_FinishesLoop()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => adapterFactory.GetAvailableAdapterTypes())
            .Returns(new List<string> { "homematic" });

        // Whitespace-only input should be trimmed to empty and finish the loop
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("   ");

        var command = new InstanceAddCommand(repo, adapterFactory, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceAddOptions
        {
            AdapterType = "homematic",
            InstanceId = "hm1"
        });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c => c.Settings.Count == 0),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InstanceAdd_EmptyValue_DoesNotAddSetting()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => adapterFactory.GetAvailableAdapterTypes())
            .Returns(new List<string> { "homematic" });

        // Enter key "host", empty value, then empty key to finish
        var readLineCallIndex = 0;
        var readLineResponses = new[] { "host", "", "" };
        A.CallTo(() => interaction.ReadLine(A<string>._))
            .ReturnsLazily(() => readLineResponses[readLineCallIndex++]);

        var command = new InstanceAddCommand(repo, adapterFactory, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceAddOptions
        {
            AdapterType = "homematic",
            InstanceId = "hm1"
        });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c => c.Settings.Count == 0),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InstanceRemove_Confirmed_RemovesInstance()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._))
            .Returns(new AdapterConfig { AdapterId = "hm1", AdapterType = "homematic", Settings = new Dictionary<string, string>() });
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);

        var command = new InstanceRemoveCommand(repo, interaction, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceRemoveOptions { InstanceId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("removed");
    }

    [Fact]
    public async Task InstanceRemove_Cancelled_DoesNotRemove()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._))
            .Returns(new AdapterConfig { AdapterId = "hm1", AdapterType = "homematic", Settings = new Dictionary<string, string>() });
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new InstanceRemoveCommand(repo, interaction, formatter);

        // Act
        var result = await command.ExecuteAsync(new InstanceRemoveOptions { InstanceId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("Cancelled");
    }
}
