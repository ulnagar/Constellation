namespace Constellation.Application.Students.TransferStudent;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class TransferStudentCommandHandler
    : ICommandHandler<TransferStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public TransferStudentCommandHandler(
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
        _logger = logger;
    }

    public async Task<Result> Handle(TransferStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TransferStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to transfer student to new school or grade");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(TransferStudentCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to transfer student to new school or grade");

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        Result newEnrolment = student.AddSchoolEnrolment(
            school.Code,
            school.Name,
            request.Grade,
            _dateTime,
            request.StartDate);

        if (newEnrolment.IsFailure)
        {
            _logger
                .ForContext(nameof(TransferStudentCommand), request, true)
                .ForContext(nameof(Error), newEnrolment.Error, true)
                .Warning("Failed to transfer student to new school or grade");

            return Result.Failure(newEnrolment.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}