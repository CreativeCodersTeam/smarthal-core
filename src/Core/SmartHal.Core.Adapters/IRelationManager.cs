using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that can manage device relations.
/// </summary>
public interface IRelationManager
{
    /// <summary>Reads all relations for a device.</summary>
    /// <param name="nativeId">The native device identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of relations for the device.</returns>
    Task<IReadOnlyList<Relation>> ReadRelationsAsync(string nativeId, CancellationToken ct = default);

    /// <summary>Creates a relation between two devices.</summary>
    /// <param name="sourceNativeId">The native identifier of the source device.</param>
    /// <param name="targetNativeId">The native identifier of the target device.</param>
    /// <param name="type">The type of relation to create.</param>
    /// <param name="ct">The cancellation token.</param>
    Task AddRelationAsync(string sourceNativeId, string targetNativeId, RelationType type, CancellationToken ct = default);

    /// <summary>Removes a relation between two devices.</summary>
    /// <param name="sourceNativeId">The native identifier of the source device.</param>
    /// <param name="targetNativeId">The native identifier of the target device.</param>
    /// <param name="type">The type of relation to remove.</param>
    /// <param name="ct">The cancellation token.</param>
    Task RemoveRelationAsync(string sourceNativeId, string targetNativeId, RelationType type, CancellationToken ct = default);
}
