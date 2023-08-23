namespace Constellation.Application.Absences.GetAbsenceForStudentResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceForStudentResponseQueryHandler
    : IQueryHandler<GetAbsenceForStudentResponseQuery, AbsenceForStudentResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsenceForStudentResponseQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsenceForStudentResponseQuery>();
    }

    public async Task<Result<AbsenceForStudentResponse>> Handle(GetAbsenceForStudentResponseQuery request, CancellationToken cancellationToken) 
    { 
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure<AbsenceForStudentResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        if (absence.Explained)
        {
            return Result.Failure<AbsenceForStudentResponse>(DomainErrors.Absences.Absence.AlreadyExplained);
        }

        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {id}", absence.StudentId);

            return Result.Failure<AbsenceForStudentResponse>(DomainErrors.Partners.Student.NotFound(absence.StudentId));
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

            return Result.Failure<AbsenceForStudentResponse>(DomainErrors.Subjects.Offering.NotFound(absence.OfferingId));
        }

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
            absence.AbsenceLength);

        return result;
    }
}
