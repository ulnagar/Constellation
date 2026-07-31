namespace Constellation.Application.Domains.EnrolmentContext.Offers.Events.EnrolmentOfferGenerated;

using Application.Interfaces.Services;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.Application.Errors;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Constellation.Core.Models.EnrolmentContext.Offer;
using Constellation.Core.Models.EnrolmentContext.Offer.Errors;
using Constellation.Core.Shared;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.EnrolmentContext.Offer.Events;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Serilog;

internal class SendOfferNotificationEmailHandler
    : IDomainEventHandler<EnrolmentOfferGeneratedDomainEvent>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendOfferNotificationEmailHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentOfferRepository offerRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _periodRepository = periodRepository;
        _offerRepository = offerRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<EnrolmentOfferGeneratedDomainEvent>();
    }

    public async Task Handle(EnrolmentOfferGeneratedDomainEvent notification, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(notification.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(EnrolmentOfferGeneratedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(notification.OfferId), true)
                .Warning("Failed to send new Offer email to parent");

            return;
        }

        Application? application = await _applicationRepository.GetApplicationById(offer.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(EnrolmentOfferGeneratedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                .Warning("Failed to send new Offer email to parent");

            return;
        }

        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(offer.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(EnrolmentOfferGeneratedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(offer.PeriodId), true)
                .Warning("Failed to send new Offer email to parent");

            return;
        }

        // Send the email
        Result email = await _emailService.SendEnrolmentOfferNotification(
            application,
            offer,
            period.Year,
            cancellationToken);

        if (email.IsFailure)
        {
            _logger
                .ForContext(nameof(EnrolmentOfferGeneratedDomainEvent), notification, true)
                .ForContext(nameof(Error), email.Error, true)
                .Warning("Failed to send new Offer email to parent");
        }
    }
}
