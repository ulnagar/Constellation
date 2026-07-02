namespace Constellation.Application.Models.ImportCache;

using System;
using System.Collections.Generic;

public sealed record StagedImportRow(int RowNumber, IReadOnlyDictionary<string, string?> Values);

public sealed class StagedImport
{
    public required Guid Token { get; init; }
    public required string OriginalFileName { get; init; }
    public required IReadOnlyList<string> Headers { get; init; }
    public required IReadOnlyList<StagedImportRow> Rows { get; init; }
    public required DateTime UploadedAtUtc { get; init; }
    public required string UploadedBy { get; init; }
}