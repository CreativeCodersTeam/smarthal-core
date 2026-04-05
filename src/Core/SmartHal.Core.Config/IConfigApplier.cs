using SmartHal.Core.Adapters;

namespace SmartHal.Core.Config;

/// <summary>
/// Applies a device diff to an adapter by writing changed parameters.
/// </summary>
public interface IConfigApplier
{
    Task ApplyDiffAsync(DeviceDiff diff, ISmartHalAdapter adapter, string nativeId, CancellationToken ct = default);
}
