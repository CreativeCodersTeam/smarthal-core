namespace SmartHal.Core.Config;

/// <summary>
/// Generates unique device identifiers with collision checking against existing files.
/// </summary>
public interface IIdGenerator
{
    /// <summary>Generates a unique device ID in the format <c>smhal-{prefix}-{slug}</c>.</summary>
    /// <param name="adapterType">The adapter type identifier.</param>
    /// <param name="deviceName">The human-readable device name.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A unique device identifier.</returns>
    Task<string> GenerateDeviceIdAsync(string adapterType, string deviceName, CancellationToken ct = default);
}
