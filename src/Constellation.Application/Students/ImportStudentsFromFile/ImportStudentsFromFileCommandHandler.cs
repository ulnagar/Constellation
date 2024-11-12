#nullable enable
namespace Constellation.Application.Students.ImportStudentsFromFile;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Students.ValueObjects;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.Students.Enums;
using Core.Models.Students.Errors;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ImportStudentsFromFileCommandHandler
: ICommandHandler<ImportStudentsFromFileCommand, List<ImportStatusDto>>
{
    private readonly IExcelService _excelService;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ImportStudentsFromFileCommandHandler(
        IExcelService excelService,
        ISchoolRepository schoolRepository,
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _excelService = excelService;
        _schoolRepository = schoolRepository;
        _studentRepository = studentRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ImportStudentsFromFileCommand>();
    }

    public async Task<Result<List<ImportStatusDto>>> Handle(ImportStudentsFromFileCommand request,
        CancellationToken cancellationToken)
    {
        List<ImportStatusDto> response = new();

        List<ImportStudentDto> importedStudents = await _excelService.ImportStudentsFromFile(request.ImportFile, cancellationToken);

        _logger
            .Information("Requested to initiate bulk import of {count} students", importedStudents.Count);

        List<School> schools = await _schoolRepository.GetAll(cancellationToken);

        List<Student> students = await _studentRepository.GetAll(cancellationToken);
        List<Student> updatedStudents = new();

        foreach (var entry in importedStudents)
        {
            _logger
                .ForContext(nameof(ImportStudentDto), entry, true)
                .Information("Processing student with name {firstName} {lastName}", entry.FirstName, entry.LastName);

            Result<Name> name = Name.Create(entry.FirstName, string.Empty, entry.LastName);

            if (name.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportStudentDto), entry, true)
                    .ForContext(nameof(Error), name.Error, true)
                    .Warning("Failed to create new student");

                response.Add(new(
                    entry.RowNumber,
                    false,
                    name.Error));

                continue;
            }

            Grade grade = DetermineGrade(entry.Grade);
            Gender gender = DetermineGender(entry.Gender);
            School? school = LookupSchool(entry.School, schools);
            
            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(entry.StudentReferenceNumber);

            if (studentReferenceNumber.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportStudentDto), entry, true)
                    .Information("Detected new student, creating...");

                Result<Student> student = Student.Create(
                    name.Value,
                    gender,
                    _dateTime);

                if (student.IsFailure)
                {
                    _logger
                        .ForContext(nameof(ImportStudentDto), entry, true)
                        .ForContext(nameof(Error), student.Error, true)
                        .Warning("Failed to create new student");

                    response.Add(new(
                        entry.RowNumber,
                        false,
                        student.Error));

                    continue;
                }
                
                if (!string.IsNullOrWhiteSpace(entry.School) && grade != Grade.SpecialProgram)
                {
                    if (school is null)
                    {
                        _logger
                            .ForContext(nameof(ImportStudentDto), entry, true)
                            .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(entry.School), true)
                            .Warning("Failed to create new student");

                        response.Add(new(
                            entry.RowNumber,
                            false,
                            DomainErrors.Partners.School.NotFound(entry.School)));

                        continue;
                    }

                    _logger
                        .ForContext(nameof(ImportStudentDto), entry, true)
                        .ForContext(nameof(Student), student.Value, true)
                        .Information("Adding school enrolment details to student");

                    student.Value.AddSchoolEnrolment(
                        school.Code,
                        school.Name,
                        grade,
                        _dateTime);
                }

                _studentRepository.Insert(student.Value);

                response.Add(new(
                    entry.RowNumber,
                    true,
                    null));

                continue;
            }

            Result<EmailAddress> emailAddress = EmailAddress.Create(entry.EmailAddress);

            if (emailAddress.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportStudentDto), entry, true)
                    .ForContext(nameof(Error), emailAddress.Error, true)
                    .Warning("Failed to create new student");

                response.Add(new(
                    entry.RowNumber,
                    false,
                    emailAddress.Error));

                continue;
            }

            Student? existing = students.SingleOrDefault(student => 
                student.StudentReferenceNumber == studentReferenceNumber.Value ||
                student.EmailAddress == emailAddress.Value);

            if (existing is null)
            {
                _logger
                    .ForContext(nameof(ImportStudentDto), entry, true)
                    .Information("Detected new student, creating...");

                Result<Student> student = Student.Create(
                    studentReferenceNumber.Value,
                    name.Value,
                    emailAddress.Value,
                    grade,
                    school,
                    gender,
                    _dateTime);

                if (student.IsFailure)
                {
                    _logger
                        .ForContext(nameof(ImportStudentDto), entry, true)
                        .ForContext(nameof(Error), student.Error, true)
                        .Warning("Failed to create new student");

                    response.Add(new(
                        entry.RowNumber,
                        false,
                        student.Error));

                    continue;
                }

                if (!string.IsNullOrWhiteSpace(entry.School) && grade != Grade.SpecialProgram)
                {
                    if (school is null)
                    {
                        _logger
                            .ForContext(nameof(ImportStudentDto), entry, true)
                            .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(entry.School), true)
                            .Warning("Failed to create new student");

                        response.Add(new(
                            entry.RowNumber,
                            false,
                            DomainErrors.Partners.School.NotFound(entry.School)));

                        continue;
                    }

                    _logger
                        .ForContext(nameof(ImportStudentDto), entry, true)
                        .ForContext(nameof(Student), student.Value, true)
                        .Information("Adding school enrolment details to student");

                    student.Value.AddSchoolEnrolment(
                        school.Code,
                        school.Name,
                        grade,
                        _dateTime);
                }

                _studentRepository.Insert(student.Value);

                response.Add(new(
                    entry.RowNumber,
                    true,
                    null));
            }
            else
            {
                _logger
                    .ForContext(nameof(ImportStudentDto), entry, true)
                    .ForContext(nameof(Student), existing, true)
                    .Information("Detected existing student, updating...");

                updatedStudents.Add(existing);

                if (existing.IsDeleted)
                {
                    _logger
                        .ForContext(nameof(ImportStudentDto), entry, true)
                        .ForContext(nameof(Student), existing, true)
                        .Information("Existing student is marked withdrawn, updating...");

                    if (school is not null && grade != Grade.SpecialProgram)
                    {
                        existing.Reinstate(school, grade, _dateTime);

                        response.Add(new(
                            entry.RowNumber,
                            true,
                            null));
                    }
                    else
                    {
                        response.Add(new(
                            entry.RowNumber,
                            false,
                            StudentErrors.InvalidEnrolmentDetails));
                    }

                    continue;
                }

                SchoolEnrolment? enrolment = existing.CurrentEnrolment;

                if (school is null && enrolment is not null)
                {
                    _logger
                        .ForContext(nameof(ImportStudentDto), entry, true)
                        .ForContext(nameof(Student), existing, true)
                        .Information("Detected expired school enrolment details, removing...");

                    // Remove current SchoolEnrolment
                    existing.RemoveSchoolEnrolment(enrolment, _dateTime);
                }

                if (school is not null && grade != Grade.SpecialProgram)
                {
                    if (enrolment is not null &&
                        (enrolment.SchoolCode != school.Code || enrolment.Grade != grade))
                    {
                        _logger
                            .ForContext(nameof(ImportStudentDto), entry, true)
                            .ForContext(nameof(Student), existing, true)
                            .Information("Adding school enrolment details to student");

                        existing.AddSchoolEnrolment(school.Code, school.Name, grade, _dateTime);
                    }
                }

                response.Add(new(
                    entry.RowNumber,
                    true,
                    null));
            }
        }

        if (request.RemoveExcess)
        {
            List<Student> excessStudents = students.Where(entry =>
                    !entry.IsDeleted &&
                    !updatedStudents.Contains(entry))
                .ToList();

            _logger
                .Information("Requested to remove {count} students not included in import", excessStudents.Count);

            foreach (var student in excessStudents)
            {
                _logger
                    .ForContext(nameof(Student), student, true)
                    .Information("Detected excess student {name}, withdrawing...", student.Name.DisplayName);

                student.Withdraw(_dateTime);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return response;
    }

    private static Grade DetermineGrade(string grade) =>
        grade switch
        {
            "5" => Grade.Y05,
            "6" or "6*" => Grade.Y06,
            "7" or "7*" => Grade.Y07,
            "8" or "8*" => Grade.Y08,
            "9" => Grade.Y09,
            "10" => Grade.Y10,
            "11" => Grade.Y11,
            "12" => Grade.Y12,
            _ => Grade.SpecialProgram
        };

    private static Gender DetermineGender(string gender) =>
        gender switch
        {
            "M" => Gender.Male,
            "F" => Gender.Female,
            _ => Gender.NonBinary
        };

    private static School? LookupSchool(string potentialSchool, List<School> schools)
    {
        School? school = schools.FirstOrDefault(entry => entry.Code == potentialSchool);

        if (school is not null)
            return school;

        school = schools.FirstOrDefault(entry => entry.Name == potentialSchool);

        return school;
    }
}
