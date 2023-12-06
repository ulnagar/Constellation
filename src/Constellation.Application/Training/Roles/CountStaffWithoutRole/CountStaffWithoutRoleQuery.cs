namespace Constellation.Application.Training.Roles.CountStaffWithoutRole;

using Abstractions.Messaging;

public sealed record CountStaffWithoutRoleQuery()
    : IQuery<int>;