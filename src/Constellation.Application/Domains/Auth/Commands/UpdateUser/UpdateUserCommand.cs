namespace Constellation.Application.Domains.Auth.Commands.UpdateUser;

using Abstractions.Messaging;
using System;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email)
    : ICommand;