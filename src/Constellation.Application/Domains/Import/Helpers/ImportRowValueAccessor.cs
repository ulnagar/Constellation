namespace Constellation.Application.Domains.Import.Helpers;

using Constellation.Application.Models.ImportCache;
using System.Collections.Generic;

public static class ImportRowValueAccessor
{
    public static bool IsMapped(IReadOnlyDictionary<string, string?> columnMapping, string fieldKey) =>
        columnMapping.TryGetValue(fieldKey, out string? header) && !string.IsNullOrWhiteSpace(header);

    public static string? Get(
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        string fieldKey)
    {
        if (!IsMapped(columnMapping, fieldKey))
            return null;

        if (!columnMapping.ContainsKey(fieldKey))
            return null;

        string? key = columnMapping[fieldKey];

        if (string.IsNullOrWhiteSpace(key))
            return null;

        string? rawValue = row.Values.GetValueOrDefault(key);

        return Normalize(rawValue);
    }

    private static string? Normalize(string? value)
    {
        if (value is null)
            return null;

        string trimmed = value.Trim();

        // A cell that's whitespace-only (e.g. " ") should behave the same as a
        // genuinely blank cell downstream — not as a non-empty string of spaces.
        return trimmed.Length == 0 ? null : trimmed;
    }
}
