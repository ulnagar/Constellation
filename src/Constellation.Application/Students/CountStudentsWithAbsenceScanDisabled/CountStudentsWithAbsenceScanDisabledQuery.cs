namespace Constellation.Application.Students.CountStudentsWithAbsenceScanDisabled;

using Abstractions.Messaging;

public sealed record CountStudentsWithAbsenceScanDisabledQuery()
    : IQuery<(int Whole, int Partial)>;