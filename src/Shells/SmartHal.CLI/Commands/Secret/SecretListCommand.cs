using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Secrets;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>
/// Lists all secret keys (without values).
/// </summary>
[UsedImplicitly]
[CliCommand(["secret", "list"], Name = "list", Description = "List all secret keys")]
public class SecretListCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    SecretsProviderFactory secretsFactory,
    OutputFormatter formatter,
    ILogger<SecretListCommand> logger) : ICliCommand
{
    private readonly ILogger<SecretListCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync()
    {
        _logger.LogInformation("Listing secrets");
        var meta = await configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = secretsFactory.Create(meta.SecretsProvider, cliContext.ConfigPath);

        var keys = await provider.ListKeysAsync().ConfigureAwait(false);

        formatter.WriteTable(
            keys.ToList(),
            ("Key", k => k));

        return CommandResult.Success;
    }
}
