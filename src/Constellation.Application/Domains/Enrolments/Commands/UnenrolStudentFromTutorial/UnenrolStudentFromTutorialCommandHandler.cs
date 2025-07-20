namespace Constellation.Application.Domains.Enrolments.Commands.UnenrolStudentFromTutorial;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UnenrolStudentFromTutorialCommandHandler
: ICommandHandler<UnenrolStudentFromTutorialCommand>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UnenrolStudentFromTutorialCommandHandler(
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UnenrolStudentFromTutorialCommand>();
    }

    public async Task<Result> Handle(UnenrolStudentFromTutorialCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find Student with Id {id}", request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        List<TutorialEnrolment> tutorialEnrolments = enrolments
            .OfType<TutorialEnrolment>()
            .Where(enrolment => enrolment.TutorialId == request.TutorialId)
            .ToList();

        foreach (TutorialEnrolment enrolment in tutorialEnrolments)
        {
            Result actionRequest = enrolment.Cancel();

            if (actionRequest.IsFailure)
                return actionRequest;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
