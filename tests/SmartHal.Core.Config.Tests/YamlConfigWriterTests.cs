using Microsoft.Extensions.Logging.Abstractions;
using AwesomeAssertions;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

public class YamlConfigWriterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly YamlConfigWriter _sut = new YamlConfigWriter(NullLogger<YamlConfigWriter>.Instance);
    private readonly YamlConfigReader _reader = new YamlConfigReader(NullLogger<YamlConfigReader>.Instance);

    public YamlConfigWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    // --- Roundtrip Tests ---

    [Fact]
    public async Task MetaRoundtrip_WriteAndRead_ReturnsIdenticalData()
    {
        // Arrange
        var meta = new MetaConfig { SchemaVersion = "1.0", SecretsProvider = "file" };

        // Act
        await _sut.WriteMetaAsync(_tempDir, meta);
        var result = await _reader.ReadMetaAsync(_tempDir);

        // Assert
        result.SchemaVersion.Should().Be("1.0");
        result.SecretsProvider.Should().Be("file");
    }

    [Fact]
    public async Task RoomsRoundtrip_WriteAndRead_ReturnsIdenticalData()
    {
        // Arrange
        var rooms = new RoomsConfig
        {
            Rooms = [new Room { Id = "room-1", Name = "Wohnzimmer", Floor = "0" }],
            Groups = [new Group { Id = "group-1", Name = "Lichter" }]
        };

        // Act
        await _sut.WriteRoomsAsync(_tempDir, rooms);
        var result = await _reader.ReadRoomsAsync(_tempDir);

        // Assert
        result.Rooms.Should().HaveCount(1);
        result.Rooms[0].Id.Should().Be("room-1");
        result.Groups.Should().HaveCount(1);
        result.Groups[0].Id.Should().Be("group-1");
    }

    [Fact]
    public async Task AdapterConfigRoundtrip_WriteAndRead_ReturnsIdenticalData()
    {
        // Arrange
        var config = new AdapterConfig
        {
            AdapterId = "hm-eg",
            AdapterType = "homematic",
            Settings = new Dictionary<string, string> { ["host"] = "192.168.1.1" }
        };
        var filePath = Path.Combine(_tempDir, "adapter.yaml");

        // Act
        await _sut.WriteAdapterConfigAsync(filePath, config);
        var result = await _reader.ReadAdapterConfigAsync(filePath);

        // Assert
        result.AdapterId.Should().Be("hm-eg");
        result.AdapterType.Should().Be("homematic");
        result.Settings["host"].Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task DeviceRoundtrip_WriteAndRead_ReturnsIdenticalData()
    {
        // Arrange
        var device = new Device
        {
            Id = "dev-001",
            AdapterId = "hm-eg",
            NativeId = "ABC123",
            Type = "light",
            Name = "Licht Wohnzimmer",
            RoomId = "room-1"
        };
        var filePath = Path.Combine(_tempDir, "device.yaml");

        // Act
        await _sut.WriteDeviceAsync(filePath, device);
        var result = await _reader.ReadDeviceAsync(filePath);

        // Assert
        result.Id.Should().Be("dev-001");
        result.Name.Should().Be("Licht Wohnzimmer");
        result.RoomId.Should().Be("room-1");
    }

    // --- InitializeDirectoryStructureAsync ---

    [Fact]
    public async Task InitializeDirectoryStructureAsync_CreatesExpectedStructure()
    {
        // Arrange
        var configDir = Path.Combine(_tempDir, "new-config");

        // Act
        await _sut.InitializeDirectoryStructureAsync(configDir);

        // Assert
        Directory.Exists(configDir).Should().BeTrue();
        Directory.Exists(Path.Combine(configDir, "adapters")).Should().BeTrue();
        Directory.Exists(Path.Combine(configDir, "devices")).Should().BeTrue();
        File.Exists(Path.Combine(configDir, "meta.yaml")).Should().BeTrue();
        File.Exists(Path.Combine(configDir, "rooms.yaml")).Should().BeTrue();
    }

    [Fact]
    public async Task InitializeDirectoryStructureAsync_ExistingMeta_DoesNotOverwrite()
    {
        // Arrange
        var configDir = Path.Combine(_tempDir, "existing-config");
        Directory.CreateDirectory(configDir);
        var metaPath = Path.Combine(configDir, "meta.yaml");
        await File.WriteAllTextAsync(metaPath, "schema_version: \"2.0\"");

        // Act
        await _sut.InitializeDirectoryStructureAsync(configDir);

        // Assert
        var meta = await _reader.ReadMetaAsync(configDir);
        meta.SchemaVersion.Should().Be("2.0");
    }

    // --- DeleteDeviceFileAsync ---

    [Fact]
    public async Task DeleteDeviceFileAsync_ExistingFile_DeletesFile()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "device.yaml");
        await File.WriteAllTextAsync(filePath, "id: dev-001");

        // Act
        await _sut.DeleteDeviceFileAsync(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDeviceFileAsync_NonExistentFile_DoesNotThrow()
    {
        // Act
        var act = () => _sut.DeleteDeviceFileAsync(Path.Combine(_tempDir, "nonexistent.yaml"));

        // Assert
        await act.Should().NotThrowAsync();
    }

    // --- WriteAsync creates parent directories ---

    [Fact]
    public async Task WriteAdapterConfigAsync_ParentDirMissing_CreatesDirectory()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "sub", "dir", "adapter.yaml");

        // Act
        await _sut.WriteAdapterConfigAsync(filePath, new AdapterConfig { AdapterId = "test" });

        // Assert
        File.Exists(filePath).Should().BeTrue();
    }
}
