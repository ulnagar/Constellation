namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.RecordParentResponseToOffer;

using Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Interfaces;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Serilog;

internal sealed class RecordParentResponseToOfferCommandHandler
    : ICommandHandler<RecordParentResponseToOfferCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RecordParentResponseToOfferCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RecordParentResponseToOfferCommand>();
    }

    public async Task<Result> Handle(RecordParentResponseToOfferCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(RecordParentResponseToOfferCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to update Offer with parent response");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result update = offer.Respond(request.Response, request.CourtOrder, request.HealthConditions, request.RequestLaptop);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(RecordParentResponseToOfferCommand), request, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update Offer with parent response");

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
