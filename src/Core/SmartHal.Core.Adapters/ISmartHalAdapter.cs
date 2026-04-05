namespace SmartHal.Core.Adapters;

/// <summary>
/// Base interface that every adapter must implement.
/// Provides connection management and identification.
/// </summary>
public interface ISmartHalAdapter : IAsyncDisposable
{
    /// <summary>Gets the instance identifier of this adapter (e.g. "homematic-eg").</summary>
    string AdapterId { get; }

    /// <summary>Gets the human-readable display name of this adapter instance.</summary>
    string DisplayName { get; }

    /// <summary>Tests whether the adapter can connect to its backend system.</summary>
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
}
