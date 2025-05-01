namespace Constellation.Application.Domains.Students.Queries.CountStudentsWithPendingAwards;

using Abstractions.Messaging;

public sealed record CountStudentsWithPendingAwardsQuery()
    : IQuery<int>;