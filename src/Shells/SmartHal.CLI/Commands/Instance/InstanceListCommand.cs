using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
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
    OutputFormatter formatter,
    ILogger<InstanceListCommand> logger) : ICliCommand
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<InstanceListCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync()
    {
        _logger.LogInformation("Listing adapter instances");

        var configs = await _configRepository.GetAllAdapterConfigsAsync().ConfigureAwait(false);

        _formatter.WriteTable(
            configs.ToList(),
            ("Instance ID", c => c.AdapterId),
            ("Type", c => c.AdapterType),
            ("Settings", c => c.Settings.Count.ToString()));

        return CommandResult.Success;
    }
}
