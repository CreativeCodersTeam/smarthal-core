namespace SmartHal.Core.Backup;

/// <summary>
/// Centralized layout helpers for snapshot directories. Keeps the on-disk structure
/// in one place so the manager and orchestrator stay consistent.
/// </summary>
internal static class SnapshotPaths
{
    public const string ManifestFileName = "manifest.yaml";
    public const string DevicesDirectoryName = "devices";
    public const string AdapterBackupsDirectoryName = "adapter-backups";

    public static string GetSnapshotDirectory(string snapshotsRoot, string snapshotId)
        => Path.Combine(snapshotsRoot, snapshotId);

    public static string GetManifestPath(string snapshotsRoot, string snapshotId)
        => Path.Combine(snapshotsRoot, snapshotId, ManifestFileName);

    public static string GetDeviceConfigPath(string snapshotsRoot, string snapshotId, string deviceId)
        => Path.Combine(snapshotsRoot, snapshotId, DevicesDirectoryName, $"{deviceId}.yaml");

    public static string GetDeviceConfigRelativePath(string deviceId)
        => Path.Combine(DevicesDirectoryName, $"{deviceId}.yaml");

    public static string GetAdapterBackupPath(string snapshotsRoot, string snapshotId, string deviceId)
        => Path.Combine(snapshotsRoot, snapshotId, AdapterBackupsDirectoryName, $"{deviceId}.yaml");

    public static string GetAdapterBackupRelativePath(string deviceId)
        => Path.Combine(AdapterBackupsDirectoryName, $"{deviceId}.yaml");
}
