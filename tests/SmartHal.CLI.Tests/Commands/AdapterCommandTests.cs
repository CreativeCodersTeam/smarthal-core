using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Commands.Adapter;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using Spectre.Console.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartHal.CLI.Commands;

public class AdapterCommandTests
{
    [Fact]
    public async Task AdapterList_DisplaysTypesAndConfigs()
    {
        // Arrange
        var adapterFactory = A.Fake<IAdapterFactory>();
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => adapterFactory.GetAvailableAdapterTypes())
            .Returns(new List<string> { "homematic", "zigbee" });
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig>
            {
                new AdapterConfig { AdapterId = "hm1", AdapterType = "homematic", Settings = new Dictionary<string, string> { ["host"] = "192.168.1.1" } }
            });

        var command = new AdapterListCommand(adapterFactory, repo, console, formatter, NullLogger<AdapterListCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync();

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("homematic");
        console.Output.Should().Contain("zigbee");
        console.Output.Should().Contain("hm1");
    }

    [Fact]
    public async Task AdapterTest_Successful_ReturnsSuccess()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var adapter = A.Fake<ISmartHalAdapter>();

        var adapterConfig = new AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string>()
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapter);
        A.CallTo(() => adapter.TestConnectionAsync(A<CancellationToken>._)).Returns(true);
        A.CallTo(() => adapter.DisplayName).Returns("HomeMatic CCU");

        var command = new AdapterTestCommand(repo, adapterFactory, formatter, NullLogger<AdapterTestCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterTestOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("successful");
    }

    [Fact]
    public async Task AdapterTest_Failed_ReturnsErrorExitCode()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var adapterFactory = A.Fake<IAdapterFactory>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        var adapter = A.Fake<ISmartHalAdapter>();

        var adapterConfig = new AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string>()
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(adapterConfig);
        A.CallTo(() => adapterFactory.CreateAdapter(adapterConfig)).Returns(adapter);
        A.CallTo(() => adapter.TestConnectionAsync(A<CancellationToken>._)).Returns(false);

        var command = new AdapterTestCommand(repo, adapterFactory, formatter, NullLogger<AdapterTestCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterTestOptions { AdapterId = "hm1" });

        // Assert
        result.ExitCode.Should().Be(1);
        console.Output.Should().Contain("failed");
    }

    [Fact]
    public async Task AdapterConfigure_NotFound_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => repo.GetAdapterConfigAsync("unknown", A<CancellationToken>._))
            .Throws(new Exception("Not found"));

        var command = new AdapterConfigureCommand(repo, interaction, console, formatter, NullLogger<AdapterConfigureCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterConfigureOptions { AdapterId = "unknown" });

        // Assert
        result.ExitCode.Should().Be(1);
        console.Output.Should().Contain("not found");
    }

    [Fact]
    public async Task AdapterConfigure_UpdatesSetting_SavesConfig()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var config = new AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string> { ["host"] = "192.168.1.1" }
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(config);
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("192.168.1.2");

        var command = new AdapterConfigureCommand(repo, interaction, console, formatter, NullLogger<AdapterConfigureCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterConfigureOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c => c.Settings["host"] == "192.168.1.2"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AdapterConfigure_NoChanges_DoesNotSave()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var config = new AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string> { ["host"] = "192.168.1.1" }
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(config);
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("");

        var command = new AdapterConfigureCommand(repo, interaction, console, formatter, NullLogger<AdapterConfigureCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterConfigureOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveAdapterConfigAsync(A<AdapterConfig>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        console.Output.Should().Contain("No changes");
    }

    [Fact]
    public async Task AdapterConfigure_SecretKey_UsesReadSecret()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var config = new AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string> { ["api_key"] = "old_secret" }
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(config);
        A.CallTo(() => interaction.ReadSecret(A<string>._)).Returns("new_secret");

        var command = new AdapterConfigureCommand(repo, interaction, console, formatter, NullLogger<AdapterConfigureCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterConfigureOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => interaction.ReadSecret(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c => c.Settings["api_key"] == "new_secret"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AdapterConfigure_MultipleSettings_UpdatesAll()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var config = new AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string>
            {
                ["host"] = "192.168.1.1",
                ["port"] = "2001"
            }
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(config);

        // First call for "host", second call for "port"
        var callIndex = 0;
        var responses = new[] { "10.0.0.1", "8080" };
        A.CallTo(() => interaction.ReadLine(A<string>._))
            .ReturnsLazily(() => responses[callIndex++]);

        var command = new AdapterConfigureCommand(repo, interaction, console, formatter, NullLogger<AdapterConfigureCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterConfigureOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.SaveAdapterConfigAsync(
            A<AdapterConfig>.That.Matches(c =>
                c.Settings["host"] == "10.0.0.1" && c.Settings["port"] == "8080"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AdapterConfigure_EmptySecretValue_DoesNotUpdateSetting()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var config = new AdapterConfig
        {
            AdapterId = "hm1",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string> { ["api_key"] = "old_secret" }
        };

        A.CallTo(() => repo.GetAdapterConfigAsync("hm1", A<CancellationToken>._)).Returns(config);
        A.CallTo(() => interaction.ReadSecret(A<string>._)).Returns("");

        var command = new AdapterConfigureCommand(repo, interaction, console, formatter, NullLogger<AdapterConfigureCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new AdapterConfigureOptions { AdapterId = "hm1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        // Empty secret means "skip" — config should not be saved
        A.CallTo(() => repo.SaveAdapterConfigAsync(A<AdapterConfig>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
