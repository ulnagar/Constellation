namespace Constellation.Application.Domains.Tutorials.Commands.UpdateTutorial;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTutorialCommandHandler
: ICommandHandler<UpdateTutorialCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateTutorialCommandHandler(
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
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTutorialCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.Id, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(UpdateTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.Id), true)
                .Warning("Failed to update Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure(TutorialErrors.NotFound(request.Id));
        }

        Result update = tutorial.Update(request.Name, request.StartDate, request.EndDate, _dateTime);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateTutorialCommand), request, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
