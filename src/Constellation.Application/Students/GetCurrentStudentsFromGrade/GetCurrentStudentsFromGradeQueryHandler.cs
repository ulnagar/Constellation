﻿namespace Constellation.Application.Students.GetCurrentStudentsFromGrade;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsFromGradeQueryHandler
    : IQueryHandler<GetCurrentStudentsFromGradeQuery, List<StudentResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetCurrentStudentsFromGradeQueryHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
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
            School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            string schoolName = string.Empty;

            if (school is not null)
                schoolName = school.Name;

            response.Add(new(
                student.StudentId,
                student.GetName(),
                student.Gender,
                student.CurrentGrade,
                student.PortalUsername,
                student.EmailAddress,
                schoolName,
                student.SchoolCode,
                student.IsDeleted));
        }

        return response;
    }
}
