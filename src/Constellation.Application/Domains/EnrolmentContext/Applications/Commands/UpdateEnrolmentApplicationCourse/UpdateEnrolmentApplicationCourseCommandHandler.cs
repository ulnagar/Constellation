namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplicationCourse;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class UpdateEnrolmentApplicationCourseCommandHandler
: ICommandHandler<UpdateEnrolmentApplicationCourseCommand>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateEnrolmentApplicationCourseCommandHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateEnrolmentApplicationCourseCommand>();
    }

    public async Task<Result> Handle(UpdateEnrolmentApplicationCourseCommand request, CancellationToken cancellationToken)
    {
        Application? application = await _applicationRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationCourseCommand), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to update Application Course status");

            return Result.Failure(EnrolmentApplicationErrors.NotFound(request.ApplicationId));
        }

        Result update = application.UpdateCourse(request.Course, request.Status);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationCourseCommand), request, true)
                .ForContext(nameof(Application), application, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update Application Course status");

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
