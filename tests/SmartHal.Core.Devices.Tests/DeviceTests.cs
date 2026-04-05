using AwesomeAssertions;

namespace SmartHal.Core.Devices;

public class DeviceTests
{
    [Fact]
    public void DefaultInitialization_HasEmptyStringsAndLists()
    {
        var device = new Device();

        device.Id.Should().BeEmpty();
        device.AdapterId.Should().BeEmpty();
        device.NativeId.Should().BeEmpty();
        device.Name.Should().BeEmpty();
        device.RoomId.Should().BeNull();
        device.GroupIds.Should().BeEmpty();
        device.Parameters.Should().BeEmpty();
        device.Channels.Should().BeEmpty();
        device.Relations.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var device = new Device
        {
            Id = "dev-001",
            AdapterId = "hm-001",
            NativeId = "HM-1234",
            Type = DeviceType.SwitchActuator,
            Name = "Living Room Light",
            RoomId = "room-01"
        };

        device.Id.Should().Be("dev-001");
        device.AdapterId.Should().Be("hm-001");
        device.NativeId.Should().Be("HM-1234");
        device.Type.Should().Be(DeviceType.SwitchActuator);
        device.Name.Should().Be("Living Room Light");
        device.RoomId.Should().Be("room-01");
    }

    [Fact]
    public void Channels_CanBeAdded()
    {
        var device = new Device();
        var channel = new Channel { Number = 1, Name = "Switch" };

        device.Channels.Add(channel);

        device.Channels.Should().HaveCount(1);
        device.Channels[0].Name.Should().Be("Switch");
    }

    [Fact]
    public void Relations_CanBeAdded()
    {
        var device = new Device();
        var relation = new Relation
        {
            Id = "rel-001",
            Type = RelationType.DirectLink,
            TargetId = "dev-002",
            Symmetric = true
        };

        device.Relations.Add(relation);

        device.Relations.Should().HaveCount(1);
        device.Relations[0].Type.Should().Be(RelationType.DirectLink);
        device.Relations[0].Symmetric.Should().BeTrue();
    }

    [Fact]
    public void Parameters_CanBeSet()
    {
        var device = new Device();

        device.Parameters["level"] = ParameterValue.FromNumber(75);
        device.Parameters["state"] = ParameterValue.FromBool(true);

        device.Parameters.Should().HaveCount(2);
        device.Parameters["level"].NumberValue.Should().Be(75);
        device.Parameters["state"].BoolValue.Should().BeTrue();
    }

    [Fact]
    public void GroupIds_CanBeAdded()
    {
        var device = new Device();

        device.GroupIds.Add("grp-001");
        device.GroupIds.Add("grp-002");

        device.GroupIds.Should().HaveCount(2);
    }
}
