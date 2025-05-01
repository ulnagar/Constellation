namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsFromGrade;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students.Errors;
using Core.Shared;
using Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsFromGradeQueryHandler
    : IQueryHandler<GetCurrentStudentsFromGradeQuery, List<StudentResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetCurrentStudentsFromGradeQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetCurrentStudentsFromGradeQuery request, CancellationToken cancellationToken)
    {
        List<StudentResponse> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(request.Grade, cancellationToken);

        if (students is null)
        {
            _logger
                .ForContext(nameof(GetCurrentStudentsFromGradeQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFoundForGrade(request.Grade), true)
                .Warning("Could not retrieve students from grade");

            return Result.Failure<List<StudentResponse>>(StudentErrors.NotFoundForGrade(request.Grade));
        }

        foreach (Student student in students)
        {
            SchoolEnrolment enrolment = student.CurrentEnrolment;

            bool currentEnrolment = true;

            if (enrolment is null)
            {
                currentEnrolment = false;

                // retrieve most recent applicable school enrolment
                if (student.SchoolEnrolments.Count > 0)
                {
                    int maxYear = student.SchoolEnrolments.Max(item => item.Year);

                    SchoolEnrolmentId enrolmentId = student.SchoolEnrolments
                        .Where(entry => entry.Year == maxYear)
                        .Select(entry => new { entry.Id, Date = entry.EndDate ?? DateOnly.MaxValue })
                        .MaxBy(entry => entry.Date)
                        .Id;

                    enrolment = student.SchoolEnrolments.FirstOrDefault(entry => entry.Id == enrolmentId);
                }
            }

            response.Add(new(
                student.Id,
                student.StudentReferenceNumber,
                student.Name,
                student.PreferredGender,
                enrolment?.Grade,
                student.EmailAddress,
                enrolment?.SchoolName,
                enrolment?.SchoolCode,
                currentEnrolment,
                student.IsDeleted));
        }

        return response;
    }
}
