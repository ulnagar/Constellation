namespace Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ConvertAbsenceToAbsenceEntryCommandHandler
    : ICommandHandler<ConvertAbsenceToAbsenceEntryCommand, AbsenceEntry>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public ConvertAbsenceToAbsenceEntryCommandHandler(
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
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

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find offering with Id {id}", absence.OfferingId);

            return Result.Failure<AbsenceEntry>(DomainErrors.Subjects.Offering.NotFound(absence.OfferingId));
        }

        string timeframe = string.Empty;
        if (absence.Type == AbsenceType.Partial)
            timeframe = absence.AbsenceTimeframe;

        return new AbsenceEntry(
            absence.Id,
            absence.Date,
            absence.PeriodName,
            absence.PeriodTimeframe,
            offering.Name,
            timeframe);
    }
}
