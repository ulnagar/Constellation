namespace Constellation.Application.Domains.Tutorials.Requests.Commands.ScheduleTutorialRequest;

using Abstractions.Messaging;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ScheduleTutorialRequestCommandHandler
: ICommandHandler<ScheduleTutorialRequestCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ScheduleTutorialRequestCommandHandler(
        ITutorialRepository tutorialRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ScheduleTutorialRequestCommand>();
    }

    public async Task<Result> Handle(ScheduleTutorialRequestCommand request, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(request.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(request.RequestId), true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(TutorialRequestErrors.NotFound(request.RequestId));
        }

        RequestPlan plan = new(
            request.Name,
            request.Periods,
            request.StartDate);

        Result result = tutorialRequest.ScheduleRequest(
            plan,
            request.Comment,
            _currentUserService.UserName,
            _dateTime);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(result.Error);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
