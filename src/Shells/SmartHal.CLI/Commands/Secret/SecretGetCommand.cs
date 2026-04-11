using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Secrets;
using Spectre.Console;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>
/// Retrieves and displays a secret value.
/// </summary>
[UsedImplicitly]
[CliCommand(["secret", "get"], Name = "get", Description = "Get a secret value")]
public class SecretGetCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    SecretsProviderFactory secretsFactory,
    IAnsiConsole console,
    ILogger<SecretGetCommand> logger) : ICliCommand<SecretGetOptions>
{
    private readonly CliContext _cliContext = Ensure.NotNull(cliContext);
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly SecretsProviderFactory _secretsFactory = Ensure.NotNull(secretsFactory);
    private readonly IAnsiConsole _console = Ensure.NotNull(console);
    private readonly ILogger<SecretGetCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretGetOptions options)
    {
        _logger.LogInformation("Getting secret {Key}", options.Key);
        var meta = await _configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = _secretsFactory.Create(meta.SecretsProvider, _cliContext.ConfigPath);

        var value = await provider.GetSecretAsync(options.Key).ConfigureAwait(false);

        // Value goes to stdout for piping
        _console.WriteLine(value);
        return CommandResult.Success;
    }
}
