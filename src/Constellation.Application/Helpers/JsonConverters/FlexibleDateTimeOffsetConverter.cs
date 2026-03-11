namespace Constellation.Application.Helpers.JsonConverters;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FlexibleDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private static readonly (string format, DateTimeStyles style)[] _formats =
    [
        ("yyyy-MM-dd HH:mm:ss zzz",  DateTimeStyles.None),           // with offset, space separated e.g. "2026-03-11 12:59:12 +1100"
        ("yyyy-MM-dd HH:mm:sszzz",   DateTimeStyles.None),           // with offset, no space e.g. "2026-03-11 12:59:12+11:00"
        ("yyyy-MM-dd HH:mm:ss",      DateTimeStyles.AssumeLocal),    // no offset - treat as local time e.g. "2026-03-11 13:00:28"
        ("yyyy-MM-ddTHH:mm:sszzz",   DateTimeStyles.None),           // ISO 8601 with timezone
        ("yyyy-MM-ddTHH:mm:ss",      DateTimeStyles.AssumeLocal),    // ISO 8601 no timezone
        ("yyyy-MM-ddTHH:mm:ssZ",     DateTimeStyles.AssumeUniversal) // ISO 8601 UTC
    ];

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            return default;

        foreach ((string format, DateTimeStyles style) in _formats)
        {
            if (DateTimeOffset.TryParseExact(
                    value,
                    format,
                    CultureInfo.InvariantCulture,
                    style,
                    out DateTimeOffset result))
                return result;
        }

        // Final fallback - let the runtime take its best guess
        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out DateTimeOffset fallback))
        {
            return fallback;
        }

        throw new JsonException($"Unable to convert \"{value}\" to DateTimeOffset");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
    }
}