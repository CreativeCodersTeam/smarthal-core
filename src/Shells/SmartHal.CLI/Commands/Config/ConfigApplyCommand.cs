using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Config;

/// <summary>Options for the config apply command.</summary>
public class ConfigApplyOptions
{
    /// <summary>The device ID to apply configuration to.</summary>
    [OptionValue(0, HelpText = "The device ID to apply configuration to")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Preview changes without applying.</summary>
    [OptionParameter('d', "dry-run", HelpText = "Preview changes without applying")]
    public bool DryRun { get; set; }
}

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
    OutputFormatter formatter) : ICliCommand<ConfigApplyOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigApplyOptions options)
    {
        var device = await configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);
        var adapterConfig = await configRepository.GetAdapterConfigAsync(device.AdapterId).ConfigureAwait(false);

        await using var adapter = adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is not IDeviceReader reader)
        {
            OutputFormatter.WriteError($"Adapter '{device.AdapterId}' does not support reading device state.");
            return new CommandResult(1);
        }

        var liveDevice = await reader.ReadDeviceAsync(device.NativeId).ConfigureAwait(false);
        var diff = differ.ComputeDiff(device, liveDevice);

        if (!diff.HasChanges)
        {
            OutputFormatter.WriteSuccess("No changes to apply.");
            return CommandResult.Success;
        }

        formatter.WriteTable(
            diff.Changes.ToList(),
            ("Kind", c => c.Kind.ToString()),
            ("Path", c => c.Path),
            ("Config Value", c => c.OldValue ?? "-"),
            ("New Value", c => c.NewValue ?? "-"));

        if (options.DryRun)
        {
            OutputFormatter.WriteSuccess("Dry run — no changes applied.");
            return CommandResult.Success;
        }

        await applier.ApplyDiffAsync(diff, adapter, device.NativeId).ConfigureAwait(false);

        OutputFormatter.WriteSuccess($"Applied {diff.Changes.Count} change(s) to device '{options.DeviceId}'.");
        return CommandResult.Success;
    }
}
