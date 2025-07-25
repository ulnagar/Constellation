namespace Constellation.Application.Domains.Tutorials.Commands.RemoveAllSessionsFromTutorial;

using Abstractions.Messaging;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveAllSessionsFromTutorialCommandHandler
: ICommandHandler<RemoveAllSessionsFromTutorialCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveAllSessionsFromTutorialCommandHandler(
        ITutorialRepository tutorialRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RemoveAllSessionsFromTutorialCommand>();
    }

    public async Task<Result> Handle(RemoveAllSessionsFromTutorialCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(RemoveAllSessionsFromTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Failed to remove all sessions from tutorial");

            return Result.Failure(TutorialErrors.NotFound(request.TutorialId));
        }

        foreach (TutorialSession session in tutorial.Sessions.Where(session => !session.IsDeleted))
        {
            tutorial.DeleteSession(session);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
