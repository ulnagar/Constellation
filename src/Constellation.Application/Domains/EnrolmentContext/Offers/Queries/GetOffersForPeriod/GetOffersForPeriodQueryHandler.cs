namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetOffersForPeriod;

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

internal sealed class GetOffersForPeriodQueryHandler
    : IQueryHandler<GetOffersForPeriodQuery, List<EnrolmentOfferResponse>>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetOffersForPeriodQueryHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentPeriodRepository periodRepository,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _applicationRepository = applicationRepository;
        _periodRepository = periodRepository;
        _logger = logger
            .ForContext<GetOffersForPeriodQuery>();
    }

    public async Task<Result<List<EnrolmentOfferResponse>>> Handle(GetOffersForPeriodQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentOfferResponse> responses = [];
        
        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(GetOffersForPeriodQuery), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to find Enrolment Period linked to Offer");

            return Result.Failure<List<EnrolmentOfferResponse>>(EnrolmentPeriodErrors.NotFound(request.PeriodId));
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
                application.Id,
                period.Id,
                period.Label,
                application.StudentReferenceNumber,
                application.StudentName,
                application.StudentGender,
                application.ParentName,
                application.ParentEmailAddress,
                application.ParentPhoneNumber,
                application.ApplicationReference ?? string.Empty,
                application.CurrentSchoolCode,
                application.CurrentSchool ?? string.Empty,
                application.DestinationSchoolCode,
                application.DestinationSchool ?? string.Empty,
                application.Program,
                application.Grade,
                offer.Response));
        }

        return responses;
    }
}
