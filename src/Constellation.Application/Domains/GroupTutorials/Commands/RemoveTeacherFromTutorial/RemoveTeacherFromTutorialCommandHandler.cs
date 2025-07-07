namespace Constellation.Application.Domains.GroupTutorials.Commands.RemoveTeacherFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Shared;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
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
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        TutorialTeacher teacherRecord = tutorial.Teachers.FirstOrDefault(teacher => teacher.Id == request.TeacherId);

        if (teacherRecord is null)
            return Result.Failure(DomainErrors.GroupTutorials.TutorialTeacher.NotFound);

        StaffMember staffEntity = await _staffRepository.GetById(teacherRecord.StaffId, cancellationToken);

        if (staffEntity is null)
            return Result.Failure(StaffMemberErrors.NotFound(teacherRecord.StaffId));

        tutorial.RemoveTeacher(staffEntity, request.TakesEffectOn);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
