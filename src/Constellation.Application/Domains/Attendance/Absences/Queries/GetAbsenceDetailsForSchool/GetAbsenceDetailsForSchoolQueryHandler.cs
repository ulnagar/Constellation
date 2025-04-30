namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForSchool;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Absences;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsForSchoolQueryHandler
    : IQueryHandler<GetAbsenceDetailsForSchoolQuery, SchoolAbsenceDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsForSchoolQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsenceDetailsForSchoolQuery>();
    }

    public async Task<Result<SchoolAbsenceDetailsResponse>> Handle(GetAbsenceDetailsForSchoolQuery request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with id {id} when trying to retrieve for schools portal", request.AbsenceId);

            return Result.Failure<SchoolAbsenceDetailsResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with id {student_id} when trying to retrieve absence {@absence}", absence.StudentId, absence);

            return Result.Failure<SchoolAbsenceDetailsResponse>(StudentErrors.NotFound(absence.StudentId));
        }

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        SchoolAbsenceDetailsResponse entry = new(
            student.Name,
            offering?.Name,
            absence.Id,
            absence.Date.ToDateTime(TimeOnly.MinValue),
            absence.PeriodName,
            absence.AbsenceTimeframe);

        return entry;
    }
}
