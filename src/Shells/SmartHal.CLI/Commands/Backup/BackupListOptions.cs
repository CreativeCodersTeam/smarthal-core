using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup list command.</summary>
public class BackupListOptions
{
    /// <summary>Filter by scope.</summary>
    [OptionParameter('s', "scope", HelpText = "Filter by scope (Device, Adapter, Room, All)")]
    public string? Scope { get; set; }

    /// <summary>Filter by minimum date.</summary>
    [OptionParameter(default(char), "since", HelpText = "Show only snapshots since this date")]
    public string? Since { get; set; }
}