using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using FakeItEasy;
using SmartHal.CLI.Commands.Backup;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;
using Spectre.Console.Testing;

namespace SmartHal.CLI.Commands;

public class BackupCommandTests
{
    [Fact]
    public async Task BackupCreate_CreatesSnapshotWithCorrectScope()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
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

        var result = await command.ExecuteAsync(new BackupCreateOptions { DeviceId = "dev1" });
        result.Should().Be(CommandResult.Success);

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r =>
                r.Scope == ConfigScope.Device && r.ScopeId == "dev1" && r.Trigger == "manual"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupCreate_NoScope_UsesAll()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
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

        var result = await command.ExecuteAsync(new BackupCreateOptions());
        result.Should().Be(CommandResult.Success);

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Scope == ConfigScope.All && r.ScopeId == null),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupList_PassesFilterToManager()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => snapshotManager.ListSnapshotsAsync(A<SnapshotFilter?>._, A<CancellationToken>._))
            .Returns(new List<SnapshotManifest>());

        var command = new BackupListCommand(snapshotManager, formatter);

        var result = await command.ExecuteAsync(new BackupListOptions { Scope = "Device" });
        result.Should().Be(CommandResult.Success);

        A.CallTo(() => snapshotManager.ListSnapshotsAsync(
            A<SnapshotFilter?>.That.Matches(f => f != null && f.Scope == ConfigScope.Device),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupDelete_Confirmed_DeletesSnapshot()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);

        var command = new BackupDeleteCommand(snapshotManager, interaction, formatter);

        var result = await command.ExecuteAsync(new BackupDeleteOptions { SnapshotId = "snap1" });
        result.Should().Be(CommandResult.Success);

        A.CallTo(() => snapshotManager.DeleteSnapshotAsync("snap1", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupDelete_Cancelled_DoesNotDelete()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new BackupDeleteCommand(snapshotManager, interaction, formatter);

        var result = await command.ExecuteAsync(new BackupDeleteOptions { SnapshotId = "snap1" });
        result.Should().Be(CommandResult.Success);

        A.CallTo(() => snapshotManager.DeleteSnapshotAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task BackupCleanup_AppliesRetentionPolicy()
    {
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(A<RetentionPolicy>._, A<CancellationToken>._))
            .Returns(3);

        var command = new BackupCleanupCommand(snapshotManager, formatter);

        var result = await command.ExecuteAsync(new BackupCleanupOptions
        {
            MaxAge = 30,
            MaxCount = 10,
            KeepManual = true
        });
        result.Should().Be(CommandResult.Success);

        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(
            A<RetentionPolicy>.That.Matches(p =>
                p.MaxSnapshots == 10 && p.MaxAge == TimeSpan.FromDays(30) && p.KeepManualSnapshots),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
