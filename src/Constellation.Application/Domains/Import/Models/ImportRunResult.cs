namespace Constellation.Application.Domains.Import.Models;

using Core.Shared;
using System.Collections.Generic;

public sealed record ImportRunResult<T>(
    int TotalRows,
    int SuccessfulRows,
    IReadOnlyList<RowImportFailure> Failures)
    where T : class
{
    public int SucceededCount => SuccessfulRows;
    public int FailedCount => Failures.Count;
    public bool HasFailures => Failures.Count > 0;
}

public sealed record RowImportFailure(int RowNumber, Error Error);