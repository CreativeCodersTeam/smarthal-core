using AwesomeAssertions;
using FakeItEasy;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Backup;

public sealed class SnapshotManifestSerializationTests : IDisposable
{
    private readonly string _root;
    private readonly IConfigRepository _repo;
    private readonly SnapshotManager _sut;

    public SnapshotManifestSerializationTests()
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

    [Fact]
    public async Task ManifestRoundtrip_PreservesAllFields()
    {
        // Arrange
        var device = new Device
        {
            Id = "dev-001",
            AdapterId = "hm-eg",
            NativeId = "ABC123",
            Name = "Living Room Light"
        };
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var request = new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = "dev-001",
            Mode = SnapshotMode.Reference,
            Description = "Initial backup before tweaking brightness",
            Trigger = "manual"
        };

        // Act
        var written = await _sut.CreateSnapshotAsync(request);
        var read = await _sut.GetSnapshotAsync(written.SnapshotId);

        // Assert — every persisted field must come back with the exact same value.
        read.SnapshotId.Should().Be(written.SnapshotId);
        read.Scope.Should().Be(ConfigScope.Device);
        read.ScopeId.Should().Be("dev-001");
        read.Mode.Should().Be(SnapshotMode.Reference);
        read.Description.Should().Be(request.Description);
        read.Trigger.Should().Be("manual");
        read.CreatedAt.Should().BeCloseTo(written.CreatedAt, TimeSpan.FromSeconds(1));
        read.Entries.Should().ContainSingle();

        var entry = read.Entries.Single();
        entry.DeviceId.Should().Be("dev-001");
        entry.AdapterId.Should().Be("hm-eg");
        entry.NativeId.Should().Be("ABC123");
        entry.ConfigFilePath.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateSnapshotAsync_NullDescription_PersistsNullDescription()
    {
        // Arrange
        var device = new Device
        {
            Id = "dev-001",
            AdapterId = "hm-eg",
            NativeId = "ABC123",
            Name = "Living Room Light"
        };
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var request = new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = "dev-001",
            Mode = SnapshotMode.Reference,
            Description = null,
            Trigger = "manual"
        };

        // Act
        var written = await _sut.CreateSnapshotAsync(request);
        var read = await _sut.GetSnapshotAsync(written.SnapshotId);

        // Assert
        read.Description.Should().BeNull();
    }

    [Fact]
    public async Task CreateSnapshotAsync_SpecialCharactersInDescription_PreservesExactly()
    {
        // Arrange
        var device = new Device
        {
            Id = "dev-001",
            AdapterId = "hm-eg",
            NativeId = "ABC123",
            Name = "Living Room Light"
        };
        A.CallTo(() => _repo.GetDeviceAsync("dev-001", A<CancellationToken>._)).Returns(device);

        var description = "Über <backup> & \"restore\" — done!";
        var request = new SnapshotRequest
        {
            Scope = ConfigScope.Device,
            ScopeId = "dev-001",
            Mode = SnapshotMode.Reference,
            Description = description,
            Trigger = "manual"
        };

        // Act
        var written = await _sut.CreateSnapshotAsync(request);
        var read = await _sut.GetSnapshotAsync(written.SnapshotId);

        // Assert
        read.Description.Should().Be(description);
    }
}
