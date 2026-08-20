namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferDocumentsCollected;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class MarkOfferDocumentsCollectedCommandHandler
: ICommandHandler<MarkOfferDocumentsCollectedCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkOfferDocumentsCollectedCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<MarkOfferDocumentsCollectedCommand>();
    }

    public async Task<Result> Handle(MarkOfferDocumentsCollectedCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(MarkOfferDocumentsCollectedCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to mark Offer as Documents Collected");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result result = offer.MarkDocumentsCollected(request.Username);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferDocumentsCollectedCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to mark Offer as Documents Collected");

            return result;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
