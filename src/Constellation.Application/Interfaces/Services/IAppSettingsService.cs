namespace Constellation.Application.Interfaces.Services;

using Core.Models.AppSettings.Enums;
using Domains.AppSettings.Models;

public interface IAppSettingsService
{
    Task<CoversConfiguration?> Covers(CancellationToken cancellationToken = default);
    Task Covers(CoversConfiguration settings, CancellationToken cancellationToken = default);

    Task<LessonsConfiguration?> Lessons(CancellationToken cancellationToken = default);
    Task Lessons(LessonsConfiguration settings, CancellationToken cancellationToken = default);

    Task<ContactsConfiguration?> Contacts(ContactPosition position, CancellationToken cancellationToken = default);
    Task Contacts(ContactsConfiguration settings, CancellationToken cancellationToken = default);
}
