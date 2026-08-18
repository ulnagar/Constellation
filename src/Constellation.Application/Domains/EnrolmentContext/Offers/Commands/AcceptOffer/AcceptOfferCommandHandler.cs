namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.AcceptOffer;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class AcceptOfferCommandHandler
: ICommandHandler<AcceptOfferCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AcceptOfferCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AcceptOfferCommand>();
    }

    public async Task<Result> Handle(AcceptOfferCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(AcceptOfferCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to accept Enrolment Offer");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result response = offer.Respond(OfferResponse.Accepted, request.HasCourtOrders, request.HasHealthConditions);

        if (response.IsFailure)
        {
            _logger
                .ForContext(nameof(AcceptOfferCommand), request, true)
                .ForContext(nameof(Error), response.Error, true)
                .Warning("Failed to accept Enrolment Offer");

            return Result.Failure(response.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
