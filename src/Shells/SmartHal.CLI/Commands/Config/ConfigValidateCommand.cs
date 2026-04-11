using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Config;

/// <summary>
/// Validates the SmartHal configuration.
/// </summary>
[UsedImplicitly]
[CliCommand(["config", "validate"], Name = "validate", Description = "Validate the configuration")]
public class ConfigValidateCommand(
    CliContext cliContext,
    IConfigValidator validator,
    IConfigRepository configRepository,
    OutputFormatter formatter,
    ILogger<ConfigValidateCommand> logger) : ICliCommand<ConfigValidateOptions>
{
    private readonly CliContext _cliContext = Ensure.NotNull(cliContext);
    private readonly IConfigValidator _validator = Ensure.NotNull(validator);
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<ConfigValidateCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigValidateOptions options)
    {
        _logger.LogInformation("Validating config at {ConfigPath}", _cliContext.ConfigPath);

        var result = await _validator.ValidateStructureAsync(_cliContext.ConfigPath).ConfigureAwait(false);

        if (options.Full && result.IsValid)
        {
            var semanticResult = await _validator.ValidateSemanticAsync(_configRepository).ConfigureAwait(false);
            result = MergeResults(result, semanticResult);
        }

        if (result.IsValid && result.Warnings.Count == 0)
        {
            _formatter.WriteSuccess("Configuration is valid.");
            return CommandResult.Success;
        }

        _formatter.WriteValidationResults(
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
