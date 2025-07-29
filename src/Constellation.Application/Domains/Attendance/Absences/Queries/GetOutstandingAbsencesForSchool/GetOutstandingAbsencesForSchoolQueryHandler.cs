namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetOutstandingAbsencesForSchool;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Absences;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOutstandingAbsencesForSchoolQueryHandler
    : IQueryHandler<GetOutstandingAbsencesForSchoolQuery, List<OutstandingAbsencesForSchoolResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetOutstandingAbsencesForSchoolQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetOutstandingAbsencesForSchoolQuery>();
    }

    public async Task<Result<List<OutstandingAbsencesForSchoolResponse>>> Handle(GetOutstandingAbsencesForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<OutstandingAbsencesForSchoolResponse> results = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        if (students.Count == 0)
        {
            _logger.Information("No students found for school {code} when trying to retrieve outstanding absences", request.SchoolCode);

            return results;
        }

        foreach (Student student in students)
        {
            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<Absence> absences = await _absenceRepository.GetForStudentFromCurrentYear(student.Id, cancellationToken);

            if (absences.Count == 0)
                continue;

            foreach (Absence absence in absences)
            {
                if (absence.Explained)
                    continue;

                Response pendingResponse = absence
                    .Responses
                    .FirstOrDefault(response => 
                        response.VerificationStatus == ResponseVerificationStatus.Pending);

                Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                OutstandingAbsencesForSchoolResponse entry = new(
                    absence.Id,
                    student.Name.DisplayName,
                    enrolment.Grade,
                    absence.Type.Value,
                    absence.Date.ToDateTime(TimeOnly.MinValue),
                    absence.PeriodName,
                    absence.PeriodTimeframe,
                    absence.AbsenceLength,
                    absence.AbsenceTimeframe,
                    offering?.Name,
                    pendingResponse?.Id);

                results.Add(entry);
            }
        }

        return results;
    }
}
