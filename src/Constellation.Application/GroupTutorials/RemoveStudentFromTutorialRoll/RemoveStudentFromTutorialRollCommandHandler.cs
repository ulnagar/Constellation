namespace Constellation.Application.GroupTutorials.RemoveStudentFromTutorialRoll;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
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
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        TutorialRoll roll = tutorial.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.NotFound(request.RollId));

        Student student = await _studentRepository.GetForExistCheck(request.StudentId);

        if (student is null)
            return Result.Failure(StudentErrors.NotFound(request.StudentId));

        TutorialRollStudent studentRecord = roll.Students.FirstOrDefault(entry => entry.StudentId == student.Id);

        if (studentRecord is null)
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.StudentNotFound(student.Id));

        if (studentRecord.Enrolled)
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.RemoveEnrolledStudent(student.Id));

        roll.RemoveStudent(student.Id);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
