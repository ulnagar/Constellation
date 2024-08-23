namespace Constellation.Application.Absences.GetAbsenceDetailsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsForStudentQueryHandler
    : IQueryHandler<GetAbsenceDetailsForStudentQuery, AbsenceForStudentResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsForStudentQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsenceDetailsForStudentQuery>();
    }

    public async Task<Result<AbsenceForStudentResponse>> Handle(GetAbsenceDetailsForStudentQuery request, CancellationToken cancellationToken) 
    { 
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure<AbsenceForStudentResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }
        
        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {id}", absence.StudentId);

            return Result.Failure<AbsenceForStudentResponse>(StudentErrors.NotFound(absence.StudentId));
        }

        Name? studentName = student.GetName();

        if (studentName is null)
        {
            _logger.Warning("Could not create student name from {@student}", student);

            return Result.Failure<AbsenceForStudentResponse>(new("ValueObjects.Name.NotCreated", "Could not create name object"));
        }

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find offering with Id {id}", absence.OfferingId);

            return Result.Failure<AbsenceForStudentResponse>(OfferingErrors.NotFound(absence.OfferingId));
        }

        Response response = absence.GetExplainedResponse();
        
        AbsenceForStudentResponse result = new(
            absence.Id,
            studentName,
            student.StudentId,
            offering.Id,
            offering.Name,
            absence.Date,
            absence.Type,
            absence.PeriodName,
            absence.PeriodTimeframe,
            absence.AbsenceTimeframe,
            absence.AbsenceLength,
            absence.AbsenceReason.Value,
            response?.Explanation,
            response?.VerificationStatus,
            response is null ? null : response.VerificationStatus == ResponseVerificationStatus.NotRequired ? response.From : response.Verifier,
            absence.Explained);

        return result;
    }
}
