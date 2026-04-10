using System.Text.RegularExpressions;
using CreativeCoders.Core;

namespace SmartHal.Core.Config;

/// <summary>
/// Generates unique device identifiers in the format <c>smhal-{prefix}-{slug}</c>.
/// Checks for collisions against existing device files in the configuration directory.
/// </summary>
public partial class DeviceIdGenerator : IIdGenerator
{
    private static readonly Dictionary<string, string> PrefixMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["homematic"] = "hm",
        ["zigbee"] = "zb",
        ["knx"] = "knx",
        ["mqtt"] = "mqtt",
        ["evcc"] = "evcc"
    };

    private static readonly Dictionary<string, string> UmlautMap = new Dictionary<string, string>
    {
        ["ä"] = "ae",
        ["ö"] = "oe",
        ["ü"] = "ue",
        ["ß"] = "ss"
    };

    private readonly string _devicesPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceIdGenerator"/> class.
    /// </summary>
    /// <param name="configPath">The root configuration directory path.</param>
    public DeviceIdGenerator(string configPath)
    {
        Ensure.IsNotNullOrWhitespace(configPath);

        _devicesPath = Path.Combine(configPath, "devices");
    }

    /// <inheritdoc />
    public Task<string> GenerateDeviceIdAsync(string adapterType, string deviceName, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(adapterType);
        Ensure.IsNotNullOrWhitespace(deviceName);

        var prefix = GetPrefix(adapterType);
        var slug = GenerateSlug(deviceName);
        var baseId = $"smhal-{prefix}-{slug}";

        var existingIds = GetExistingDeviceIds();
        var id = baseId;
        var counter = 2;

        while (existingIds.Contains(id))
        {
            id = $"{baseId}-{counter}";
            counter++;
        }

        return Task.FromResult(id);
    }

    /// <summary>
    /// Gets the prefix for a given adapter type using the prefix map.
    /// Falls back to the first 3-4 characters of the adapter type for unknown types.
    /// </summary>
    /// <param name="adapterType">The adapter type identifier.</param>
    /// <returns>The prefix string for the adapter type.</returns>
    internal static string GetPrefix(string adapterType)
    {
        if (PrefixMap.TryGetValue(adapterType, out var prefix))
        {
            return prefix;
        }

        var length = Math.Min(adapterType.Length, adapterType.Length <= 3 ? 3 : 4);

        return adapterType[..length].ToLowerInvariant();
    }

    /// <summary>
    /// Generates a URL-friendly slug from a device name.
    /// Converts umlauts, removes special characters, and enforces length limits.
    /// </summary>
    /// <param name="deviceName">The device name to convert to a slug.</param>
    /// <returns>A URL-friendly slug derived from the device name.</returns>
    internal static string GenerateSlug(string deviceName)
    {
        var result = deviceName.ToLowerInvariant();

        // Replace umlauts
        foreach (var (umlaut, replacement) in UmlautMap)
        {
            result = result.Replace(umlaut, replacement, StringComparison.Ordinal);
        }

        // Keep only a-z, 0-9, hyphens; replace everything else with hyphens
        result = InvalidCharsRegex().Replace(result, "-");

        // Collapse multiple hyphens
        result = MultipleHyphensRegex().Replace(result, "-");

        // Trim leading/trailing hyphens
        result = result.Trim('-');

        // Enforce max length of 30 characters
        if (result.Length > 30)
        {
            result = result[..30].TrimEnd('-');
        }

        return result;
    }

    private HashSet<string> GetExistingDeviceIds()
    {
        if (!Directory.Exists(_devicesPath))
        {
            return [];
        }

        return Directory.GetFiles(_devicesPath, "*.yaml")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    [GeneratedRegex("[^a-z0-9-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleHyphensRegex();
}
