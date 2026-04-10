using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup create command.</summary>
public class BackupCreateOptions
{
    /// <summary>Scope to a single device.</summary>
    [OptionParameter('d', "device", HelpText = "Scope to a single device")]
    public string? DeviceId { get; set; }

    /// <summary>Scope to all devices of an adapter.</summary>
    [OptionParameter('a', "adapter", HelpText = "Scope to all devices of an adapter")]
    public string? AdapterId { get; set; }

    /// <summary>Scope to all devices in a room.</summary>
    [OptionParameter('r', "room", HelpText = "Scope to all devices in a room")]
    public string? RoomId { get; set; }

    /// <summary>Snapshot mode (reference or embedded).</summary>
    [OptionParameter('m', "mode", HelpText = "Snapshot mode: reference or embedded", DefaultValue = "reference")]
    public string Mode { get; set; } = "reference";

    /// <summary>Description for the snapshot.</summary>
    [OptionParameter(default(char), "description", HelpText = "Description for the snapshot")]
    public string? Description { get; set; }
}