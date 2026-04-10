using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Config;

/// <summary>Options for the config init command.</summary>
public class ConfigInitOptions
{
    /// <summary>Override the configuration path.</summary>
    [OptionParameter('p', "path", HelpText = "Override the configuration path")]
    public string? Path { get; set; }
}

/// <summary>
/// Initializes the SmartHal configuration directory structure.
/// </summary>
[UsedImplicitly]
[CliCommand(["config", "init"], Name = "init", Description = "Initialize a new configuration directory")]
public class ConfigInitCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    IUserInteraction interaction) : ICliCommand<ConfigInitOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigInitOptions options)
    {
        var configPath = options.Path ?? cliContext.ConfigPath;

        var secretsProvider = interaction.ReadLine("Secrets provider [auto/env/file]: ")?.Trim();
        if (string.IsNullOrWhiteSpace(secretsProvider))
        {
            secretsProvider = "auto";
        }

        var meta = new MetaConfig
        {
            SchemaVersion = "1.0",
            SecretsProvider = secretsProvider,
            ConfigPath = configPath
        };

        await configRepository.InitializeAsync(meta).ConfigureAwait(false);

        OutputFormatter.WriteSuccess($"Configuration initialized at {configPath}");
        return CommandResult.Success;
    }
}
