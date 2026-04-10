using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.CLI.Commands.Device;

/// <summary>
/// Lists all devices, optionally filtered.
/// </summary>
[UsedImplicitly]
[CliCommand(["device", "list"], Name = "list", Description = "List all devices")]
public class DeviceListCommand(
    IConfigRepository configRepository,
    OutputFormatter formatter) : ICliCommand<DeviceListOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceListOptions options)
    {
        var filter = new DeviceFilter
        {
            AdapterId = options.AdapterId,
            RoomId = options.RoomId,
            Type = string.IsNullOrEmpty(options.Type) ? null : (DeviceType?)new DeviceType(options.Type!),
            GroupId = options.GroupId,
            NamePattern = options.NamePattern
        };

        var devices = await configRepository.ListDevicesAsync(filter).ConfigureAwait(false);

        formatter.WriteTable(
            devices.ToList(),
            ("ID", d => d.Id),
            ("Name", d => d.Name),
            ("Type", d => d.Type.Value),
            ("Adapter", d => d.AdapterId),
            ("Room", d => d.RoomId ?? "-"));

        return CommandResult.Success;
    }
}
