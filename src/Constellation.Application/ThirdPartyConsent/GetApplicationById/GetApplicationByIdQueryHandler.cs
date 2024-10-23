namespace Constellation.Application.ThirdPartyConsent.GetApplicationById;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetApplicationByIdQueryHandler
    : IQueryHandler<GetApplicationByIdQuery, ApplicationResponse>
{
    private readonly IConsentRepository _consentRepository;
    private readonly ILogger _logger;

    public GetApplicationByIdQueryHandler(
        IConsentRepository consentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _logger = logger.ForContext<GetApplicationByIdQuery>();
    }

    public async Task<Result<ApplicationResponse>> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetApplicationByIdQuery), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to retrieve Consent Application");

            return Result.Failure<ApplicationResponse>(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }

        return new ApplicationResponse(
            application.Id,
            application.Name,
            application.Purpose,
            application.InformationCollected,
            application.StoredCountry,
            application.SharedWith,
            application.ConsentRequired);
    }
}
