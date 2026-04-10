using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup restore command.</summary>
public class BackupRestoreOptions
{
    /// <summary>The snapshot ID to restore.</summary>
    [OptionValue(0, HelpText = "The snapshot ID to restore")]
    public string SnapshotId { get; set; } = string.Empty;

    /// <summary>Preview changes without applying.</summary>
    [OptionParameter('d', "dry-run", HelpText = "Preview changes without restoring")]
    public bool DryRun { get; set; }
}