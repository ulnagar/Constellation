namespace Constellation.Core.Models.EnrolmentContext.Application.Repositories;

using EnrolmentPeriod.Identifiers;
using EnrolmentStudent.Identifiers;
using Students.ValueObjects;
using ApplicationId = Identifiers.ApplicationId;

public interface IEnrolmentApplicationRepository
{
    Task<List<Application>> GetAll(CancellationToken cancellationToken = default);
    Task<Application?> GetApplicationById(ApplicationId id, CancellationToken cancellationToken = default);
    Task<Application?> GetApplicationByReference(EnrolmentPeriodId periodId, string reference, CancellationToken cancellationToken = default);
    Task<Application?> GetApplicationByStudentId(EnrolmentPeriodId periodId, EnrolmentStudentId studentId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationFromStudentId(EnrolmentPeriodId periodId, List<EnrolmentStudentId> studentIds, CancellationToken cancellationToken = default);

    Task<List<Application>> GetApplicationsByPeriod(EnrolmentPeriodId id, CancellationToken cancellationToken = default);
    Task<List<Application>> GetListFromIds(List<ApplicationId> ids, CancellationToken cancellationToken = default);

    void Insert(Application application);
}
