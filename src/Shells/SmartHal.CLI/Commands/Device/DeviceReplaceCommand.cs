using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Device;

/// <summary>Options for the device replace command.</summary>
public class DeviceReplaceOptions
{
    /// <summary>The device ID to replace.</summary>
    [OptionValue(0, HelpText = "The device ID to replace")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>The new native ID for the replacement device.</summary>
    [OptionValue(1, HelpText = "The new native ID")]
    public string NewNativeId { get; set; } = string.Empty;
}

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
    IUserInteraction interaction) : ICliCommand<DeviceReplaceOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceReplaceOptions options)
    {
        var device = await configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);

        if (!interaction.Confirm($"Replace device '{options.DeviceId}' native ID '{device.NativeId}' -> '{options.NewNativeId}'?"))
        {
            OutputFormatter.WriteSuccess("Cancelled.");
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
                OutputFormatter.WriteSuccess($"Applied {diff.Changes.Count} change(s) to new device.");
            }
        }

        OutputFormatter.WriteSuccess(
            $"Replaced device '{options.DeviceId}': {oldNativeId} -> {options.NewNativeId}.");

        return CommandResult.Success;
    }
}
