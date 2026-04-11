using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Config;

/// <summary>
/// Initializes the SmartHal configuration directory structure.
/// </summary>
[UsedImplicitly]
[CliCommand(["config", "init"], Name = "init", Description = "Initialize a new configuration directory")]
public class ConfigInitCommand(
    CliContext cliContext,
    IConfigRepository configRepository,
    IUserInteraction interaction,
    OutputFormatter formatter,
    ILogger<ConfigInitCommand> logger) : ICliCommand<ConfigInitOptions>
{
    private readonly ILogger<ConfigInitCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigInitOptions options)
    {
        var configPath = options.Path ?? cliContext.ConfigPath;

        _logger.LogInformation("Initializing config at {ConfigPath}", configPath);

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

        formatter.WriteSuccess($"Configuration initialized at {configPath}");
        return CommandResult.Success;
    }
}
