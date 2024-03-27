namespace Constellation.Application.GroupTutorials.AddStudentToTutorialRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.GroupTutorials.AddStudentToRoll;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStudentToTutorialRollCommandHandler
    : ICommandHandler<AddStudentToTutorialRollCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStudentToTutorialRollCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddStudentToTutorialRollCommand request, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        TutorialRoll roll = tutorial.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.NotFound(request.RollId));

        Student student = await _studentRepository.GetForExistCheck(request.StudentId);

        if (student is null)
            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StudentId));

        roll.AddStudent(student.StudentId, false);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
