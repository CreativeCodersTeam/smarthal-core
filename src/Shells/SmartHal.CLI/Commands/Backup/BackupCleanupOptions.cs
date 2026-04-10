using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup cleanup command.</summary>
public class BackupCleanupOptions
{
    /// <summary>Maximum age in days.</summary>
    [OptionParameter('a', "max-age", HelpText = "Maximum age in days")]
    public int? MaxAge { get; set; }

    /// <summary>Maximum number of snapshots to keep.</summary>
    [OptionParameter('c', "max-count", HelpText = "Maximum number of snapshots to keep")]
    public int? MaxCount { get; set; }

    /// <summary>Keep manual snapshots regardless of policy.</summary>
    [OptionParameter('k', "keep-manual", HelpText = "Keep manual snapshots")]
    public bool KeepManual { get; set; }
}