namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetChartDataForEnrolmentStatus;

using Abstractions.Messaging;

public sealed class GetChartDataForEnrolmentStatusQuery()
    : IQuery<List<ChartResponse>>;