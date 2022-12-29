namespace Constellation.Application.AdminDashboards.RemoveUserFromRole;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record RemoveUserFromRoleCommand(
    Guid RoleId,
    Guid UserId) : ICommand;
