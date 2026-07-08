namespace Constellation.Application.Domains.Import.Helpers;

using Constellation.Application.Models.ImportCache;
using System.Collections.Generic;

public static class ImportRowValueAccessor
{
    public static string? Get(
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        string fieldKey)
    {
        if (!columnMapping.TryGetValue(fieldKey, out string? header) || header is null)
            return null;

        return row.Values.GetValueOrDefault(header);
    }
}
