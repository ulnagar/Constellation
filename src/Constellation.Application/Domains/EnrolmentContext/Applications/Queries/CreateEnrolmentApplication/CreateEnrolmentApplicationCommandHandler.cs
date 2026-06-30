namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.CreateEnrolmentApplication;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class CreateEnrolmentApplicationCommandHandler
: ICommandHandler<CreateEnrolmentApplicationCommand>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateEnrolmentApplicationCommandHandler(
        IEnrolmentApplicationRepository repository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateEnrolmentApplicationCommand>();
    }

    public async Task<Result> Handle(CreateEnrolmentApplicationCommand request, CancellationToken cancellationToken)
    {
        Result<Application> application = Application.Create(
            request.PeriodId,
            request.StudentReferenceNumber,
            request.StudentName,
            request.StudentGender,
            request.DateOfBirth,
            request.StudentEmailAddress,
            request.ParentName,
            request.ParentEmailAddress,
            request.ParentPhoneNumber,
            request.MailingAddress,
            request.ApplicationReference,
            request.CurrentSchoolCode,
            request.CurrentSchool,
            request.DestinationSchoolCode,
            request.DestinationSchool,
            request.Program,
            request.Grade);

        if (application.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateEnrolmentApplicationCommand), request, true)
                .ForContext(nameof(Error), application.Error, true)
                .Warning("Failed to create new Enrolment Application");

            return application;
        }

        _repository.Insert(application.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
