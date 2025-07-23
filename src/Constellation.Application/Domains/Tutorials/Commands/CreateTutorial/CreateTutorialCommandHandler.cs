namespace Constellation.Application.Domains.Tutorials.Commands.CreateTutorial;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTutorialCommandHandler
: ICommandHandler<CreateTutorialCommand, TutorialId>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateTutorialCommandHandler(
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
            .ForContext<CreateTutorialCommand>();
    }

    public async Task<Result<TutorialId>> Handle(CreateTutorialCommand request, CancellationToken cancellationToken)
    {
        Result<Tutorial> tutorial = Tutorial.Create(
            request.Name,
            request.StartDate,
            request.EndDate,
            _dateTime);

        if (tutorial.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTutorialCommand), request, true)
                .ForContext(nameof(Error), tutorial.Error, true)
                .Warning("Failed to create Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure<TutorialId>(tutorial.Error);
        }

        bool alreadyExists = await _tutorialRepository.DoesTutorialAlreadyExist(tutorial.Value.Name, tutorial.Value.StartDate, tutorial.Value.EndDate, cancellationToken);

        if (alreadyExists)
        {
            _logger
                .ForContext(nameof(CreateTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.Validation.AlreadyExists)
                .Warning("Failed to create Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure<TutorialId>(TutorialErrors.Validation.AlreadyExists);
        }
        
        _tutorialRepository.Insert(tutorial.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return tutorial.Value.Id;
    }
}
