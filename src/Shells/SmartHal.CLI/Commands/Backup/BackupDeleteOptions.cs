using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup delete command.</summary>
public class BackupDeleteOptions
{
    /// <summary>The snapshot ID to delete.</summary>
    [OptionValue(0, HelpText = "The snapshot ID to delete")]
    public string SnapshotId { get; set; } = string.Empty;
}