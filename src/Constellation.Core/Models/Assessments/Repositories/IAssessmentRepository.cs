namespace Constellation.Core.Models.Assessments.Repositories;

using Identifiers;

public interface IAssessmentRepository
{
    Task<Assessment?> GetById(AssessmentId id, CancellationToken cancellationToken = default);
}
