#nullable enable
using Constellation;

namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesWithFilterForReport;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsencesWithFilterForReportQueryHandler
    : IQueryHandler<GetAbsencesWithFilterForReportQuery, List<FilteredAbsenceResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;

    public GetAbsencesWithFilterForReportQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
    }

    public async Task<Result<List<FilteredAbsenceResponse>>> Handle(GetAbsencesWithFilterForReportQuery request, CancellationToken cancellationToken)
    {
        List<FilteredAbsenceResponse> result = [];

        if (request.StudentIds.Count == 0 &&
            request.OfferingCodes.Count == 0 &&
            request.Grades.Count == 0 &&
            request.SchoolCodes.Count == 0)
            return result;

        List<Student> students = [];

        if (request.StudentIds.Count > 0)
            students.AddRange(await _studentRepository.GetListFromIds(request.StudentIds, cancellationToken));

        if (request.OfferingCodes.Count > 0 ||
            request.Grades.Count > 0 ||
            request.SchoolCodes.Count > 0)
            students.AddRange(await _studentRepository
                .GetFilteredStudents(
                    request.OfferingCodes,
                    request.Grades,
                    request.SchoolCodes,
                    cancellationToken));

        students = students
            .Distinct()
            .ToList();

        List<Absence> absences = await _absenceRepository.GetForStudents(students.Select(student => student.Id).ToList(), cancellationToken);

        foreach (Absence absence in absences)
        {
            Student student = students.First(student => student.Id == absence.StudentId);

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            Offering? offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            string offeringName = offering?.Name;

            result.Add(new(
                student.Name,
                enrolment.SchoolName,
                enrolment.Grade,
                offeringName,
                absence.Id,
                absence.Explained,
                absence.Date,
                absence.Type,
                absence.AbsenceTimeframe,
                absence.PeriodName,
                absence.Notifications.Count,
                absence.Responses.Count));
        }

        return result;
    }
}
