#nullable enable
namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesWithFilterForReport;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Models.Tutorials.Repositories;
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
    private readonly ITutorialRepository _tutorialRepository;

    public GetAbsencesWithFilterForReportQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
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

            string activityName = string.Empty;

            if (absence.Source == AbsenceSource.Offering)
            {
                OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                Offering? offering = await _offeringRepository.GetById(offeringId, cancellationToken);

                if (offering is not null)
                    activityName = offering.Name;
            }

            if (absence.Source == AbsenceSource.Tutorial)
            {
                TutorialId tutorialId = TutorialId.FromValue(absence.SourceId);

                Tutorial tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

                if (tutorial is not null)
                    activityName = tutorial.Name;
            }

            result.Add(new(
                student.Name,
                enrolment.SchoolName,
                enrolment.Grade,
                activityName,
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
