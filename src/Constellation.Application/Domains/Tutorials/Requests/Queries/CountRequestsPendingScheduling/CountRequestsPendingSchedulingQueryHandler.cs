namespace Constellation.Application.Domains.Tutorials.Requests.Queries.CountRequestsPendingScheduling;

using Abstractions.Messaging;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountRequestsPendingSchedulingQueryHandler
: IQueryHandler<CountRequestsPendingSchedulingQuery, int>
{
    private readonly ITutorialRepository _tutorialRepository;

    public CountRequestsPendingSchedulingQueryHandler(
        ITutorialRepository tutorialRepository)
    {
        _tutorialRepository = tutorialRepository;
    }

    public async Task<Result<int>> Handle(CountRequestsPendingSchedulingQuery request, CancellationToken cancellationToken)
    {
        return await _tutorialRepository.CountApprovedRequests(cancellationToken);
    }
}
