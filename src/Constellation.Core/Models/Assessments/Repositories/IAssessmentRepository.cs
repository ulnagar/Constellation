namespace Constellation.Core.Models.Assessments.Repositories;

using Identifiers;
using Students.Identifiers;
using ValueObjects;

public interface IAssessmentRepository
{
    Task<Assessment?> GetAssessmentById(AssessmentId id, CancellationToken cancellationToken = default);

    Task<List<Assessment>> GetCurrentAssessments(CancellationToken cancellationToken = default);
    Task<List<Assessment>> GetAssessmentsForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<Assessment>> GetCurrentAssessmentsForStudent(StudentId studentId, CancellationToken cancellationToken = default);

    Task<Provision?> GetProvisionById(ProvisionId id, CancellationToken cancellationToken = default);
    Task<List<Provision>> GetProvisions(CancellationToken cancellationToken = default);
    Task<bool> DoesProvisionCodeExist(ProvisionCode code, CancellationToken cancellationToken = default);

    Task<StudentProvision?> GetStudentProvisionById(StudentProvisionId id, CancellationToken cancellationToken = default);
    Task<List<StudentProvision>> GetStudentProvisionsFromCurrentYear(CancellationToken cancellationToken = default);
    Task<List<StudentProvision>> GetStudentProvisions(CancellationToken cancellationToken = default);
    Task<bool> DoesCurrentStudentProvisionExist(StudentId studentId, ProvisionId provisionId, int year, CancellationToken cancellationToken = default);


    void Insert(Assessment assessment);
    void Insert(Provision provision);
    void Insert(StudentProvision studentProvision);
}
