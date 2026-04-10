using CreativeCoders.SysConsole.Cli.Parsing;

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