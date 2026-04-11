using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
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
    private readonly CliContext _cliContext = Ensure.NotNull(cliContext);
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly SecretsProviderFactory _secretsFactory = Ensure.NotNull(secretsFactory);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<SecretListCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync()
    {
        _logger.LogInformation("Listing secrets");
        var meta = await _configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = _secretsFactory.Create(meta.SecretsProvider, _cliContext.ConfigPath);

        var keys = await provider.ListKeysAsync().ConfigureAwait(false);

        _formatter.WriteTable(
            keys.ToList(),
            ("Key", k => k));

        return CommandResult.Success;
    }
}
