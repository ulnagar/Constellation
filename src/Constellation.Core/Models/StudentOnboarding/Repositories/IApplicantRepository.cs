namespace Constellation.Core.Models.StudentOnboarding.Repositories;

using Enums;
using Identifiers;

public interface IApplicantRepository
{
    Task<Application?> GetApplicationById(ApplicationId applicationId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByApplicantId(ApplicantId applicantId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByParentId(ParentId parentId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByProgram(Program program, CancellationToken cancellationToken = default);

    Task<List<Application>> GetAllApplications(CancellationToken cancellationToken = default);
    Task<List<Application>> GetCurrentApplications(CancellationToken cancellationToken = default);

    Task<bool> DoesApplicationIdExist(ApplicationId applicationId, CancellationToken cancellationToken = default);

    void Insert(Application application);
}
