namespace Constellation.Application.GroupTutorials.SubmitRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SubmitRollCommandHandler
    : ICommandHandler<SubmitRollCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitRollCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitRollCommand request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWithRollsById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        var roll = tutorial.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.NotFound(request.RollId));
        }

        var staffMember = await _staffRepository.FromEmailForExistCheck(request.StaffEmail);

        if (staffMember is null)
        {
            return Result.Failure(DomainErrors.Partners.Staff.NotFoundByEmail(request.StaffEmail));
        }

        var result = tutorial.SubmitRoll(roll, staffMember, request.StudentPresence);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
