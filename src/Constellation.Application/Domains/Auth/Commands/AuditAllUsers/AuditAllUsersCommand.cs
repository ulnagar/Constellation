namespace Constellation.Application.Domains.Auth.Commands.AuditAllUsers;

using Abstractions.Messaging;

public sealed record AuditAllUsersCommand()
    : ICommand;