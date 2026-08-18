namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferDeclinedByStaff;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Interfaces;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Shared;
using Serilog;

internal sealed class MarkOfferDeclinedByStaffCommandHandler
    : ICommandHandler<MarkOfferDeclinedByStaffCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkOfferDeclinedByStaffCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<MarkOfferDeclinedByStaffCommand>();
    }

    public async Task<Result> Handle(MarkOfferDeclinedByStaffCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(MarkOfferDeclinedByStaffCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to mark Offer as declined");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result declined = offer.Respond(OfferResponse.Declined);

        if (declined.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferDeclinedByStaffCommand), request, true)
                .ForContext(nameof(Error), declined.Error, true)
                .Warning("Failed to mark Offer as declined");

            return Result.Failure(declined.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
