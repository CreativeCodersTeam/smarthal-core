using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>Options for the adapter test command.</summary>
public class AdapterTestOptions
{
    /// <summary>The adapter ID to test.</summary>
    [OptionValue(0, HelpText = "The adapter ID to test")]
    public string AdapterId { get; set; } = string.Empty;
}

/// <summary>
/// Tests the connection to an adapter.
/// </summary>
[UsedImplicitly]
[CliCommand(["adapter", "test"], Name = "test", Description = "Test adapter connection")]
public class AdapterTestCommand(
    IConfigRepository configRepository,
    IAdapterFactory adapterFactory) : ICliCommand<AdapterTestOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(AdapterTestOptions options)
    {
        var config = await configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        await using var adapter = adapterFactory.CreateAdapter(config);

        var success = await adapter.TestConnectionAsync().ConfigureAwait(false);

        if (success)
        {
            OutputFormatter.WriteSuccess($"Connection to '{options.AdapterId}' ({adapter.DisplayName}) successful.");
            return CommandResult.Success;
        }

        OutputFormatter.WriteError($"Connection to '{options.AdapterId}' failed.");
        return new CommandResult(1);
    }
}
