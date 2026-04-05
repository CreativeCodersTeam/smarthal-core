namespace SmartHal.Core.Config;

/// <summary>
/// Generates unique device identifiers with collision checking against existing files.
/// </summary>
public interface IIdGenerator
{
    /// <summary>Generates a unique device ID in the format <c>smhal-{prefix}-{slug}</c>.</summary>
    Task<string> GenerateDeviceIdAsync(string adapterType, string deviceName, CancellationToken ct = default);
}
