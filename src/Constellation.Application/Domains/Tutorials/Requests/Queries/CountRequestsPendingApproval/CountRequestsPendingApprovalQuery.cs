namespace Constellation.Application.Domains.Tutorials.Requests.Queries.CountRequestsPendingApproval;

using Abstractions.Messaging;

public sealed record CountRequestsPendingApprovalQuery()
    : IQuery<int>;
