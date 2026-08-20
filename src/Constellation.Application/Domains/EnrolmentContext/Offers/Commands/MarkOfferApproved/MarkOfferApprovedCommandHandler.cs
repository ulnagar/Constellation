namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferApproved;

using Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferReviewCompleted;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class MarkOfferApprovedCommandHandler
: ICommandHandler<MarkOfferApprovedCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkOfferApprovedCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<MarkOfferApprovedCommand>();
    }

    public async Task<Result> Handle(MarkOfferApprovedCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(MarkOfferApprovedCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to mark Offer as Accepted");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result result = offer.MarkFinalApproval(true, request.Username);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferApprovedCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to mark Offer as Accepted");

            return result;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
