namespace Constellation.Core.Models.EnrolmentContext.Application.Repositories;

using EnrolmentPeriod;
using EnrolmentPeriod.Identifiers;
using ApplicationId = Identifiers.ApplicationId;

public interface IEnrolmentApplicationRepository
{
    Task<Application?> GetApplicationById(ApplicationId id, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByPeriod(EnrolmentPeriodId id, CancellationToken cancellationToken = default);

    Task<List<EnrolmentPeriod>> GetAllEnrolmentPeriods(CancellationToken cancellationToken = default);
    Task<List<EnrolmentPeriod>> GetCurrentEnrolmentPeriods(CancellationToken cancellationToken = default);

    void Insert(Application application);
}
