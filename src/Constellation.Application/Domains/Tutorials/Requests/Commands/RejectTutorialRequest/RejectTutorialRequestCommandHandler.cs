namespace Constellation.Application.Domains.Tutorials.Requests.Commands.RejectTutorialRequest;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Tutorials.Enums;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Repositories;
using Core.Models.Tutorials;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RejectTutorialRequestCommandHandler
:ICommandHandler<RejectTutorialRequestCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RejectTutorialRequestCommandHandler(
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
            .ForContext<RejectTutorialRequestCommand>();
    }

    public async Task<Result> Handle(RejectTutorialRequestCommand request, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(request.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(RejectTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(request.RequestId), true)
                .Warning("Failed to reject Tutorial Request");

            return Result.Failure(TutorialRequestErrors.NotFound(request.RequestId));
        }

        Result result = tutorialRequest.Review(RequestStatus.Rejected, request.Comment, _currentUserService.UserName, _dateTime);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RejectTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reject Tutorial Request");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
