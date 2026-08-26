namespace Constellation.Application.Domains.Auth.Commands.AddPermissionToRole;

using Abstractions.Messaging;
using Application.Models.Auth;
using System;

public sealed record AddPermissionToRoleCommand(
    Guid RoleId,
    AuthPermission Permission)
    : ICommand;