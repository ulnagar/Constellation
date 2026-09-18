namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Commands.EditGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Core.Models.GroupTutorials.Errors;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EditGroupTutorialCommandHandler
    : ICommandHandler<EditGroupTutorialCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditGroupTutorialCommandHandler(IGroupTutorialRepository groupTutorialRepository, IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(EditGroupTutorialCommand request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository
            .GetById(request.Id, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure(GroupTutorialErrors.NotFound(request.Id));
        }

        tutorial.Edit(request.Name, request.StartDate, request.EndDate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
