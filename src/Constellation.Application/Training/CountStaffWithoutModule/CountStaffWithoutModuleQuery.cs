namespace Constellation.Application.Training.CountStaffWithoutModule;

using Abstractions.Messaging;

public sealed record CountStaffWithoutModuleQuery()
    : IQuery<int>;