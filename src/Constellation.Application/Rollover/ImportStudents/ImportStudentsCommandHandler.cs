namespace Constellation.Application.Rollover.ImportStudents;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.Errors;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ImportStudentsCommandHandler
    : ICommandHandler<ImportStudentsCommand, List<ImportResult>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public ImportStudentsCommandHandler(
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
        _logger = logger.ForContext<ImportStudentsCommand>();
    }

    public async Task<Result<List<ImportResult>>> Handle(ImportStudentsCommand request, CancellationToken cancellationToken)
    {
        List<ImportResult> results = new();

        List<School> schools = await _schoolRepository.GetAll(cancellationToken);

        foreach (StudentImportRecord entry in request.Records)
        {
            School school = schools.FirstOrDefault(school => school.Code == entry.SchoolCode);

            if (school is null)
            {
                _logger
                    .ForContext(nameof(StudentImportRecord), entry, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(entry.SchoolCode), true)
                    .Warning("Failed to import new student");

                results.Add(new(entry, Result.Failure(DomainErrors.Partners.School.NotFound(entry.SchoolCode))));

                continue;
            }
            
            Student existingStudent = await _studentRepository.GetBySRN(entry.StudentReferenceNumber, cancellationToken);

            if (existingStudent is not null)
            {
                switch (existingStudent.IsDeleted)
                {
                    case false:
                        _logger
                            .ForContext(nameof(StudentImportRecord), entry, true)
                            .ForContext(nameof(Error), StudentErrors.AlreadyExists(existingStudent.Id), true)
                            .Warning("Failed to import new student");

                        results.Add(new(entry, Result.Failure(StudentErrors.AlreadyExists(existingStudent.Id))));
                        break;
                    case true:
                        existingStudent.Reinstate(school, entry.Grade, _dateTime.CurrentYear, _dateTime);

                        results.Add(new(entry, Result.Success()));
                        break;
                }

                continue;
            }
            
            Result<Name> name = Name.Create(entry.FirstName, entry.PreferredName, entry.LastName);

            if (name.IsFailure)
            {
                _logger
                    .ForContext(nameof(StudentImportRecord), entry, true)
                    .ForContext(nameof(Error), name.Error, true)
                    .Warning("Failed to import new student");

                results.Add(new(entry, Result.Failure(name.Error)));

                continue;

            }

            Result<EmailAddress> emailAddress = EmailAddress.Create(entry.EmailAddress);

            if (emailAddress.IsFailure)
            {
                _logger
                    .ForContext(nameof(StudentImportRecord), entry, true)
                    .ForContext(nameof(Error), emailAddress.Error, true)
                    .Warning("Failed to import new student");

                results.Add(new(entry, Result.Failure(emailAddress.Error)));

                continue;
            }

            Gender gender = Gender.FromValue(entry.Gender);

            Result<Student> newStudent = Student.Create(
                entry.StudentReferenceNumber,
                name.Value,
                emailAddress.Value,
                entry.Grade,
                school,
                _dateTime.CurrentYear,
                gender,
                _dateTime);

            if (newStudent.IsFailure)
            {
                _logger
                    .ForContext(nameof(StudentImportRecord), entry, true)
                    .ForContext(nameof(Error), newStudent.Error, true)
                    .Warning("Failed to import new student");

                results.Add(new(entry, Result.Failure(newStudent.Error)));

                continue;
            }

            _studentRepository.Insert(newStudent.Value);

            results.Add(new(entry, Result.Success()));
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return results;
    }
}
