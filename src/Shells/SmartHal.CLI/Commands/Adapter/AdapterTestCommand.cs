using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
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
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IAdapterFactory _adapterFactory = Ensure.NotNull(adapterFactory);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<AdapterTestCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(AdapterTestOptions options)
    {
        _logger.LogInformation("Testing adapter connection for instance {InstanceId}", options.AdapterId);

        var config = await _configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        await using var adapter = _adapterFactory.CreateAdapter(config);

        var success = await adapter.TestConnectionAsync().ConfigureAwait(false);

        if (success)
        {
            _formatter.WriteSuccess($"Connection to '{options.AdapterId}' ({adapter.DisplayName}) successful.");
            return CommandResult.Success;
        }

        _formatter.WriteError($"Connection to '{options.AdapterId}' failed.");
        return new CommandResult(1);
    }
}
