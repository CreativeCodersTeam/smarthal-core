namespace SmartHal.Core.Config;

/// <summary>
/// Represents the content of <c>meta.yaml</c> — global metadata including schema version and secrets provider.
/// </summary>
public class MetaConfig
{
    /// <summary>Gets or sets the configuration schema version.</summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>Gets or sets the secrets provider name (e.g. "auto", "env", "file").</summary>
    public string SecretsProvider { get; set; } = "auto";

    /// <summary>Gets or sets the optional configuration directory path override.</summary>
    public string? ConfigPath { get; set; }
}
