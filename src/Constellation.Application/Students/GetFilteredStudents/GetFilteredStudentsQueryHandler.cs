﻿namespace Constellation.Application.Students.GetFilteredStudents;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Enrolments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
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
            StudentFilter.Active => await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken),
            StudentFilter.Inactive => await _studentRepository.GetInactiveStudentsWithSchool(cancellationToken),
            _ => await _studentRepository.GetAllWithSchool(cancellationToken)
        };

        List<FilteredStudentResponse> response = new();

        foreach (Student student in students)
        {
            int enrolmentCount = await _enrolmentRepository.GetCurrentCountByStudentId(student.StudentId, cancellationToken);

            response.Add(new(
                student.StudentId,
                student.GetName(),
                student.Gender,
                student.CurrentGrade,
                student.School.Name,
                student.SchoolCode,
                enrolmentCount,
                student.IsDeleted));
        }

        return response;
    }
}
