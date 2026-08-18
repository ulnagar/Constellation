namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetEnrolmentOfferById;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetEnrolmentOfferByIdQueryHandler
: IQueryHandler<GetEnrolmentOfferByIdQuery, EnrolmentOfferDetailsResponse>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetEnrolmentOfferByIdQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentPeriodRepository periodRepository,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _offerRepository = offerRepository;
        _periodRepository = periodRepository;
        _logger = logger
            .ForContext<GetEnrolmentOfferByIdQuery>();
    }

    public async Task<Result<EnrolmentOfferDetailsResponse>> Handle(GetEnrolmentOfferByIdQuery request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentOfferByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to retrieve Offer");

            return Result.Failure<EnrolmentOfferDetailsResponse>(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Application? application = await _applicationRepository.GetApplicationById(offer.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentOfferByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                .Warning("Failed to retrieve Offer");

            return Result.Failure<EnrolmentOfferDetailsResponse>(EnrolmentApplicationErrors.NotFound(offer.ApplicationId));
        }

        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(offer.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentOfferByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(offer.PeriodId), true)
                .Warning("Failed to retrieve Offer");

            return Result.Failure<EnrolmentOfferDetailsResponse>(EnrolmentPeriodErrors.NotFound(offer.PeriodId));
        }

        return new EnrolmentOfferDetailsResponse(
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
            application.ApplicationReference,
            application.CurrentSchoolCode,
            application.CurrentSchool,
            application.DestinationSchoolCode,
            application.DestinationSchool,
            application.Program,
            application.Grade,
            offer.Response,
            offer.OfferedAt,
            offer.RespondBy,
            offer.RespondedAt,
            offer.HasCourtOrders,
            offer.HasHealthConcerns,
            offer.RequestedLaptop);
    }
}
