namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetOfferForResponse;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetOfferForResponseQueryHandler
: IQueryHandler<GetOfferForResponseQuery, OfferResponse>
{
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly ILogger _logger;

    public GetOfferForResponseQueryHandler(
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentApplicationRepository applicationRepository,
        ILogger logger)
    {
        _offerRepository = offerRepository;
        _applicationRepository = applicationRepository;
        _logger = logger
            .ForContext<GetOfferForResponseQuery>();
    }

    public async Task<Result<OfferResponse>> Handle(GetOfferForResponseQuery request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(GetOfferForResponseQuery), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to retrieve Offer for response");

            return Result.Failure<OfferResponse>(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Application? application = await _applicationRepository.GetApplicationById(offer.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetOfferForResponseQuery), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                .Warning("Failed to retrieve Offer for response");

            return Result.Failure<OfferResponse>(EnrolmentApplicationErrors.NotFound(offer.ApplicationId));
        }

        return new OfferResponse(
            offer.Id,
            offer.ApplicationId,
            offer.PeriodId,
            application.StudentName,
            application.Grade,
            application.Program,
            application.SelectedCourses.Where(entry => entry.Status == CourseSelectionStatus.Approved).ToList(),
            offer.Status,
            offer.OfferedAt,
            offer.RespondBy,
            offer.RespondedAt,
            offer.HasCourtOrders,
            offer.HasHealthConcerns,
            offer.RequestedLaptop);
    }
}
