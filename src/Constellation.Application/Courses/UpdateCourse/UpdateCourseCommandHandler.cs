namespace Constellation.Application.Courses.UpdateCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Models.Subjects.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal class UpdateCourseCommandHandler
    : ICommandHandler<UpdateCourseCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateCourseCommandHandler(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateCourseCommand>();
    }

    public async Task<Result> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);
        
        if (course is null)
        {
            _logger
                .ForContext(nameof(UpdateCourseCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Could not update Course");

            return Result.Failure(CourseErrors.NotFound(request.CourseId));
        }

        Result updateRequest = course.Update(
            request.Name,
            request.Code,
            request.Grade,
            request.FacultyId,
            request.FTEValue);

        if (updateRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateCourseCommand), request, true)
                .ForContext(nameof(Error), updateRequest.Error)
                .Warning("Could not update Course");

            return Result.Failure(updateRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
