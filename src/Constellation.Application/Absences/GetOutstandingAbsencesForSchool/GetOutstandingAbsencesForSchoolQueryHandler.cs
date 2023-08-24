namespace Constellation.Application.Absences.GetOutstandingAbsencesForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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

        var students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        if (students.Count == 0)
        {
            _logger.Information("No students found for school {code} when trying to retrieve outstanding absences", request.SchoolCode);

            return results;
        }

        foreach (var student in students)
        {
            var nameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

            if (nameRequest.IsFailure)
            {
                _logger.Warning("Could not create name for student {@student}", student);

                continue;
            }

            var absences = await _absenceRepository.GetForStudentFromCurrentYear(student.StudentId, cancellationToken);

            if (absences.Count == 0)
                continue;

            foreach (var absence in absences)
            {
                if (absence.Explained)
                    continue;

                var pendingResponse = absence
                    .Responses
                    .FirstOrDefault(response => 
                        response.VerificationStatus == Core.Models.Absences.ResponseVerificationStatus.Pending);

                var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                var entry = new OutstandingAbsencesForSchoolResponse(
                    absence.Id,
                    nameRequest.Value.DisplayName,
                    student.CurrentGrade,
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
