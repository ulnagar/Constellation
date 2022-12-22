namespace Constellation.Application.GroupTutorials.RemoveTeacherFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeacherFromTutorialCommandHandler
    : ICommandHandler<RemoveTeacherFromTutorialCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveTeacherFromTutorialCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveTeacherFromTutorialCommand request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWithTeachersById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        var teacherRecord = tutorial.Teachers.FirstOrDefault(teacher => teacher.Id == request.TeacherId);

        if (teacherRecord is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialTeacher.NotFound);
        }

        var staffEntry = await _staffRepository.GetForExistCheck(teacherRecord.StaffId);

        if (staffEntry is null)
        {
            return Result.Failure(DomainErrors.Partners.Staff.NotFound(teacherRecord.StaffId));
        }

        tutorial.RemoveTeacher(staffEntry, request.TakesEffectOn);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
