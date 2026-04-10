using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Commands.Backup;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands;

public class BackupCommandTests
{
    [Fact]
    public async Task BackupCreate_CreatesSnapshotWithCorrectScope()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json });
        var manifest = new SnapshotManifest
        {
            SnapshotId = "2026-04-10T10-00-00_device_dev1",
            CreatedAt = DateTimeOffset.UtcNow,
            Scope = ConfigScope.Device,
            ScopeId = "dev1",
            Mode = SnapshotMode.Reference,
            Trigger = "manual",
            Entries = []
        };

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        var original = Console.Out;
        Console.SetOut(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync(new BackupCreateOptions { DeviceId = "dev1" });
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetOut(original);
        }

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r =>
                r.Scope == ConfigScope.Device && r.ScopeId == "dev1" && r.Trigger == "manual"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupCreate_NoScope_UsesAll()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json });
        var manifest = new SnapshotManifest
        {
            SnapshotId = "2026-04-10T10-00-00_all",
            CreatedAt = DateTimeOffset.UtcNow,
            Scope = ConfigScope.All,
            Entries = []
        };

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        var original = Console.Out;
        Console.SetOut(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync(new BackupCreateOptions());
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetOut(original);
        }

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Scope == ConfigScope.All && r.ScopeId == null),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupList_PassesFilterToManager()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json });

        A.CallTo(() => snapshotManager.ListSnapshotsAsync(A<SnapshotFilter?>._, A<CancellationToken>._))
            .Returns(new List<SnapshotManifest>());

        var command = new BackupListCommand(snapshotManager, formatter);

        var original = Console.Out;
        Console.SetOut(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync(new BackupListOptions { Scope = "Device" });
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetOut(original);
        }

        A.CallTo(() => snapshotManager.ListSnapshotsAsync(
            A<SnapshotFilter?>.That.Matches(f => f != null && f.Scope == ConfigScope.Device),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupDelete_Confirmed_DeletesSnapshot()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var interaction = A.Fake<IUserInteraction>();
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);

        var command = new BackupDeleteCommand(snapshotManager, interaction);

        var origErr = Console.Error;
        Console.SetError(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync(new BackupDeleteOptions { SnapshotId = "snap1" });
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetError(origErr);
        }

        A.CallTo(() => snapshotManager.DeleteSnapshotAsync("snap1", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupDelete_Cancelled_DoesNotDelete()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var interaction = A.Fake<IUserInteraction>();
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new BackupDeleteCommand(snapshotManager, interaction);

        var origErr = Console.Error;
        Console.SetError(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync(new BackupDeleteOptions { SnapshotId = "snap1" });
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetError(origErr);
        }

        A.CallTo(() => snapshotManager.DeleteSnapshotAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task BackupCleanup_AppliesRetentionPolicy()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(A<RetentionPolicy>._, A<CancellationToken>._))
            .Returns(3);

        var command = new BackupCleanupCommand(snapshotManager);

        var origErr = Console.Error;
        Console.SetError(new StringWriter());
        try
        {
            var result = await command.ExecuteAsync(new BackupCleanupOptions
            {
                MaxAge = 30,
                MaxCount = 10,
                KeepManual = true
            });
            result.Should().Be(CommandResult.Success);
        }
        finally
        {
            Console.SetError(origErr);
        }

        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(
            A<RetentionPolicy>.That.Matches(p =>
                p.MaxSnapshots == 10 && p.MaxAge == TimeSpan.FromDays(30) && p.KeepManualSnapshots),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
