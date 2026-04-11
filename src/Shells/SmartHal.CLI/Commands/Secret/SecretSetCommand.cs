using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Secrets;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>
/// Sets a secret value.
/// </summary>
[UsedImplicitly]
[CliCommand(["secret", "set"], Name = "set", Description = "Set a secret")]
public class SecretSetCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    SecretsProviderFactory secretsFactory,
    IUserInteraction interaction,
    OutputFormatter formatter,
    ILogger<SecretSetCommand> logger) : ICliCommand<SecretSetOptions>
{
    private readonly ILogger<SecretSetCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretSetOptions options)
    {
        _logger.LogInformation("Setting secret {Key}", options.Key);
        var meta = await configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = secretsFactory.Create(meta.SecretsProvider, cliContext.ConfigPath);

        var value = options.Value ?? interaction.ReadSecret($"Enter value for '{options.Key}': ");

        await provider.SetSecretAsync(options.Key, value).ConfigureAwait(false);

        formatter.WriteSuccess($"Secret '{options.Key}' set.");
        return CommandResult.Success;
    }
}
