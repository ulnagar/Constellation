namespace Constellation.Application.GroupTutorials.RemoveStudentFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStudentFromTutorialCommandHandler
    : ICommandHandler<RemoveStudentFromTutorialCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveStudentFromTutorialCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveStudentFromTutorialCommand request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWithStudentsById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        var studentRecord = tutorial.Enrolments.FirstOrDefault(enrolment => enrolment.Id == request.EnrolmentId);

        if (studentRecord is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialEnrolment.NotFound);
        }

        var studentEntry = await _studentRepository.GetForExistCheck(studentRecord.StudentId);

        if (studentEntry is null)
        {
            return Result.Failure(DomainErrors.Partners.Student.NotFound(studentRecord.StudentId));
        }

        tutorial.UnenrolStudent(studentEntry, request.EffectiveFrom);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
