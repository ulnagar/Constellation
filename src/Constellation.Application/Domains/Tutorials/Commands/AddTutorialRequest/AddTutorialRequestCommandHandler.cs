namespace Constellation.Application.Domains.Tutorials.Commands.AddTutorialRequest;

using Abstractions.Messaging;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Errors;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTutorialRequestCommandHandler
    :ICommandHandler<AddTutorialRequestCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTutorialRequestCommandHandler(
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        ITutorialRepository tutorialRepository,
        IPeriodRepository periodRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _tutorialRepository = tutorialRepository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddTutorialRequestCommand>();
    }

    public async Task<Result> Handle(AddTutorialRequestCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AddTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to register requested Tutorial Support");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        string courseName = string.Empty;

        if (request.CourseId != CourseId.Empty)
        {
            Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

            if (course is null)
            {
                _logger
                    .ForContext(nameof(AddTutorialRequestCommand), request, true)
                    .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                    .Warning("Failed to register requested Tutorial Support");

                return Result.Failure(CourseErrors.NotFound(request.CourseId));
            }

            courseName = course.Name;
        }

        List<Period> periods = await _periodRepository.GetListFromIds(request.PeriodIds, cancellationToken);

        if (periods.Count == 0)
        {
            _logger
                .ForContext(nameof(AddTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), PeriodErrors.IsNull, true)
                .Warning("Failed to register requested Tutorial Support");

            return Result.Failure(PeriodErrors.IsNull);
        }

        Request tutorialRequest = Request.Create(
            student,
            request.TutorialType,
            courseName,
            periods,
            request.Justification);

        _tutorialRepository.Insert(tutorialRequest);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}