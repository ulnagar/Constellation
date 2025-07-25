namespace Constellation.Application.Domains.Tutorials.Commands.AddSessionToTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddSessionToTutorialCommandHandler
: ICommandHandler<AddSessionToTutorialCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddSessionToTutorialCommandHandler(
        ITutorialRepository tutorialRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddSessionToTutorialCommand>();
    }

    public async Task<Result> Handle(AddSessionToTutorialCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.Id, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(AddSessionToTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.Id), true)
                .Warning("Failed to add session to tutorial");

            return Result.Failure(TutorialErrors.NotFound(request.Id));
        }

        Result result = tutorial.AddSession(
            request.Week,
            request.Day,
            request.StartTime,
            request.EndTime,
            request.StaffId);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddSessionToTutorialCommand), request, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add session to tutorial");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
