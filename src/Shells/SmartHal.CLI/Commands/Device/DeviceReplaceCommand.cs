using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Device;

/// <summary>
/// Replaces a device's native ID (e.g. after hardware replacement) and applies configuration.
/// </summary>
[UsedImplicitly]
[CliCommand(["device", "replace"], Name = "replace", Description = "Replace a device (swap native ID)")]
public class DeviceReplaceCommand(
    IConfigRepository configRepository,
    ISnapshotManager snapshotManager,
    IConfigApplier applier,
    IConfigDiffer differ,
    Core.Adapters.IAdapterFactory adapterFactory,
    IUserInteraction interaction,
    OutputFormatter formatter,
    ILogger<DeviceReplaceCommand> logger) : ICliCommand<DeviceReplaceOptions>
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly ISnapshotManager _snapshotManager = Ensure.NotNull(snapshotManager);
    private readonly IConfigApplier _applier = Ensure.NotNull(applier);
    private readonly IConfigDiffer _differ = Ensure.NotNull(differ);
    private readonly Core.Adapters.IAdapterFactory _adapterFactory = Ensure.NotNull(adapterFactory);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<DeviceReplaceCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceReplaceOptions options)
    {
        _logger.LogInformation("Replacing device {DeviceId} native ID to {NewNativeId}", options.DeviceId, options.NewNativeId);

        var device = await _configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);

        if (!_interaction.Confirm($"Replace device '{options.DeviceId}' native ID '{device.NativeId}' -> '{options.NewNativeId}'?"))
        {
            _formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        // 1. Pre-replace snapshot
        await _snapshotManager.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = options.DeviceId,
            Trigger = "pre-replace",
            Description = $"Pre-replace snapshot for {options.DeviceId}"
        }).ConfigureAwait(false);

        // 2. Update native_id in YAML
        var oldNativeId = device.NativeId;
        device.NativeId = options.NewNativeId;
        await _configRepository.SaveDeviceAsync(device).ConfigureAwait(false);

        // 3. Apply configuration to new device
        var adapterConfig = await _configRepository.GetAdapterConfigAsync(device.AdapterId).ConfigureAwait(false);
        await using var adapter = _adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is Core.Adapters.IDeviceReader reader)
        {
            var liveDevice = await reader.ReadDeviceAsync(options.NewNativeId).ConfigureAwait(false);
            var diff = _differ.ComputeDiff(device, liveDevice);

            if (diff.HasChanges)
            {
                await _applier.ApplyDiffAsync(diff, adapter, options.NewNativeId).ConfigureAwait(false);
                _formatter.WriteSuccess($"Applied {diff.Changes.Count} change(s) to new device.");
            }
        }

        _formatter.WriteSuccess(
            $"Replaced device '{options.DeviceId}': {oldNativeId} -> {options.NewNativeId}.");

        return CommandResult.Success;
    }
}
