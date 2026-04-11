using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using SmartHal.CLI.Commands.Secret;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core;
using SmartHal.Core.Secrets;
using Spectre.Console.Testing;

namespace SmartHal.CLI.Commands;

/// <summary>
/// Tests for secret commands. Since <see cref="SecretsProviderFactory.Create"/> is non-virtual,
/// these tests use the real env provider where possible and focus on command flow.
/// </summary>
public class SecretCommandTests
{
    [Fact]
    public async Task SecretGet_ReturnsValueToStdout()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory(NullLogger<SecretsProviderFactory>.Instance);
        var console = new TestConsole();

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });

        Environment.SetEnvironmentVariable("SMARTHAL_TEST_KEY", "hello_world");

        var command = new SecretGetCommand(cliContext, repo, factory, console, NullLogger<SecretGetCommand>.Instance);

        try
        {
            // Act
            var result = await command.ExecuteAsync(new SecretGetOptions { Key = "test.key" });

            // Assert
            result.Should().Be(CommandResult.Success);
            console.Output.Trim().Should().Be("hello_world");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SMARTHAL_TEST_KEY", null);
        }
    }

    [Fact]
    public async Task SecretSet_WithValueOption_SetsDirectly()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory(NullLogger<SecretsProviderFactory>.Instance);
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });

        var command = new SecretSetCommand(cliContext, repo, factory, interaction, formatter, NullLogger<SecretSetCommand>.Instance);

        // Act & Assert — Env provider is read-only, so set should throw
        await Assert.ThrowsAsync<SmartHalSecretsProviderException>(
            () => command.ExecuteAsync(new SecretSetOptions { Key = "test.key", Value = "val" }));

        // When value is provided via option, ReadSecret should never be called
        A.CallTo(() => interaction.ReadSecret(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task SecretSet_WithoutValue_ReadsInteractively()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory(NullLogger<SecretsProviderFactory>.Instance);
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });
        A.CallTo(() => interaction.ReadSecret(A<string>._)).Returns("interactive_val");

        var command = new SecretSetCommand(cliContext, repo, factory, interaction, formatter, NullLogger<SecretSetCommand>.Instance);

        // Act & Assert — Env provider is read-only, so set should throw after reading
        await Assert.ThrowsAsync<SmartHalSecretsProviderException>(
            () => command.ExecuteAsync(new SecretSetOptions { Key = "test.key" }));

        // ReadSecret should have been called since no value was provided
        A.CallTo(() => interaction.ReadSecret(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SecretDelete_Cancelled_DoesNotProceed()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory(NullLogger<SecretsProviderFactory>.Instance);
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new SecretDeleteCommand(cliContext, repo, factory, interaction, formatter, NullLogger<SecretDeleteCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync(new SecretDeleteOptions { Key = "some_key" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("Cancelled");

        // When cancelled, the meta config should never be loaded
        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task SecretDelete_Confirmed_DeletesSecret()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory(NullLogger<SecretsProviderFactory>.Instance);
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);
        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });

        var command = new SecretDeleteCommand(cliContext, repo, factory, interaction, formatter, NullLogger<SecretDeleteCommand>.Instance);

        // Act & Assert — Env provider is read-only, so delete should throw
        await Assert.ThrowsAsync<SmartHalSecretsProviderException>(
            () => command.ExecuteAsync(new SecretDeleteOptions { Key = "some_key" }));

        // Meta should have been loaded since user confirmed
        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SecretSet_GetMetaFailure_ThrowsException()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory(NullLogger<SecretsProviderFactory>.Instance);
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Throws(new InvalidOperationException("Config not initialized"));

        var command = new SecretSetCommand(cliContext, repo, factory, interaction, formatter, NullLogger<SecretSetCommand>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => command.ExecuteAsync(new SecretSetOptions { Key = "test.key", Value = "val" }));
    }

    [Fact]
    public async Task SecretList_RunsSuccessfully()
    {
        // Arrange
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory(NullLogger<SecretsProviderFactory>.Instance);
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });

        var command = new SecretListCommand(cliContext, repo, factory, formatter, NullLogger<SecretListCommand>.Instance);

        // Act
        var result = await command.ExecuteAsync();

        // Assert
        result.Should().Be(CommandResult.Success);
    }
}
