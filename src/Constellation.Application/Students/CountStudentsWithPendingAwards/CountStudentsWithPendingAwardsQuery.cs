namespace Constellation.Application.Students.CountStudentsWithPendingAwards;

using Abstractions.Messaging;

public sealed record CountStudentsWithPendingAwardsQuery()
    : IQuery<int>;