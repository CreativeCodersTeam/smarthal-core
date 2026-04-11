using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using Spectre.Console;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>
/// Interactively configures an adapter instance.
/// </summary>
[UsedImplicitly]
[CliCommand(["adapter", "configure"], Name = "configure", Description = "Configure an adapter instance")]
public class AdapterConfigureCommand(
    IConfigRepository configRepository,
    IUserInteraction interaction,
    IAnsiConsole console,
    OutputFormatter formatter,
    ILogger<AdapterConfigureCommand> logger) : ICliCommand<AdapterConfigureOptions>
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly IAnsiConsole _console = Ensure.NotNull(console);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<AdapterConfigureCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(AdapterConfigureOptions options)
    {
        _logger.LogInformation("Configuring adapter instance {InstanceId}", options.AdapterId);

        AdapterConfig config;

        try
        {
            config = await _configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        }
        catch
        {
            _formatter.WriteError($"Adapter '{options.AdapterId}' not found.");
            return new CommandResult(1);
        }

        _console.MarkupLine(
            $"Configuring adapter [bold]'{Markup.Escape(config.AdapterId)}'[/] (type: {Markup.Escape(config.AdapterType)})");
        _console.MarkupLine("[dim]Enter settings (empty value to skip):[/]");

        var updated = false;
        foreach (var key in config.Settings.Keys.ToList())
        {
            var current = config.Settings[key];
            var isSecret = key.EndsWith("_key", StringComparison.OrdinalIgnoreCase);
            var prompt = $"  {key} [{(isSecret ? "***" : current)}]: ";

            var value = isSecret
                ? _interaction.ReadSecret(prompt)
                : _interaction.ReadLine(prompt);

            if (!string.IsNullOrEmpty(value))
            {
                config.Settings[key] = value;
                updated = true;
            }
        }

        if (updated)
        {
            await _configRepository.SaveAdapterConfigAsync(config).ConfigureAwait(false);
            _formatter.WriteSuccess($"Adapter '{options.AdapterId}' configuration updated.");
        }
        else
        {
            _formatter.WriteSuccess("No changes made.");
        }

        return CommandResult.Success;
    }
}
