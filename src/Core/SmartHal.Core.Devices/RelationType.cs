namespace SmartHal.Core.Devices;

/// <summary>
/// Represents the type of a relation between devices. Implemented as a readonly record struct
/// with well-known constants to allow adapter-specific extensions.
/// </summary>
public readonly record struct RelationType(string Value)
{
    public static readonly RelationType DirectLink = new RelationType("direct_link");
    public static readonly RelationType GroupMember = new RelationType("group_member");
    public static readonly RelationType Scene = new RelationType("scene");

    /// <inheritdoc />
    public override string ToString() => Value;

    public static implicit operator string(RelationType t) => t.Value;
    public static implicit operator RelationType(string v) => new RelationType(v);
}
