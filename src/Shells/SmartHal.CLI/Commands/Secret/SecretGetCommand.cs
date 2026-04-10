using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Secrets;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>Options for the secret get command.</summary>
public class SecretGetOptions
{
    /// <summary>The secret key to retrieve.</summary>
    [OptionValue(0, HelpText = "The secret key")]
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Retrieves and displays a secret value.
/// </summary>
[UsedImplicitly]
[CliCommand(["secret", "get"], Name = "get", Description = "Get a secret value")]
public class SecretGetCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    SecretsProviderFactory secretsFactory) : ICliCommand<SecretGetOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretGetOptions options)
    {
        var meta = await configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = secretsFactory.Create(meta.SecretsProvider, cliContext.ConfigPath);

        var value = await provider.GetSecretAsync(options.Key).ConfigureAwait(false);

        // Value goes to stdout for piping
        Console.WriteLine(value);
        return CommandResult.Success;
    }
}
