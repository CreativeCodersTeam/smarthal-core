using AwesomeAssertions;

namespace SmartHal.Core.Devices;

public class ParameterValueTests
{
    [Fact]
    public void FromString_SetsKindAndValue()
    {
        var pv = ParameterValue.FromString("hello");

        pv.Kind.Should().Be(ParameterKind.String);
        pv.StringValue.Should().Be("hello");
        pv.NumberValue.Should().BeNull();
        pv.BoolValue.Should().BeNull();
    }

    [Fact]
    public void FromNumber_SetsKindAndValue()
    {
        var pv = ParameterValue.FromNumber(42.5);

        pv.Kind.Should().Be(ParameterKind.Number);
        pv.NumberValue.Should().Be(42.5);
        pv.StringValue.Should().BeNull();
        pv.BoolValue.Should().BeNull();
    }

    [Fact]
    public void FromBool_SetsKindAndValue()
    {
        var pv = ParameterValue.FromBool(true);

        pv.Kind.Should().Be(ParameterKind.Boolean);
        pv.BoolValue.Should().BeTrue();
        pv.StringValue.Should().BeNull();
        pv.NumberValue.Should().BeNull();
    }

    [Fact]
    public void FromEnum_SetsKindAndValue()
    {
        var pv = ParameterValue.FromEnum("ON");

        pv.Kind.Should().Be(ParameterKind.Enum);
        pv.StringValue.Should().Be("ON");
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        ParameterValue pv = "test";

        pv.Kind.Should().Be(ParameterKind.String);
        pv.StringValue.Should().Be("test");
    }

    [Fact]
    public void ImplicitConversion_FromDouble()
    {
        ParameterValue pv = 3.14;

        pv.Kind.Should().Be(ParameterKind.Number);
        pv.NumberValue.Should().Be(3.14);
    }

    [Fact]
    public void ImplicitConversion_FromInt()
    {
        ParameterValue pv = 42;

        pv.Kind.Should().Be(ParameterKind.Number);
        pv.NumberValue.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromBool()
    {
        ParameterValue pv = true;

        pv.Kind.Should().Be(ParameterKind.Boolean);
        pv.BoolValue.Should().BeTrue();
    }

    [Fact]
    public void WithKind_StringToEnum_Converts()
    {
        var pv = ParameterValue.FromString("ON");

        var result = pv.WithKind(ParameterKind.Enum);

        result.Kind.Should().Be(ParameterKind.Enum);
        result.StringValue.Should().Be("ON");
    }

    [Fact]
    public void WithKind_NonStringToEnum_ReturnsUnchanged()
    {
        var pv = ParameterValue.FromNumber(42);

        var result = pv.WithKind(ParameterKind.Enum);

        result.Kind.Should().Be(ParameterKind.Number);
        result.NumberValue.Should().Be(42);
    }

    [Fact]
    public void WithKind_StringToString_ReturnsUnchanged()
    {
        var pv = ParameterValue.FromString("hello");

        var result = pv.WithKind(ParameterKind.String);

        result.Should().Be(pv);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = ParameterValue.FromString("hello");
        var b = ParameterValue.FromString("hello");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = ParameterValue.FromString("hello");
        var b = ParameterValue.FromString("world");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentKinds_AreNotEqual()
    {
        var a = ParameterValue.FromString("ON");
        var b = ParameterValue.FromEnum("ON");

        a.Should().NotBe(b);
    }
}
