namespace Constellation.Application.Domains.Tutorials.Requests.Queries.CountRequestsPendingScheduling;

using Abstractions.Messaging;

public sealed record CountRequestsPendingSchedulingQuery()
    : IQuery<int>;