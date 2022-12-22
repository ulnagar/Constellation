namespace Constellation.Application.GroupTutorials.AddStudentToTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStudentToTutorialCommandHandler
    : ICommandHandler<AddStudentToTutorialCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ITutorialEnrolmentRepository _tutorialEnrolmentRepository;
    private readonly IStudentRepository _studentRepository; 
    private readonly IUnitOfWork _unitOfWork;

    public AddStudentToTutorialCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        ITutorialEnrolmentRepository tutorialEnrolmentRepository,
        IUnitOfWork unitOfWork,
        IStudentRepository studentRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _tutorialEnrolmentRepository = tutorialEnrolmentRepository;
        _unitOfWork = unitOfWork;
        _studentRepository = studentRepository;
    }

    public async Task<Result> Handle(AddStudentToTutorialCommand request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWithStudentsById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        var student = await _studentRepository.GetForExistCheck(request.StudentId);

        if (student is null)
        {
            return Result.Failure(DomainErrors.Partners.Student.NotFound(request.StudentId));
        }

        var result = tutorial.EnrolStudent(student, request.EffectiveTo);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        if (result.Value.CreatedAt == DateTime.MinValue)
            _tutorialEnrolmentRepository.Insert(result.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
