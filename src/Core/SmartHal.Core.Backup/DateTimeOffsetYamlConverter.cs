using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SmartHal.Core.Backup;

/// <summary>
/// Serializes <see cref="DateTimeOffset"/> as an ISO 8601 string instead of YamlDotNet's
/// default property-by-property representation. The default behavior breaks roundtripping
/// because the deserialized value is a property bag rather than a real DateTimeOffset.
/// </summary>
internal sealed class DateTimeOffsetYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();

        if (string.IsNullOrEmpty(scalar.Value))
        {
            return type == typeof(DateTimeOffset?) ? null : default(DateTimeOffset);
        }

        return DateTimeOffset.Parse(scalar.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar(string.Empty));
            return;
        }

        var dto = (DateTimeOffset)value;
        // ISO 8601 round-trip format keeps offset and sub-second precision.
        emitter.Emit(new Scalar(dto.ToString("o", CultureInfo.InvariantCulture)));
    }
}
