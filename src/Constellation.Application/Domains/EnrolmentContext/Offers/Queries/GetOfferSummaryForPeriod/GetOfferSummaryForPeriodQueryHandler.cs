namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetOfferSummaryForPeriod;

using Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Offers.Models;
using Constellation.Core.Models.EnrolmentContext.Application.Repositories;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.Offer;
using Core.Shared;
using Serilog;
using System.Collections.Generic;

internal sealed class GetOfferSummaryForPeriodQueryHandler
    : IQueryHandler<GetOfferSummaryForPeriodQuery, List<EnrolmentOfferSummaryResponse>>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetOfferSummaryForPeriodQueryHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentPeriodRepository periodRepository,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _applicationRepository = applicationRepository;
        _periodRepository = periodRepository;
        _logger = logger
            .ForContext<GetOfferSummaryForPeriodQuery>();
    }

    public async Task<Result<List<EnrolmentOfferSummaryResponse>>> Handle(GetOfferSummaryForPeriodQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentOfferSummaryResponse> responses = [];
        
        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(GetOfferSummaryForPeriodQuery), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to find Enrolment Period linked to Offer");

            return Result.Failure<List<EnrolmentOfferSummaryResponse>>(EnrolmentPeriodErrors.NotFound(request.PeriodId));
        }

        List<Offer> offers = await _offerRepository.GetForPeriod(request.PeriodId, cancellationToken);
        
        List<Application> applications = await _applicationRepository.GetApplicationsByPeriod(request.PeriodId, cancellationToken);

        foreach (Offer offer in offers)
        {
            Application? application = applications.FirstOrDefault(entry => entry.Id == offer.ApplicationId);

            if (application is null)
            {
                _logger
                    .ForContext(nameof(Offer), offer, true)
                    .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                    .Warning("Failed to find Enrolment Application linked to Offer");

                continue;
            }

            responses.Add(new(
                offer.Id,
                application.StudentName,
                application.StudentGender,
                application.ApplicationReference ?? string.Empty,
                application.DestinationSchoolCode,
                application.DestinationSchool ?? string.Empty,
                application.Grade,
                offer.Status,
                offer.Response));
        }

        return responses;
    }
}
