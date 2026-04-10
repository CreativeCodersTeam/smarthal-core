using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Secrets;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>Options for the secret delete command.</summary>
public class SecretDeleteOptions
{
    /// <summary>The secret key to delete.</summary>
    [OptionValue(0, HelpText = "The secret key to delete")]
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Deletes a secret after confirmation.
/// </summary>
[UsedImplicitly]
[CliCommand(["secret", "delete"], Name = "delete", Description = "Delete a secret")]
public class SecretDeleteCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    SecretsProviderFactory secretsFactory,
    IUserInteraction interaction) : ICliCommand<SecretDeleteOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(SecretDeleteOptions options)
    {
        if (!interaction.Confirm($"Delete secret '{options.Key}'?"))
        {
            OutputFormatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        var meta = await configRepository.GetMetaAsync().ConfigureAwait(false);
        var provider = secretsFactory.Create(meta.SecretsProvider, cliContext.ConfigPath);

        await provider.DeleteSecretAsync(options.Key).ConfigureAwait(false);

        OutputFormatter.WriteSuccess($"Secret '{options.Key}' deleted.");
        return CommandResult.Success;
    }
}
