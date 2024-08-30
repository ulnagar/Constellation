namespace Constellation.Application.Students.ReinstateStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateStudentCommandHandler
    : ICommandHandler<ReinstateStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public ReinstateStudentCommandHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<ReinstateStudentCommand>();
    }

    public async Task<Result> Handle(ReinstateStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(ReinstateStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to reinstate student with id {id}", request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        if (!student.IsDeleted)
        {
            return Result.Success();
        }

        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(ReinstateStudentCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to reinstate student with id {Id}", request.StudentId);

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        Result result = student.Reinstate(
            school,
            request.Grade,
            _dateTime.CurrentYear,
            _dateTime);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ReinstateStudentCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reinstate student with id {Id}", request.StudentId);

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}