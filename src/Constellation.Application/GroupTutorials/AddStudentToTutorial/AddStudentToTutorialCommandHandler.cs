namespace Constellation.Application.GroupTutorials.AddStudentToTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Shared;
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

        Student student = await _studentRepository.GetForExistCheck(request.StudentId);

        if (student is null)
        {
            return Result.Failure(DomainErrors.Partners.Student.NotFound(request.StudentId));
        }

        Result<TutorialEnrolment> result = tutorial.EnrolStudent(student, request.EffectiveTo);

        if (result.IsFailure)
            return result;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
