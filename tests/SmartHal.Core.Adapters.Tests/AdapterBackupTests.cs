using AwesomeAssertions;

namespace SmartHal.Core.Adapters;

public class AdapterBackupTests
{
    [Fact]
    public void DefaultInitialization_HasEmptyValues()
    {
        // Act
        var backup = new AdapterBackup();

        // Assert
        backup.NativeId.Should().BeEmpty();
        backup.AdapterType.Should().BeEmpty();
        backup.Data.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var backup = new AdapterBackup
        {
            NativeId = "HM-1234",
            AdapterType = "homematic",
            CreatedAt = now,
            Data = { ["config"] = "some-data" }
        };

        // Assert
        backup.NativeId.Should().Be("HM-1234");
        backup.AdapterType.Should().Be("homematic");
        backup.CreatedAt.Should().Be(now);
        backup.Data.Should().HaveCount(1);
    }
}

public class RestorePreviewTests
{
    [Fact]
    public void DefaultInitialization_HasEmptyValues()
    {
        // Act
        var preview = new RestorePreview();

        // Assert
        preview.NativeId.Should().BeEmpty();
        preview.Changes.Should().BeEmpty();
        preview.RequiresDeviceRestart.Should().BeFalse();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Act
        var preview = new RestorePreview
        {
            NativeId = "HM-1234",
            Changes =
            [
                new ParameterChange
                {
                    ParameterName = "LEVEL",
                    CurrentValue = "50",
                    NewValue = "75"
                }
            ],
            RequiresDeviceRestart = true
        };

        // Assert
        preview.Changes.Should().HaveCount(1);
        preview.Changes[0].ParameterName.Should().Be("LEVEL");
        preview.RequiresDeviceRestart.Should().BeTrue();
    }
}
