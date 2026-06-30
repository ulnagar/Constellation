namespace Constellation.Core.Models.EnrolmentContext.Application.Repositories;

using EnrolmentPeriod;
using ApplicationId = Identifiers.ApplicationId;

public interface IEnrolmentApplicationRepository
{
    Task<Application?> GetApplicationById(ApplicationId id, CancellationToken cancellationToken = default);

    Task<List<EnrolmentPeriod>> GetCurrentEnrolmentPeriods(CancellationToken cancellationToken = default);

    void Insert(Application application);
}
