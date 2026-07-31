namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferPending;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class MarkOfferPendingCommandHandler
: ICommandHandler<MarkOfferPendingCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkOfferPendingCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<MarkOfferPendingCommand>();
    }

    public async Task<Result> Handle(MarkOfferPendingCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(MarkOfferPendingCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to send new Offer email to parent");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        // Update the Offer with the new dates
        Result update = offer.MarkOffered(DateTimeOffset.Now);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferPendingCommand), request, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to send new Offer email to parent");

            return Result.Failure(update.Error);
        }

        // Save the changes
        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return Result.Success();
    }
}
