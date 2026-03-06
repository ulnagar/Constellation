namespace Constellation.Application.Helpers.JsonConverters;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FlexibleDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private static readonly string[] _formats =
    [
        "yyyy-MM-dd HH:mm:ss",      // SMSGlobal incoming date format
        "yyyy-MM-ddTHH:mm:sszzz",   // ISO 8601 with timezone
        "yyyy-MM-ddTHH:mm:ss",      // ISO 8601 without timezone
        "yyyy-MM-ddTHH:mm:ssZ"      // ISO 8601 UTC
    ];

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            return default;

        if (DateTimeOffset.TryParseExact(
                value,
                _formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTimeOffset result))
            return result;

        // Fall back to standard parse if none of the explicit formats match
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
            return result;

        throw new JsonException($"Unable to convert \"{value}\" to DateTimeOffset");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
    }
}