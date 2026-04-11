using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
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
    private readonly CliContext _cliContext = Ensure.NotNull(cliContext);
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly SecretsProviderFactory _secretsFactory = Ensure.NotNull(secretsFactory);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<SecretSetCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretSetOptions options)
    {
        _logger.LogInformation("Setting secret {Key}", options.Key);
        var meta = await _configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = _secretsFactory.Create(meta.SecretsProvider, _cliContext.ConfigPath);

        var value = options.Value ?? _interaction.ReadSecret($"Enter value for '{options.Key}': ");

        await provider.SetSecretAsync(options.Key, value).ConfigureAwait(false);

        _formatter.WriteSuccess($"Secret '{options.Key}' set.");
        return CommandResult.Success;
    }
}
