namespace Constellation.Application.Domains.Auth.Commands.AddUserToRole;

using Abstractions.Messaging;
using System;

public sealed record AddUserToRoleCommand(
    Guid RoleId,
    Guid UserId) : ICommand;
