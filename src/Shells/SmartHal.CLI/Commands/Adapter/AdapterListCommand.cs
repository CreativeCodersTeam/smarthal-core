using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using Spectre.Console;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>
/// Lists registered adapter types and configured adapter instances.
/// </summary>
[UsedImplicitly]
[CliCommand(["adapter", "list"], Name = "list", Description = "List registered adapter types and instances")]
public class AdapterListCommand(
    IAdapterFactory adapterFactory,
    IConfigRepository configRepository,
    IAnsiConsole console,
    OutputFormatter formatter,
    ILogger<AdapterListCommand> logger) : ICliCommand
{
    private readonly ILogger<AdapterListCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync()
    {
        _logger.LogInformation("Listing adapters");

        var types = adapterFactory.GetAvailableAdapterTypes();
        console.MarkupLine($"Registered adapter types: [bold]{Markup.Escape(string.Join(", ", types))}[/]");
        console.WriteLine();

        var configs = await configRepository.GetAllAdapterConfigsAsync().ConfigureAwait(false);

        formatter.WriteTable(
            configs.ToList(),
            ("Adapter ID", c => c.AdapterId),
            ("Type", c => c.AdapterType),
            ("Settings", c => c.Settings.Count.ToString()));

        return CommandResult.Success;
    }
}
