namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Queries;

using Constellation.Application.Features.ShortTerm.Covers.Queries;
using Constellation.Core.Abstractions;

public class GetTeamLinkForOfferingQueryHandler : IRequestHandler<GetTeamLinkForOfferingQuery, string>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamLinkForOfferingQueryHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<string> Handle(GetTeamLinkForOfferingQuery request, CancellationToken cancellationToken)
    {
        return await _teamRepository.GetLinkByOffering(request.ClassName, request.Year, cancellationToken);
    }
}
