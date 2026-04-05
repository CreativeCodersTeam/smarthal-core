namespace SmartHal.Core.Config;

/// <summary>
/// Validates configuration structure and semantic correctness.
/// </summary>
public interface IConfigValidator
{
    /// <summary>Validates the file structure and syntax of the configuration directory.</summary>
    /// <param name="configPath">The root configuration directory path.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<ValidationResult> ValidateStructureAsync(string configPath, CancellationToken ct = default);

    /// <summary>Validates semantic correctness (references, uniqueness) against the repository.</summary>
    /// <param name="repo">The configuration repository to validate against.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<ValidationResult> ValidateSemanticAsync(IConfigRepository repo, CancellationToken ct = default);
}
