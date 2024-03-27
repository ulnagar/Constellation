namespace Constellation.Application.GroupTutorials.AddTeacherToTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Repositories;
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
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        Staff teacher = await _staffRepository.GetForExistCheck(request.StaffId);

        if (teacher is null)
            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));

        Result<TutorialTeacher> result = tutorial.AddTeacher(teacher, request.EffectiveTo);

        if (result.IsFailure) 
            return result;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
