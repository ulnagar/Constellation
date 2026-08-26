namespace Constellation.Application.Domains.Auth.Commands.UpdateRole;

using Abstractions.Messaging;
using Application.Models.Identity.Enums;
using System;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    AppRoleType Type)
    : ICommand<Guid>;
