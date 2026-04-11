using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
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
    private readonly CliContext _cliContext = Ensure.NotNull(cliContext);
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<ConfigInitCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigInitOptions options)
    {
        var configPath = options.Path ?? _cliContext.ConfigPath;

        _logger.LogInformation("Initializing config at {ConfigPath}", configPath);

        var secretsProvider = _interaction.ReadLine("Secrets provider [auto/env/file]: ")?.Trim();
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

        await _configRepository.InitializeAsync(meta).ConfigureAwait(false);

        _formatter.WriteSuccess($"Configuration initialized at {configPath}");
        return CommandResult.Success;
    }
}
