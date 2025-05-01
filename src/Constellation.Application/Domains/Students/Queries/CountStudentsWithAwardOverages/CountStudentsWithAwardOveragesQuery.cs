namespace Constellation.Application.Domains.Students.Queries.CountStudentsWithAwardOverages;

using Constellation.Application.Abstractions.Messaging;

public sealed record CountStudentsWithAwardOveragesQuery()
    : IQuery<int>;