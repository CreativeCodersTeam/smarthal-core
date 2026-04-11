using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
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
    private readonly CliContext _cliContext = Ensure.NotNull(cliContext);
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly SecretsProviderFactory _secretsFactory = Ensure.NotNull(secretsFactory);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<SecretDeleteCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretDeleteOptions options)
    {
        _logger.LogInformation("Deleting secret {Key}", options.Key);
        if (!_interaction.Confirm($"Delete secret '{options.Key}'?"))
        {
            _formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        var meta = await _configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = _secretsFactory.Create(meta.SecretsProvider, _cliContext.ConfigPath);

        await provider.DeleteSecretAsync(options.Key).ConfigureAwait(false);

        _formatter.WriteSuccess($"Secret '{options.Key}' deleted.");
        return CommandResult.Success;
    }
}
