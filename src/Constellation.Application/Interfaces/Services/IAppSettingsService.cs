namespace Constellation.Application.Interfaces.Services;

using Domains.AppSettings.Models;

public interface IAppSettingsService
{
    Task<CoversConfiguration?> Covers(CancellationToken cancellationToken = default);
    Task Covers(CoversConfiguration settings, CancellationToken cancellationToken = default);

    Task<LessonsConfiguration?> Lessons(CancellationToken cancellationToken = default);
    Task Lessons(LessonsConfiguration settings, CancellationToken cancellationToken = default);
}
