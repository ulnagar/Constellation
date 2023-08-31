namespace Constellation.Application.Absences.ConvertResponseToAbsenceExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
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
    private readonly ILogger _logger;

    public ConvertResponseToAbsenceExplanationCommandHandler(
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
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

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find offering with Id {id}", absence.OfferingId);

            return Result.Failure<AbsenceExplanation>(DomainErrors.Subjects.Offering.NotFound(absence.OfferingId));
        }

        string timeframe = string.Empty;
        if (absence.Type == AbsenceType.Partial)
            timeframe = absence.AbsenceTimeframe;

        return new AbsenceExplanation(
            absence.Date,
            absence.PeriodName,
            absence.PeriodTimeframe,
            offering.Name,
            timeframe,
            response.Explanation);
    }
}
