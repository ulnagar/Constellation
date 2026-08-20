namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.CountOffersInPendingAcceptanceStatus;

using Abstractions.Messaging;

public sealed record CountOffersInPendingAcceptanceStatusQuery()
    : IQuery<int>;
