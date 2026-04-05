namespace SmartHal.Core.Config;

/// <summary>
/// Validates configuration structure and semantic correctness.
/// </summary>
public interface IConfigValidator
{
    /// <summary>Validates the file structure and syntax of the configuration directory.</summary>
    Task<ValidationResult> ValidateStructureAsync(string configPath, CancellationToken ct = default);

    /// <summary>Validates semantic correctness (references, uniqueness) against the repository.</summary>
    Task<ValidationResult> ValidateSemanticAsync(IConfigRepository repo, CancellationToken ct = default);
}
