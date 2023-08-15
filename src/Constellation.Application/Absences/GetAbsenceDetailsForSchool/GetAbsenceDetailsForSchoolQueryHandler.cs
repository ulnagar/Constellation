namespace Constellation.Application.Absences.GetAbsenceDetailsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsForSchoolQueryHandler
    : IQueryHandler<GetAbsenceDetailsForSchoolQuery, SchoolAbsenceDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsForSchoolQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        ICourseOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsenceDetailsForSchoolQuery>();
    }

    public async Task<Result<SchoolAbsenceDetailsResponse>> Handle(GetAbsenceDetailsForSchoolQuery request, CancellationToken cancellationToken)
    {
        var absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with id {id} when trying to retrieve for schools portal", request.AbsenceId);

            return Result.Failure<SchoolAbsenceDetailsResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        var student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with id {student_id} when trying to retrieve absence {@absence}", absence.StudentId, absence);

            return Result.Failure<SchoolAbsenceDetailsResponse>(DomainErrors.Partners.Staff.NotFound(absence.StudentId));
        }

        var nameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

        var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        var entry = new SchoolAbsenceDetailsResponse(
            nameRequest.Value.DisplayName,
            offering?.Name,
            absence.Id,
            absence.Date.ToDateTime(TimeOnly.MinValue),
            absence.PeriodName,
            absence.AbsenceTimeframe);

        return entry;
    }
}
