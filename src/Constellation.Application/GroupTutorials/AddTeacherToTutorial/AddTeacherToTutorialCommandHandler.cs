namespace Constellation.Application.GroupTutorials.AddTeacherToTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeacherToTutorialCommandHandler
    : ICommandHandler<AddTeacherToTutorialCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTeacherToTutorialCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddTeacherToTutorialCommand request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWithTeachersById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialNotFound(request.TutorialId));
        }

        var teacher = await _staffRepository.GetForExistCheck(request.StaffId);

        if (teacher is null)
        {
            return Result.Failure(new Error(
                "GroupTutorials.TutorialTeacher.TeacherNotFound",
                $"Could not find a teacher with the id {request.StaffId}"));
        }

        tutorial.AddTeacher(teacher, request.EffectiveTo);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
