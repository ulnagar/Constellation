namespace Constellation.Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Core.Models.Offerings.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ConvertAbsenceToAbsenceEntryCommandHandler
    : ICommandHandler<ConvertAbsenceToAbsenceEntryCommand, AbsenceEntry>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public ConvertAbsenceToAbsenceEntryCommandHandler(
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _logger = logger.ForContext<ConvertAbsenceToAbsenceEntryCommand>();
    }

    public async Task<Result<AbsenceEntry>> Handle(ConvertAbsenceToAbsenceEntryCommand request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure<AbsenceEntry>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        string activityName = string.Empty;

        if (absence.Source == AbsenceSource.Offering)
        {
            OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

            Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

            if (offering is null)
            {
                _logger.Warning("Could not find offering with Id {id}", offeringId);

                return Result.Failure<AbsenceEntry>(OfferingErrors.NotFound(offeringId));
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

                return Result.Failure<AbsenceEntry>(TutorialErrors.NotFound(tutorialId));
            }

            activityName = tutorial.Name;
        }

        string timeframe = string.Empty;
        if (absence.Type == AbsenceType.Partial)
            timeframe = absence.AbsenceTimeframe;

        return new AbsenceEntry(
            absence.Id,
            absence.Date,
            absence.PeriodName,
            absence.PeriodTimeframe,
            activityName,
            timeframe,
            absence.AbsenceLength);
    }
}
