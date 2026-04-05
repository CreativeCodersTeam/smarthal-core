using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that can manage device relations.
/// </summary>
public interface IRelationManager
{
    /// <summary>Reads all relations for a device.</summary>
    Task<IReadOnlyList<Relation>> ReadRelationsAsync(string nativeId, CancellationToken ct = default);

    /// <summary>Creates a relation between two devices.</summary>
    Task AddRelationAsync(string sourceNativeId, string targetNativeId, RelationType type, CancellationToken ct = default);

    /// <summary>Removes a relation between two devices.</summary>
    Task RemoveRelationAsync(string sourceNativeId, string targetNativeId, RelationType type, CancellationToken ct = default);
}
