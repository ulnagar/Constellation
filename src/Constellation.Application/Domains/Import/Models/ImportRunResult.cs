namespace Constellation.Application.Domains.Import.Models;

using Core.Shared;
using System.Collections.Generic;

public sealed record ImportRunResult<T>(
    int TotalRows,
    IReadOnlyList<RowImportSuccess<T>> CreatedObjects,
    IReadOnlyList<RowImportFailure> Failures)
    where T : class
{
    public int SucceededCount => CreatedObjects.Count;
    public int FailedCount => Failures.Count;
    public bool HasFailures => Failures.Count > 0;
}

public sealed record RowImportFailure(int RowNumber, Error Error);
public sealed record RowImportSuccess<T>(int RowNumber, T Model);