﻿namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromOfferingGrade;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsFromOfferingGradeQueryHandler
    : IQueryHandler<GetStudentsFromOfferingGradeQuery, List<StudentFromGradeResponse>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentsFromOfferingGradeQueryHandler(
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetStudentsFromOfferingGradeQuery>();
    }

    public async Task<Result<List<StudentFromGradeResponse>>> Handle(GetStudentsFromOfferingGradeQuery request, CancellationToken cancellationToken)
    {
        List<StudentFromGradeResponse> response = new();

        Course course = await _courseRepository.GetByOfferingId(request.OfferingId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(GetStudentsFromOfferingGradeQuery), request, true)
                .ForContext(nameof(Error), CourseErrors.NoneFound, true)
                .Warning("Failed to retrieve students from Grade of Offering");

            return Result.Failure<List<StudentFromGradeResponse>>(CourseErrors.NoneFound);
        }
        
        List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(course.Grade, cancellationToken);

        foreach (Student student in students)
        {
            response.Add(new(
                student.Id,
                student.Name));
        }

        return response;
    }
}

