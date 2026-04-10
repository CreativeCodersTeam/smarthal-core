using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
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
    OutputFormatter formatter) : ICliCommand<DeviceReplaceOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceReplaceOptions options)
    {
        var device = await configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);

        if (!interaction.Confirm($"Replace device '{options.DeviceId}' native ID '{device.NativeId}' -> '{options.NewNativeId}'?"))
        {
            formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        // 1. Pre-replace snapshot
        await snapshotManager.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = options.DeviceId,
            Trigger = "pre-replace",
            Description = $"Pre-replace snapshot for {options.DeviceId}"
        }).ConfigureAwait(false);

        // 2. Update native_id in YAML
        var oldNativeId = device.NativeId;
        device.NativeId = options.NewNativeId;
        await configRepository.SaveDeviceAsync(device).ConfigureAwait(false);

        // 3. Apply configuration to new device
        var adapterConfig = await configRepository.GetAdapterConfigAsync(device.AdapterId).ConfigureAwait(false);
        await using var adapter = adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is Core.Adapters.IDeviceReader reader)
        {
            var liveDevice = await reader.ReadDeviceAsync(options.NewNativeId).ConfigureAwait(false);
            var diff = differ.ComputeDiff(device, liveDevice);

            if (diff.HasChanges)
            {
                await applier.ApplyDiffAsync(diff, adapter, options.NewNativeId).ConfigureAwait(false);
                formatter.WriteSuccess($"Applied {diff.Changes.Count} change(s) to new device.");
            }
        }

        formatter.WriteSuccess(
            $"Replaced device '{options.DeviceId}': {oldNativeId} -> {options.NewNativeId}.");

        return CommandResult.Success;
    }
}
