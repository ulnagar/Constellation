namespace Constellation.Core.Models.Assessments.Repositories;

using Identifiers;
using Students.Identifiers;

public interface IAssessmentRepository
{
    Task<Assessment?> GetById(AssessmentId id, CancellationToken cancellationToken = default);

    Task<List<Assessment>> GetCurrentAssessments(CancellationToken cancellationToken = default);
    Task<List<Assessment>> GetForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<Assessment>> GetCurrentForStudent(StudentId studentId, CancellationToken cancellationToken = default);

    void Insert(Assessment assessment);
}
