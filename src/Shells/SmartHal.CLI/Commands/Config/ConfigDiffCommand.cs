using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Config;

/// <summary>
/// Compares the YAML configuration with the current adapter state for a device.
/// </summary>
[UsedImplicitly]
[CliCommand(["config", "diff"], Name = "diff", Description = "Show config diff for a device")]
public class ConfigDiffCommand(
    IConfigRepository configRepository,
    IConfigDiffer differ,
    IAdapterFactory adapterFactory,
    OutputFormatter formatter,
    ILogger<ConfigDiffCommand> logger) : ICliCommand<ConfigDiffOptions>
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IConfigDiffer _differ = Ensure.NotNull(differ);
    private readonly IAdapterFactory _adapterFactory = Ensure.NotNull(adapterFactory);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<ConfigDiffCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigDiffOptions options)
    {
        _logger.LogInformation("Computing diff for device {DeviceId}", options.DeviceId);

        var device = await _configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);
        var adapterConfig = await _configRepository.GetAdapterConfigAsync(device.AdapterId).ConfigureAwait(false);

        await using var adapter = _adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is not IDeviceReader reader)
        {
            _formatter.WriteError($"Adapter '{device.AdapterId}' does not support reading device state.");
            return new CommandResult(1);
        }

        var liveDevice = await reader.ReadDeviceAsync(device.NativeId).ConfigureAwait(false);
        var diff = _differ.ComputeDiff(device, liveDevice);

        if (!diff.HasChanges)
        {
            _formatter.WriteSuccess("No differences found.");
            return CommandResult.Success;
        }

        _formatter.WriteTable(
            diff.Changes.ToList(),
            ("Kind", c => c.Kind.ToString()),
            ("Path", c => c.Path),
            ("Config Value", c => c.OldValue ?? "-"),
            ("Live Value", c => c.NewValue ?? "-"));

        return CommandResult.Success;
    }
}
