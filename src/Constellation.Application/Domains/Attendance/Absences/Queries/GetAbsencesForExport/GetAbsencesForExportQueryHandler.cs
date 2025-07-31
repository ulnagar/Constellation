namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesForExport;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsencesForExportQueryHandler
    : IQueryHandler<GetAbsencesForExportQuery, List<AbsenceExportResponse>>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public GetAbsencesForExportQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _logger = logger.ForContext<GetAbsencesForExportQuery>();
    }

    public async Task<Result<List<AbsenceExportResponse>>> Handle(GetAbsencesForExportQuery request, CancellationToken cancellationToken)
    {
        List<Absence> absences = await _absenceRepository.GetAllFromCurrentYear(cancellationToken);

        if (request.Filter.StudentId != StudentId.Empty)
            absences = absences
                .Where(absence => absence.StudentId == request.Filter.StudentId)
                .ToList();

        if (request.Filter.StartDate.HasValue)
            absences = absences
                .Where(absence => absence.Date >= DateOnly.FromDateTime(request.Filter.StartDate.Value))
                .ToList();

        if (request.Filter.EndDate.HasValue)
            absences = absences
                .Where(absence => absence.Date <= DateOnly.FromDateTime(request.Filter.EndDate.Value))
                .ToList();

        List<AbsenceExportResponse> results = new();

        foreach (Absence absence in absences)
        {
            Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

            if (student is null)
                continue;

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            if (!string.IsNullOrWhiteSpace(request.Filter.SchoolCode) && enrolment.SchoolCode != request.Filter.SchoolCode)
                continue;

            if (request.Filter.Grade.HasValue && enrolment.Grade != (Grade)request.Filter.Grade)
                continue;

            string activityName = string.Empty;

            if (absence.Source == AbsenceSource.Offering)
            {
                OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

                if (offering is null)
                {
                    _logger.Warning("Could not find offering with Id {id}", offeringId);

                    continue;
                }

                activityName = offering.Name;
            }

            if (absence.Source == AbsenceSource.Tutorial)
            {
                TutorialId tutorialId = TutorialId.FromValue(absence.SourceId);

                Tutorial tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

                if (tutorial is null)
                {
                    _logger.Warning("Could not find tutorial with Id {id}", tutorialId);

                    continue;
                }

                activityName = tutorial.Name;
            }

            AbsenceExportResponse entry = new(
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                absence.Explained,
                absence.Type,
                absence.Date,
                absence.PeriodName,
                absence.AbsenceLength,
                absence.AbsenceTimeframe,
                activityName,
                absence.Notifications.Count,
                absence.Responses.Count);

            results.Add(entry);
        }

        results = results
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Timeframe)
            .ThenBy(a => a.SchoolName)
            .ThenBy(a => a.StudentGrade)
            .Where(a => a.Date.Year == DateTime.Now.Year)
            .ToList();

        return results;
    }
}
