using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using Spectre.Console;

namespace SmartHal.CLI.Commands.Instance;

/// <summary>
/// Creates a new adapter instance and starts interactive configuration.
/// </summary>
[UsedImplicitly]
[CliCommand(["instance", "add"], Name = "add", Description = "Add a new adapter instance")]
public class InstanceAddCommand(
    IConfigRepository configRepository,
    IAdapterFactory adapterFactory,
    IUserInteraction interaction,
    IAnsiConsole console,
    OutputFormatter formatter,
    ILogger<InstanceAddCommand> logger) : ICliCommand<InstanceAddOptions>
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IAdapterFactory _adapterFactory = Ensure.NotNull(adapterFactory);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly IAnsiConsole _console = Ensure.NotNull(console);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<InstanceAddCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(InstanceAddOptions options)
    {
        _logger.LogInformation("Adding new adapter instance of type {AdapterType}", options.AdapterType);

        var availableTypes = _adapterFactory.GetAvailableAdapterTypes();
        if (!availableTypes.Contains(options.AdapterType))
        {
            _formatter.WriteError(
                $"Unknown adapter type '{options.AdapterType}'. Available: {string.Join(", ", availableTypes)}");
            return new CommandResult(1);
        }

        var config = new AdapterConfig
        {
            AdapterId = options.InstanceId,
            AdapterType = options.AdapterType,
            Settings = new Dictionary<string, string>()
        };

        _console.MarkupLine(
            $"Configuring new instance [bold]'{Markup.Escape(options.InstanceId)}'[/] (type: {Markup.Escape(options.AdapterType)})");

        // Collect basic settings interactively
        while (true)
        {
            var key = _interaction.ReadLine("  Setting key (empty to finish): ")?.Trim();
            if (string.IsNullOrEmpty(key)) break;

            var isSecret = key.EndsWith("_key", StringComparison.OrdinalIgnoreCase);
            var value = isSecret
                ? _interaction.ReadSecret($"  {key} (secret): ")
                : _interaction.ReadLine($"  {key}: ");

            if (!string.IsNullOrEmpty(value))
            {
                config.Settings[key] = value;
            }
        }

        await _configRepository.SaveAdapterConfigAsync(config).ConfigureAwait(false);

        _formatter.WriteSuccess($"Instance '{options.InstanceId}' created.");
        return CommandResult.Success;
    }
}
