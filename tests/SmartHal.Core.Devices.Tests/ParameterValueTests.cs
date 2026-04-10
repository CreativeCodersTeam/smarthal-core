using AwesomeAssertions;

namespace SmartHal.Core.Devices;

public class ParameterValueTests
{
    [Fact]
    public void FromString_SetsKindAndValue()
    {
        // Act
        var pv = ParameterValue.FromString("hello");

        // Assert
        pv.Kind.Should().Be(ParameterKind.String);
        pv.StringValue.Should().Be("hello");
        pv.NumberValue.Should().BeNull();
        pv.BoolValue.Should().BeNull();
    }

    [Fact]
    public void FromNumber_SetsKindAndValue()
    {
        // Act
        var pv = ParameterValue.FromNumber(42.5);

        // Assert
        pv.Kind.Should().Be(ParameterKind.Number);
        pv.NumberValue.Should().Be(42.5);
        pv.StringValue.Should().BeNull();
        pv.BoolValue.Should().BeNull();
    }

    [Fact]
    public void FromBool_SetsKindAndValue()
    {
        // Act
        var pv = ParameterValue.FromBool(true);

        // Assert
        pv.Kind.Should().Be(ParameterKind.Boolean);
        pv.BoolValue.Should().BeTrue();
        pv.StringValue.Should().BeNull();
        pv.NumberValue.Should().BeNull();
    }

    [Fact]
    public void FromEnum_SetsKindAndValue()
    {
        // Act
        var pv = ParameterValue.FromEnum("ON");

        // Assert
        pv.Kind.Should().Be(ParameterKind.Enum);
        pv.StringValue.Should().Be("ON");
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        // Act
        ParameterValue pv = "test";

        // Assert
        pv.Kind.Should().Be(ParameterKind.String);
        pv.StringValue.Should().Be("test");
    }

    [Fact]
    public void ImplicitConversion_FromDouble()
    {
        // Act
        ParameterValue pv = 3.14;

        // Assert
        pv.Kind.Should().Be(ParameterKind.Number);
        pv.NumberValue.Should().Be(3.14);
    }

    [Fact]
    public void ImplicitConversion_FromInt()
    {
        // Act
        ParameterValue pv = 42;

        // Assert
        pv.Kind.Should().Be(ParameterKind.Number);
        pv.NumberValue.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromBool()
    {
        // Act
        ParameterValue pv = true;

        // Assert
        pv.Kind.Should().Be(ParameterKind.Boolean);
        pv.BoolValue.Should().BeTrue();
    }

    [Fact]
    public void WithKind_StringToEnum_Converts()
    {
        // Arrange
        var pv = ParameterValue.FromString("ON");

        // Act
        var result = pv.WithKind(ParameterKind.Enum);

        // Assert
        result.Kind.Should().Be(ParameterKind.Enum);
        result.StringValue.Should().Be("ON");
    }

    [Fact]
    public void WithKind_NonStringToEnum_ReturnsUnchanged()
    {
        // Arrange
        var pv = ParameterValue.FromNumber(42);

        // Act
        var result = pv.WithKind(ParameterKind.Enum);

        // Assert
        result.Kind.Should().Be(ParameterKind.Number);
        result.NumberValue.Should().Be(42);
    }

    [Fact]
    public void WithKind_StringToString_ReturnsUnchanged()
    {
        // Arrange
        var pv = ParameterValue.FromString("hello");

        // Act
        var result = pv.WithKind(ParameterKind.String);

        // Assert
        result.Should().Be(pv);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var a = ParameterValue.FromString("hello");
        var b = ParameterValue.FromString("hello");

        // Act & Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var a = ParameterValue.FromString("hello");
        var b = ParameterValue.FromString("world");

        // Act & Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentKinds_AreNotEqual()
    {
        // Arrange
        var a = ParameterValue.FromString("ON");
        var b = ParameterValue.FromEnum("ON");

        // Act & Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void FromNumber_NaN_StoresNaN()
    {
        // Act
        var pv = ParameterValue.FromNumber(double.NaN);

        // Assert
        pv.NumberValue.Should().Be(double.NaN);
        pv.Kind.Should().Be(ParameterKind.Number);
    }

    [Fact]
    public void FromString_Null_StoresNull()
    {
        // Act
        var pv = ParameterValue.FromString(null!);

        // Assert
        pv.StringValue.Should().BeNull();
        pv.Kind.Should().Be(ParameterKind.String);
    }

    [Fact]
    public void WithKind_EnumToString_ReturnsUnchanged()
    {
        // Arrange
        var pv = ParameterValue.FromEnum("ON");

        // Act
        var result = pv.WithKind(ParameterKind.String);

        // Assert
        result.Kind.Should().Be(ParameterKind.Enum);
    }

    [Fact]
    public void WithKind_BoolToNumber_ReturnsUnchanged()
    {
        // Arrange
        var pv = ParameterValue.FromBool(true);

        // Act
        var result = pv.WithKind(ParameterKind.Number);

        // Assert
        result.Kind.Should().Be(ParameterKind.Boolean);
    }
}
