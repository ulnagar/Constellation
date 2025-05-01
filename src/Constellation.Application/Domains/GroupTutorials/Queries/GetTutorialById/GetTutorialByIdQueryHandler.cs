namespace Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialByIdQueryHandler
    : IQueryHandler<GetTutorialByIdQuery, GroupTutorialResponse>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;

    public GetTutorialByIdQueryHandler(IGroupTutorialRepository groupTutorialRepository)
	{
        _groupTutorialRepository = groupTutorialRepository;
    }

    public async Task<Result<GroupTutorialResponse>> Handle(GetTutorialByIdQuery request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure<GroupTutorialResponse>(new Error(
                "GroupTutorials.GroupTutorial.NotFound",
                $"The tutorial with Id {request.TutorialId} was not found."));
        }

        var response = new GroupTutorialResponse(tutorial.Id, tutorial.Name, tutorial.StartDate, tutorial.EndDate);

        return response;
    }
}
