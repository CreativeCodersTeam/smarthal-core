using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>
/// Lists registered adapter types and configured adapter instances.
/// </summary>
[UsedImplicitly]
[CliCommand(["adapter", "list"], Name = "list", Description = "List registered adapter types and instances")]
public class AdapterListCommand(
    IAdapterFactory adapterFactory,
    IConfigRepository configRepository,
    OutputFormatter formatter) : ICliCommand
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync()
    {
        var types = adapterFactory.GetAvailableAdapterTypes();
        Console.Error.WriteLine($"Registered adapter types: {string.Join(", ", types)}");
        Console.Error.WriteLine();

        var configs = await configRepository.GetAllAdapterConfigsAsync().ConfigureAwait(false);

        formatter.WriteTable(
            configs.ToList(),
            ("Adapter ID", c => c.AdapterId),
            ("Type", c => c.AdapterType),
            ("Settings", c => c.Settings.Count.ToString()));

        return CommandResult.Success;
    }
}
