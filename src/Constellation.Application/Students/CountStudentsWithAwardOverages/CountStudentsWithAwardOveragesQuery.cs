namespace Constellation.Application.Students.CountStudentsWithAwardOverages;

using Constellation.Application.Abstractions.Messaging;

public sealed record CountStudentsWithAwardOveragesQuery()
    : IQuery<int>;