namespace Constellation.Application.Domains.Offerings.Commands.AddMultipleSessionsToOffering;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Timetables;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddMultipleSessionsToOfferingCommandHandler
    : ICommandHandler<AddMultipleSessionsToOfferingCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddMultipleSessionsToOfferingCommandHandler(
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddMultipleSessionsToOfferingCommand>();
    }

    public async Task<Result> Handle(AddMultipleSessionsToOfferingCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(AddMultipleSessionsToOfferingCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Failed to add multiple Sessions to Offering");

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        foreach (PeriodId periodId in request.PeriodIds)
        {
            Period period = await _periodRepository.GetById(periodId, cancellationToken);

            if (period is null)
            {
                Error error = new("Subjects.Periods.NotFound", $"Could not find Period with Id {periodId}");

                _logger
                    .ForContext(nameof(AddMultipleSessionsToOfferingCommand), request, true)
                    .ForContext(nameof(Error), error, true)
                    .Warning("Failed to add multiple Sessions to Offering");

                return Result.Failure(error);
            }

            if (offering.Sessions.Any(session => !session.IsDeleted && session.PeriodId == periodId))
            {
                // Existing non-deleted session for this period found

                continue;
            }

            offering.AddSession(periodId);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}