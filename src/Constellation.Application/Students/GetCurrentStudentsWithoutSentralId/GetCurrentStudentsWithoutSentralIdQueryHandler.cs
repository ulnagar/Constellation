namespace Constellation.Application.Students.GetCurrentStudentsWithoutSentralId;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Shared;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsWithoutSentralIdQueryHandler
: IQueryHandler<GetCurrentStudentsWithoutSentralIdQuery, List<StudentResponse>>
{
    private readonly IStudentRepository _studentRepository;

    public GetCurrentStudentsWithoutSentralIdQueryHandler(
        IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetCurrentStudentsWithoutSentralIdQuery request, CancellationToken cancellationToken)
    {
        List<StudentResponse> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithoutSentralId(cancellationToken);

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
