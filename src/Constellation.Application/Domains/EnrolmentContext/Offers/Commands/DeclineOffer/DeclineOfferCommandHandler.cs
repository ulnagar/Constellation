namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.DeclineOffer;

using Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Interfaces;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Serilog;

internal sealed class DeclineOfferCommandHandler
    : ICommandHandler<DeclineOfferCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public DeclineOfferCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<DeclineOfferCommand>();
    }

    public async Task<Result> Handle(DeclineOfferCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(DeclineOfferCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to decline Enrolment Offer");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result response = offer.Respond(OfferStatus.Declined);

        if (response.IsFailure)
        {
            _logger
                .ForContext(nameof(DeclineOfferCommand), request, true)
                .ForContext(nameof(Error), response.Error, true)
                .Warning("Failed to decline Enrolment Offer");

            return Result.Failure(response.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
