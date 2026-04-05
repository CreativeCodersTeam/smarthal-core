using SmartHal.Core.Adapters;

namespace SmartHal.Core.Config;

/// <summary>
/// Applies a device diff to an adapter by writing changed parameters.
/// </summary>
public interface IConfigApplier
{
    /// <summary>Applies a device diff to an adapter by writing changed parameters.</summary>
    /// <param name="diff">The device diff to apply.</param>
    /// <param name="adapter">The adapter to write the changes to.</param>
    /// <param name="nativeId">The native device identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    Task ApplyDiffAsync(DeviceDiff diff, ISmartHalAdapter adapter, string nativeId, CancellationToken ct = default);
}
