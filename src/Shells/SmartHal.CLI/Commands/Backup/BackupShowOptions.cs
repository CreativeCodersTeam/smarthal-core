using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup show command.</summary>
public class BackupShowOptions
{
    /// <summary>The snapshot ID to display.</summary>
    [OptionValue(0, HelpText = "The snapshot ID to display")]
    public string SnapshotId { get; set; } = string.Empty;
}