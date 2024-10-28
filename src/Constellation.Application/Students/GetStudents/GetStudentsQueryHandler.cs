namespace Constellation.Application.Students.GetStudents;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Students.Models;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsQueryHandler
    : IQueryHandler<GetStudentsQuery, List<StudentResponse>>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentsQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        List<Student> students = await _studentRepository.GetFilteredStudents(new(), new(), new(), cancellationToken);

        if (students.Count == 0)
        {
            return Result.Failure<List<StudentResponse>>(StudentErrors.NotFound(StudentId.Empty));
        }

        List<StudentResponse> response = new();

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

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
