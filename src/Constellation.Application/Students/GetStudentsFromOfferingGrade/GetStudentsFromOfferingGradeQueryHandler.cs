namespace Constellation.Application.Students.GetStudentsFromOfferingGrade;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students;
using Core.Errors;
using Core.Models;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsFromOfferingGradeQueryHandler
    : IQueryHandler<GetStudentsFromOfferingGradeQuery, List<StudentFromGradeResponse>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetStudentsFromOfferingGradeQueryHandler(
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
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

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(course.Grade, cancellationToken);

        foreach (Student student in students)
        {
            School school = schools.FirstOrDefault(school => school.Code == student.SchoolCode);

            if (school is null)
            {
                _logger
                    .ForContext(nameof(GetStudentsFromOfferingGradeQuery), request, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(student.SchoolCode), true)
                    .Warning("Failed to retrieve students from Grade of Offering");

                continue;
            }

            response.Add(new(
                student.StudentId,
                student.GetName()));
        }

        return response;
    }
}

