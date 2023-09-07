namespace Constellation.Application.Offerings.UpdateOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateOfferingCommandHandler
    : ICommandHandler<UpdateOfferingCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateOfferingCommandHandler(
        IOfferingRepository offeringRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateOfferingCommand>();
    }

    public async Task<Result> Handle(UpdateOfferingCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(UpdateOfferingCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Failed to update Offering");

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        if (request.StartDate > request.EndDate)
        {
            _logger
                .ForContext(nameof(UpdateOfferingCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.Validation.StartDateAfterEndDate, true)
                .Warning("Failed to update Offering");

            return Result.Failure(OfferingErrors.Validation.StartDateAfterEndDate);
        }

        if (request.EndDate < _dateTime.Today)
        {
            _logger
                .ForContext(nameof(UpdateOfferingCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.Validation.EndDateInPast, true)
                .Warning("Failed to update Offering");

            return Result.Failure<OfferingId>(OfferingErrors.Validation.EndDateInPast);
        }

        offering.Update(request.StartDate, request.EndDate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
