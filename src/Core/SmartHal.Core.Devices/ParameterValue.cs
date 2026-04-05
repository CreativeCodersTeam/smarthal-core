namespace SmartHal.Core.Devices;

/// <summary>
/// Represents a typed parameter value that can hold a string, number, boolean, or enum value.
/// </summary>
public readonly record struct ParameterValue
{
    /// <summary>Gets the kind of value stored.</summary>
    public ParameterKind Kind { get; }

    /// <summary>Gets the string value, if <see cref="Kind"/> is <see cref="ParameterKind.String"/> or <see cref="ParameterKind.Enum"/>.</summary>
    public string? StringValue { get; }

    /// <summary>Gets the numeric value, if <see cref="Kind"/> is <see cref="ParameterKind.Number"/>.</summary>
    public double? NumberValue { get; }

    /// <summary>Gets the boolean value, if <see cref="Kind"/> is <see cref="ParameterKind.Boolean"/>.</summary>
    public bool? BoolValue { get; }

    private ParameterValue(ParameterKind kind, string? stringValue = null, double? numberValue = null, bool? boolValue = null)
    {
        Kind = kind;
        StringValue = stringValue;
        NumberValue = numberValue;
        BoolValue = boolValue;
    }

    /// <summary>Creates a string parameter value.</summary>
    /// <param name="v">The string value.</param>
    /// <returns>A new <see cref="ParameterValue"/> of kind <see cref="ParameterKind.String"/>.</returns>
    public static ParameterValue FromString(string v) => new ParameterValue(ParameterKind.String, stringValue: v);

    /// <summary>Creates a numeric parameter value.</summary>
    /// <param name="v">The numeric value.</param>
    /// <returns>A new <see cref="ParameterValue"/> of kind <see cref="ParameterKind.Number"/>.</returns>
    public static ParameterValue FromNumber(double v) => new ParameterValue(ParameterKind.Number, numberValue: v);

    /// <summary>Creates a boolean parameter value.</summary>
    /// <param name="v">The boolean value.</param>
    /// <returns>A new <see cref="ParameterValue"/> of kind <see cref="ParameterKind.Boolean"/>.</returns>
    public static ParameterValue FromBool(bool v) => new ParameterValue(ParameterKind.Boolean, boolValue: v);

    /// <summary>Creates an enum parameter value.</summary>
    /// <param name="v">The enum value as string.</param>
    /// <returns>A new <see cref="ParameterValue"/> of kind <see cref="ParameterKind.Enum"/>.</returns>
    public static ParameterValue FromEnum(string v) => new ParameterValue(ParameterKind.Enum, stringValue: v);

    /// <summary>
    /// Returns a new <see cref="ParameterValue"/> with the specified kind.
    /// Only converts <see cref="ParameterKind.String"/> to <see cref="ParameterKind.Enum"/>; other kinds are returned unchanged.
    /// </summary>
    /// <param name="newKind">The desired parameter kind.</param>
    /// <returns>A new <see cref="ParameterValue"/> with the updated kind, or the current instance if no conversion is needed.</returns>
    public ParameterValue WithKind(ParameterKind newKind) =>
        newKind == ParameterKind.Enum && Kind == ParameterKind.String
            ? FromEnum(StringValue!)
            : this;

    public static implicit operator ParameterValue(string v) => FromString(v);
    public static implicit operator ParameterValue(double v) => FromNumber(v);
    public static implicit operator ParameterValue(bool v) => FromBool(v);
    public static implicit operator ParameterValue(int v) => FromNumber(v);
}
