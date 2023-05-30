namespace Constellation.Application.Absences.GetAbsenceResponseDetailsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceResponseDetailsForSchoolQueryHandler
    : IQueryHandler<GetAbsenceResponseDetailsForSchoolQuery, SchoolAbsenceResponseDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsenceResponseDetailsForSchoolQueryHandler(
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IStudentRepository studentRepository,
        ICourseOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsenceResponseDetailsForSchoolQuery>();
    }

    public async Task<Result<SchoolAbsenceResponseDetailsResponse>> Handle(GetAbsenceResponseDetailsForSchoolQuery request, CancellationToken cancellationToken)
    {
        var absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure<SchoolAbsenceResponseDetailsResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        var response = await _responseRepository.GetById(request.ResponseId, cancellationToken);

        if (response is null)
        {
            _logger.Warning("Could not find response with Id {response_id} attached to absence with id {absence_id}", request.ResponseId, request.AbsenceId);

            return Result.Failure<SchoolAbsenceResponseDetailsResponse>(DomainErrors.Absences.Response.NotFound(request.ResponseId));
        }

        var student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {studentId}", absence.StudentId);

            return Result.Failure<SchoolAbsenceResponseDetailsResponse>(DomainErrors.Partners.Student.NotFound(absence.StudentId));
        }

        var nameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

        var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        var entry = new SchoolAbsenceResponseDetailsResponse(
            nameRequest.Value,
            offering?.Name,
            absence.Id,
            absence.Date,
            absence.PeriodName,
            absence.PeriodTimeframe,
            absence.AbsenceTimeframe,
            absence.AbsenceLength,
            response.Explanation);

        return entry;
    }
}
