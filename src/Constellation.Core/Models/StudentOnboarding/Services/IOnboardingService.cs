namespace Constellation.Core.Models.StudentOnboarding.Services;

using Constellation.Core.Models.Common.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Shared;
using ValueObjects;

public interface IOnboardingService
{
    Task<Result<Applicant>> ApplicantFactory(StudentReferenceNumber? srn, Name name, EmailAddress? emailAddress, Gender? gender, IndigenousStatus indigenousStatus, CancellationToken cancellationToken = default);

}
