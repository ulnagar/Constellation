namespace Constellation.Application.Domains.Courses.Commands.CreateCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Core.Models.Subjects.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal class CreateCourseCommandHandler
    : ICommandHandler<CreateCourseCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateCourseCommandHandler(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateCourseCommand>();
    }

    public async Task<Result> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        Result<Course> createRequest = Course.Create(
            request.Name,
            request.Code,
            request.Grade,
            request.FacultyId,
            request.FTEValue,
            request.Target);

        if (createRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateCourseCommand), request, true)
                .ForContext(nameof(Error), createRequest.Error)
                .Warning("Could not create Course");

            return Result.Failure(createRequest.Error);
        }

        _courseRepository.Insert(createRequest.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
