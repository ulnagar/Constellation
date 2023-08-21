namespace Constellation.Application.Absences.GetAbsencesWithFilterForReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsencesWithFilterForReportQueryHandler
    : IQueryHandler<GetAbsencesWithFilterForReportQuery, List<FilteredAbsenceResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetAbsencesWithFilterForReportQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        ICourseOfferingRepository offeringRepository,
        ISchoolRepository schoolRepository)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<FilteredAbsenceResponse>>> Handle(GetAbsencesWithFilterForReportQuery request, CancellationToken cancellationToken)
    {
        List<FilteredAbsenceResponse> result = new();

        if (!request.StudentIds.Any() &&
            !request.OfferingCodes.Any() &&
            !request.Grades.Any() &&
            !request.SchoolCodes.Any())
            return result;

        List<Student> students = new();

        if (request.StudentIds.Any())
            students.AddRange(await _studentRepository.GetListFromIds(request.StudentIds, cancellationToken));

        if (request.OfferingCodes.Any() ||
            request.Grades.Any() ||
            request.SchoolCodes.Any())
            students.AddRange(await _studentRepository
                .GetFilteredStudents(
                    request.OfferingCodes,
                    request.Grades,
                    request.SchoolCodes,
                    cancellationToken));

        students = students
            .Distinct()
            .ToList();

        List<Absence> absences = await _absenceRepository.GetForStudents(students.Select(student => student.StudentId).ToList(), cancellationToken);

        foreach (var absence in absences)
        {
            var student = students.First(student => student.StudentId == absence.StudentId);

            Name? studentName = student.GetName();

            if (studentName is null)
                continue;

            CourseOffering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            string offeringName = offering?.Name;

            School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            result.Add(new(
                studentName,
                school.Name,
                student.CurrentGrade,
                offeringName,
                absence.Id,
                absence.Explained,
                absence.Date,
                absence.Type,
                absence.AbsenceTimeframe,
                absence.PeriodName,
                absence.Notifications.Count(),
                absence.Responses.Count()));
        }

        return result;
    }
}
