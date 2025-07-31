namespace Constellation.Application.Domains.Attendance.Absences.Commands.ConvertResponseToAbsenceExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Models.Tutorials.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ConvertResponseToAbsenceExplanationCommandHandler
    : ICommandHandler<ConvertResponseToAbsenceExplanationCommand, AbsenceExplanation>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public ConvertResponseToAbsenceExplanationCommandHandler(
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _logger = logger.ForContext<ConvertResponseToAbsenceExplanationCommand>();
    }

    public async Task<Result<AbsenceExplanation>> Handle(ConvertResponseToAbsenceExplanationCommand request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure<AbsenceExplanation>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Response response = absence.Responses.FirstOrDefault(response => response.Id == request.ResponseId);

        if (response is null)
        {
            _logger.Warning("Could not find absence response with Id {id}", request.ResponseId);

            return Result.Failure<AbsenceExplanation>(DomainErrors.Absences.Response.NotFound(request.ResponseId));
        }

        string activityName = string.Empty;

        if (absence.Source == AbsenceSource.Offering)
        {
            OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

            Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

            if (offering is null)
            {
                _logger.Warning("Could not find offering with Id {id}", offeringId);

                return Result.Failure<AbsenceExplanation>(OfferingErrors.NotFound(offeringId));
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

                return Result.Failure<AbsenceExplanation>(TutorialErrors.NotFound(tutorialId));
            }

            activityName = tutorial.Name;
        }

        string timeframe = string.Empty;
        if (absence.Type == AbsenceType.Partial)
            timeframe = absence.AbsenceTimeframe;

        return new AbsenceExplanation(
            absence.Date,
            absence.PeriodName,
            absence.PeriodTimeframe,
            activityName,
            timeframe,
            response.Explanation);
    }
}
