using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Commands.Config;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using Spectre.Console.Testing;

namespace SmartHal.CLI.Commands;

public class ConfigCommandTests
{
    [Fact]
    public async Task ConfigInit_InitializesRepository()
    {
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("env");

        var command = new ConfigInitCommand(cliContext, repo, interaction, formatter);

        var result = await command.ExecuteAsync(new ConfigInitOptions());

        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.InitializeAsync(
            A<MetaConfig>.That.Matches(m => m.SecretsProvider == "env"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ConfigInit_EmptyInput_DefaultsToAuto()
    {
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var repo = A.Fake<IConfigRepository>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => interaction.ReadLine(A<string>._)).Returns("");

        var command = new ConfigInitCommand(cliContext, repo, interaction, formatter);

        var result = await command.ExecuteAsync(new ConfigInitOptions());

        result.Should().Be(CommandResult.Success);
        A.CallTo(() => repo.InitializeAsync(
            A<MetaConfig>.That.Matches(m => m.SecretsProvider == "auto"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ConfigValidate_Valid_ReturnsSuccess()
    {
        var cliContext = new CliContext { ConfigPath = "/tmp/test" };
        var validator = A.Fake<IConfigValidator>();
        var repo = A.Fake<IConfigRepository>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(cliContext, console);
        A.CallTo(() => validator.ValidateStructureAsync(cliContext.ConfigPath, A<CancellationToken>._))
            .Returns(new ValidationResult());

        var command = new ConfigValidateCommand(cliContext, validator, repo, formatter);

        var result = await command.ExecuteAsync(new ConfigValidateOptions());

        result.Should().Be(CommandResult.Success);
    }

    [Fact]
    public async Task ConfigValidate_WithErrors_ReturnsConfigValidationError()
    {
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

        var result = await command.ExecuteAsync(new ConfigValidateOptions());
        result.ExitCode.Should().Be(Core.ExitCodes.ConfigValidationError);
    }

    [Fact]
    public async Task ConfigValidate_Full_RunsSemanticValidation()
    {
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

        var result = await command.ExecuteAsync(new ConfigValidateOptions { Full = true });

        result.Should().Be(CommandResult.Success);
        A.CallTo(() => validator.ValidateSemanticAsync(repo, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
