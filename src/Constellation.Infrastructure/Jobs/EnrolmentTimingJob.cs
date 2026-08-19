namespace Constellation.Infrastructure.Jobs;

using Application.Domains.EnrolmentContext.Interfaces;
using Application.Interfaces.Jobs;
using Application.Interfaces.Services;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Repositories;
using System;

internal sealed class EnrolmentTimingJob : IEnrolmentTimingJob
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEmailService _emailService;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EnrolmentTimingJob(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentApplicationRepository applicationRepository,
        IEmailService emailService,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _periodRepository = periodRepository;
        _applicationRepository = applicationRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<IEnrolmentTimingJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        List<Offer> offers = await _offerRepository.GetAll(cancellationToken);
        List<EnrolmentPeriod> periods = await _periodRepository.GetAllEnrolmentPeriods(cancellationToken);
        List<Application> applications = await _applicationRepository.GetAll(cancellationToken);

        foreach (Offer offer in offers)
        {
            if (offer.Status != OfferStatus.AwaitingResponse)
                continue;

            EnrolmentPeriod? period = periods.FirstOrDefault(entry => entry.Id == offer.PeriodId);
            Application? application = applications.FirstOrDefault(entry => entry.Id == offer.ApplicationId);

            if (period is null || application is null)
            {
                _logger
                    .ForContext(nameof(Offer), offer, true)
                    .Warning("Unable to find supporting data for offer");

                continue;
            }

            if (offer.IsReminderDue(DateTimeOffset.Now))
            {
                _logger
                    .ForContext(nameof(Offer), offer, true)
                    .Information("Sending reminder for {user}", application.StudentName.DisplayName);

                await _emailService.SendEnrolmentOfferReminder(application, offer, period.Year, cancellationToken);
                offer.MarkReminderSent(DateTimeOffset.Now);
            }

            if (offer.RespondBy.HasValue && offer.RespondBy < DateTimeOffset.Now)
            {
                _logger
                    .ForContext(nameof(Offer), offer, true)
                    .Information("Offer for {user} lapsed", application.StudentName.DisplayName);

                offer.MarkLapsed();
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
