namespace Constellation.Application.Domains.Students.Queries.CountStudentsWithoutSentralId;

using Abstractions.Messaging;

public sealed record CountStudentsWithoutSentralIdQuery()
    : IQuery<int>;