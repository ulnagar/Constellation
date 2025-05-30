﻿namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceResponseDetailsForSchool;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Students.Errors;
using Core.Shared;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceResponseDetailsForSchoolQueryHandler
    : IQueryHandler<GetAbsenceResponseDetailsForSchoolQuery, SchoolAbsenceResponseDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsenceResponseDetailsForSchoolQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsenceResponseDetailsForSchoolQuery>();
    }

    public async Task<Result<SchoolAbsenceResponseDetailsResponse>> Handle(GetAbsenceResponseDetailsForSchoolQuery request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure<SchoolAbsenceResponseDetailsResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Response response = absence.Responses.FirstOrDefault(response => response.Id == request.ResponseId);

        if (response is null)
        {
            _logger.Warning("Could not find response with Id {response_id} attached to absence with id {absence_id}", request.ResponseId, request.AbsenceId);

            return Result.Failure<SchoolAbsenceResponseDetailsResponse>(DomainErrors.Absences.Response.NotFound(request.ResponseId));
        }

        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {studentId}", absence.StudentId);

            return Result.Failure<SchoolAbsenceResponseDetailsResponse>(StudentErrors.NotFound(absence.StudentId));
        }

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        SchoolAbsenceResponseDetailsResponse entry = new(
            student.Name.DisplayName,
            offering?.Name,
            absence.Id,
            response.Id,
            absence.Date.ToDateTime(TimeOnly.MinValue),
            absence.PeriodName,
            absence.PeriodTimeframe,
            absence.AbsenceTimeframe,
            absence.AbsenceLength,
            response.Explanation);

        return entry;
    }
}
