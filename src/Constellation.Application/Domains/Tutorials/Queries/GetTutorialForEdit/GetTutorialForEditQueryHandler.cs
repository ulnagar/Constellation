namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialForEdit;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialForEditQueryHandler
: IQueryHandler<GetTutorialForEditQuery, TutorialForEditResponse>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public GetTutorialForEditQueryHandler(
        ITutorialRepository tutorialRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<TutorialForEditResponse>> Handle(GetTutorialForEditQuery request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(GetTutorialForEditQuery), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Failed to retrieve Tutorial for edit by user {User}", _currentUserService.UserName);

            return Result.Failure<TutorialForEditResponse>(TutorialErrors.NotFound(request.TutorialId));
        }

        return new TutorialForEditResponse(
            tutorial.Id,
            tutorial.Name,
            tutorial.StartDate,
            tutorial.EndDate);
    }
}
