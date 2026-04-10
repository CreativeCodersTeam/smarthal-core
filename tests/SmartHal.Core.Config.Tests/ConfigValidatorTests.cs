using AwesomeAssertions;
using FakeItEasy;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

public class ConfigValidatorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly YamlConfigReader _reader = new YamlConfigReader();
    private readonly YamlConfigWriter _writer = new YamlConfigWriter();

    public ConfigValidatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        Directory.CreateDirectory(Path.Combine(_tempDir, "adapters"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "devices"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    // --- Structure Validation ---

    [Fact]
    public async Task ValidateStructureAsync_MissingMeta_ReturnsError()
    {
        // Arrange
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MISSING_META");
    }

    [Fact]
    public async Task ValidateStructureAsync_ValidConfig_ReturnsValid()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "schema_version: \"1.0\"");
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateStructureAsync_UnsupportedSchemaVersion_ReturnsError()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "schema_version: \"2.0\"");
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "UNSUPPORTED_SCHEMA");
    }

    [Fact]
    public async Task ValidateStructureAsync_InvalidMetaYaml_ReturnsError()
    {
        // Arrange — use a YAML tab indentation error that YamlDotNet rejects
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "a:\n\t- invalid\n\t\ttabs");
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "INVALID_YAML");
    }

    [Fact]
    public async Task ValidateStructureAsync_InvalidAdapterYaml_ReturnsError()
    {
        // Arrange — use a YAML tab indentation error that YamlDotNet rejects
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "schema_version: \"1.0\"");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "adapters", "bad.yaml"), "a:\n\t- invalid\n\t\ttabs");
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "INVALID_YAML");
    }

    [Fact]
    public async Task ValidateStructureAsync_DeviceMissingId_ReturnsError()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "schema_version: \"1.0\"");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev.yaml"),
            """
            adapter_id: hm-eg
            native_id: ABC123
            name: Test
            """);
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MISSING_FIELD" && e.Message.Contains("id"));
    }

    [Fact]
    public async Task ValidateStructureAsync_DeviceMissingName_ReturnsError()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "schema_version: \"1.0\"");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "dev.yaml"),
            """
            id: dev-001
            adapter_id: hm-eg
            native_id: ABC123
            """);
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MISSING_FIELD" && e.Message.Contains("name"));
    }

    // --- Semantic Validation ---

    [Fact]
    public async Task ValidateSemanticAsync_InvalidAdapterRef_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig>());
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig());
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>
            {
                new DeviceSummary { Id = "dev-001", AdapterId = "nonexistent", NativeId = "N1", Name = "Test" }
            });
        A.CallTo(() => repo.GetDeviceAsync("dev-001", A<CancellationToken>._))
            .Returns(new Device { Id = "dev-001", AdapterId = "nonexistent", NativeId = "N1", Name = "Test" });

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "INVALID_ADAPTER_REF");
    }

    [Fact]
    public async Task ValidateSemanticAsync_DuplicateDeviceId_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig> { new AdapterConfig { AdapterId = "hm-eg", AdapterType = "homematic" } });
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig());
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>
            {
                new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test1" },
                new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N2", Name = "Test2" }
            });
        A.CallTo(() => repo.GetDeviceAsync("dev-001", A<CancellationToken>._))
            .Returns(new Device { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test1" });

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.Errors.Should().Contain(e => e.Code == "DUPLICATE_DEVICE_ID");
    }

    [Fact]
    public async Task ValidateSemanticAsync_InvalidRoomRef_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig> { new AdapterConfig { AdapterId = "hm-eg" } });
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig());
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>
            {
                new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test", RoomId = "nonexistent-room" }
            });
        A.CallTo(() => repo.GetDeviceAsync("dev-001", A<CancellationToken>._))
            .Returns(new Device { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test", RoomId = "nonexistent-room" });

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.Errors.Should().Contain(e => e.Code == "INVALID_ROOM_REF");
    }

    [Fact]
    public async Task ValidateSemanticAsync_DuplicateNativeIdWithinAdapter_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig> { new AdapterConfig { AdapterId = "hm-eg" } });
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig());
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>
            {
                new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", NativeId = "SAME", Name = "Test1" },
                new DeviceSummary { Id = "dev-002", AdapterId = "hm-eg", NativeId = "SAME", Name = "Test2" }
            });
        A.CallTo(() => repo.GetDeviceAsync("dev-001", A<CancellationToken>._))
            .Returns(new Device { Id = "dev-001", AdapterId = "hm-eg", NativeId = "SAME", Name = "Test1" });
        A.CallTo(() => repo.GetDeviceAsync("dev-002", A<CancellationToken>._))
            .Returns(new Device { Id = "dev-002", AdapterId = "hm-eg", NativeId = "SAME", Name = "Test2" });

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.Errors.Should().Contain(e => e.Code == "DUPLICATE_NATIVE_ID");
    }

    [Fact]
    public async Task ValidateSemanticAsync_InvalidRelationTarget_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig> { new AdapterConfig { AdapterId = "hm-eg" } });
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig());
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>
            {
                new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test" }
            });
        A.CallTo(() => repo.GetDeviceAsync("dev-001", A<CancellationToken>._))
            .Returns(new Device
            {
                Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test",
                Relations = [new Relation { Id = "rel-1", Type = "controls", TargetId = "nonexistent-dev" }]
            });

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.Errors.Should().Contain(e => e.Code == "INVALID_RELATION_TARGET");
    }

    [Fact]
    public async Task ValidateSemanticAsync_InvalidGroupRef_ReturnsError()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig> { new AdapterConfig { AdapterId = "hm-eg" } });
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig());
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>
            {
                new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test" }
            });
        A.CallTo(() => repo.GetDeviceAsync("dev-001", A<CancellationToken>._))
            .Returns(new Device
            {
                Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test",
                GroupIds = ["nonexistent-group"]
            });

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.Errors.Should().Contain(e => e.Code == "INVALID_GROUP_REF");
    }

    [Fact]
    public async Task ValidateSemanticAsync_EmptyDeviceList_ReturnsValid()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig>());
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig());
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>());

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateStructureAsync_EmptyAdaptersDir_ReturnsValid()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "schema_version: \"1.0\"");
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateStructureAsync_DeviceMissingMultipleFields_ReturnsMultipleErrors()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meta.yaml"), "schema_version: \"1.0\"");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "devices", "empty.yaml"), "type: light");
        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateStructureAsync(_tempDir);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("id"));
        result.Errors.Should().Contain(e => e.Message.Contains("adapter_id"));
        result.Errors.Should().Contain(e => e.Message.Contains("native_id"));
        result.Errors.Should().Contain(e => e.Message.Contains("name"));
    }

    [Fact]
    public async Task ValidateSemanticAsync_ValidConfig_ReturnsNoErrors()
    {
        // Arrange
        var repo = A.Fake<IConfigRepository>();
        A.CallTo(() => repo.GetAllAdapterConfigsAsync(A<CancellationToken>._))
            .Returns(new List<AdapterConfig> { new AdapterConfig { AdapterId = "hm-eg" } });
        A.CallTo(() => repo.GetRoomsAsync(A<CancellationToken>._))
            .Returns(new RoomsConfig
            {
                Rooms = [new Room { Id = "room-1", Name = "Wohnzimmer" }],
                Groups = [new Group { Id = "group-1", Name = "Lichter" }]
            });
        A.CallTo(() => repo.ListDevicesAsync(A<CancellationToken>._))
            .Returns(new List<DeviceSummary>
            {
                new DeviceSummary { Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test", RoomId = "room-1" }
            });
        A.CallTo(() => repo.GetDeviceAsync("dev-001", A<CancellationToken>._))
            .Returns(new Device
            {
                Id = "dev-001", AdapterId = "hm-eg", NativeId = "N1", Name = "Test",
                RoomId = "room-1", GroupIds = ["group-1"]
            });

        var validator = new ConfigValidator(_reader);

        // Act
        var result = await validator.ValidateSemanticAsync(repo);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
