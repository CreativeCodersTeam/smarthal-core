using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Config;

/// <summary>Options for the config validate command.</summary>
public class ConfigValidateOptions
{
    /// <summary>Run full validation (structure + semantic).</summary>
    [OptionParameter('f', "full", HelpText = "Run full validation (structure + semantic)")]
    public bool Full { get; set; }
}