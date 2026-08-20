namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferRejected;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Interfaces;
using Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferApproved;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Serilog;

internal sealed class MarkOfferRejectedCommandHandler
    : ICommandHandler<MarkOfferRejectedCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkOfferRejectedCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<MarkOfferRejectedCommand>();
    }

    public async Task<Result> Handle(MarkOfferRejectedCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(MarkOfferRejectedCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to mark Offer as Rejected");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result result = offer.MarkFinalApproval(false, request.Username);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferRejectedCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to mark Offer as Rejected");

            return result;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
