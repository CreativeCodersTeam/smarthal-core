using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Config;

/// <summary>
/// Writes YAML configuration values to the adapter for a specific device.
/// </summary>
[UsedImplicitly]
[CliCommand(["config", "apply"], Name = "apply", Description = "Apply config to a device via adapter")]
public class ConfigApplyCommand(
    IConfigRepository configRepository,
    IConfigDiffer differ,
    IConfigApplier applier,
    IAdapterFactory adapterFactory,
    OutputFormatter formatter,
    ILogger<ConfigApplyCommand> logger) : ICliCommand<ConfigApplyOptions>
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IConfigDiffer _differ = Ensure.NotNull(differ);
    private readonly IConfigApplier _applier = Ensure.NotNull(applier);
    private readonly IAdapterFactory _adapterFactory = Ensure.NotNull(adapterFactory);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<ConfigApplyCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigApplyOptions options)
    {
        _logger.LogInformation("Applying config for device {DeviceId}", options.DeviceId);

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
            _formatter.WriteSuccess("No changes to apply.");
            return CommandResult.Success;
        }

        _formatter.WriteTable(
            diff.Changes.ToList(),
            ("Kind", c => c.Kind.ToString()),
            ("Path", c => c.Path),
            ("Config Value", c => c.OldValue ?? "-"),
            ("New Value", c => c.NewValue ?? "-"));

        if (options.DryRun)
        {
            _formatter.WriteSuccess("Dry run — no changes applied.");
            return CommandResult.Success;
        }

        await _applier.ApplyDiffAsync(diff, adapter, device.NativeId).ConfigureAwait(false);

        _formatter.WriteSuccess($"Applied {diff.Changes.Count} change(s) to device '{options.DeviceId}'.");
        return CommandResult.Success;
    }
}
