namespace Constellation.Application.Domains.Students.Queries.CountStudentsWithAbsenceScanDisabled;

using Abstractions.Messaging;

public sealed record CountStudentsWithAbsenceScanDisabledQuery()
    : IQuery<(int Whole, int Partial)>;