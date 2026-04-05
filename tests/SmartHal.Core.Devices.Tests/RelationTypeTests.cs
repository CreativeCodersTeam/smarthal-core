using AwesomeAssertions;

namespace SmartHal.Core.Devices;

public class RelationTypeTests
{
    [Fact]
    public void StaticConstants_HaveCorrectValues()
    {
        RelationType.DirectLink.Value.Should().Be("direct_link");
        RelationType.GroupMember.Value.Should().Be("group_member");
        RelationType.Scene.Value.Should().Be("scene");
    }

    [Fact]
    public void ImplicitConversion_ToString()
    {
        string value = RelationType.DirectLink;

        value.Should().Be("direct_link");
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        RelationType type = "custom_relation";

        type.Value.Should().Be("custom_relation");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = new RelationType("direct_link");
        var b = RelationType.DirectLink;

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        RelationType.DirectLink.Should().NotBe(RelationType.Scene);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        RelationType.Scene.ToString().Should().Be("scene");
    }
}
