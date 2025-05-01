namespace Constellation.Application.Domains.Auth.Commands.AuditUser;

using Abstractions.Messaging;
using System;

public sealed record AuditUserCommand(Guid UserId)
    : ICommand;