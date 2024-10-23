namespace Constellation.Application.ThirdPartyConsent.GetApplicationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.ThirdPartyConsent.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetApplicationDetailsQueryHandler
    : IQueryHandler<GetApplicationDetailsQuery, ApplicationDetailsResponse>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetApplicationDetailsQueryHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetApplicationDetailsQuery>();
    }

    public async Task<Result<ApplicationDetailsResponse>> Handle(GetApplicationDetailsQuery request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        List<Consent> currentConsents = application.GetActiveConsents();

        List<ApplicationDetailsResponse.ConsentResponse> consentResponses = new();

        foreach (Consent consent in currentConsents)
        {
            Student student = await _studentRepository.GetById(consent.StudentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Application), application, true)
                    .ForContext(nameof(Consent), consent, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(consent.StudentId), true)
                    .Warning("Could not find student to include in Consent details");
                
                continue;
            }

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
            {
                _logger
                    .ForContext(nameof(Application), application, true)
                    .ForContext(nameof(Consent), consent, true)
                    .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                    .Warning("Could not find student details to include in Consent details");

                continue;
            }

            consentResponses.Add(new(
                consent.Id,
                consent.TransactionId,
                consent.StudentId,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                consent.ConsentProvided,
                consent.ProvidedBy,
                consent.ProvidedAt,
                consent.Method,
                consent.MethodNotes));
        }

        List<ConsentRequirement> requirements = await _consentRepository.GetRequirementsForApplication(application.Id, cancellationToken);

        List<ApplicationDetailsResponse.Requirement> requirementResponses = new();

        foreach (ConsentRequirement requirement in requirements)
        {
            if (requirement.IsDeleted)
                continue;

            requirementResponses.Add(new(
                requirement.Id,
                requirement.GetType().ToString(),
                requirement.Description,
                DateOnly.FromDateTime(requirement.CreatedAt)));
        }

        return new ApplicationDetailsResponse(
            application.Id,
            application.Name,
            application.Purpose,
            application.InformationCollected,
            application.StoredCountry,
            application.SharedWith,
            application.ConsentRequired,
            application.IsDeleted,
            consentResponses,
            requirementResponses);
    }
}
