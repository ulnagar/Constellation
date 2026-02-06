namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.GetSentimentList;

using Abstractions.Messaging;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class GetSentimentListQueryHandler
:IQueryHandler<GetSentimentListQuery, List<string>>
{
    private readonly ICheckInRepository _checkInRepository;

    public GetSentimentListQueryHandler(
        ICheckInRepository checkInRepository)
    {
        _checkInRepository = checkInRepository;
    }

    public async Task<Result<List<string>>> Handle(GetSentimentListQuery request, CancellationToken cancellationToken)
    {
        return await _checkInRepository.GetSentimentList(cancellationToken);
    }
}
