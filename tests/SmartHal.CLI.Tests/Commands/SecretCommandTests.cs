using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Commands.Secret;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core;
using SmartHal.Core.Secrets;

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
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory();

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });

        // Set the env var that the provider expects (prefix SMARTHAL_)
        Environment.SetEnvironmentVariable("SMARTHAL_TEST_KEY", "hello_world");

        var command = new SecretGetCommand(cliContext, repo, factory);

        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var result = await command.ExecuteAsync(new SecretGetOptions { Key = "test.key" });
            result.Should().Be(CommandResult.Success);
            writer.ToString().Trim().Should().Be("hello_world");
        }
        finally
        {
            Console.SetOut(original);
            Environment.SetEnvironmentVariable("SMARTHAL_TEST_KEY", null);
        }
    }

    [Fact]
    public async Task SecretSet_ReadOnly_ThrowsException()
    {
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory();
        var interaction = A.Fake<IUserInteraction>();

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });

        var command = new SecretSetCommand(cliContext, repo, factory, interaction);

        // Env provider is read-only, so set should throw
        await Assert.ThrowsAsync<SmartHalSecretsProviderException>(
            () => command.ExecuteAsync(new SecretSetOptions { Key = "test.key", Value = "val" }));
    }

    [Fact]
    public async Task SecretDelete_Cancelled_DoesNotProceed()
    {
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory();
        var interaction = A.Fake<IUserInteraction>();
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new SecretDeleteCommand(cliContext, repo, factory, interaction);

        var origErr = Console.Error;
        Console.SetError(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync(new SecretDeleteOptions { Key = "some_key" });
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetError(origErr);
        }

        // When cancelled, the meta config should never be loaded
        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task SecretList_RunsSuccessfully()
    {
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var factory = new SecretsProviderFactory();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json });

        A.CallTo(() => repo.GetMetaAsync(A<CancellationToken>._))
            .Returns(new MetaConfig { SecretsProvider = "env" });

        var command = new SecretListCommand(cliContext, repo, factory, formatter);

        var original = Console.Out;
        Console.SetOut(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync();
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetOut(original);
        }
    }
}
