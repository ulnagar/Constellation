namespace Constellation.Application.Domains.Auth.Commands.RemoveUserFromRole;

using Abstractions.Messaging;
using System;

public sealed record RemoveUserFromRoleCommand(
    Guid RoleId,
    Guid UserId) : ICommand;
