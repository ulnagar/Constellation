namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferAcceptedByStaff;

using Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferDeclinedByStaff;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class MarkOfferAcceptedByStaffCommandHandler
: ICommandHandler<MarkOfferAcceptedByStaffCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkOfferAcceptedByStaffCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<MarkOfferAcceptedByStaffCommand>();
    }

    public async Task<Result> Handle(MarkOfferAcceptedByStaffCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(MarkOfferAcceptedByStaffCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to mark Offer as accepted");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result declined = offer.Respond(OfferResponse.Accepted, request.CourtOrder, request.HealthConditions);

        if (declined.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferAcceptedByStaffCommand), request, true)
                .ForContext(nameof(Error), declined.Error, true)
                .Warning("Failed to mark Offer as accepted");

            return Result.Failure(declined.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
