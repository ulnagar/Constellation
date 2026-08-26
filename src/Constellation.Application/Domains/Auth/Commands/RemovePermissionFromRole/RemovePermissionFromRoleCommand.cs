namespace Constellation.Application.Domains.Auth.Commands.RemovePermissionFromRole;

using Abstractions.Messaging;
using Application.Models.Auth;
using System;

public sealed record RemovePermissionFromRoleCommand(
    Guid RoleId,
    AuthPermission Permission)
    : ICommand;