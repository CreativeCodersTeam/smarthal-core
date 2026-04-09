using AwesomeAssertions;
using FakeItEasy;
using SmartHal.Core;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Backup;

public class SnapshotManagerTests : IDisposable
{
    private readonly string _root;
    private readonly IConfigRepository _repo;
    private readonly SnapshotManager _sut;

    public SnapshotManagerTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"smarthal-snap-{Guid.NewGuid()}");
        Directory.CreateDirectory(_root);

        _repo = A.Fake<IConfigRepository>();
        _sut = new SnapshotManager(_root, _repo);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static Device CreateDevice(string id = "dev-001", string adapterId = "hm-eg", string nativeId = "ABC123") =>
        new()
        {
            Id = id,
            AdapterId = adapterId,
            NativeId = nativeId,
            Name = "Test Device",
            RoomId = "room-1"
        };

    private static DeviceSummary ToSummary(Device d) =>
        new() { Id = d.Id, AdapterId = d.AdapterId, NativeId = d.NativeId, Name = d.Name, RoomId = d.RoomId };

    // --- CreateSnapshot ---

    [Fact]
    public async Task CreateSnapshotAsync_DeviceScope_CreatesDirectoryAndManifest()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var request = new SnapshotRequest { Scope = ConfigScope.Device, ScopeId = "dev-001" };

        // Act
        var manifest = await _sut.CreateSnapshotAsync(request);

        // Assert
        manifest.SnapshotId.Should().StartWith(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"));
        manifest.SnapshotId.Should().Contain("device").And.Contain("dev-001");
        manifest.Entries.Should().ContainSingle(e => e.DeviceId == "dev-001");

        var snapshotDir = Path.Combine(_root, manifest.SnapshotId);
        Directory.Exists(snapshotDir).Should().BeTrue();
        File.Exists(Path.Combine(snapshotDir, "manifest.yaml")).Should().BeTrue();
        File.Exists(Path.Combine(snapshotDir, "devices", "dev-001.yaml")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateSnapshotAsync_AdapterScope_SnapshotsAllAdapterDevices()
    {
        // Arrange
        var d1 = CreateDevice("dev-001");
        var d2 = CreateDevice("dev-002", nativeId: "XYZ789");

        A.CallTo(() => _repo.ListDevicesAsync(
                A<DeviceFilter>.That.Matches(f => f.AdapterId == "hm-eg"), A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(d1), ToSummary(d2) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(d1);
        A.CallTo(() => _repo.GetDeviceAsync("dev-002", A<CancellationToken>._)).Returns(d2);

        var request = new SnapshotRequest { Scope = ConfigScope.Adapter, ScopeId = "hm-eg" };

        // Act
        var manifest = await _sut.CreateSnapshotAsync(request);

        // Assert
        manifest.Entries.Should().HaveCount(2);
        manifest.Entries.Should().Contain(e => e.DeviceId == "dev-001");
        manifest.Entries.Should().Contain(e => e.DeviceId == "dev-002");
    }

    [Fact]
    public async Task CreateSnapshotAsync_RoomScope_UsesRoomFilter()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.ListDevicesAsync(
                A<DeviceFilter>.That.Matches(f => f.RoomId == "room-1"), A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(device) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var request = new SnapshotRequest { Scope = ConfigScope.Room, ScopeId = "room-1" };

        // Act
        var manifest = await _sut.CreateSnapshotAsync(request);

        // Assert
        manifest.Scope.Should().Be(ConfigScope.Room);
        manifest.ScopeId.Should().Be("room-1");
        manifest.Entries.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateSnapshotAsync_AllScope_UsesUnfilteredList()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(device) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var request = new SnapshotRequest { Scope = ConfigScope.All };

        // Act
        var manifest = await _sut.CreateSnapshotAsync(request);

        // Assert
        manifest.Scope.Should().Be(ConfigScope.All);
        manifest.SnapshotId.Should().EndWith("_all");
        manifest.Entries.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateSnapshotAsync_DeviceScopeWithoutId_Throws()
    {
        // Arrange
        var request = new SnapshotRequest { Scope = ConfigScope.Device, ScopeId = null };

        // Act
        var act = () => _sut.CreateSnapshotAsync(request);

        // Assert
        await act.Should().ThrowAsync<SmartHalException>();
    }

    [Fact]
    public async Task CreateSnapshotAsync_EmbeddedMode_WritesAdapterBackup()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var capability = A.Fake<IAdapterBackupCapability>();
        A.CallTo(() => capability.BackupDeviceAsync("ABC123", A<CancellationToken>._))
            .Returns(new BackupBlob { NativeId = "ABC123", AdapterType = "homematic" });

        var lookup = A.Fake<IAdapterLookup>();
        A.CallTo(() => lookup.GetBackupCapability("hm-eg")).Returns(capability);

        var sut = new SnapshotManager(_root, _repo, lookup);

        // Act
        var manifest = await sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = "dev-001",
            Mode = SnapshotMode.Embedded
        });

        // Assert
        var entry = manifest.Entries.Single();
        entry.AdapterBackupPath.Should().NotBeNull();
        File.Exists(Path.Combine(_root, manifest.SnapshotId, entry.AdapterBackupPath!)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateSnapshotAsync_EmbeddedModeWithoutCapability_SkipsBackup()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var lookup = A.Fake<IAdapterLookup>();
        A.CallTo(() => lookup.GetBackupCapability("hm-eg")).Returns(null);

        var sut = new SnapshotManager(_root, _repo, lookup);

        // Act
        var manifest = await sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = "dev-001",
            Mode = SnapshotMode.Embedded
        });

        // Assert — no adapter backup file is created when the adapter doesn't support backups.
        manifest.Entries.Single().AdapterBackupPath.Should().BeNull();
    }

    // --- SnapshotId format ---

    [Fact]
    public void BuildSnapshotId_DeviceScope_HasExpectedFormat()
    {
        // Act
        var id = SnapshotManager.BuildSnapshotId(ConfigScope.Device, "dev-001");

        // Assert
        id.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}_device_dev-001$");
    }

    [Fact]
    public void BuildSnapshotId_AllScope_HasNoScopeId()
    {
        // Act
        var id = SnapshotManager.BuildSnapshotId(ConfigScope.All, scopeId: null);

        // Assert
        id.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}_all$");
    }

    // --- ListSnapshots ---

    [Fact]
    public async Task ListSnapshotsAsync_NoRoot_ReturnsEmpty()
    {
        // Arrange
        var emptyRoot = Path.Combine(Path.GetTempPath(), $"smarthal-snap-empty-{Guid.NewGuid()}");
        var sut = new SnapshotManager(emptyRoot, _repo);

        // Act
        var result = await sut.ListSnapshotsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListSnapshotsAsync_MultipleSnapshots_ReturnsNewestFirst()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var first = await _sut.CreateSnapshotAsync(new SnapshotRequest { Scope = ConfigScope.Device, ScopeId = "dev-001" });
        await Task.Delay(1100); // Ensure timestamp ordering at second resolution
        var second = await _sut.CreateSnapshotAsync(new SnapshotRequest { Scope = ConfigScope.Device, ScopeId = "dev-001" });

        // Act
        var result = await _sut.ListSnapshotsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].SnapshotId.Should().Be(second.SnapshotId);
        result[1].SnapshotId.Should().Be(first.SnapshotId);
    }

    [Fact]
    public async Task ListSnapshotsAsync_FilterByScope_ReturnsMatching()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);
        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(device) });

        await _sut.CreateSnapshotAsync(new SnapshotRequest { Scope = ConfigScope.Device, ScopeId = "dev-001" });
        await _sut.CreateSnapshotAsync(new SnapshotRequest { Scope = ConfigScope.All });

        // Act
        var result = await _sut.ListSnapshotsAsync(new SnapshotFilter { Scope = ConfigScope.All });

        // Assert
        result.Should().HaveCount(1);
        result[0].Scope.Should().Be(ConfigScope.All);
    }

    [Fact]
    public async Task ListSnapshotsAsync_FilterByTrigger_ReturnsMatching()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        await _sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device, ScopeId = "dev-001", Trigger = "manual"
        });
        await _sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device, ScopeId = "dev-001", Trigger = "pre-restore"
        });

        // Act
        var result = await _sut.ListSnapshotsAsync(new SnapshotFilter { Trigger = "pre-restore" });

        // Assert
        result.Should().HaveCount(1);
        result[0].Trigger.Should().Be("pre-restore");
    }

    [Fact]
    public async Task ListSnapshotsAsync_IgnoresDirectoriesWithoutManifest()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_root, "incomplete-snapshot"));

        // Act
        var result = await _sut.ListSnapshotsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // --- GetSnapshot ---

    [Fact]
    public async Task GetSnapshotAsync_Existing_ReturnsManifest()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var created = await _sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device, ScopeId = "dev-001", Description = "Initial"
        });

        // Act
        var manifest = await _sut.GetSnapshotAsync(created.SnapshotId);

        // Assert
        manifest.SnapshotId.Should().Be(created.SnapshotId);
        manifest.Description.Should().Be("Initial");
    }

    [Fact]
    public async Task GetSnapshotAsync_NotFound_Throws()
    {
        // Act
        var act = () => _sut.GetSnapshotAsync("does-not-exist");

        // Assert
        await act.Should().ThrowAsync<SmartHalException>();
    }

    // --- DeleteSnapshot ---

    [Fact]
    public async Task DeleteSnapshotAsync_Existing_RemovesDirectory()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);
        var created = await _sut.CreateSnapshotAsync(new SnapshotRequest { Scope = ConfigScope.Device, ScopeId = "dev-001" });

        // Act
        await _sut.DeleteSnapshotAsync(created.SnapshotId);

        // Assert
        Directory.Exists(Path.Combine(_root, created.SnapshotId)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSnapshotAsync_NotFound_Throws()
    {
        // Act
        var act = () => _sut.DeleteSnapshotAsync("does-not-exist");

        // Assert
        await act.Should().ThrowAsync<SmartHalException>();
    }

    // --- Retention ---

    [Fact]
    public async Task ApplyRetentionPolicyAsync_MaxSnapshots_DeletesOldestExcess()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var created = new List<SnapshotManifest>();
        for (var i = 0; i < 3; i++)
        {
            created.Add(await _sut.CreateSnapshotAsync(new SnapshotRequest
            {
                Scope = ConfigScope.Device, ScopeId = "dev-001", Trigger = "auto"
            }));
            await Task.Delay(1100);
        }

        // Act — keep only the 2 newest
        var deleted = await _sut.ApplyRetentionPolicyAsync(new RetentionPolicy { MaxSnapshots = 2, KeepManualSnapshots = false });

        // Assert
        deleted.Should().Be(1);
        var remaining = await _sut.ListSnapshotsAsync();
        remaining.Should().HaveCount(2);
        remaining.Should().NotContain(s => s.SnapshotId == created[0].SnapshotId);
    }

    [Fact]
    public async Task ApplyRetentionPolicyAsync_KeepManualSnapshots_PreservesManual()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var manual = await _sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device, ScopeId = "dev-001", Trigger = "manual"
        });
        await Task.Delay(1100);
        await _sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device, ScopeId = "dev-001", Trigger = "auto"
        });
        await Task.Delay(1100);
        await _sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device, ScopeId = "dev-001", Trigger = "auto"
        });

        // Act — only one auto-snapshot allowed; manual must survive regardless.
        var deleted = await _sut.ApplyRetentionPolicyAsync(new RetentionPolicy
        {
            MaxSnapshots = 1,
            KeepManualSnapshots = true
        });

        // Assert
        deleted.Should().Be(1);
        var remaining = await _sut.ListSnapshotsAsync();
        remaining.Should().Contain(s => s.SnapshotId == manual.SnapshotId);
    }

    [Fact]
    public async Task ApplyRetentionPolicyAsync_MaxAge_DeletesOldSnapshots()
    {
        // Arrange
        var device = CreateDevice();
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var created = await _sut.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device, ScopeId = "dev-001", Trigger = "auto"
        });

        // Manually rewrite the manifest to backdate the snapshot. Quote the value so the
        // colons in the offset don't confuse the YAML parser.
        var manifestPath = Path.Combine(_root, created.SnapshotId, "manifest.yaml");
        var content = await File.ReadAllTextAsync(manifestPath);
        var oldDate = DateTimeOffset.UtcNow.AddDays(-30);
        content = System.Text.RegularExpressions.Regex.Replace(
            content, @"created_at:.*", $"created_at: '{oldDate:o}'");
        await File.WriteAllTextAsync(manifestPath, content);

        // Act
        var deleted = await _sut.ApplyRetentionPolicyAsync(new RetentionPolicy
        {
            MaxAge = TimeSpan.FromDays(7),
            KeepManualSnapshots = false
        });

        // Assert
        deleted.Should().Be(1);
        Directory.Exists(Path.Combine(_root, created.SnapshotId)).Should().BeFalse();
    }
}
