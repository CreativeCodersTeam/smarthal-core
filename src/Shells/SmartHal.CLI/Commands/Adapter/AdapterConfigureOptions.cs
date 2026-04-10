using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>Options for the adapter configure command.</summary>
public class AdapterConfigureOptions
{
    /// <summary>The adapter ID to configure.</summary>
    [OptionValue(0, HelpText = "The adapter ID to configure")]
    public string AdapterId { get; set; } = string.Empty;
}