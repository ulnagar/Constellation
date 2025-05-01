namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetConsentDetails;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetConsentDetailsQueryHandler
: IQueryHandler<GetConsentDetailsQuery, ConsentDetailsResponse>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetConsentDetailsQueryHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetConsentDetailsQuery>();
    }

    public async Task<Result<ConsentDetailsResponse>> Handle(GetConsentDetailsQuery request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetConsentDetailsQuery), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to retrieve details of Consent Response");

            return Result.Failure<ConsentDetailsResponse>(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }

        Consent consent = application.Consents.SingleOrDefault(consent => consent.Id == request.ConsentId);

        if (consent is null)
        {
            _logger
                .ForContext(nameof(GetConsentDetailsQuery), request, true)
                .ForContext(nameof(Error), ConsentErrors.NotFound(request.ConsentId), true)
                .Warning("Failed to retrieve details of Consent Response");

            return Result.Failure<ConsentDetailsResponse>(ConsentErrors.NotFound(request.ConsentId));
        }

        Student student = await _studentRepository.GetById(consent.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetConsentDetailsQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(consent.StudentId), true)
                .Warning("Failed to retrieve details of Consent Response");

            return Result.Failure<ConsentDetailsResponse>(StudentErrors.NotFound(consent.StudentId));
        }

        return new ConsentDetailsResponse(
            consent.Id,
            consent.TransactionId,
            application.Id,
            application.Name,
            application.Purpose,
            application.InformationCollected,
            application.StoredCountry,
            application.SharedWith,
            application.ConsentRequired,
            student.Id,
            student.Name,
            student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
            student.CurrentEnrolment?.SchoolName ?? string.Empty,
            consent.ConsentProvided,
            consent.ProvidedBy,
            consent.ProvidedAt,
            consent.Method,
            consent.MethodNotes);
    }
}
