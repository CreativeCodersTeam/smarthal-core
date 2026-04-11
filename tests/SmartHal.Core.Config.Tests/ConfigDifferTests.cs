using Microsoft.Extensions.Logging.Abstractions;
using AwesomeAssertions;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

public class ConfigDifferTests
{
    private readonly ConfigDiffer _sut = new ConfigDiffer(NullLogger<ConfigDiffer>.Instance);

    // --- Identical Devices ---

    [Fact]
    public void ComputeDiff_IdenticalDevices_ReturnsNoDiff()
    {
        // Arrange
        var device = CreateDevice();

        // Act
        var diff = _sut.ComputeDiff(device, device);

        // Assert
        diff.HasChanges.Should().BeFalse();
        diff.Changes.Should().BeEmpty();
    }

    // --- Simple Property Changes ---

    [Fact]
    public void ComputeDiff_NameChanged_DetectsChange()
    {
        // Arrange
        var baseline = CreateDevice(name: "Old Name");
        var current = CreateDevice(name: "New Name");

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.HasChanges.Should().BeTrue();
        diff.Changes.Should().ContainSingle(c => c.Path == "name" && c.Kind == DiffKind.Changed);
        diff.Changes.First(c => c.Path == "name").OldValue.Should().Be("Old Name");
        diff.Changes.First(c => c.Path == "name").NewValue.Should().Be("New Name");
    }

