namespace Constellation.Application.Domains.WorkFlows.Queries.CountActiveActionsForUser;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountActiveActionsForUserQueryHandler
: IQueryHandler<CountActiveActionsForUserQuery, int>
{
    private readonly ICaseRepository _caseRepository;

    public CountActiveActionsForUserQueryHandler(
        ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<int>> Handle(CountActiveActionsForUserQuery request, CancellationToken cancellationToken) =>
        await _caseRepository.CountActiveActionsForUser(request.StaffId, cancellationToken);
}
