using Microsoft.Extensions.Logging.Abstractions;
using AwesomeAssertions;
using FakeItEasy;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

public class FileConfigRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IConfigReader _reader;
    private readonly IConfigWriter _writer;
    private readonly FileConfigRepository _sut;

    public FileConfigRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        Directory.CreateDirectory(Path.Combine(_tempDir, "adapters"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "devices"));

        _reader = A.Fake<IConfigReader>();
        _writer = A.Fake<IConfigWriter>();
        _sut = new FileConfigRepository(_tempDir, _reader, _writer, NullLogger<FileConfigRepository>.Instance);
    }

    public void Dispose()
    {
        _sut.Dispose();
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    // --- Meta ---

    [Fact]
    public async Task GetMetaAsync_DelegatesToReader()
    {
        // Arrange
        var expected = new MetaConfig { SchemaVersion = "1.0" };
        A.CallTo(() => _reader.ReadMetaAsync(_tempDir, A<CancellationToken>._)).Returns(expected);

        // Act
        var result = await _sut.GetMetaAsync();

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task SaveMetaAsync_DelegatesToWriter()
    {
        // Arrange
        var meta = new MetaConfig { SchemaVersion = "1.0" };

        // Act
        await _sut.SaveMetaAsync(meta);

        // Assert
        A.CallTo(() => _writer.WriteMetaAsync(_tempDir, meta, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InitializeAsync_CreatesStructureAndWritesMeta()
    {
        // Arrange
        var meta = new MetaConfig { SchemaVersion = "1.0" };

        // Act
        await _sut.InitializeAsync(meta);

        // Assert
        A.CallTo(() => _writer.InitializeDirectoryStructureAsync(_tempDir, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _writer.WriteMetaAsync(_tempDir, meta, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    // --- Rooms ---

    [Fact]
    public async Task GetRoomsAsync_DelegatesToReader()
    {
        // Arrange
        var expected = new RoomsConfig();
        A.CallTo(() => _reader.ReadRoomsAsync(_tempDir, A<CancellationToken>._)).Returns(expected);

        // Act
        var result = await _sut.GetRoomsAsync();

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task SaveRoomsAsync_DelegatesToWriter()
    {
        // Arrange
        var rooms = new RoomsConfig();

        // Act
        await _sut.SaveRoomsAsync(rooms);

        // Assert
        A.CallTo(() => _writer.WriteRoomsAsync(_tempDir, rooms, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    // --- Adapters ---

    [Fact]
    public async Task GetAdapterConfigAsync_ReadsCorrectFile()
    {
        // Arrange
        var expectedPath = Path.Combine(_tempDir, "adapters", "hm-eg.yaml");
        var expected = new AdapterConfig { AdapterId = "hm-eg" };
        A.CallTo(() => _reader.ReadAdapterConfigAsync(expectedPath, A<CancellationToken>._)).Returns(expected);

        // Act
        var result = await _sut.GetAdapterConfigAsync("hm-eg");

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetAllAdapterConfigsAsync_NoAdaptersDir_ReturnsEmpty()
    {
        // Arrange
        var emptyDir = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(emptyDir);
        var repo = new FileConfigRepository(emptyDir, _reader, _writer, NullLogger<FileConfigRepository>.Instance);

        try
        {
            // Act
            var result = await repo.GetAllAdapterConfigsAsync();

            // Assert
            result.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(emptyDir, true);
            repo.Dispose();
        }
    }

    [Fact]
    public async Task GetAllAdapterConfigsAsync_WithFiles_ReadsAllYamlFiles()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "adapters", "hm-eg.yaml"), "");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "adapters", "zb-01.yaml"), "");

        A.CallTo(() => _reader.ReadAdapterConfigAsync(A<string>._, A<CancellationToken>._))
            .Returns(new AdapterConfig());

        // Act
        var result = await _sut.GetAllAdapterConfigsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SaveAdapterConfigAsync_WritesToCorrectFile()
    {
        // Arrange
        var config = new AdapterConfig { AdapterId = "hm-eg" };
        var expectedPath = Path.Combine(_tempDir, "adapters", "hm-eg.yaml");

        // Act
        await _sut.SaveAdapterConfigAsync(config);

        // Assert
        A.CallTo(() => _writer.WriteAdapterConfigAsync(expectedPath, config, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    // --- Devices ---

    [Fact]
    public async Task GetDeviceAsync_ReadsCorrectFile()
    {
        // Arrange
        var expectedPath = Path.Combine(_tempDir, "devices", "dev-001.yaml");
        var expected = new Device { Id = "dev-001" };
        A.CallTo(() => _reader.ReadDeviceAsync(expectedPath, A<CancellationToken>._)).Returns(expected);

        // Act
        var result = await _sut.GetDeviceAsync("dev-001");

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task SaveDeviceAsync_WritesToCorrectFile()
    {
        // Arrange
        var device = new Device { Id = "dev-001" };
        var expectedPath = Path.Combine(_tempDir, "devices", "dev-001.yaml");

        // Act
        await _sut.SaveDeviceAsync(device);

        // Assert
        A.CallTo(() => _writer.WriteDeviceAsync(expectedPath, device, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteDeviceAsync_DeletesCorrectFile()
    {
        // Arrange
        var expectedPath = Path.Combine(_tempDir, "devices", "dev-001.yaml");

        // Act
        await _sut.DeleteDeviceAsync("dev-001");

        // Assert
        A.CallTo(() => _writer.DeleteDeviceFileAsync(expectedPath, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ListDevicesAsync_NoDevicesDir_ReturnsEmpty()
    {
        // Arrange
        var emptyDir = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(emptyDir);
        var repo = new FileConfigRepository(emptyDir, _reader, _writer, NullLogger<FileConfigRepository>.Instance);

        try
        {
            // Act
            var result = await repo.ListDevicesAsync();

            // Assert
            result.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(emptyDir, true);
            repo.Dispose();
        }
    }

    [Fact]
    public async Task ListDevicesAsync_WithFiles_ReadsSummaries()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-001.yaml"), "");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-002.yaml"), "");

        A.CallTo(() => _reader.ReadDeviceSummaryAsync(A<string>._, A<CancellationToken>._))
            .Returns(new DeviceSummary { Id = "dev" });

        // Act
        var result = await _sut.ListDevicesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    // --- Filtered Listing ---

    [Fact]
    public async Task ListDevicesAsync_FilterByAdapterId_ReturnsMatching()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-001.yaml"), "");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-002.yaml"), "");

        var callCount = 0;
        A.CallTo(() => _reader.ReadDeviceSummaryAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily(() =>
            {
                callCount++;
                return callCount == 1
                    ? new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", Name = "Test1" }
                    : new DeviceSummary { Id = "dev-002", AdapterId = "zb-01", Name = "Test2" };
            });

        // Act
        var result = await _sut.ListDevicesAsync(new DeviceFilter { AdapterId = "hm-eg" });

        // Assert
        result.Should().ContainSingle(d => d.AdapterId == "hm-eg");
    }

    [Fact]
    public async Task ListDevicesAsync_FilterByNamePattern_ReturnsMatching()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-001.yaml"), "");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-002.yaml"), "");

        var callCount = 0;
        A.CallTo(() => _reader.ReadDeviceSummaryAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily(() =>
            {
                callCount++;
                return callCount == 1
                    ? new DeviceSummary { Id = "dev-001", AdapterId = "hm", Name = "Licht Wohnzimmer" }
                    : new DeviceSummary { Id = "dev-002", AdapterId = "hm", Name = "Rolladen Küche" };
            });

        // Act
        var result = await _sut.ListDevicesAsync(new DeviceFilter { NamePattern = "*Licht*" });

        // Assert
        result.Should().ContainSingle(d => d.Name == "Licht Wohnzimmer");
    }

    [Fact]
    public async Task ListDevicesAsync_FilterByRoomId_ReturnsMatching()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-001.yaml"), "");

        A.CallTo(() => _reader.ReadDeviceSummaryAsync(A<string>._, A<CancellationToken>._))
            .Returns(new DeviceSummary { Id = "dev-001", AdapterId = "hm", Name = "Test", RoomId = "room-1" });

        // Act
        var result = await _sut.ListDevicesAsync(new DeviceFilter { RoomId = "room-1" });

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListDevicesAsync_FilterByGroupId_LoadsFullDevices()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-001.yaml"), "");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-002.yaml"), "");

        var callCount = 0;
        A.CallTo(() => _reader.ReadDeviceSummaryAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily(() =>
            {
                callCount++;
                return callCount == 1
                    ? new DeviceSummary { Id = "dev-001", AdapterId = "hm", Name = "Test1" }
                    : new DeviceSummary { Id = "dev-002", AdapterId = "hm", Name = "Test2" };
            });

        A.CallTo(() => _reader.ReadDeviceAsync(
            Path.Combine(_tempDir, "devices", "dev-001.yaml"), A<CancellationToken>._))
            .Returns(new Device { Id = "dev-001", GroupIds = ["group-lights"] });
        A.CallTo(() => _reader.ReadDeviceAsync(
            Path.Combine(_tempDir, "devices", "dev-002.yaml"), A<CancellationToken>._))
            .Returns(new Device { Id = "dev-002", GroupIds = ["group-shutters"] });

        // Act
        var result = await _sut.ListDevicesAsync(new DeviceFilter { GroupId = "group-lights" });

        // Assert
        result.Should().ContainSingle(d => d.Id == "dev-001");
    }

    [Fact]
    public async Task ListDevicesAsync_FilterByRoomId_NoMatch_ReturnsEmpty()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev-001.yaml"), "");

        A.CallTo(() => _reader.ReadDeviceSummaryAsync(A<string>._, A<CancellationToken>._))
            .Returns(new DeviceSummary { Id = "dev-001", AdapterId = "hm", Name = "Test", RoomId = "room-1" });

        // Act
        var result = await _sut.ListDevicesAsync(new DeviceFilter { RoomId = "room-99" });

        // Assert
        result.Should().BeEmpty();
    }
}
