namespace Constellation.Application.Interfaces.Services;

using Core.Shared;

public interface IImportService
{
    Task<Result<Guid>> StageImportFile(MemoryStream stream, string fileName, CancellationToken cancellationToken = default);
}