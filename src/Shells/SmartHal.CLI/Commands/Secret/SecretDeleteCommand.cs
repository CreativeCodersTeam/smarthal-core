using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Secrets;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>
/// Deletes a secret after confirmation.
/// </summary>
[UsedImplicitly]
[CliCommand(["secret", "delete"], Name = "delete", Description = "Delete a secret")]
public class SecretDeleteCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    SecretsProviderFactory secretsFactory,
    IUserInteraction interaction,
    OutputFormatter formatter,
    ILogger<SecretDeleteCommand> logger) : ICliCommand<SecretDeleteOptions>
{
    private readonly ILogger<SecretDeleteCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretDeleteOptions options)
    {
        _logger.LogInformation("Deleting secret {Key}", options.Key);
        if (!interaction.Confirm($"Delete secret '{options.Key}'?"))
        {
            formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        var meta = await configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = secretsFactory.Create(meta.SecretsProvider, cliContext.ConfigPath);

        await provider.DeleteSecretAsync(options.Key).ConfigureAwait(false);

        formatter.WriteSuccess($"Secret '{options.Key}' deleted.");
        return CommandResult.Success;
    }
}
