using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup create command.</summary>
public class BackupCreateOptions
{
    /// <summary>Scope to a single device.</summary>
    [OptionParameter('d', "device", HelpText = "Scope to a single device")]
    public string? DeviceId { get; set; }

    /// <summary>Scope to all devices of an adapter.</summary>
    [OptionParameter('a', "adapter", HelpText = "Scope to all devices of an adapter")]
    public string? AdapterId { get; set; }

    /// <summary>Scope to all devices in a room.</summary>
    [OptionParameter('r', "room", HelpText = "Scope to all devices in a room")]
    public string? RoomId { get; set; }

    /// <summary>Snapshot mode (reference or embedded).</summary>
    [OptionParameter('m', "mode", HelpText = "Snapshot mode: reference or embedded", DefaultValue = "reference")]
    public string Mode { get; set; } = "reference";

    /// <summary>Description for the snapshot.</summary>
    [OptionParameter(default(char), "description", HelpText = "Description for the snapshot")]
    public string? Description { get; set; }
}

/// <summary>
/// Creates a configuration snapshot.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "create"], Name = "create", Description = "Create a configuration snapshot")]
public class BackupCreateCommand(
    ISnapshotManager snapshotManager,
    OutputFormatter formatter) : ICliCommand<BackupCreateOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupCreateOptions options)
    {
        var (scope, scopeId) = ResolveScope(options);
        var mode = options.Mode.Equals("embedded", StringComparison.OrdinalIgnoreCase)
            ? SnapshotMode.Embedded
            : SnapshotMode.Reference;

        var request = new SnapshotRequest
        {
            Scope = scope,
            ScopeId = scopeId,
            Mode = mode,
            Description = options.Description,
            Trigger = "manual"
        };

        var manifest = await snapshotManager.CreateSnapshotAsync(request).ConfigureAwait(false);

        formatter.WriteObject(
            manifest,
            ("Snapshot ID", manifest.SnapshotId),
            ("Created", manifest.CreatedAt.ToString("o")),
            ("Scope", manifest.Scope.ToString()),
            ("Mode", manifest.Mode.ToString()),
            ("Devices", manifest.Entries.Count.ToString()));

        return CommandResult.Success;
    }

    private static (ConfigScope Scope, string? ScopeId) ResolveScope(BackupCreateOptions options)
    {
        if (options.DeviceId is not null) return (ConfigScope.Device, options.DeviceId);
        if (options.AdapterId is not null) return (ConfigScope.Adapter, options.AdapterId);
        if (options.RoomId is not null) return (ConfigScope.Room, options.RoomId);
        return (ConfigScope.All, null);
    }
}
