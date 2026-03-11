namespace Constellation.Application.Helpers;

using System.Globalization;

public static class DateTimeOffsetHelpers
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

    public static DateTimeOffset AsDateTimeOffset(this string? value)
    {
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

        throw new Exception($"Unable to convert \"{value}\" to DateTimeOffset");
    }
}