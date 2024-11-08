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
using CreateStudent;
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

        List<School> schools = await _schoolRepository.GetAll(cancellationToken);

        List<Student> students = await _studentRepository.GetAll(cancellationToken);

        foreach (var entry in importedStudents)
        {
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
                            .ForContext(nameof(CreateStudentCommand), request, true)
                            .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(entry.School), true)
                            .Warning("Failed to create new student");

                        response.Add(new(
                            entry.RowNumber,
                            false,
                            DomainErrors.Partners.School.NotFound(entry.School)));

                        continue;
                    }

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

            Student? existing = students.FirstOrDefault(student => student.StudentReferenceNumber == studentReferenceNumber.Value);

            Result<EmailAddress> emailAddress = EmailAddress.Create(entry.EmailAddress);

            if (emailAddress.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateStudentCommand), request, true)
                    .ForContext(nameof(Error), emailAddress.Error, true)
                    .Warning("Failed to create new student");

                response.Add(new(
                    entry.RowNumber,
                    false,
                    emailAddress.Error));

                continue;
            }
            
            if (existing is null)
            {
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
                        .ForContext(nameof(CreateStudentCommand), request, true)
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
                            .ForContext(nameof(CreateStudentCommand), request, true)
                            .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(entry.School), true)
                            .Warning("Failed to create new student");

                        response.Add(new(
                            entry.RowNumber,
                            false,
                            DomainErrors.Partners.School.NotFound(entry.School)));

                        continue;
                    }

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
                if (existing.IsDeleted)
                {
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
                    // Remove current SchoolEnrolment
                    existing.RemoveSchoolEnrolment(enrolment, _dateTime);
                }

                if (school is not null && grade != Grade.SpecialProgram)
                {
                    if (enrolment is not null &&
                        (enrolment.SchoolCode != school.Code || enrolment.Grade != grade))
                        existing.AddSchoolEnrolment(school.Code, school.Name, grade, _dateTime);
                }

                response.Add(new(
                    entry.RowNumber,
                    true,
                    null));
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
