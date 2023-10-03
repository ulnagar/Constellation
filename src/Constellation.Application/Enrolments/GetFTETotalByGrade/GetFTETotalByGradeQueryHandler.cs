namespace Constellation.Application.Enrolments.GetFTETotalByGrade;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Repositories;
using Core.Models;
using Core.Models.Enrolments;
using Core.Models.Offerings;
using Core.Models.Subjects;
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
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;

    public GetFTETotalByGradeQueryHandler(
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository)
    {
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
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

            List<string> maleStudentIds = students
                .Where(student => student.Gender == "M")
                .Select(student => student.StudentId)
                .ToList();

            List<string> femaleStudentIds = students
                .Where(student => student.Gender == "F")
                .Select(student => student.StudentId)
                .ToList();

            List<Offering> offerings = await _offeringRepository.GetActiveByGrade(grade, cancellationToken);

            List<Course> courses = await _courseRepository.GetByGrade(grade, cancellationToken);

            foreach (Course course in courses)
            {
                List<Enrolment> activeEnrolments = 
                    await _enrolmentRepository.GetCurrentByCourseId(course.Id, cancellationToken);

                maleFteTotal += activeEnrolments.Count(enrol => maleStudentIds.Contains(enrol.StudentId)) *
                                course.FullTimeEquivalentValue;

                femaleFteTotal += activeEnrolments.Count(enrol => femaleStudentIds.Contains(enrol.StudentId)) *
                                  course.FullTimeEquivalentValue;

                fteTotal += activeEnrolments.Count * course.FullTimeEquivalentValue;

                if (fteTotal != maleFteTotal + femaleFteTotal)
                {
                    // Something bad has happened!
                }
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
