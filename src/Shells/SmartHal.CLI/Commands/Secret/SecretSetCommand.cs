using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Secrets;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>Options for the secret set command.</summary>
public class SecretSetOptions
{
    /// <summary>The secret key.</summary>
    [OptionValue(0, HelpText = "The secret key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>The secret value (if not provided, reads interactively).</summary>
    [OptionParameter('v', "value", HelpText = "The secret value")]
    public string? Value { get; set; }
}

/// <summary>
/// Sets a secret value.
/// </summary>
[UsedImplicitly]
[CliCommand(["secret", "set"], Name = "set", Description = "Set a secret")]
public class SecretSetCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    SecretsProviderFactory secretsFactory,
    IUserInteraction interaction) : ICliCommand<SecretSetOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretSetOptions options)
    {
        var meta = await configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = secretsFactory.Create(meta.SecretsProvider, cliContext.ConfigPath);

        var value = options.Value ?? interaction.ReadSecret($"Enter value for '{options.Key}': ");

        await provider.SetSecretAsync(options.Key, value).ConfigureAwait(false);

        OutputFormatter.WriteSuccess($"Secret '{options.Key}' set.");
        return CommandResult.Success;
    }
}
