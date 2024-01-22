namespace Constellation.Application.ThirdPartyConsent.GetApplicationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.ThirdPartyConsent.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Shared;
using Serilog;
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
            Student student = await _studentRepository.GetWithSchoolById(consent.StudentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Application), application, true)
                    .ForContext(nameof(Consent), consent, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(consent.StudentId), true)
                    .Warning("Could not find student to include in Consent details");
                
                continue;
            }

            consentResponses.Add(new(
                consent.Id,
                consent.TransactionId,
                consent.StudentId,
                student.GetName(),
                student.CurrentGrade,
                student.School.Name,
                consent.ConsentProvided,
                consent.ProvidedBy,
                consent.ProvidedAt,
                consent.Method,
                consent.MethodNotes));
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
            consentResponses);
    }
}
