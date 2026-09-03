namespace Constellation.Application.Domains.Import.Interfaces;

using Application.Models.ImportCache;
using Core.Shared;
using System.Collections.Generic;

public interface IImportRowMapper<TResult, in TContext>
{
    Task<Result<TResult>> MapNew(
        StagedImportRow row, 
        IReadOnlyDictionary<string, string?> columnMapping, 
        TContext context,
        CancellationToken cancellationToken = default);

    Task<Result> ApplyUpdates(
        TResult existing,
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        TContext context,
        CancellationToken cancellationToken = default);
}
