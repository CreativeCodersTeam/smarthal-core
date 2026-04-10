using AwesomeAssertions;

namespace SmartHal.Core.Adapters;

public class AdapterMetadataAttributeTests
{
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
