namespace Constellation.Application.Domains.Enrolments.Commands.EnrolStudentInTutorial;

using Abstractions.Messaging;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Enrolments.Errors;
using Core.Models.Enrolments.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EnrolStudentInTutorialCommandHandler
: ICommandHandler<EnrolStudentInTutorialCommand>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EnrolStudentInTutorialCommandHandler(
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        ITutorialRepository tutorialRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
        _tutorialRepository = tutorialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EnrolStudentInTutorialCommand>();
    }

    public async Task<Result> Handle(EnrolStudentInTutorialCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolStudentInTutorialCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Could not enrol student in Tutorial");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(EnrolStudentInTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Could not enrol student in Tutorial");

            return Result.Failure(TutorialErrors.NotFound(request.TutorialId));
        }

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments.OfType<TutorialEnrolment>().Any(enrolment => enrolment.TutorialId == request.TutorialId))
        {
            _logger
                .ForContext(nameof(EnrolStudentInTutorialCommand), request, true)
                .ForContext(nameof(Error), EnrolmentErrors.AlreadyExistsForTutorial(request.StudentId, request.TutorialId), true)
                .Warning("Could not enrol student in Tutorial");

            return Result.Failure(EnrolmentErrors.AlreadyExistsForTutorial(request.StudentId, request.TutorialId));
        }

        Enrolment enrolment = TutorialEnrolment.Create(request.StudentId, request.TutorialId);

        _enrolmentRepository.Insert(enrolment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
