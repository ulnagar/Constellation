namespace Constellation.Application.Domains.Tutorials.Requests.Commands.ScheduleTutorialRequest;

using Abstractions.Messaging;
using Constellation.Application.Domains.Tutorials.Requests.Commands.ApproveTutorialRequest;
using Constellation.Core.Models.Tutorials.Enums;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Extensions;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Models.Tutorials.ValueObjects;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ScheduleTutorialRequestCommandHandler
: ICommandHandler<ScheduleTutorialRequestCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ScheduleTutorialRequestCommandHandler(
        IStudentRepository studentRepository,
        ITutorialRepository tutorialRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _tutorialRepository = tutorialRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ScheduleTutorialRequestCommand>();
    }

    public async Task<Result> Handle(ScheduleTutorialRequestCommand request, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(request.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(request.RequestId), true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(TutorialRequestErrors.NotFound(request.RequestId));
        }

        Result result = tutorialRequest.Review(RequestStatus.Scheduled, request.Comment, _currentUserService.UserName, _dateTime);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(result.Error);
        }

        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(StudentErrors.NotFound(tutorialRequest.StudentId));
        }

        TutorialName name = TutorialName.FromValue($"{student.CurrentEnrolment.Grade.AsNumber()}T{student.Name.PreferredName[0]}{student.Name.LastName[0]}X1");

        // Check that TutorialName isn't already taken for this calendar year.

        var tutorial = Tutorial.Create()

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
