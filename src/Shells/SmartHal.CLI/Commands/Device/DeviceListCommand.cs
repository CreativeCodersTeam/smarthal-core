using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.CLI.Commands.Device;

/// <summary>Options for the device list command.</summary>
public class DeviceListOptions
{
    /// <summary>Filter by adapter ID.</summary>
    [OptionParameter('a', "adapter", HelpText = "Filter by adapter ID")]
    public string? AdapterId { get; set; }

    /// <summary>Filter by room ID.</summary>
    [OptionParameter('r', "room", HelpText = "Filter by room ID")]
    public string? RoomId { get; set; }

    /// <summary>Filter by device type.</summary>
    [OptionParameter('t', "type", HelpText = "Filter by device type")]
    public string? Type { get; set; }

    /// <summary>Filter by group ID.</summary>
    [OptionParameter('g', "group", HelpText = "Filter by group ID")]
    public string? GroupId { get; set; }

    /// <summary>Filter by name (glob pattern).</summary>
    [OptionParameter('n', "name", HelpText = "Filter by name (glob pattern)")]
    public string? NamePattern { get; set; }
}

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
