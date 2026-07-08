namespace Constellation.Application.Domains.Import.Interfaces;

using Application.Models.ImportCache;
using Core.Shared;
using System.Collections.Generic;

public interface IImportRowMapper<TResult, in TContext>
{
    Task<Result<TResult>> Map(
        StagedImportRow row, 
        IReadOnlyDictionary<string, string?> columnMapping, 
        TContext context,
        CancellationToken cancellationToken = default);
}
