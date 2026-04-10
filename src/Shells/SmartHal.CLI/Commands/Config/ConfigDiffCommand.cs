using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
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
    OutputFormatter formatter) : ICliCommand<ConfigDiffOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(ConfigDiffOptions options)
    {
        var device = await configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);
        var adapterConfig = await configRepository.GetAdapterConfigAsync(device.AdapterId).ConfigureAwait(false);

        await using var adapter = adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is not IDeviceReader reader)
        {
            formatter.WriteError($"Adapter '{device.AdapterId}' does not support reading device state.");
            return new CommandResult(1);
        }

        var liveDevice = await reader.ReadDeviceAsync(device.NativeId).ConfigureAwait(false);
        var diff = differ.ComputeDiff(device, liveDevice);

        if (!diff.HasChanges)
        {
            formatter.WriteSuccess("No differences found.");
            return CommandResult.Success;
        }

        formatter.WriteTable(
            diff.Changes.ToList(),
            ("Kind", c => c.Kind.ToString()),
            ("Path", c => c.Path),
            ("Config Value", c => c.OldValue ?? "-"),
            ("Live Value", c => c.NewValue ?? "-"));

        return CommandResult.Success;
    }
}
