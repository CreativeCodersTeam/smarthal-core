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
    public async Task BackupCreate_DeviceScope_CreatesSnapshotWithDeviceScope()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var manifest = CreateManifest(ConfigScope.Device, "dev1");

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupCreateOptions { DeviceId = "dev1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r =>
                r.Scope == ConfigScope.Device && r.ScopeId == "dev1" && r.Trigger == "manual"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupCreate_NoScope_UsesAll()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var manifest = CreateManifest(ConfigScope.All, null);

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupCreateOptions());

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Scope == ConfigScope.All && r.ScopeId == null),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupCreate_AdapterScope_CreatesSnapshotWithAdapterScope()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var manifest = CreateManifest(ConfigScope.Adapter, "hm1");

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupCreateOptions { AdapterId = "hm1" });

        // Assert
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Scope == ConfigScope.Adapter && r.ScopeId == "hm1"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupCreate_RoomScope_CreatesSnapshotWithRoomScope()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var manifest = CreateManifest(ConfigScope.Room, "living_room");

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupCreateOptions { RoomId = "living_room" });

        // Assert
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Scope == ConfigScope.Room && r.ScopeId == "living_room"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("embedded")]
    [InlineData("EMBEDDED")]
    public async Task BackupCreate_EmbeddedMode_SetsEmbeddedSnapshotMode(string modeValue)
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var manifest = CreateManifest(ConfigScope.All, null, SnapshotMode.Embedded);

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        // Act
        await command.ExecuteAsync(new BackupCreateOptions { Mode = modeValue });

        // Assert
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Mode == SnapshotMode.Embedded),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupCreate_ReferenceMode_SetsReferenceSnapshotMode()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);
        var manifest = CreateManifest(ConfigScope.All, null);

        A.CallTo(() => snapshotManager.CreateSnapshotAsync(A<SnapshotRequest>._, A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupCreateCommand(snapshotManager, formatter);

        // Act
        await command.ExecuteAsync(new BackupCreateOptions { Mode = "reference" });

        // Assert
        A.CallTo(() => snapshotManager.CreateSnapshotAsync(
            A<SnapshotRequest>.That.Matches(r => r.Mode == SnapshotMode.Reference),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupList_PassesFilterToManager()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => snapshotManager.ListSnapshotsAsync(A<SnapshotFilter?>._, A<CancellationToken>._))
            .Returns(new List<SnapshotManifest>());

        var command = new BackupListCommand(snapshotManager, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupListOptions { Scope = "Device" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.ListSnapshotsAsync(
            A<SnapshotFilter?>.That.Matches(f => f != null && f.Scope == ConfigScope.Device),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupList_InvalidScope_DoesNotSetScopeFilter()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => snapshotManager.ListSnapshotsAsync(A<SnapshotFilter?>._, A<CancellationToken>._))
            .Returns(new List<SnapshotManifest>());

        var command = new BackupListCommand(snapshotManager, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupListOptions { Scope = "invalid_scope" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.ListSnapshotsAsync(
            A<SnapshotFilter?>.That.Matches(f => f != null && f.Scope == null),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupList_SinceFilter_ParsesDate()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        A.CallTo(() => snapshotManager.ListSnapshotsAsync(A<SnapshotFilter?>._, A<CancellationToken>._))
            .Returns(new List<SnapshotManifest>());

        var command = new BackupListCommand(snapshotManager, formatter);

        // Act
        await command.ExecuteAsync(new BackupListOptions { Since = "2026-01-01" });

        // Assert
        A.CallTo(() => snapshotManager.ListSnapshotsAsync(
            A<SnapshotFilter?>.That.Matches(f => f != null && f.Since != null),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupDelete_Confirmed_DeletesSnapshot()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);

        var command = new BackupDeleteCommand(snapshotManager, interaction, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupDeleteOptions { SnapshotId = "snap1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.DeleteSnapshotAsync("snap1", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupDelete_Cancelled_DoesNotDelete()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new BackupDeleteCommand(snapshotManager, interaction, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupDeleteOptions { SnapshotId = "snap1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.DeleteSnapshotAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task BackupCleanup_AppliesRetentionPolicy()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(A<RetentionPolicy>._, A<CancellationToken>._))
            .Returns(3);

        var command = new BackupCleanupCommand(snapshotManager, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupCleanupOptions
        {
            MaxAge = 30,
            MaxCount = 10,
            KeepManual = true
        });

        // Assert
        result.Should().Be(CommandResult.Success);
        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(
            A<RetentionPolicy>.That.Matches(p =>
                p.MaxSnapshots == 10 && p.MaxAge == TimeSpan.FromDays(30) && p.KeepManualSnapshots),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        console.Output.Should().Contain("3 snapshot(s)");
    }

    [Fact]
    public async Task BackupCleanup_NoMaxAge_SetsNullMaxAge()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);
        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(A<RetentionPolicy>._, A<CancellationToken>._))
            .Returns(0);

        var command = new BackupCleanupCommand(snapshotManager, formatter);

        // Act
        await command.ExecuteAsync(new BackupCleanupOptions { MaxCount = 5 });

        // Assert
        A.CallTo(() => snapshotManager.ApplyRetentionPolicyAsync(
            A<RetentionPolicy>.That.Matches(p => p.MaxAge == null && p.MaxSnapshots == 5),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BackupShow_DisplaysManifestAndEntries()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Json }, console);

        var manifest = new SnapshotManifest
        {
            SnapshotId = "snap1",
            CreatedAt = DateTimeOffset.Parse("2026-01-15T10:00:00Z"),
            Scope = ConfigScope.All,
            Mode = SnapshotMode.Reference,
            Trigger = "manual",
            Description = "Test snapshot",
            Entries =
            [
                new ManifestEntry { DeviceId = "dev1", AdapterId = "hm1", NativeId = "001", ConfigFilePath = "devices/dev1.yaml" }
            ]
        };

        A.CallTo(() => snapshotManager.GetSnapshotAsync("snap1", A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupShowCommand(snapshotManager, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupShowOptions { SnapshotId = "snap1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("snap1");
        console.Output.Should().Contain("dev1");
    }

    [Fact]
    public async Task BackupShow_NoEntries_DoesNotRenderEntryTable()
    {
        // Arrange
        var snapshotManager = A.Fake<ISnapshotManager>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext { OutputFormat = OutputFormat.Table }, console);

        var manifest = new SnapshotManifest
        {
            SnapshotId = "snap-empty",
            CreatedAt = DateTimeOffset.UtcNow,
            Scope = ConfigScope.All,
            Entries = []
        };

        A.CallTo(() => snapshotManager.GetSnapshotAsync("snap-empty", A<CancellationToken>._))
            .Returns(manifest);

        var command = new BackupShowCommand(snapshotManager, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupShowOptions { SnapshotId = "snap-empty" });

        // Assert
        result.Should().Be(CommandResult.Success);
        // No entry table headers should appear for empty entries
        console.Output.Should().NotContain("Native ID");
    }

    [Fact]
    public async Task BackupRestore_NoChanges_ReturnsSuccess()
    {
        // Arrange
        var orchestrator = A.Fake<IRestoreOrchestrator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        A.CallTo(() => orchestrator.PreviewRestoreAsync("snap1", A<CancellationToken>._))
            .Returns(new RestorePreviewResult { Devices = [] });

        var command = new BackupRestoreCommand(orchestrator, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupRestoreOptions { SnapshotId = "snap1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("No changes to restore");
    }

    [Fact]
    public async Task BackupRestore_DryRun_DoesNotApply()
    {
        // Arrange
        var orchestrator = A.Fake<IRestoreOrchestrator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var preview = new RestorePreviewResult
        {
            Devices =
            [
                new DeviceRestorePreview
                {
                    DeviceId = "dev1",
                    DeviceExists = true,
                    Diff = new DeviceDiff
                    {
                        Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "level", OldValue = "0.0", NewValue = "1.0" }]
                    }
                }
            ]
        };

        A.CallTo(() => orchestrator.PreviewRestoreAsync("snap1", A<CancellationToken>._))
            .Returns(preview);

        var command = new BackupRestoreCommand(orchestrator, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupRestoreOptions { SnapshotId = "snap1", DryRun = true });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("Dry run");
        A.CallTo(() => orchestrator.RestoreAsync(A<string>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task BackupRestore_Confirmed_RestoresSuccessfully()
    {
        // Arrange
        var orchestrator = A.Fake<IRestoreOrchestrator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var preview = new RestorePreviewResult
        {
            Devices =
            [
                new DeviceRestorePreview
                {
                    DeviceId = "dev1",
                    DeviceExists = true,
                    Diff = new DeviceDiff
                    {
                        Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "level", OldValue = "0", NewValue = "1" }]
                    }
                }
            ]
        };

        A.CallTo(() => orchestrator.PreviewRestoreAsync("snap1", A<CancellationToken>._))
            .Returns(preview);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);
        A.CallTo(() => orchestrator.RestoreAsync("snap1", A<bool>._, A<CancellationToken>._))
            .Returns(new RestoreResult { DevicesRestored = 1, DevicesSkipped = 0 });

        var command = new BackupRestoreCommand(orchestrator, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupRestoreOptions { SnapshotId = "snap1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("Restored 1 device(s)");
    }

    [Fact]
    public async Task BackupRestore_Cancelled_DoesNotRestore()
    {
        // Arrange
        var orchestrator = A.Fake<IRestoreOrchestrator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var preview = new RestorePreviewResult
        {
            Devices =
            [
                new DeviceRestorePreview
                {
                    DeviceId = "dev1",
                    DeviceExists = true,
                    Diff = new DeviceDiff
                    {
                        Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "p", OldValue = "a", NewValue = "b" }]
                    }
                }
            ]
        };

        A.CallTo(() => orchestrator.PreviewRestoreAsync("snap1", A<CancellationToken>._)).Returns(preview);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(false);

        var command = new BackupRestoreCommand(orchestrator, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupRestoreOptions { SnapshotId = "snap1" });

        // Assert
        result.Should().Be(CommandResult.Success);
        console.Output.Should().Contain("Cancelled");
        A.CallTo(() => orchestrator.RestoreAsync(A<string>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task BackupRestore_WithErrors_ReturnsNonZeroExitCode()
    {
        // Arrange
        var orchestrator = A.Fake<IRestoreOrchestrator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var preview = new RestorePreviewResult
        {
            Devices =
            [
                new DeviceRestorePreview
                {
                    DeviceId = "dev1",
                    DeviceExists = true,
                    Diff = new DeviceDiff
                    {
                        Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "p", OldValue = "a", NewValue = "b" }]
                    }
                }
            ]
        };

        A.CallTo(() => orchestrator.PreviewRestoreAsync("snap1", A<CancellationToken>._)).Returns(preview);
        A.CallTo(() => interaction.Confirm(A<string>._, A<bool>._)).Returns(true);
        A.CallTo(() => orchestrator.RestoreAsync("snap1", A<bool>._, A<CancellationToken>._))
            .Returns(new RestoreResult
            {
                DevicesRestored = 0,
                DevicesSkipped = 0,
                Errors = [new RestoreError { DeviceId = "dev1", Message = "Adapter offline" }]
            });

        var command = new BackupRestoreCommand(orchestrator, interaction, console, formatter);

        // Act
        var result = await command.ExecuteAsync(new BackupRestoreOptions { SnapshotId = "snap1" });

        // Assert
        result.ExitCode.Should().Be(1);
        console.Output.Should().Contain("Adapter offline");
    }

    [Fact]
    public async Task BackupRestore_DeletedDevice_ShowsDeletedMarker()
    {
        // Arrange
        var orchestrator = A.Fake<IRestoreOrchestrator>();
        var interaction = A.Fake<IUserInteraction>();
        var console = new TestConsole();
        var formatter = new OutputFormatter(new CliContext(), console);

        var preview = new RestorePreviewResult
        {
            Devices =
            [
                new DeviceRestorePreview
                {
                    DeviceId = "dev-gone",
                    DeviceExists = false,
                    Diff = new DeviceDiff
                    {
                        Changes = [new DiffEntry { Kind = DiffKind.Added, Path = "level", NewValue = "1" }]
                    }
                }
            ]
        };

        A.CallTo(() => orchestrator.PreviewRestoreAsync("snap1", A<CancellationToken>._)).Returns(preview);

        var command = new BackupRestoreCommand(orchestrator, interaction, console, formatter);

        // Act
        await command.ExecuteAsync(new BackupRestoreOptions { SnapshotId = "snap1", DryRun = true });

        // Assert
        console.Output.Should().Contain("deleted");
    }

    private static SnapshotManifest CreateManifest(
        ConfigScope scope,
        string? scopeId,
        SnapshotMode mode = SnapshotMode.Reference) =>
        new SnapshotManifest
    {
        SnapshotId = $"snap-{scope}-{scopeId ?? "all"}",
        CreatedAt = DateTimeOffset.UtcNow,
        Scope = scope,
        ScopeId = scopeId,
        Mode = mode,
        Trigger = "manual",
        Entries = []
    };
}
