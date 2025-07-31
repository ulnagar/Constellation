namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForStudent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Identifiers;
using Core.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsForStudentQueryHandler
    : IQueryHandler<GetAbsenceDetailsForStudentQuery, AbsenceForStudentResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsForStudentQueryHandler(
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

        string activityName = string.Empty;

        if (absence.Source == AbsenceSource.Offering)
        {
            OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

            Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

            if (offering is null)
            {
                _logger.Warning("Could not find offering with Id {id}", offeringId);

                return Result.Failure<AbsenceForStudentResponse>(OfferingErrors.NotFound(offeringId));
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

                return Result.Failure<AbsenceForStudentResponse>(TutorialErrors.NotFound(tutorialId));
            }

            activityName = tutorial.Name;
        }

        Response response = absence.GetExplainedResponse();
        
        AbsenceForStudentResponse result = new(
            absence.Id,
            student.Name,
            student.Id,
            activityName,
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
