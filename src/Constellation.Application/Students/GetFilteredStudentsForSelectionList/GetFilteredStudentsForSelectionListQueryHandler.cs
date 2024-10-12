namespace Constellation.Application.Students.GetFilteredStudentsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFilteredStudentsForSelectionListQueryHandler
    : IQueryHandler<GetFilteredStudentsForSelectionListQuery, List<StudentForSelectionList>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetFilteredStudentsForSelectionListQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<Result<List<StudentForSelectionList>>> Handle(GetFilteredStudentsForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<StudentForSelectionList> response = new();
        List<Student> filteredStudents = new();

        List<Student> studentsFromGrade = new();
        List<Student> studentsFromClass = new();
        List<Student> studentsFromCourse = new();

        if (request.FromGrades.Count > 0)
        {
            foreach (Grade grade in request.FromGrades)
            {
                List<Student> gradeStudents = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

                studentsFromGrade.AddRange(gradeStudents);
            }
        }

        if (request.FromOffering.Count > 0)
        {
            foreach (OfferingId offeringId in  request.FromOffering)
            {
                List<Student> offeringStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offeringId, cancellationToken);

                studentsFromClass.AddRange(offeringStudents);
            }
        }

        if (request.FromCourse.Count > 0)
        {
            foreach (CourseId courseId in request.FromCourse)
            {
                List<Student> courseStudents = await _studentRepository.GetCurrentEnrolmentsForCourse(courseId, cancellationToken);

                studentsFromCourse.AddRange(courseStudents);
            }
        }

        if (studentsFromGrade.Count > 0 && studentsFromClass.Count == 0 && studentsFromCourse.Count == 0)
            filteredStudents = studentsFromGrade;

        if (studentsFromGrade.Count == 0 && studentsFromClass.Count > 0 && studentsFromCourse.Count == 0)
            filteredStudents = studentsFromClass;

        if (studentsFromGrade.Count == 0 && studentsFromClass.Count == 0 && studentsFromCourse.Count > 0)
            filteredStudents = studentsFromCourse;

        if (studentsFromGrade.Count > 0 && studentsFromClass.Count > 0)
            filteredStudents = studentsFromGrade.Where(student => studentsFromClass.Contains(student)).ToList();

        if (studentsFromGrade.Count > 0 && studentsFromCourse.Count > 0)
            filteredStudents = studentsFromGrade.Where(student => studentsFromCourse.Contains(student)).ToList();

        if (studentsFromClass.Count > 0 && studentsFromCourse.Count > 0)
            filteredStudents = studentsFromClass.Where(student => studentsFromCourse.Contains(student)).ToList();

        if (studentsFromGrade.Count > 0 && studentsFromClass.Count > 0 && studentsFromCourse.Count > 0)
        {
            filteredStudents = studentsFromGrade.Where(student => studentsFromClass.Contains(student)).ToList();
            filteredStudents = filteredStudents.Where(student => studentsFromCourse.Contains(student)).ToList();
        }

        foreach (Student student in filteredStudents)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            response.Add(new(
                student.Id,
                student.Name,
                enrolment.Grade));
        }

        return response;
    }
}
