namespace Constellation.Core.Models.StudentOnboarding.Repositories;

using Identifiers;

public interface IApplicantRepository
{
    Task<bool> DoesApplicantIdExist(ApplicantId applicantId, CancellationToken cancellationToken = default);
}
