using CreativeCoders.Cli.Core;
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
    private readonly ILogger<AdapterConfigureCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(AdapterConfigureOptions options)
    {
        _logger.LogInformation("Configuring adapter instance {InstanceId}", options.AdapterId);

        AdapterConfig config;

        try
        {
            config = await configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        }
        catch
        {
            formatter.WriteError($"Adapter '{options.AdapterId}' not found.");
            return new CommandResult(1);
        }

        console.MarkupLine(
            $"Configuring adapter [bold]'{Markup.Escape(config.AdapterId)}'[/] (type: {Markup.Escape(config.AdapterType)})");
        console.MarkupLine("[dim]Enter settings (empty value to skip):[/]");

        var updated = false;
        foreach (var key in config.Settings.Keys.ToList())
        {
            var current = config.Settings[key];
            var isSecret = key.EndsWith("_key", StringComparison.OrdinalIgnoreCase);
            var prompt = $"  {key} [{(isSecret ? "***" : current)}]: ";

            var value = isSecret
                ? interaction.ReadSecret(prompt)
                : interaction.ReadLine(prompt);

            if (!string.IsNullOrEmpty(value))
            {
                config.Settings[key] = value;
                updated = true;
            }
        }

        if (updated)
        {
            await configRepository.SaveAdapterConfigAsync(config).ConfigureAwait(false);
            formatter.WriteSuccess($"Adapter '{options.AdapterId}' configuration updated.");
        }
        else
        {
            formatter.WriteSuccess("No changes made.");
        }

        return CommandResult.Success;
    }
}
