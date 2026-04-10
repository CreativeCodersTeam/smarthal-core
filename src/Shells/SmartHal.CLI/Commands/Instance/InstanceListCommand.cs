using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Instance;

/// <summary>
/// Lists all configured adapter instances.
/// </summary>
[UsedImplicitly]
[CliCommand(["instance", "list"], Name = "list", Description = "List adapter instances")]
public class InstanceListCommand(
    IConfigRepository configRepository,
    OutputFormatter formatter) : ICliCommand
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync()
    {
        var configs = await configRepository.GetAllAdapterConfigsAsync().ConfigureAwait(false);

        formatter.WriteTable(
            configs.ToList(),
            ("Instance ID", c => c.AdapterId),
            ("Type", c => c.AdapterType),
            ("Settings", c => c.Settings.Count.ToString()));

        return CommandResult.Success;
    }
}
