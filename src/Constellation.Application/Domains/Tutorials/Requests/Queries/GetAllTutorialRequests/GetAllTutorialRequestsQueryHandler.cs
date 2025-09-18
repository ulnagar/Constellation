namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetAllTutorialRequests;

using Abstractions.Messaging;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Enums;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllTutorialRequestsQueryHandler
: IQueryHandler<GetAllTutorialRequestsQuery, List<TutorialRequestSummaryResponse>>
{
    private readonly ITutorialRepository _tutorialRepository;

    public GetAllTutorialRequestsQueryHandler(
        ITutorialRepository tutorialRepository)
    {
        _tutorialRepository = tutorialRepository;
    }

    public async Task<Result<List<TutorialRequestSummaryResponse>>> Handle(GetAllTutorialRequestsQuery request, CancellationToken cancellationToken)
    {
        List<TutorialRequestSummaryResponse> responses = [];

        List<Request> requests = await _tutorialRepository.GetAllRequests(cancellationToken);

        foreach (Request tutorialRequest in requests)
        {
            DateOnly actionDate = tutorialRequest.Status == RequestStatus.Requested
                ? DateOnly.FromDateTime(tutorialRequest.CreatedAt)
                : DateOnly.FromDateTime(tutorialRequest.ModifiedAt);

            responses.Add(new(
                tutorialRequest.Id,
                tutorialRequest.Student,
                tutorialRequest.Type,
                tutorialRequest.Subject,
                tutorialRequest.Status,
                actionDate));
        }

        return responses;
    }
}
