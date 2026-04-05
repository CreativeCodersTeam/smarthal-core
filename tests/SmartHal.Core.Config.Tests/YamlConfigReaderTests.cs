using AwesomeAssertions;
using SmartHal.Core;
using SmartHal.Core.Config;

namespace SmartHal.Core.Config;

public class YamlConfigReaderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly YamlConfigReader _sut = new();

    public YamlConfigReaderTests()
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

    // --- ReadMetaAsync ---

    [Fact]
    public async Task ReadMetaAsync_ValidYaml_ReturnsMetaConfig()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"),
            """
            schema_version: "1.0"
            secrets_provider: auto
            """);

        // Act
        var meta = await _sut.ReadMetaAsync(_tempDir);

        // Assert
        meta.SchemaVersion.Should().Be("1.0");
        meta.SecretsProvider.Should().Be("auto");
    }

    [Fact]
    public async Task ReadMetaAsync_FileNotFound_ThrowsConfigFileException()
    {
        // Act
        var act = () => _sut.ReadMetaAsync(_tempDir);

        // Assert
        await act.Should().ThrowAsync<SmartHalConfigFileException>();
    }

    [Fact]
    public async Task ReadMetaAsync_InvalidYaml_ThrowsConfigFileException()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"),
            """
            : invalid: yaml: [
            """);

        // Act
        var act = () => _sut.ReadMetaAsync(_tempDir);

        // Assert
        await act.Should().ThrowAsync<SmartHalConfigFileException>();
    }

    // --- ReadRoomsAsync ---

    [Fact]
    public async Task ReadRoomsAsync_ValidYaml_ReturnsRoomsConfig()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "rooms.yaml"),
            """
            rooms:
              - id: room-1
                name: Wohnzimmer
                floor: 0
            groups:
              - id: group-1
                name: Lichter
            """);

        // Act
        var rooms = await _sut.ReadRoomsAsync(_tempDir);

        // Assert
        rooms.Rooms.Should().HaveCount(1);
        rooms.Rooms[0].Id.Should().Be("room-1");
        rooms.Rooms[0].Name.Should().Be("Wohnzimmer");
        rooms.Groups.Should().HaveCount(1);
        rooms.Groups[0].Id.Should().Be("group-1");
    }

    // --- ReadAdapterConfigAsync ---

    [Fact]
    public async Task ReadAdapterConfigAsync_ValidYaml_ReturnsAdapterConfig()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "adapter.yaml");
        await File.WriteAllTextAsync(filePath,
            """
            adapter_id: hm-eg
            adapter_type: homematic
            settings:
              host: 192.168.1.100
              port: "2001"
            """);

        // Act
        var config = await _sut.ReadAdapterConfigAsync(filePath);

        // Assert
        config.AdapterId.Should().Be("hm-eg");
        config.AdapterType.Should().Be("homematic");
        config.Settings.Should().ContainKey("host");
    }

    // --- ReadDeviceAsync ---

    [Fact]
    public async Task ReadDeviceAsync_ValidYaml_ReturnsDevice()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "device.yaml");
        await File.WriteAllTextAsync(filePath,
            """
            id: dev-001
            adapter_id: hm-eg
            native_id: ABC123
            type: light
            name: Licht Wohnzimmer
            room_id: room-1
            """);

        // Act
        var device = await _sut.ReadDeviceAsync(filePath);

        // Assert
        device.Id.Should().Be("dev-001");
        device.AdapterId.Should().Be("hm-eg");
        device.NativeId.Should().Be("ABC123");
        device.Name.Should().Be("Licht Wohnzimmer");
        device.RoomId.Should().Be("room-1");
    }

    // --- ReadDeviceSummaryAsync ---

    [Fact]
    public async Task ReadDeviceSummaryAsync_ValidYaml_ReturnsSummaryWithFilePath()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "device.yaml");
        await File.WriteAllTextAsync(filePath,
            """
            id: dev-001
            adapter_id: hm-eg
            native_id: ABC123
            type: light
            name: Licht Wohnzimmer
            room_id: room-1
            parameters:
              brightness: 80
            """);

        // Act
        var summary = await _sut.ReadDeviceSummaryAsync(filePath);

        // Assert
        summary.Id.Should().Be("dev-001");
        summary.AdapterId.Should().Be("hm-eg");
        summary.Name.Should().Be("Licht Wohnzimmer");
        summary.FilePath.Should().Be(filePath);
    }

    [Fact]
    public async Task ReadDeviceSummaryAsync_FileNotFound_ThrowsConfigFileException()
    {
        // Act
        var act = () => _sut.ReadDeviceSummaryAsync(Path.Combine(_tempDir, "nonexistent.yaml"));

        // Assert
        await act.Should().ThrowAsync<SmartHalConfigFileException>();
    }

    [Fact]
    public async Task ReadMetaAsync_EmptyFile_ReturnsDefaultMetaConfig()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "");

        // Act
        var meta = await _sut.ReadMetaAsync(_tempDir);

        // Assert
        meta.Should().NotBeNull();
    }

    [Fact]
    public async Task ReadMetaAsync_WhitespaceOnly_ReturnsDefaultMetaConfig()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "   \n\n  ");

        // Act
        var meta = await _sut.ReadMetaAsync(_tempDir);

        // Assert
        meta.Should().NotBeNull();
    }

    [Fact]
    public async Task ReadRoomsAsync_FileNotFound_ThrowsConfigFileException()
    {
        // Act
        var act = () => _sut.ReadRoomsAsync(_tempDir);

        // Assert
        await act.Should().ThrowAsync<SmartHalConfigFileException>();
    }

    [Fact]
    public async Task ReadAdapterConfigAsync_FileNotFound_ThrowsConfigFileException()
    {
        // Act
        var act = () => _sut.ReadAdapterConfigAsync(Path.Combine(_tempDir, "nonexistent.yaml"));

        // Assert
        await act.Should().ThrowAsync<SmartHalConfigFileException>();
    }
}
