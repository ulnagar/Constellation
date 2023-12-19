namespace Constellation.Application.Students.CountStudentsWithoutSentralId;

using Abstractions.Messaging;

public sealed record CountStudentsWithoutSentralIdQuery()
    : IQuery<int>;