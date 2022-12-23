namespace Constellation.Application.GroupTutorials.RemoveStudentFromTutorialRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStudentFromTutorialRollCommandHandler
    : ICommandHandler<RemoveStudentFromTutorialRollCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveStudentFromTutorialRollCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveStudentFromTutorialRollCommand request, CancellationToken cancellationToken)
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

        var student = await _studentRepository.GetForExistCheck(request.StudentId);

        if (student is null)
        {
            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StudentId));
        }

        var studentRecord = roll.Students.FirstOrDefault(entry => entry.StudentId == student.StudentId);

        if (studentRecord is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.StudentNotFound(student.StudentId));
        }

        if (studentRecord.Enrolled)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.RemoveEnrolledStudent(student.StudentId));
        }

        roll.RemoveStudent(student.StudentId);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
