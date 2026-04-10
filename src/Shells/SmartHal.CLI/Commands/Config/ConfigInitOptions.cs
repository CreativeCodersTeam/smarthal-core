using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Config;

/// <summary>Options for the config init command.</summary>
public class ConfigInitOptions
{
    /// <summary>Override the configuration path.</summary>
    [OptionParameter('p', "path", HelpText = "Override the configuration path")]
    public string? Path { get; set; }
}