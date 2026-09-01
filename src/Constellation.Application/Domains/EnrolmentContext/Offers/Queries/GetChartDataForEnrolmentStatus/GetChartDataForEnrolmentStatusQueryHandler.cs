namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetChartDataForEnrolmentStatus;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Shared;

internal sealed class GetChartDataForEnrolmentStatusQueryHandler
: IQueryHandler<GetChartDataForEnrolmentStatusQuery, List<ChartResponse>>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentOfferRepository _offerRepository;

    public GetChartDataForEnrolmentStatusQueryHandler(
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentOfferRepository offerRepository)
    {
        _periodRepository = periodRepository;
        _offerRepository = offerRepository;
    }

    public async Task<Result<List<ChartResponse>>> Handle(GetChartDataForEnrolmentStatusQuery request, CancellationToken cancellationToken)
    {
        List<ChartResponse> response = [];

        List<EnrolmentPeriod> periods = await _periodRepository.GetCurrentYearEnrolmentPeriods(cancellationToken);

        foreach (EnrolmentPeriod period in periods)
        {
            List<Offer> offers = await _offerRepository.GetForPeriod(period.Id, cancellationToken);

            Dictionary<string, int> countByStatus = offers.GroupBy(entry => entry.Status)
                .ToDictionary(
                    entry => entry.Key.ToString(), 
                    entry => entry.Count());

            if (countByStatus.Count == 0)
                continue;

            response.Add(new (
                period.Id,
                period.Label,
                countByStatus));
        }

        return response;
    }
}
