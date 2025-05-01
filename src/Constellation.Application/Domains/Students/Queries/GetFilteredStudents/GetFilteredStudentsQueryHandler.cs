namespace Constellation.Application.Domains.Students.Queries.GetFilteredStudents;

using Constellation.Application.Abstractions.Messaging;
using Core.Enums;
using Core.Models.Enrolments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFilteredStudentsQueryHandler
    : IQueryHandler<GetFilteredStudentsQuery, List<FilteredStudentResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ILogger _logger;

    public GetFilteredStudentsQueryHandler(
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _enrolmentRepository = enrolmentRepository;
        _logger = logger;
    }

    public async Task<Result<List<FilteredStudentResponse>>> Handle(GetFilteredStudentsQuery request, CancellationToken cancellationToken)
    {
        List<Student> students = request.Filter switch
        {
            StudentFilter.Active => await _studentRepository.GetCurrentStudents(cancellationToken),
            StudentFilter.Inactive => await _studentRepository.GetInactiveStudents(cancellationToken),
            _ => await _studentRepository.GetAll(cancellationToken)
        };

        List<FilteredStudentResponse> response = new();

        foreach (Student student in students)
        {
            int enrolmentCount = await _enrolmentRepository.GetCurrentCountByStudentId(student.Id, cancellationToken);

            SchoolEnrolment enrolment = student.CurrentEnrolment;
            
            if (enrolment is null)
            {
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

                response.Add(new(
                    student.Id,
                    student.StudentReferenceNumber,
                    student.Name,
                    student.PreferredGender,
                    enrolment?.Grade,
                    enrolment?.SchoolName,
                    enrolment?.SchoolCode,
                    enrolmentCount,
                    false,
                    student.IsDeleted));
            }
            else
            {
                response.Add(new(
                    student.Id,
                    student.StudentReferenceNumber,
                    student.Name,
                    student.PreferredGender,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    enrolment.SchoolCode,
                    enrolmentCount,
                    true,
                    student.IsDeleted));
            }
        }

        return response;
    }
}
