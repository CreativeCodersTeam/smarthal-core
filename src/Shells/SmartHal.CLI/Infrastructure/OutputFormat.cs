namespace SmartHal.CLI.Infrastructure;

/// <summary>
/// Supported output formats for CLI results.
/// </summary>
public enum OutputFormat
{
    /// <summary>Human-readable table format (default).</summary>
    Table,

    /// <summary>Machine-readable JSON format.</summary>
    Json,

    /// <summary>YAML format, consistent with the config file format.</summary>
    Yaml
}
