namespace Constellation.Application.Absences.GetAbsencesForExport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IAbsenceNotificationRepository _notificationRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsencesForExportQueryHandler(
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IAbsenceNotificationRepository notificationRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ICourseOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _notificationRepository = notificationRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsencesForExportQuery>();
    }

    public async Task<Result<List<AbsenceExportResponse>>> Handle(GetAbsencesForExportQuery request, CancellationToken cancellationToken)
    {
        var absences = await _absenceRepository.GetAllFromCurrentYear(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Filter.StudentId))
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

        foreach (var absence in absences)
        {
            var student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.Filter.SchoolCode) && student.SchoolCode != request.Filter.SchoolCode)
                continue;

            if (request.Filter.Grade.HasValue && student.CurrentGrade != (Grade)request.Filter.Grade)
                continue;

            var school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            var studentNameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

            var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            var notifications = await _notificationRepository.GetCountForAbsence(absence.Id, cancellationToken);

            var responses = await _responseRepository.GetCountForAbsence(absence.Id, cancellationToken);

            var entry = new AbsenceExportResponse(
                studentNameRequest.Value,
                student.CurrentGrade,
                school.Name,
                absence.Explained,
                absence.Type,
                absence.Date,
                absence.PeriodName,
                absence.AbsenceLength,
                absence.AbsenceTimeframe,
                offering.Name,
                notifications,
                responses);

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
