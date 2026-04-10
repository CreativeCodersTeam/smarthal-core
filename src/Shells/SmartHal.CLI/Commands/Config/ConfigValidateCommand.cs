using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Config;

/// <summary>Options for the config validate command.</summary>
public class ConfigValidateOptions
{
    /// <summary>Run full validation (structure + semantic).</summary>
    [OptionParameter('f', "full", HelpText = "Run full validation (structure + semantic)")]
    public bool Full { get; set; }
}

/// <summary>
/// Validates the SmartHal configuration.
/// </summary>
[UsedImplicitly]
[CliCommand(["config", "validate"], Name = "validate", Description = "Validate the configuration")]
public class ConfigValidateCommand(
    CliContext cliContext,
    IConfigValidator validator,
    IConfigRepository configRepository) : ICliCommand<ConfigValidateOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigValidateOptions options)
    {
        var result = await validator.ValidateStructureAsync(cliContext.ConfigPath).ConfigureAwait(false);

        if (options.Full && result.IsValid)
        {
            var semanticResult = await validator.ValidateSemanticAsync(configRepository).ConfigureAwait(false);
            result = MergeResults(result, semanticResult);
        }

        if (result.IsValid && result.Warnings.Count == 0)
        {
            OutputFormatter.WriteSuccess("Configuration is valid.");
            return CommandResult.Success;
        }

        OutputFormatter.WriteValidationResults(
            result.Errors.Select(e => (e.Code, e.Message, e.FilePath)).ToList(),
            result.Warnings.Select(w => (w.Code, w.Message, w.FilePath)).ToList());

        return result.IsValid ? CommandResult.Success : new CommandResult(ExitCodes.ConfigValidationError);
    }

    private static ValidationResult MergeResults(ValidationResult a, ValidationResult b)
    {
        var merged = new ValidationResult();
        merged.Errors.AddRange(a.Errors);
        merged.Errors.AddRange(b.Errors);
        merged.Warnings.AddRange(a.Warnings);
        merged.Warnings.AddRange(b.Warnings);
        return merged;
    }
}
