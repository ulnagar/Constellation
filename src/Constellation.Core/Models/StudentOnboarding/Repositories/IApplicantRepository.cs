namespace Constellation.Core.Models.StudentOnboarding.Repositories;

using Enums;
using Identifiers;
using Students.ValueObjects;
using ValueObjects;

public interface IOnboardingRepository
{
    Task<Application?> GetApplicationById(ApplicationId applicationId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByApplicantId(ApplicantId applicantId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByParentId(ParentId parentId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByProgram(Program program, CancellationToken cancellationToken = default);

    Task<List<Application>> GetAllApplications(CancellationToken cancellationToken = default);
    Task<List<Application>> GetCurrentApplications(CancellationToken cancellationToken = default);

    Task<bool> DoesApplicationIdExist(ApplicationId applicationId, CancellationToken cancellationToken = default);

    Task<Applicant?> GetApplicantById(ApplicantId applicantId, CancellationToken cancellationToken = default);
    Task<Applicant?> GetApplicantByStudentReferenceNumber(StudentReferenceNumber srn, CancellationToken cancellationToken = default);
    Task<Applicant?> GetApplicantByEmailAddress(EmailAddress emailAddress, CancellationToken cancellationToken = default);

    void Insert(Application application);
    void Insert(Applicant applicant);
}
