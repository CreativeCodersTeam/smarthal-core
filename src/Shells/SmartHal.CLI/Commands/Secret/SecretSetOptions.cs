using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>Options for the secret set command.</summary>
public class SecretSetOptions
{
    /// <summary>The secret key.</summary>
    [OptionValue(0, HelpText = "The secret key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>The secret value (if not provided, reads interactively).</summary>
    [OptionParameter('v', "value", HelpText = "The secret value")]
    public string? Value { get; set; }
}