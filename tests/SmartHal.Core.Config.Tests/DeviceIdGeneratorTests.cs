using AwesomeAssertions;

namespace SmartHal.Core.Config;

public class DeviceIdGeneratorTests : IDisposable
{
    private readonly string _tempDir;

    public DeviceIdGeneratorTests()
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

    // --- GetPrefix ---

    [Theory]
    [InlineData("homematic", "hm")]
    [InlineData("zigbee", "zb")]
    [InlineData("knx", "knx")]
    [InlineData("mqtt", "mqtt")]
    [InlineData("evcc", "evcc")]
    public void GetPrefix_KnownAdapterType_ReturnsExpectedPrefix(string adapterType, string expected)
    {
        // Act
        var result = DeviceIdGenerator.GetPrefix(adapterType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetPrefix_KnownAdapterType_IsCaseInsensitive()
    {
        // Act
        var result = DeviceIdGenerator.GetPrefix("Homematic");

        // Assert
        result.Should().Be("hm");
    }

    [Theory]
    [InlineData("shelly", "shel")]
    [InlineData("custom", "cust")]
    [InlineData("ab", "ab")]
    [InlineData("abc", "abc")]
    public void GetPrefix_UnknownAdapterType_ReturnsTruncatedLowercase(string adapterType, string expected)
    {
        // Act
        var result = DeviceIdGenerator.GetPrefix(adapterType);

        // Assert
        result.Should().Be(expected);
    }

    // --- GenerateSlug ---

    [Fact]
    public void GenerateSlug_SimpleName_ReturnsLowercaseSlug()
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug("Licht Wohnzimmer");

        // Assert
        result.Should().Be("licht-wohnzimmer");
    }

    [Theory]
    [InlineData("Küche", "kueche")]
    [InlineData("Wohnzimmer Tür", "wohnzimmer-tuer")]
    [InlineData("Straßenlampe", "strassenlampe")]
    [InlineData("Büro Öffner", "buero-oeffner")]
    public void GenerateSlug_WithUmlauts_ReplacesCorrectly(string input, string expected)
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateSlug_SpecialCharacters_ReplacedWithHyphens()
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug("Licht #1 (Wohnzimmer)");

        // Assert
        result.Should().Be("licht-1-wohnzimmer");
    }

    [Fact]
    public void GenerateSlug_MultipleConsecutiveHyphens_Collapsed()
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug("Licht---Wohnzimmer");

        // Assert
        result.Should().Be("licht-wohnzimmer");
    }

    [Fact]
    public void GenerateSlug_LeadingAndTrailingSpecialChars_Trimmed()
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug("---Licht---");

        // Assert
        result.Should().Be("licht");
    }

    [Fact]
    public void GenerateSlug_VeryLongName_TruncatedTo30Chars()
    {
        // Arrange
        var longName = "abcdefghijklmnopqrstuvwxyz-abcdefghij";

        // Act
        var result = DeviceIdGenerator.GenerateSlug(longName);

        // Assert
        result.Length.Should().BeLessThanOrEqualTo(30);
    }

    [Fact]
    public void GenerateSlug_TruncationRemovesTrailingHyphen()
    {
        // Arrange — 31 chars with hyphen at position 30
        var name = "abcdefghijklmnopqrstuvwxyz-abc-x";

        // Act
        var result = DeviceIdGenerator.GenerateSlug(name);

        // Assert
        result.Should().NotEndWith("-");
        result.Length.Should().BeLessThanOrEqualTo(30);
    }

    // --- GenerateDeviceIdAsync ---

    [Fact]
    public async Task GenerateDeviceIdAsync_SimpleCase_ReturnsCorrectFormat()
    {
        // Arrange
        var generator = new DeviceIdGenerator(_tempDir);

        // Act
        var id = await generator.GenerateDeviceIdAsync("homematic", "Licht Wohnzimmer");

        // Assert
        id.Should().Be("smhal-hm-licht-wohnzimmer");
    }

    [Fact]
    public async Task GenerateDeviceIdAsync_Collision_AppendsSuffix()
    {
        // Arrange
        var devicesDir = Path.Combine(_tempDir, "devices");
        Directory.CreateDirectory(devicesDir);
        await File.WriteAllTextAsync(Path.Combine(devicesDir, "smhal-hm-licht-wohnzimmer.yaml"), "id: smhal-hm-licht-wohnzimmer");

        var generator = new DeviceIdGenerator(_tempDir);

        // Act
        var id = await generator.GenerateDeviceIdAsync("homematic", "Licht Wohnzimmer");

        // Assert
        id.Should().Be("smhal-hm-licht-wohnzimmer-2");
    }

    [Fact]
    public async Task GenerateDeviceIdAsync_MultipleCollisions_IncrementsCounter()
    {
        // Arrange
        var devicesDir = Path.Combine(_tempDir, "devices");
        Directory.CreateDirectory(devicesDir);
        await File.WriteAllTextAsync(Path.Combine(devicesDir, "smhal-hm-licht-wohnzimmer.yaml"), "");
        await File.WriteAllTextAsync(Path.Combine(devicesDir, "smhal-hm-licht-wohnzimmer-2.yaml"), "");
        await File.WriteAllTextAsync(Path.Combine(devicesDir, "smhal-hm-licht-wohnzimmer-3.yaml"), "");

        var generator = new DeviceIdGenerator(_tempDir);

        // Act
        var id = await generator.GenerateDeviceIdAsync("homematic", "Licht Wohnzimmer");

        // Assert
        id.Should().Be("smhal-hm-licht-wohnzimmer-4");
    }

    [Fact]
    public async Task GenerateDeviceIdAsync_NoDevicesDir_ReturnsBaseId()
    {
        // Arrange — no devices/ directory exists
        var generator = new DeviceIdGenerator(_tempDir);

        // Act
        var id = await generator.GenerateDeviceIdAsync("zigbee", "Sensor");

        // Assert
        id.Should().Be("smhal-zb-sensor");
    }

    [Fact]
    public void GenerateSlug_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateSlug_OnlySpecialCharacters_ReturnsEmpty()
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug("---!!!###");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzabc", 29)]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcd", 30)]
    public void GenerateSlug_BoundaryLengths_HandledCorrectly(string input, int expectedLength)
    {
        // Act
        var result = DeviceIdGenerator.GenerateSlug(input);

        // Assert
        result.Length.Should().Be(expectedLength);
    }

    [Fact]
    public async Task GenerateDeviceIdAsync_WithUmlauts_ProducesCleanId()
    {
        // Arrange
        var generator = new DeviceIdGenerator(_tempDir);

        // Act
        var id = await generator.GenerateDeviceIdAsync("homematic", "Rolladen Küche");

        // Assert
        id.Should().Be("smhal-hm-rolladen-kueche");
    }
}
