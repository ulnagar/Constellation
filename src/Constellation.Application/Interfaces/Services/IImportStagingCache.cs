namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Models.ImportCache;
using System;

public interface IImportStagingCache
{
    Guid Stage(StagedImport import);
    bool TryGet(Guid token, out StagedImport import);
    void Remove(Guid token);
}