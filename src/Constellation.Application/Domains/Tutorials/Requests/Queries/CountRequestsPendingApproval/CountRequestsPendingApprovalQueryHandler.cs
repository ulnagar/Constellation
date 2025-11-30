namespace Constellation.Application.Domains.Tutorials.Requests.Queries.CountRequestsPendingApproval;

using Abstractions.Messaging;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountRequestsPendingApprovalQueryHandler
    : IQueryHandler<CountRequestsPendingApprovalQuery, int>
{
    private readonly ITutorialRepository _tutorialRepository;

    public CountRequestsPendingApprovalQueryHandler(
        ITutorialRepository tutorialRepository)
    {
        _tutorialRepository = tutorialRepository;
    }

    public async Task<Result<int>> Handle(CountRequestsPendingApprovalQuery request, CancellationToken cancellationToken)
    {
        return await _tutorialRepository.CountPendingRequests(cancellationToken);
    }
}
