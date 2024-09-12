namespace Constellation.Application.Students.CreateStudent;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Errors;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateStudentCommandHandler
: ICommandHandler<CreateStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateStudentCommandHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateStudentCommand>();
    }

    public async Task<Result> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        Result<Name> name = Name.Create(request.FirstName, request.PreferredName, request.LastName);

        if (name.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateStudentCommand), request, true)
                .ForContext(nameof(Error), name.Error, true)
                .Warning("Failed to create new student");

            return Result.Failure(name.Error);
        }

        Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(request.SRN);

        if (studentReferenceNumber.IsFailure)
        {
            Result<Student> student = Student.Create(
                name.Value,
                request.Gender,
                _dateTime);

            if (student.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateStudentCommand), request, true)
                    .ForContext(nameof(Error), student.Error, true)
                    .Warning("Failed to create new student");

                return Result.Failure(student.Error);
            }

            if (!string.IsNullOrWhiteSpace(request.SchoolCode) && request.Grade != Grade.SpecialProgram)
            {
                School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

                if (school is null)
                {
                    _logger
                        .ForContext(nameof(CreateStudentCommand), request, true)
                        .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                        .Warning("Failed to create new student");

                    return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
                }

                student.Value.AddSchoolEnrolment(
                    request.SchoolCode,
                    school.Name,
                    request.Grade,
                    _dateTime);
            }

            _studentRepository.Insert(student.Value);
        }
        else
        {
            Student? existing = await _studentRepository.GetBySRN(studentReferenceNumber.Value, cancellationToken);

            if (existing is not null)
            {
                _logger
                    .ForContext(nameof(CreateStudentCommand), request, true)
                    .ForContext(nameof(Error), StudentErrors.AlreadyExists(existing.Id), true)
                    .Warning("Failed to create new student");

                return Result.Failure(StudentErrors.AlreadyExists(existing.Id));
            }

            Result<EmailAddress> emailAddress = EmailAddress.Create(request.EmailAddress);

            if (emailAddress.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateStudentCommand), request, true)
                    .ForContext(nameof(Error), emailAddress.Error, true)
                    .Warning("Failed to create new student");

                return Result.Failure(emailAddress.Error);
            }

            School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

            if (school is null)
            {
                _logger
                    .ForContext(nameof(CreateStudentCommand), request, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                    .Warning("Failed to create new student");

                return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
            }

            Result<Student> student = Student.Create(
                studentReferenceNumber.Value,
                name.Value,
                emailAddress.Value,
                request.Grade,
                school,
                request.Gender,
                _dateTime);

            if (student.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateStudentCommand), request, true)
                    .ForContext(nameof(Error), student.Error, true)
                    .Warning("Failed to create new student");

                return Result.Failure(student.Error);
            }

            _studentRepository.Insert(student.Value);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
