namespace Constellation.Application.Domains.Training.Queries.CountStaffWithoutModule;

using Abstractions.Messaging;

public sealed record CountStaffWithoutModuleQuery()
    : IQuery<int>;