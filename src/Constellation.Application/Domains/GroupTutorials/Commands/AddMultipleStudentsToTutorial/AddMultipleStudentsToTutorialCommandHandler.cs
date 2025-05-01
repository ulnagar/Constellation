namespace Constellation.Application.Domains.GroupTutorials.Commands.AddMultipleStudentsToTutorial;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.GroupTutorials;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddMultipleStudentsToTutorialCommandHandler
: ICommandHandler<AddMultipleStudentsToTutorialCommand>
{
    private readonly IGroupTutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddMultipleStudentsToTutorialCommandHandler(
        IGroupTutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddMultipleStudentsToTutorialCommand>();
    }

    public async Task<Result> Handle(AddMultipleStudentsToTutorialCommand request, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(AddMultipleStudentsToTutorialCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId), true)
                .Warning("Could not create Enrolments for multiple Students");

            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        foreach (StudentId studentId in request.StudentIds)
        {
            Student student = await _studentRepository.GetById(studentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(AddMultipleStudentsToTutorialCommand), request, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(studentId), true)
                    .Warning("Could not create Enrolments for multiple Students");

                continue;
            }

            Result<TutorialEnrolment> result = tutorial.EnrolStudent(student);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(AddMultipleStudentsToTutorialCommand), request, true)
                    .ForContext(nameof(Student), student, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Could not create Enrolments for multiple Students");
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
