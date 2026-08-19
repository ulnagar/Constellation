namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.AddOfferNote;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Interfaces;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Shared;
using Serilog;

internal sealed class AddOfferNoteCommandHandler
    : ICommandHandler<AddOfferNoteCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddOfferNoteCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddOfferNoteCommand>();
    }
    public async Task<Result> Handle(AddOfferNoteCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(AddOfferNoteCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to add Note to Offer");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Result result = offer.AddReviewNote(request.Note, request.CreatedBy);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddOfferNoteCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add Note to Offer");

            return result;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
