namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.SendOfferEmailToParent;

using Abstractions.Messaging;
using Application.Interfaces.Services;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class SendOfferEmailToParentCommandHandler
: ICommandHandler<SendOfferEmailToParentCommand>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendOfferEmailToParentCommandHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _applicationRepository = applicationRepository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger
            .ForContext<SendOfferEmailToParentCommand>();
    }

    public async Task<Result> Handle(SendOfferEmailToParentCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(SendOfferEmailToParentCommand), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to send new Offer email to parent");

            return Result.Failure(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Application? application = await _applicationRepository.GetApplicationById(offer.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(SendOfferEmailToParentCommand), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                .Warning("Failed to send new Offer email to parent");

            return Result.Failure(EnrolmentApplicationErrors.NotFound(offer.ApplicationId));
        }

        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(offer.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(SendOfferEmailToParentCommand), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(offer.PeriodId), true)
                .Warning("Failed to send new Offer email to parent");

            return Result.Failure(EnrolmentPeriodErrors.NotFound(offer.PeriodId));
        }

        // Update the Offer with the new dates
        Result update = offer.MarkOffered(DateTimeOffset.Now);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(SendOfferEmailToParentCommand), request, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to send new Offer email to parent");

            return Result.Failure(update.Error);
        }

        // Save the changes
        await _unitOfWork.CompleteAsync(cancellationToken);


        // Send the email
        Result email = await _emailService.SendEnrolmentOfferNotification(
            application,
            offer,
            period.Year,
            cancellationToken);

        if (email.IsFailure)
        {
            _logger
                .ForContext(nameof(SendOfferEmailToParentCommand), request, true)
                .ForContext(nameof(Error), email.Error, true)
                .Warning("Failed to send new Offer email to parent");

            return Result.Failure(email.Error);
        }


        return Result.Success();
    }
}
