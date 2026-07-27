namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.CreateOfferFromApplication;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class CreateOfferFromApplicationCommandHandler
: ICommandHandler<CreateOfferFromApplicationCommand, OfferId>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateOfferFromApplicationCommandHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateOfferFromApplicationCommand>();
    }

    public async Task<Result<OfferId>> Handle(CreateOfferFromApplicationCommand request, CancellationToken cancellationToken)
    {
        Application? application = await _applicationRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(CreateOfferFromApplicationCommand), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to convert Enrolment Application to Error");

            return Result.Failure<OfferId>(EnrolmentApplicationErrors.NotFound(request.ApplicationId));
        }

        Result<Offer> offer = Offer.Create(application);

        if (offer.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateOfferFromApplicationCommand), request, true)
                .ForContext(nameof(Error), offer.Error, true)
                .Warning("Failed to convert Enrolment Application to Error");

            return Result.Failure<OfferId>(offer.Error);
        }

        Result applicationUpdate = application.UpdateStatus(ApplicationStatus.Offered);

        if (applicationUpdate.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateOfferFromApplicationCommand), request, true)
                .ForContext(nameof(Error), applicationUpdate.Error, true)
                .Warning("Failed to convert Enrolment Application to Error");

            return Result.Failure<OfferId>(applicationUpdate.Error);
        }

        _offerRepository.Insert(offer.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return offer.Value.Id;
    }
}
