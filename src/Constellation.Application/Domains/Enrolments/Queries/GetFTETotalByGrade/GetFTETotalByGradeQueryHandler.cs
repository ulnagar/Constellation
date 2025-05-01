namespace Constellation.Application.Domains.Enrolments.Queries.GetFTETotalByGrade;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFTETotalByGradeQueryHandler
    : IQueryHandler<GetFTETotalByGradeQuery, List<GradeFTESummaryResponse>>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;

    public GetFTETotalByGradeQueryHandler(
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository)
    {
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<List<GradeFTESummaryResponse>>> Handle(GetFTETotalByGradeQuery request, CancellationToken cancellationToken)
    {
        List<GradeFTESummaryResponse> response = new();

        foreach (Grade grade in Enum.GetValues<Grade>())
        {
            decimal maleFteTotal = 0;
            decimal femaleFteTotal = 0;
            decimal fteTotal = 0;

            List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

            List<StudentId> maleStudentIds = students
                .Where(student => student.Gender == Gender.Male)
                .Select(student => student.Id)
                .ToList();

            List<StudentId> femaleStudentIds = students
                .Where(student => student.Gender == Gender.Female)
                .Select(student => student.Id)
                .ToList();
            
            List<Course> courses = await _courseRepository.GetByGrade(grade, cancellationToken);

            foreach (Course course in courses)
            {
                List<Enrolment> activeEnrolments = await _enrolmentRepository.GetCurrentByCourseId(course.Id, cancellationToken);

                maleFteTotal += activeEnrolments.Count(enrol => maleStudentIds.Contains(enrol.StudentId)) * course.FullTimeEquivalentValue;

                femaleFteTotal += activeEnrolments.Count(enrol => femaleStudentIds.Contains(enrol.StudentId)) * course.FullTimeEquivalentValue;

                fteTotal += activeEnrolments.Count * course.FullTimeEquivalentValue;
            }

            response.Add(new(
                grade,
                maleStudentIds.Count(),
                maleFteTotal,
                femaleStudentIds.Count(),
                femaleFteTotal));
        }

        return response;
    }
}
