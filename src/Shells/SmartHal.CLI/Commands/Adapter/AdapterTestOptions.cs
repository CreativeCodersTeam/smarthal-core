using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>Options for the adapter test command.</summary>
public class AdapterTestOptions
{
    /// <summary>The adapter ID to test.</summary>
    [OptionValue(0, HelpText = "The adapter ID to test")]
    public string AdapterId { get; set; } = string.Empty;
}