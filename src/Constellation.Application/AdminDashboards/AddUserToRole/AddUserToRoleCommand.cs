namespace Constellation.Application.AdminDashboards.AddUserToRole;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record AddUserToRoleCommand(
    Guid RoleId,
    Guid UserId) : ICommand;
