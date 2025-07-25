namespace Constellation.Application.Domains.Tutorials.Commands.RemoveSessionFromTutorial;

using Abstractions.Messaging;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveSessionFromTutorialCommandHandler
: ICommandHandler<RemoveSessionFromTutorialCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveSessionFromTutorialCommandHandler(
        ITutorialRepository tutorialRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RemoveSessionFromTutorialCommand>();
    }

    public async Task<Result> Handle(RemoveSessionFromTutorialCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(RemoveSessionFromTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Failed to remove session from tutorial");

            return Result.Failure(TutorialErrors.NotFound(request.TutorialId));
        }

        TutorialSession session = tutorial.Sessions.FirstOrDefault(session => session.Id == request.SessionId);

        if (session is null)
        {
            _logger
                .ForContext(nameof(RemoveSessionFromTutorialCommand), request, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), TutorialSessionErrors.NotFound(request.SessionId), true)
                .Warning("Failed to remove session from tutorial");
            
            return Result.Failure(TutorialSessionErrors.NotFound(request.SessionId));
        }

        tutorial.DeleteSession(session);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
