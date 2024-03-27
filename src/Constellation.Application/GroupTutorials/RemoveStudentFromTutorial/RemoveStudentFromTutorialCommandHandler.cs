namespace Constellation.Application.GroupTutorials.RemoveStudentFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Students.Errors;
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
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        TutorialEnrolment studentRecord = tutorial.Enrolments.FirstOrDefault(enrolment => enrolment.Id == request.EnrolmentId);

        if (studentRecord is null)
            return Result.Failure(DomainErrors.GroupTutorials.TutorialEnrolment.NotFound);

        Student studentEntity = await _studentRepository.GetForExistCheck(studentRecord.StudentId);

        if (studentEntity is null)
            return Result.Failure(StudentErrors.NotFound(studentRecord.StudentId));

        tutorial.UnenrolStudent(studentEntity, request.EffectiveFrom);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
