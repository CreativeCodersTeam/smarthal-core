using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>
/// Tests the connection to an adapter.
/// </summary>
[UsedImplicitly]
[CliCommand(["adapter", "test"], Name = "test", Description = "Test adapter connection")]
public class AdapterTestCommand(
    IConfigRepository configRepository,
    IAdapterFactory adapterFactory,
    OutputFormatter formatter,
    ILogger<AdapterTestCommand> logger) : ICliCommand<AdapterTestOptions>
{
    private readonly ILogger<AdapterTestCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(AdapterTestOptions options)
    {
        _logger.LogInformation("Testing adapter connection for instance {InstanceId}", options.AdapterId);

        var config = await configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        await using var adapter = adapterFactory.CreateAdapter(config);

        var success = await adapter.TestConnectionAsync().ConfigureAwait(false);

        if (success)
        {
            formatter.WriteSuccess($"Connection to '{options.AdapterId}' ({adapter.DisplayName}) successful.");
            return CommandResult.Success;
        }

        formatter.WriteError($"Connection to '{options.AdapterId}' failed.");
        return new CommandResult(1);
    }
}
