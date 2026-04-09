namespace SmartHal.Core.Backup;

/// <summary>
/// Defines the depth of data captured in a snapshot.
/// </summary>
public enum SnapshotMode
{
    /// <summary>Only YAML configuration is captured. Fast and lightweight.</summary>
    Reference,

    /// <summary>YAML configuration plus adapter-specific backup data. Complete but slower.</summary>
    Embedded
}
