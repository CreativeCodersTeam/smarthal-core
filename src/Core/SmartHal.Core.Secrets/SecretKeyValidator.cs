using System.Text.RegularExpressions;

namespace SmartHal.Core.Secrets;

/// <summary>
/// Validates secret key names and converts them to environment variable names.
/// Keys follow the format <c>{adapter-id}.{setting-name}</c> and may only contain
/// lowercase letters, digits, hyphens, underscores, and dots.
/// </summary>
public static partial class SecretKeyValidator
{
    private const string EnvironmentVariablePrefix = "SMARTHAL_";

    /// <summary>
    /// Validates whether the given key follows the secret key naming convention.
    /// </summary>
    /// <param name="key">The secret key to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the key is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(string key) =>
        !string.IsNullOrWhiteSpace(key) && ValidKeyPattern().IsMatch(key);

    /// <summary>
    /// Converts a secret key to its corresponding environment variable name.
    /// Example: <c>homematic-eg.api_key</c> becomes <c>SMARTHAL_HOMEMATIC_EG_API_KEY</c>.
    /// </summary>
    /// <param name="key">The secret key to convert.</param>
    /// <returns>The corresponding environment variable name.</returns>
    public static string ToEnvironmentVariable(string key) =>
        EnvironmentVariablePrefix + key.Replace('-', '_').Replace('.', '_').ToUpperInvariant();

    /// <summary>
    /// Converts an environment variable name back to a secret key.
    /// Only works for variables with the <c>SMARTHAL_</c> prefix.
    /// </summary>
    /// <param name="envVar">The environment variable name to convert.</param>
    /// <returns>The corresponding secret key, or <see langword="null"/> if the variable does not have the expected prefix.</returns>
    public static string? FromEnvironmentVariable(string envVar)
    {
        if (!envVar.StartsWith(EnvironmentVariablePrefix, StringComparison.Ordinal))
        {
            return null;
        }

        return envVar[EnvironmentVariablePrefix.Length..].ToLowerInvariant();
    }

    [GeneratedRegex(@"^[a-z0-9][a-z0-9._-]*[a-z0-9]$")]
    private static partial Regex ValidKeyPattern();
}
