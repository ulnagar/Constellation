namespace Constellation.Application.ThirdPartyConsent.GetApplications;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.ThirdPartyConsent.Repositories;
using Core.Models.ThirdPartyConsent;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetApplicationsQueryHandler
    : IQueryHandler<GetApplicationsQuery, List<ApplicationSummaryResponse>>
{
    private readonly IConsentRepository _consentRepository;

    public GetApplicationsQueryHandler(
        IConsentRepository consentRepository)
    {
        _consentRepository = consentRepository;
    }

    public async Task<Result<List<ApplicationSummaryResponse>>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        List<ApplicationSummaryResponse> response = new();

        List<Application> applications = await _consentRepository.GetAllApplications(cancellationToken);

        foreach (Application application in applications)
        {
            response.Add(new(
                application.Id,
                application.Name,
                application.Purpose,
                application.ConsentRequired,
                application.IsDeleted,
                application.GetActiveConsents().Count));
        }

        return response;
    }
}
