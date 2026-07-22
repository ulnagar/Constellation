namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplicationStatus;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class UpdateEnrolmentApplicationStatusCommandHandler
: ICommandHandler<UpdateEnrolmentApplicationStatusCommand>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateEnrolmentApplicationStatusCommandHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateEnrolmentApplicationStatusCommand>();
    }

    public async Task<Result> Handle(UpdateEnrolmentApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        Application? application = await _applicationRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationStatusCommand), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to update status of Enrolment Application");

            return Result.Failure(EnrolmentApplicationErrors.NotFound(request.ApplicationId));
        }

        Result result = application.UpdateStatus(request.Status);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationStatusCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update status of Enrolment Application");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