    [Fact]
    public void ComputeDiff_TypeChanged_DetectsChange()
    {
        // Arrange
        var baseline = CreateDevice(type: "light");
        var current = CreateDevice(type: "switch");

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c => c.Path == "type" && c.Kind == DiffKind.Changed);
    }

    [Fact]
    public void ComputeDiff_RoomIdChanged_DetectsChange()
    {
        // Arrange
        var baseline = CreateDevice(roomId: "room-1");
        var current = CreateDevice(roomId: "room-2");

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c => c.Path == "room_id" && c.Kind == DiffKind.Changed);
    }

    // --- Parameter Changes ---

    [Fact]
    public void ComputeDiff_ParameterAdded_DetectsAddition()
    {
        // Arrange
        var baseline = CreateDevice();
        var current = CreateDevice();
        current.Parameters["brightness"] = ParameterValue.FromNumber(80);

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "parameters.brightness" && c.Kind == DiffKind.Added);
    }

    [Fact]
    public void ComputeDiff_ParameterRemoved_DetectsRemoval()
    {
        // Arrange
        var baseline = CreateDevice();
        baseline.Parameters["brightness"] = ParameterValue.FromNumber(80);
        var current = CreateDevice();

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "parameters.brightness" && c.Kind == DiffKind.Removed);
    }

    [Fact]
    public void ComputeDiff_ParameterChanged_DetectsChange()
    {
        // Arrange
        var baseline = CreateDevice();
        baseline.Parameters["brightness"] = ParameterValue.FromNumber(50);
        var current = CreateDevice();
        current.Parameters["brightness"] = ParameterValue.FromNumber(80);

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "parameters.brightness" && c.Kind == DiffKind.Changed);
    }

    // --- Group Changes ---

    [Fact]
    public void ComputeDiff_GroupAdded_DetectsAddition()
    {
        // Arrange
        var baseline = CreateDevice();
        var current = CreateDevice();
        current.GroupIds.Add("group-lights");

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "group_ids.group-lights" && c.Kind == DiffKind.Added);
    }

    [Fact]
    public void ComputeDiff_GroupRemoved_DetectsRemoval()
    {
        // Arrange
        var baseline = CreateDevice();
        baseline.GroupIds.Add("group-lights");
        var current = CreateDevice();

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "group_ids.group-lights" && c.Kind == DiffKind.Removed);
    }

    // --- Channel Changes ---

    [Fact]
    public void ComputeDiff_ChannelAdded_DetectsAddition()
    {
        // Arrange
        var baseline = CreateDevice();
        var current = CreateDevice();
        current.Channels.Add(new Channel { Number = 1, Name = "Switch" });

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "channels[1]" && c.Kind == DiffKind.Added);
    }

    [Fact]
    public void ComputeDiff_ChannelRemoved_DetectsRemoval()
    {
        // Arrange
        var baseline = CreateDevice();
        baseline.Channels.Add(new Channel { Number = 1, Name = "Switch" });
        var current = CreateDevice();

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "channels[1]" && c.Kind == DiffKind.Removed);
    }

    [Fact]
    public void ComputeDiff_ChannelParameterChanged_DetectsChange()
    {
        // Arrange
        var baseline = CreateDevice();
        baseline.Channels.Add(new Channel
        {
            Number = 1,
            Name = "Switch",
            Parameters = new Dictionary<string, ParameterValue> { ["mode"] = ParameterValue.FromString("auto") }
        });

        var current = CreateDevice();
        current.Channels.Add(new Channel
        {
            Number = 1,
            Name = "Switch",
            Parameters = new Dictionary<string, ParameterValue> { ["mode"] = ParameterValue.FromString("manual") }
        });

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "channels[1].parameters.mode" && c.Kind == DiffKind.Changed);
    }

    // --- Relation Changes ---

    [Fact]
    public void ComputeDiff_RelationAdded_DetectsAddition()
    {
        // Arrange
        var baseline = CreateDevice();
        var current = CreateDevice();
        current.Relations.Add(new Relation { Id = "rel-1", Type = "controls", TargetId = "dev-2" });

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "relations.rel-1" && c.Kind == DiffKind.Added);
    }

    [Fact]
    public void ComputeDiff_RelationRemoved_DetectsRemoval()
    {
        // Arrange
        var baseline = CreateDevice();
        baseline.Relations.Add(new Relation { Id = "rel-1", Type = "controls", TargetId = "dev-2" });
        var current = CreateDevice();

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c =>
            c.Path == "relations.rel-1" && c.Kind == DiffKind.Removed);
    }

    // --- RoomId null transitions ---

    [Fact]
    public void ComputeDiff_RoomIdNullToValue_DetectsChange()
    {
        // Arrange
        var baseline = CreateDevice(roomId: null);
        var current = CreateDevice(roomId: "room-1");

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c => c.Path == "room_id" && c.Kind == DiffKind.Changed);
    }

    [Fact]
    public void ComputeDiff_RoomIdValueToNull_DetectsChange()
    {
        // Arrange
        var baseline = CreateDevice(roomId: "room-1");
        var current = CreateDevice(roomId: null);

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().ContainSingle(c => c.Path == "room_id" && c.Kind == DiffKind.Changed);
    }

    [Fact]
    public void ComputeDiff_MultipleParameterChanges_AllDetected()
    {
        // Arrange
        var baseline = CreateDevice();
        baseline.Parameters["brightness"] = ParameterValue.FromNumber(50);
        baseline.Parameters["color"] = ParameterValue.FromString("red");

        var current = CreateDevice();
        current.Parameters["brightness"] = ParameterValue.FromNumber(80);
        current.Parameters["color"] = ParameterValue.FromString("blue");

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.Changes.Should().HaveCount(2);
        diff.Changes.Should().Contain(c => c.Path == "parameters.brightness");
        diff.Changes.Should().Contain(c => c.Path == "parameters.color");
    }

    // --- DeviceId ---

    [Fact]
    public void ComputeDiff_SetsDeviceIdFromCurrent()
    {
        // Arrange
        var baseline = CreateDevice(id: "dev-old");
        var current = CreateDevice(id: "dev-current");

        // Act
        var diff = _sut.ComputeDiff(baseline, current);

        // Assert
        diff.DeviceId.Should().Be("dev-current");
    }

    private static Device CreateDevice(
        string id = "dev-001",
        string name = "Test Device",
        string type = "light",
        string? roomId = null)
    {
        return new Device
        {
            Id = id,
            AdapterId = "hm-001",
            NativeId = "ABC123",
            Type = type,
            Name = name,
            RoomId = roomId
        };
    }
}
