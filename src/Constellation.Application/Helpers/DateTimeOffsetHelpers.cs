namespace Constellation.Application.Helpers;

using System.Globalization;

public static class DateTimeOffsetHelpers
{
    private static readonly string[] _formats =
    [
        "yyyy-MM-dd HH:mm:ss zzz",      // SMSGlobal send response format e.g. "2026-03-11 12:59:12 +1100"
        "yyyy-MM-dd HH:mm:sszzz",       // without space before offset
        "yyyy-MM-dd HH:mm:ss",          // no offset
        "yyyy-MM-ddTHH:mm:sszzz",       // ISO 8601 with timezone
        "yyyy-MM-ddTHH:mm:ss",          // ISO 8601 without timezone
        "yyyy-MM-ddTHH:mm:ssZ"          // ISO 8601 UTC
    ];

    public static DateTimeOffset AsDateTimeOffset(this string? value)
    {
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

        throw new Exception($"Unable to convert \"{value}\" to DateTimeOffset");
    }
}