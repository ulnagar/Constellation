namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplication;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class UpdateEnrolmentApplicationCommandHandler
: ICommandHandler<UpdateEnrolmentApplicationCommand>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateEnrolmentApplicationCommandHandler(
        IEnrolmentApplicationRepository repository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateEnrolmentApplicationCommand>();
    }

    public async Task<Result> Handle(UpdateEnrolmentApplicationCommand request, CancellationToken cancellationToken)
    {
        Application? application = await _repository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationCommand), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to update Enrolment Application");

            return Result.Failure(EnrolmentApplicationErrors.NotFound(request.ApplicationId));
        }

        Result updateResult = application.Update(
            request.ApplicationReference,
            request.CurrentSchoolCode,
            request.CurrentSchool,
            request.DestinationSchoolCode,
            request.DestinationSchool,
            request.Program,
            request.Grade);

        if (updateResult.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationCommand), request, true)
                .ForContext(nameof(Error), updateResult.Error, true)
                .Warning("Failed to update Enrolment Application");

            return updateResult;
        }

        foreach (CourseSelection course in application.SelectedCourses.ToList())
        {
            CourseSelection? existingCourse = request.Courses.FirstOrDefault(entry => entry.Course == course.Course);

            if (existingCourse is null)
            {
                application.UpdateCourse(course.Course, course.Line, CourseSelectionStatus.Withdrawn);
                continue;
            }

            if (existingCourse.Line != course.Line)
                application.UpdateCourse(course.Course, existingCourse.Line, course.Status);
        }

        foreach (CourseSelection course in request.Courses)
        {
            CourseSelection? existingCourse = application.SelectedCourses.FirstOrDefault(entry => entry.Course == course.Course);

            if (existingCourse is null)
            {
                application.AddCourse(course.Course, course.Line);
                continue;
            }

            if (existingCourse.Line != course.Line)
                application.UpdateCourse(course.Course, course.Line, existingCourse.Status);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
