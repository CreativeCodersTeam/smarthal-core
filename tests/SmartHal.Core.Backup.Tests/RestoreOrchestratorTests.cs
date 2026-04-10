using AwesomeAssertions;
using FakeItEasy;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Backup;

public sealed class RestoreOrchestratorTests : IDisposable
{
    private readonly string _root;
    private readonly IConfigRepository _repo;
    private readonly IConfigReader _reader;
    private readonly IConfigDiffer _differ;
    private readonly SnapshotManager _snapshotManager;
    private readonly RestoreOrchestrator _sut;

    public RestoreOrchestratorTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"smarthal-restore-{Guid.NewGuid()}");
        Directory.CreateDirectory(_root);

        _repo = A.Fake<IConfigRepository>();
        _reader = A.Fake<IConfigReader>();
        _differ = A.Fake<IConfigDiffer>();
        _snapshotManager = new SnapshotManager(_root, _repo);
        _sut = new RestoreOrchestrator(_root, _snapshotManager, _repo, _reader, _differ);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static Device CreateDevice(string id = "dev-001", string adapterId = "hm-eg", string nativeId = "ABC123") =>
        new Device
        {
            Id = id,
            AdapterId = adapterId,
            NativeId = nativeId,
            Name = "Test Device"
        };

    private static DeviceSummary ToSummary(Device d) => new DeviceSummary { Id = d.Id, AdapterId = d.AdapterId, NativeId = d.NativeId, Name = d.Name };

    private async Task<SnapshotManifest> CreateSnapshotForAsync(Device device)
    {
        A.CallTo(() => _repo.GetDeviceAsync(device.Id, A<CancellationToken>._)).Returns(device);
        return await _snapshotManager.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = device.Id
        });
    }

    // --- Preview ---

    [Fact]
    public async Task PreviewRestoreAsync_ReturnsDiffPerDevice()
    {
        // Arrange
        var snapshotDevice = CreateDevice();
        snapshotDevice.Name = "Snapshot Name";
        var manifest = await CreateSnapshotForAsync(snapshotDevice);

        var liveDevice = CreateDevice();
        liveDevice.Name = "Current Name";

        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(liveDevice) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(liveDevice);
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>._, A<CancellationToken>._)).Returns(snapshotDevice);

        var expectedDiff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "name" }]
        };
        A.CallTo(() => _differ.ComputeDiff(liveDevice, snapshotDevice)).Returns(expectedDiff);

        // Act
        var result = await _sut.PreviewRestoreAsync(manifest.SnapshotId);

        // Assert
        result.SnapshotId.Should().Be(manifest.SnapshotId);
        result.Devices.Should().ContainSingle();
        result.Devices[0].DeviceId.Should().Be("dev-001");
        result.Devices[0].DeviceExists.Should().BeTrue();
        result.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task PreviewRestoreAsync_DeviceDeletedSinceSnapshot_DiffsAgainstEmpty()
    {
        // Arrange
        var snapshotDevice = CreateDevice();
        var manifest = await CreateSnapshotForAsync(snapshotDevice);

        // Live config no longer contains the device.
        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>());
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>._, A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _differ.ComputeDiff(A<Device>.That.Matches(d => d.Id == "dev-001"), snapshotDevice))
            .Returns(new DeviceDiff
            {
                DeviceId = "dev-001",
                Changes = [new DiffEntry { Kind = DiffKind.Added, Path = "name", NewValue = "Test Device" }]
            });

        // Act
        var result = await _sut.PreviewRestoreAsync(manifest.SnapshotId);

        // Assert
        result.Devices.Single().DeviceExists.Should().BeFalse();
    }

    // --- Restore ---

    [Fact]
    public async Task RestoreAsync_RestoresDevicesAndCreatesPreRestoreSnapshot()
    {
        // Arrange
        var snapshotDevice = CreateDevice();
        var manifest = await CreateSnapshotForAsync(snapshotDevice);

        var liveDevice = CreateDevice();
        liveDevice.Name = "Different Name";

        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(liveDevice) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(liveDevice);
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>._, A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _differ.ComputeDiff(A<Device>._, A<Device>._))
            .Returns(new DeviceDiff
            {
                DeviceId = "dev-001",
                Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "name" }]
            });

        // Act
        var result = await _sut.RestoreAsync(manifest.SnapshotId);

        // Assert
        result.Success.Should().BeTrue();
        result.DevicesRestored.Should().Be(1);
        A.CallTo(() => _repo.SaveDeviceAsync(snapshotDevice, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        // A pre-restore snapshot must have been created.
        var snapshots = await _snapshotManager.ListSnapshotsAsync(new SnapshotFilter { Trigger = "pre-restore" });
        snapshots.Should().ContainSingle();
    }

    [Fact]
    public async Task RestoreAsync_NoChangesAndNotForced_SkipsRestore()
    {
        // Arrange
        var snapshotDevice = CreateDevice();
        var manifest = await CreateSnapshotForAsync(snapshotDevice);

        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(snapshotDevice) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>._, A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _differ.ComputeDiff(A<Device>._, A<Device>._))
            .Returns(new DeviceDiff { DeviceId = "dev-001" }); // No changes

        // Act
        var result = await _sut.RestoreAsync(manifest.SnapshotId, force: false);

        // Assert
        result.DevicesRestored.Should().Be(0);
        A.CallTo(() => _repo.SaveDeviceAsync(A<Device>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task RestoreAsync_Forced_RestoresEvenWithoutChanges()
    {
        // Arrange
        var snapshotDevice = CreateDevice();
        var manifest = await CreateSnapshotForAsync(snapshotDevice);

        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(snapshotDevice) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>._, A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _differ.ComputeDiff(A<Device>._, A<Device>._))
            .Returns(new DeviceDiff { DeviceId = "dev-001" }); // No changes

        // Act
        var result = await _sut.RestoreAsync(manifest.SnapshotId, force: true);

        // Assert
        result.DevicesRestored.Should().Be(1);
    }

    [Fact]
    public async Task RestoreAsync_PerDeviceFailure_CollectsErrorAndContinues()
    {
        // Arrange — snapshot with two devices, save fails for the first.
        var d1 = CreateDevice("dev-001");
        var d2 = CreateDevice("dev-002", nativeId: "XYZ");

        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(d1);
        A.CallTo(() => _repo.GetDeviceAsync("dev-002", A<CancellationToken>._)).Returns(d2);
        A.CallTo(() => _repo.ListDevicesAsync(
                A<DeviceFilter>.That.Matches(f => f.AdapterId == "hm-eg"), A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(d1), ToSummary(d2) });

        var manifest = await _snapshotManager.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Adapter,
            ScopeId = "hm-eg"
        });

        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(d1), ToSummary(d2) });
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>.That.Contains("dev-001"), A<CancellationToken>._)).Returns(d1);
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>.That.Contains("dev-002"), A<CancellationToken>._)).Returns(d2);
        A.CallTo(() => _differ.ComputeDiff(A<Device>._, A<Device>._))
            .Returns(new DeviceDiff
            {
                DeviceId = "dev-001",
                Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "name" }]
            });

        A.CallTo(() => _repo.SaveDeviceAsync(d1, A<CancellationToken>._))
            .Throws(new InvalidOperationException("disk full"));

        // Act
        var result = await _sut.RestoreAsync(manifest.SnapshotId);

        // Assert
        result.Success.Should().BeFalse();
        result.DevicesRestored.Should().Be(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].DeviceId.Should().Be("dev-001");
        result.Errors[0].Message.Should().Contain("disk full");
    }

    [Fact]
    public async Task RestoreAsync_NonExistentSnapshot_Throws()
    {
        // Act
        var act = () => _sut.RestoreAsync("does-not-exist");

        // Assert
        await act.Should().ThrowAsync<SmartHalException>();
    }

    [Fact]
    public async Task PreviewRestoreAsync_NonExistentSnapshot_Throws()
    {
        // Act
        var act = () => _sut.PreviewRestoreAsync("does-not-exist");

        // Assert
        await act.Should().ThrowAsync<SmartHalException>();
    }

    [Fact]
    public async Task RestoreAsync_PreRestoreSnapshot_HasCorrectTrigger()
    {
        // Arrange
        var snapshotDevice = CreateDevice();
        var manifest = await CreateSnapshotForAsync(snapshotDevice);

        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(snapshotDevice) });
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>._, A<CancellationToken>._)).Returns(snapshotDevice);
        A.CallTo(() => _differ.ComputeDiff(A<Device>._, A<Device>._))
            .Returns(new DeviceDiff
            {
                DeviceId = "dev-001",
                Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "name" }]
            });

        // Act
        await _sut.RestoreAsync(manifest.SnapshotId);

        // Assert — exactly one pre-restore snapshot exists with the right trigger.
        var preRestores = await _snapshotManager.ListSnapshotsAsync(new SnapshotFilter { Trigger = "pre-restore" });
        preRestores.Should().ContainSingle();
        preRestores[0].Trigger.Should().Be("pre-restore");
        preRestores[0].Mode.Should().Be(SnapshotMode.Reference);
    }

    [Fact]
    public async Task RestoreAsync_EmbeddedBackup_AppliesAdapterRestore()
    {
        // Arrange
        var device = CreateDevice();

        var capability = A.Fake<IAdapterBackupCapability>();
        A.CallTo(() => capability.BackupDeviceAsync("ABC123", A<CancellationToken>._))
            .Returns(new BackupBlob { NativeId = "ABC123", AdapterType = "homematic" });

        var lookup = A.Fake<IAdapterLookup>();
        A.CallTo(() => lookup.GetBackupCapability("hm-eg")).Returns(capability);

        var manager = new SnapshotManager(_root, _repo, lookup);
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var manifest = await manager.CreateSnapshotAsync(new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = "dev-001",
            Mode = SnapshotMode.Embedded
        });

        var orchestrator = new RestoreOrchestrator(_root, manager, _repo, _reader, _differ, lookup);

        A.CallTo(() => _repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary> { ToSummary(device) });
        A.CallTo(() => _reader.ReadDeviceAsync(A<string>._, A<CancellationToken>._)).Returns(device);
        A.CallTo(() => _differ.ComputeDiff(A<Device>._, A<Device>._))
            .Returns(new DeviceDiff
            {
                DeviceId = "dev-001",
                Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "name" }]
            });

        // Act
        var result = await orchestrator.RestoreAsync(manifest.SnapshotId);

        // Assert
        result.Success.Should().BeTrue();
        A.CallTo(() => capability.RestoreDeviceAsync("ABC123", A<BackupBlob>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
