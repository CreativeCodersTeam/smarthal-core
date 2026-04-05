using AwesomeAssertions;
using FakeItEasy;
using SmartHal.Core;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

public class ConfigApplierTests
{
    private readonly ConfigApplier _sut = new();

    [Fact]
    public async Task ApplyDiffAsync_AdapterNotWriter_ThrowsOperationException()
    {
        // Arrange
        var adapter = A.Fake<ISmartHalAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");
        var diff = new DeviceDiff { DeviceId = "dev-001" };

        // Act
        var act = () => _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        await act.Should().ThrowAsync<SmartHalAdapterOperationException>()
            .WithMessage("*does not support writing*");
    }

    [Fact]
    public async Task ApplyDiffAsync_ChangedParameter_WritesToAdapter()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");

        var diff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes =
            [
                new DiffEntry
                {
                    Kind = DiffKind.Changed,
                    Path = "parameters.brightness",
                    OldValue = "50",
                    NewValue = "80"
                }
            ]
        };

        // Act
        await _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            "native-1", "brightness", A<ParameterValue>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ApplyDiffAsync_AddedParameter_WritesToAdapter()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");

        var diff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes =
            [
                new DiffEntry
                {
                    Kind = DiffKind.Added,
                    Path = "parameters.color",
                    NewValue = "red"
                }
            ]
        };

        // Act
        await _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            "native-1", "color", A<ParameterValue>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ApplyDiffAsync_RemovedParameter_SkipsWrite()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");

        var diff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes =
            [
                new DiffEntry
                {
                    Kind = DiffKind.Removed,
                    Path = "parameters.old_param",
                    OldValue = "value"
                }
            ]
        };

        // Act
        await _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            A<string>._, A<string>._, A<ParameterValue>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ApplyDiffAsync_NonParameterChange_SkipsWrite()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");

        var diff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes =
            [
                new DiffEntry
                {
                    Kind = DiffKind.Changed,
                    Path = "name",
                    OldValue = "Old",
                    NewValue = "New"
                }
            ]
        };

        // Act
        await _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            A<string>._, A<string>._, A<ParameterValue>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ApplyDiffAsync_WriteFails_CollectsErrorsAndThrows()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            A<string>._, A<string>._, A<ParameterValue>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("write failed"));

        var diff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes =
            [
                new DiffEntry { Kind = DiffKind.Changed, Path = "parameters.p1", NewValue = "v1" },
                new DiffEntry { Kind = DiffKind.Changed, Path = "parameters.p2", NewValue = "v2" }
            ]
        };

        // Act
        var act = () => _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        var ex = await act.Should().ThrowAsync<SmartHalAdapterOperationException>();
        ex.Which.Message.Should().Contain("2 parameter(s)");
    }

    [Fact]
    public async Task ApplyDiffAsync_EmptyDiff_DoesNothing()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");
        var diff = new DeviceDiff { DeviceId = "dev-001" };

        // Act
        await _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            A<string>._, A<string>._, A<ParameterValue>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ApplyDiffAsync_OperationCanceledException_Propagates()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            A<string>._, A<string>._, A<ParameterValue>._, A<CancellationToken>._))
            .Throws(new OperationCanceledException());

        var diff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes = [new DiffEntry { Kind = DiffKind.Changed, Path = "parameters.p1", NewValue = "v1" }]
        };

        // Act
        var act = () => _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ApplyDiffAsync_NullNewValue_UsesEmptyString()
    {
        // Arrange
        var adapter = A.Fake<IWritableAdapter>();
        A.CallTo(() => adapter.AdapterId).Returns("test-adapter");

        var diff = new DeviceDiff
        {
            DeviceId = "dev-001",
            Changes = [new DiffEntry { Kind = DiffKind.Added, Path = "parameters.p1", NewValue = null }]
        };

        // Act
        await _sut.ApplyDiffAsync(diff, adapter, "native-1");

        // Assert
        A.CallTo(() => adapter.WriteDeviceParameterAsync(
            "native-1", "p1", A<ParameterValue>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    /// <summary>Combined interface for FakeItEasy to create a fake that is both adapter and writer.</summary>
    public interface IWritableAdapter : ISmartHalAdapter, IDeviceWriter;
}
