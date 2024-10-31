namespace Constellation.Core.Models.ThirdPartyConsent.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = Identifiers.ApplicationId;

public interface IConsentRepository
{
    Task<Application> GetApplicationById(ApplicationId applicationId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetAllActiveApplications(CancellationToken cancellationToken = default);
    Task<List<Application>> GetAllApplications(CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsWithoutRequiredConsent(CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationWithConsentForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetApplicationsByTransactionId(ConsentTransactionId transactionId, CancellationToken cancellationToken = default);

    Task<bool?> IsMostRecentResponse(ConsentId consentId, CancellationToken cancellationToken = default);

    Task<List<ConsentRequirement>> GetRequirementsForApplication(ApplicationId applicationId, CancellationToken cancellationToken = default);
    Task<ConsentRequirement> GetRequirementById(ConsentRequirementId requirementId, CancellationToken cancellationToken = default);
    Task<List<ConsentRequirement>> GetAllRequirements(CancellationToken cancellationToken = default);

    Task<List<CourseConsentRequirement>> GetRequirementsForCourse(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<GradeConsentRequirement>> GetRequirementsForGrade(Grade grade, CancellationToken cancellationToken = default);
    Task<List<StudentConsentRequirement>> GetRequirementsForStudent(StudentId studentId, CancellationToken cancellationToken = default);

    Task<Transaction> GetTransactionById(ConsentTransactionId transactionId, CancellationToken cancellationToken = default);

    void Insert(Application application);
    void Insert(ConsentRequirement requirement);
    void Insert(Transaction transaction);
}
