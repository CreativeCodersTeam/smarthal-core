using AwesomeAssertions;

namespace SmartHal.Core.Adapters;

public class AdapterMetadataAttributeTests
{
    [Fact]
    public void Constructor_SetsRequiredProperties()
    {
        // Act
        var attr = new AdapterMetadataAttribute("homematic", "HomeMatic CCU");

        // Assert
        attr.AdapterType.Should().Be("homematic");
        attr.DisplayName.Should().Be("HomeMatic CCU");
        attr.Description.Should().BeNull();
    }

    [Fact]
    public void Description_CanBeSet()
    {
        // Act
        var attr = new AdapterMetadataAttribute("homematic", "HomeMatic CCU")
        {
            Description = "HomeMatic CCU adapter"
        };

        // Assert
        attr.Description.Should().Be("HomeMatic CCU adapter");
    }

    [Fact]
    public void AttributeUsage_IsClassLevel_NoMultiple()
    {
        // Act
        var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
            typeof(AdapterMetadataAttribute), typeof(AttributeUsageAttribute))!;

        // Assert
        usage.ValidOn.Should().Be(AttributeTargets.Class);
        usage.AllowMultiple.Should().BeFalse();
    }
}
