namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.RecordParentResponseToOffer;

using Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Interfaces;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Abstractions.Services;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentParent.Errors;
using Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentParent.Repositories;
using Core.Shared;
using Serilog;

internal sealed class RecordParentResponseToOfferCommandHandler
    : ICommandHandler<RecordParentResponseToOfferCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentParentRepository _parentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RecordParentResponseToOfferCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentParentRepository parentRepository,
        ICurrentUserService currentUserService,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _applicationRepository = applicationRepository;
        _parentRepository = parentRepository;
        _currentUserService = currentUserService;
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

        Guid? parentUserId = await _currentUserService.GetUserId();

        if (parentUserId is null)
        {
            _logger
                .ForContext(nameof(RecordParentResponseToOfferCommand), request, true)
                .ForContext(nameof(Error), EnrolmentParentErrors.InvalidId, true)
                .Warning("Failed to update Offer with parent response");

            return Result.Failure(EnrolmentParentErrors.InvalidId);
        }

        EnrolmentParentId parentId = EnrolmentParentId.FromValue(parentUserId.Value);

        EnrolmentParent? parent = await _parentRepository.GetById(parentId, cancellationToken);

        if (parent is null)
        {
            _logger
                .ForContext(nameof(RecordParentResponseToOfferCommand), request, true)
                .ForContext(nameof(Error), EnrolmentParentErrors.NotFound(parentId), true)
                .Warning("Failed to update Offer with parent response");

            return Result.Failure(EnrolmentParentErrors.NotFound(parentId));
        }

        Application? application = await _applicationRepository.GetApplicationById(offer.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(RecordParentResponseToOfferCommand), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                .Warning("Failed to update Offer with parent response");

            return Result.Failure(EnrolmentApplicationErrors.NotFound(offer.ApplicationId));
        }

        if (application.Parents.All(entry => entry.Id != parentId))
        {
            _logger
                .ForContext(nameof(RecordParentResponseToOfferCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.InvalidParent(parentId), true)
                .Warning("Failed to update Offer with parent response");

            return Result.Failure(EnrolmentOfferErrors.InvalidParent(parentId));
        }
        
        Result update = offer.Respond(request.Response, parent.Id, request.CourtOrder, request.HealthConditions, request.RequestLaptop);

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
