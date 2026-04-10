using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
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
    OutputFormatter formatter) : ICliCommand<AdapterTestOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(AdapterTestOptions options)
    {
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
