namespace Constellation.Application.AdminDashboards.RepairSchoolContactUser;

using Constellation.Application.Abstractions.Messaging;

public sealed record RepairSchoolContactUserCommand(
    int ContactId)
    : ICommand;
