namespace Constellation.Application.ThirdPartyConsent.GetApplicationsWithoutRequiredConsent;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetApplicationsWithoutRequiredConsentQueryHandler
: IQueryHandler<GetApplicationsWithoutRequiredConsentQuery, List<ApprovedApplicationResponse>>
{
    private readonly IConsentRepository _consentRepository;
    private readonly ILogger _logger;

    public GetApplicationsWithoutRequiredConsentQueryHandler(
        IConsentRepository consentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _logger = logger
            .ForContext<GetApplicationsWithoutRequiredConsentQuery>();
    }

    public async Task<Result<List<ApprovedApplicationResponse>>> Handle(GetApplicationsWithoutRequiredConsentQuery request, CancellationToken cancellationToken)
    {
        List<Application> applications = await _consentRepository.GetApplicationsWithoutRequiredConsent(cancellationToken);

        List<ApprovedApplicationResponse> response = applications
            .Select(application =>
                new ApprovedApplicationResponse(application.Id, application.Name, application.Purpose, application.ApplicationLink))
            .ToList();

        return response;
    }
}
