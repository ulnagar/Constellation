namespace Constellation.Infrastructure.Services;

using Application.Interfaces.Repositories;
using Core.Abstractions.Clock;
using Core.Models.Common.Enums;
using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Enums;
using Core.Models.StudentOnboarding.Errors;
using Core.Models.StudentOnboarding.Repositories;
using Core.Models.StudentOnboarding.Services;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using ApplicationId = Constellation.Core.Models.StudentOnboarding.Identifiers.ApplicationId;

internal sealed class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _onboardingRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public OnboardingService(
        IOnboardingRepository onboardingRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _onboardingRepository = onboardingRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<IOnboardingService>();
    }

    public async Task<Result<Applicant>> ApplicantFactory(
        StudentReferenceNumber? srn, 
        Name name, 
        EmailAddress? emailAddress, 
        Gender? gender,
        IndigenousStatus indigenousStatus,
        CancellationToken cancellationToken = default)
    {
        if (srn is not null)
        {
            Applicant? existingApplicant = await _onboardingRepository.GetApplicantByStudentReferenceNumber(srn, cancellationToken);

            if (existingApplicant is not null)
                return existingApplicant;
        }

        if (emailAddress is not null)
        {
            Applicant? existingApplicant = await _onboardingRepository.GetApplicantByEmailAddress(emailAddress, cancellationToken);

            if (existingApplicant is not null)
                return existingApplicant;
        }

        Applicant applicant = new(
            srn,
            name,
            emailAddress,
            gender,
            indigenousStatus);

        _onboardingRepository.Insert(applicant);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return applicant;
    }
}
