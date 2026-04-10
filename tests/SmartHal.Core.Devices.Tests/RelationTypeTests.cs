using AwesomeAssertions;

namespace SmartHal.Core.Devices;

public class RelationTypeTests
{
    [Fact]
    public void StaticConstants_HaveCorrectValues()
    {
        // Act & Assert
        RelationType.DirectLink.Value.Should().Be("direct_link");
        RelationType.GroupMember.Value.Should().Be("group_member");
        RelationType.Scene.Value.Should().Be("scene");
    }

    [Fact]
    public void ImplicitConversion_ToString()
    {
        // Act
        string value = RelationType.DirectLink;

        // Assert
        value.Should().Be("direct_link");
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        // Act
        RelationType type = "custom_relation";

        // Assert
        type.Value.Should().Be("custom_relation");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var a = new RelationType("direct_link");
        var b = RelationType.DirectLink;

        // Act & Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        // Act & Assert
        RelationType.DirectLink.Should().NotBe(RelationType.Scene);
    }

    [Fact]
    public void ToString_Scene_ReturnsSceneString()
    {
        // Act & Assert
        RelationType.Scene.ToString().Should().Be("scene");
    }

    [Fact]
    public void ImplicitConversion_EmptyString_CreatesRelationType()
    {
        // Act
        RelationType type = "";

        // Assert
        type.Value.Should().Be("");
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHash()
    {
        // Arrange
        var a = new RelationType("direct_link");
        var b = new RelationType("direct_link");

        // Act & Assert
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
