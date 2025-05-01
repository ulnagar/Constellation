namespace Constellation.Application.Domains.GroupTutorials.Commands.AddStudentToTutorial;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStudentToTutorialCommandHandler
    : ICommandHandler<AddStudentToTutorialCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStudentRepository _studentRepository; 
    private readonly IUnitOfWork _unitOfWork;

    public AddStudentToTutorialCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IUnitOfWork unitOfWork,
        IStudentRepository studentRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _unitOfWork = unitOfWork;
        _studentRepository = studentRepository;
    }

    public async Task<Result> Handle(AddStudentToTutorialCommand request, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        Result<TutorialEnrolment> result = tutorial.EnrolStudent(student, request.EffectiveTo);

        if (result.IsFailure)
            return result;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
