namespace Constellation.Infrastructure.Extensions;

using System;
using System.Text.Json;

public static class JsonElementExtensions
{
    public static string? ExtractString(this JsonElement element, string propertyName)
    {
        bool propertyExists = element.TryGetProperty(propertyName, out JsonElement property);
        return propertyExists ? property.GetString() : null;
    }

    public static Guid ExtractGuid(this JsonElement element, string propertyName)
    {
        bool propertyExists = element.TryGetProperty(propertyName, out JsonElement property);
        return propertyExists ? property.GetGuid() : Guid.Empty;
    }

    public static bool? ExtractBool(this JsonElement element, string propertyName)
    {
        bool propertyExists = element.TryGetProperty(propertyName, out JsonElement property);
        return propertyExists ? property.GetBoolean() : null;
    }

    public static DateOnly? ExtractDateOnly(this JsonElement element, string propertyName)
    {
        bool propertyExists = element.TryGetProperty(propertyName, out JsonElement property);
        if (!propertyExists)
            return null;

        string propertyValue = property.GetString();
        return !string.IsNullOrWhiteSpace(propertyValue) ? DateOnly.Parse(propertyValue) : null;
    }

    public static TimeOnly? ExtractTimeOnly(this JsonElement element, string propertyName)
    {
        bool propertyExists = element.TryGetProperty(propertyName, out JsonElement property);
        if (!propertyExists)
            return null;

        string propertyValue = property.GetString();
        return !string.IsNullOrWhiteSpace(propertyValue) ? TimeOnly.Parse(propertyValue) : null;
    }

    public static int ExtractInt(this JsonElement element, string propertyName)
    {
        bool propertyExists = element.TryGetProperty(propertyName, out JsonElement property);
        if (!propertyExists)
            return default;

        if (property.ValueKind == JsonValueKind.String)
        {
            string propertyValue = property.GetString();
            return !string.IsNullOrWhiteSpace(propertyValue) ? int.Parse(propertyValue) : default;
        }

        if (property.ValueKind == JsonValueKind.Number)
            return property.GetInt32();

        return default;
    }
}
