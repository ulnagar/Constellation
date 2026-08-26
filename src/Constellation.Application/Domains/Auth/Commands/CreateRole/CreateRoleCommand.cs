namespace Constellation.Application.Domains.Auth.Commands.CreateRole;

using Abstractions.Messaging;
using Application.Models.Identity.Enums;
using System;

public sealed record CreateRoleCommand(
    string Name,
    AppRoleType Type)
    : ICommand<Guid>;
